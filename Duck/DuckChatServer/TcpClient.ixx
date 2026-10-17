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

import ChatProtocol;

namespace Duck
{
	enum class ETryParsePacketResult
	{
		Incomplete,
		Invalid,
		Success
	};

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

		bool TryExtractPacket(std::vector<std::uint8_t>& outPayload, Duck::ECommand& outCommand)
		{
			outPayload.clear();

			while (mReceiveAccumulatorBuffer.size() >= Duck::ChatProtocol::HEADER_SIZE)
			{
				if (!IsHeaderAtBufferFront(mReceiveAccumulatorBuffer))
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				std::vector<std::uint8_t> parsedPacket;
				Duck::ECommand parsedCommand = Duck::ECommand::CHAT;
				std::size_t packetSize = 0;

				ETryParsePacketResult parseResult = TryParsePacket(mReceiveAccumulatorBuffer, parsedPacket, parsedCommand, packetSize);
				if (parseResult == ETryParsePacketResult::Incomplete)
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				if (parseResult == ETryParsePacketResult::Invalid)
				{
					return false;
				}

				for (std::size_t bufferIndex = 0; bufferIndex < packetSize; ++bufferIndex)
				{
					mReceiveAccumulatorBuffer.pop_front();
				}

				outPayload = std::move(parsedPacket);
				outCommand = parsedCommand;
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
		bool IsHeaderAtBufferFront(std::deque<std::uint8_t> buffer) const
		{
			return buffer[0] == Duck::ChatProtocol::HEADER[0]
				&& buffer[1] == Duck::ChatProtocol::HEADER[1]
				&& buffer[2] == Duck::ChatProtocol::HEADER[2]
				&& buffer[3] == Duck::ChatProtocol::HEADER[3];
		}

		ETryParsePacketResult TryParsePacket(
			std::deque<std::uint8_t> buffer,
			std::vector<std::uint8_t>& outPayload,
			Duck::ECommand& outCommand,
			std::size_t& outPacketSize) const
		{
			const std::size_t HEADER_SIZE = Duck::ChatProtocol::HEADER_SIZE;
			const std::size_t COMMAND_INDEX = HEADER_SIZE;
			const std::size_t PAYLOAD_LENGTH_INDEX = COMMAND_INDEX + Duck::ChatProtocol::COMMAND_SIZE;
			const std::size_t PAYLOAD_START_INDEX = PAYLOAD_LENGTH_INDEX + Duck::ChatProtocol::PAYLOAD_LENGTH_SIZE;
			const std::size_t LENGTH_FIELD_END = PAYLOAD_START_INDEX;

			if (buffer.size() < LENGTH_FIELD_END)
			{
				return ETryParsePacketResult::Incomplete;
			}

			const std::size_t PAYLOAD_SIZE =
				static_cast<std::size_t>(buffer[PAYLOAD_LENGTH_INDEX])
				| (static_cast<std::size_t>(buffer[PAYLOAD_LENGTH_INDEX + 1]) << 8);
			const std::size_t CHECKSUM_INDEX = PAYLOAD_START_INDEX + PAYLOAD_SIZE;
			const std::size_t TAIL_INDEX = CHECKSUM_INDEX + Duck::ChatProtocol::CHECKSUM_SIZE;
			const std::size_t PACKET_SIZE = Duck::ChatProtocol::PACKET_MIN_SIZE + PAYLOAD_SIZE;

			if (buffer.size() < PACKET_SIZE)
			{
				return ETryParsePacketResult::Incomplete;
			}

			if (buffer[TAIL_INDEX] != Duck::ChatProtocol::TAIL[0]
				|| buffer[TAIL_INDEX + 1] != Duck::ChatProtocol::TAIL[1]
				|| buffer[TAIL_INDEX + 2] != Duck::ChatProtocol::TAIL[2]
				|| buffer[TAIL_INDEX + 3] != Duck::ChatProtocol::TAIL[3])
			{
				return ETryParsePacketResult::Invalid;
			}

			Duck::ECommand parsedCommand;
			switch (buffer[COMMAND_INDEX])
			{
			case static_cast<int>(Duck::ECommand::CHAT):
				parsedCommand = Duck::ECommand::CHAT;
				break;
			default:
				assert(false && "ECommand 처리 누락 또는 잘못된 명령");
				return ETryParsePacketResult::Invalid;
			}

			std::uint8_t calcCheckSum = 0;
			calcCheckSum += buffer[PAYLOAD_LENGTH_INDEX];
			calcCheckSum += buffer[PAYLOAD_LENGTH_INDEX + 1];
			calcCheckSum += static_cast<std::uint8_t>(parsedCommand);

			std::vector<std::uint8_t> parsedPayload;
			parsedPayload.reserve(PAYLOAD_SIZE);
			for (std::size_t payloadIndex = 0; payloadIndex < PAYLOAD_SIZE; ++payloadIndex)
			{
				const std::uint8_t payloadByte = buffer[PAYLOAD_START_INDEX + payloadIndex];
				parsedPayload.push_back(payloadByte);
				calcCheckSum += payloadByte;
			}

			if (calcCheckSum != buffer[CHECKSUM_INDEX])
			{
				return ETryParsePacketResult::Invalid;
			}

			outPayload = std::move(parsedPayload);
			outCommand = parsedCommand;
			outPacketSize = PACKET_SIZE;
			return ETryParsePacketResult::Success;
		}

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
