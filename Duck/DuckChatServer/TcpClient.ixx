module;
#include <WinSock2.h>
#include <utility>
#include <mutex>
#include <thread>
#include <vector>
#include <deque>
#include <cstdint>
#include <iostream>
#include <atomic>
#include <assert.h>

export module TcpClient;

import DuckPacket;

namespace Duck
{
	export class TcpClient final
	{
	public:
		explicit TcpClient(SOCKET socket)
			: mSocket(socket)
		{}
		~TcpClient()
		{
			EndReceive();
			CloseSocket();
		}

		void StartReceive()
		{
			if (mIsReceive)
			{
				return;
			}

			mIsReceive = true;
			mReceiveThread = std::thread(&TcpClient::Receive, this);
		}

		void EndReceive()
		{
			if (!mIsReceive)
			{
				return;
			}

			if (mSocket != INVALID_SOCKET) // 이렇게 shutdown을 해야 Receive에서 무한이 recv 함수를 기다리는 사태를 막음
			{
				shutdown(mSocket, SD_RECEIVE);
			}

			if (mReceiveThread.joinable())
			{
				mIsReceive = false;
				mReceiveThread.join();
			}
		}

		bool TryExtractPacket(Duck::DuckPacket& outProtocol)
		{

			// TODO: 이 함수 전체에 mReceiveMutex 락을 거는게 맞는지 확인 필요(성능상 좋지 않을 것으로 예상됨)
			std::scoped_lock<std::mutex> lock(mReceiveMutex);

			while (mReceiveAccumulatorBuffer.size() >= Duck::DuckPacket::HEADER_SIZE)
			{
				if (!Duck::DuckPacket::IsHeaderAtBufferFront(mReceiveAccumulatorBuffer))
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				Duck::ETryParsePacketResult parseResult = Duck::DuckPacket::TryParsePacket(mReceiveAccumulatorBuffer, outProtocol);
				if (parseResult == Duck::ETryParsePacketResult::Incomplete)
				{
					return false;
				}

				if (parseResult == Duck::ETryParsePacketResult::Invalid)
				{
					mReceiveAccumulatorBuffer.pop_front();
					return false;
				}

				for (std::size_t bufferIndex = 0; bufferIndex < outProtocol.GetProtocolSize(); ++bufferIndex)
				{
					mReceiveAccumulatorBuffer.pop_front();
				}

				return true;
			}


			return false;
		}

		// Getter

		SOCKET GetSocket() const
		{
			return mSocket;
		}



	private:
		void Receive()
		{
			// 소켓 송수신 로직
			const int recvBufferLen = 1024;
			char recvBuffer[1024] = {};

			int result = 0;

			while (mIsReceive)
			{
				result = recv(mSocket, recvBuffer, recvBufferLen, 0);

				if (result > 0)
				{
					std::uint32_t readSize = result;
					std::scoped_lock<std::mutex> lock(mReceiveMutex);

					for (size_t i = 0; i < readSize; ++i)
					{
						mReceiveAccumulatorBuffer.push_back(recvBuffer[i]);
					}

					continue;
				}

				if (result == 0)
				{
					std::cout << "소켓: " << mSocket << " - 정상 종료" << std::endl;
					break;
				}

				if (!mIsReceive)
				{
					break;
				}
			}
		}

		void CloseSocket()
		{
			if (mSocket != INVALID_SOCKET)
			{
				closesocket(mSocket);
				mSocket = INVALID_SOCKET;
			}
		}

	public:

	private:
		SOCKET mSocket = INVALID_SOCKET;
		std::deque<std::uint8_t> mReceiveAccumulatorBuffer = std::deque<std::uint8_t>(); // 계속 누적해서 쌓아두는 버퍼

		std::atomic_bool mIsReceive = false;
		std::thread mReceiveThread;
		std::mutex mReceiveMutex;
	};
}
