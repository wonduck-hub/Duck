module;
#include <WinSock2.h>
#include <ws2tcpip.h>
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

using namespace std;

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

				assert(outProtocol.GetVersion() != Duck::EVersion::NONE && "패킷 읽기 실패");

				for (std::size_t bufferIndex = 0; bufferIndex < outProtocol.GetProtocolSize(); ++bufferIndex)
				{
					mReceiveAccumulatorBuffer.pop_front();
				}

				return true;
			}


			return false;
		}

		static bool TryConnect(const char* address, const char* port, std::unique_ptr<TcpClient>& outClientOrNull)
		{
			outClientOrNull.reset();

			WSADATA wsaData;
			struct addrinfo* result = nullptr;
			struct addrinfo hints;

			ZeroMemory(&hints, sizeof(addrinfo));
			hints.ai_family = AF_INET;
			hints.ai_socktype = SOCK_STREAM;
			hints.ai_protocol = IPPROTO_TCP;

			int errResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
			if (errResult != 0)
			{
				cout << "WSAStartup failed: " << errResult << endl;
				return false;
			}

			errResult = getaddrinfo(address, port, &hints, &result);
			if (errResult != 0)
			{
				cout << "getaddrinfo failed: " << errResult << endl;
				WSACleanup();
				return false;
			}

			SOCKET connectSocket;

			connectSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
			if (connectSocket == INVALID_SOCKET)
			{
				cout << "Error at socket(): " << WSAGetLastError() << endl;
				freeaddrinfo(result);
				WSACleanup();
				return false;
			}

			errResult = connect(connectSocket, result->ai_addr, (int)result->ai_addrlen);
			if (errResult == SOCKET_ERROR)
			{
				cout << "Error at connect(): " << WSAGetLastError() << endl;
				closesocket(connectSocket);
				freeaddrinfo(result);
				WSACleanup();
				return false;
			}

			outClientOrNull = std::make_unique<TcpClient>(connectSocket);
			return true;
		}

		bool Send(const Duck::DuckPacket& packet)
		{
			const std::vector<std::uint8_t> packetBytes = packet.GetPacketBytes();

			std::size_t sentBytes = 0; // 이전에 보냈던 bytes 기억용 변수

			std::scoped_lock<std::mutex> lock(mSendMutex);

			while (sentBytes < packetBytes.size())
			{
				const std::size_t remainingBytes = packetBytes.size() - sentBytes;
				const int sendSize = static_cast<int>(
					remainingBytes > 65536 ? 65536 : remainingBytes);

				const char* sendData = reinterpret_cast<const char*>(
					packetBytes.data() + sentBytes);

				const int result = send(mSocket, sendData, sendSize, 0);

				if (result == SOCKET_ERROR || result == 0)
				{
					return false;
				}

				sentBytes += static_cast<std::size_t>(result);
			}

			return true;
		}

		// Getter

		SOCKET GetSocket() const
		{
			return mSocket;
		}

		bool IsSocketOpen() const
		{
			return mSocket != SOCKET_ERROR && mSocket != INVALID_SOCKET;
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

		std::mutex mSendMutex;
	};
}