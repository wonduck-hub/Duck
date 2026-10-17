module;
#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <queue>
#include <assert.h>
#include <cstdint>
#include <deque>
#include <utility>
#include <vector>
#include <mutex>

export module TcpServer;

import ChatProtocol;
import TcpClient;

using namespace std;

namespace Duck
{
	export class TcpChatServer final
	{
		enum
		{
			RECEIVE_ACCUMULATOR_BUFFER = 4096,
			RECEIVE_BUFFER_SIZE = 1024
		};

	public:
		TcpChatServer()
		{
			mClients = vector<TcpClient>();
			mClients.reserve(256);

			mClients.push_back(INVALID_SOCKET);
		}
		virtual ~TcpChatServer() = default;

		bool initServer()
		{
			int result;

			result = WSAStartup(MAKEWORD(2, 2), &mWsaData);
			if (result != 0)
			{
				cout << "WSAStartup failed: " << result << endl;
				return false;
			}

			struct addrinfo* addressInfo = nullptr;
			struct addrinfo hints;

			ZeroMemory(&hints, sizeof(hints));
			hints.ai_family = AF_INET;
			hints.ai_socktype = SOCK_STREAM;
			hints.ai_protocol = IPPROTO_TCP;
			hints.ai_flags = AI_PASSIVE;

			// hints와 result는 각각 입력용 구조체와 실제 개체, 개체이미지 타입이 같음(C 스타일 코드에서 종종 보임)
			result = getaddrinfo(nullptr, DEFAULT_PORT, &hints, &addressInfo);
			if (result != 0)
			{
				cout << "getaddrinfo failed: " << result << endl;
				WSACleanup();
				return false;
			}

			mListenSocket = INVALID_SOCKET;

			mListenSocket = socket(addressInfo->ai_family, addressInfo->ai_socktype, addressInfo->ai_protocol);
			if (mListenSocket == INVALID_SOCKET)
			{
				cout << "socket failed: " << WSAGetLastError() << endl;
				closesocket(mListenSocket);
				WSACleanup();
				return false;
			}

			// Setup the TCP listening socket
			result = bind(mListenSocket, addressInfo->ai_addr, (int)addressInfo->ai_addrlen);
			if (result != 0)
			{
				cout << "bind failed: " << WSAGetLastError() << endl;
				closesocket(mListenSocket);
				WSACleanup();
				return false;
			}

			// 수신 대기
			if (listen(mListenSocket, SOMAXCONN) == SOCKET_ERROR)
			{
				cout << "listen failed: " << WSAGetLastError() << endl;
				closesocket(mListenSocket);
				WSACleanup();
				return false;
			}

			mIsRunning = true;

			return true;
		}

		bool StartServer()
		{
			assert(mListenSocket != INVALID_SOCKET && "mClientSocket is INVALID_SOCKET");

			mIsRunning = true;
			mIsConnectingClient = true;

			return true;
		}

		void StopServer()
		{
			mIsRunning = false;

			if (mListenSocket != INVALID_SOCKET)
			{
				closesocket(mListenSocket);
				mListenSocket = INVALID_SOCKET;
			}

			StopConnectClient();
		}

		void ClearReceiveAccumulator()
		{
			while (!mReceiveAccumulatorBuffer.empty())
			{
				mReceiveAccumulatorBuffer.pop_front();
			}
		}

		void ReceiveData()
		{
			// 소켓 송수신 로직
			const int recvBufferLen = RECEIVE_BUFFER_SIZE;
			char recvBuffer[RECEIVE_BUFFER_SIZE];

			int result;

			for (int clientIndex = 0; clientIndex < mClientCount; ++clientIndex)
			{
				// TODO: 패킷을 받아올 때 모든 클라이언트로부터 받을 수 있으므로 이에 대한 처리 필요
				{
					std::scoped_lock<std::mutex> lock(sClientMutext);
					result = mClients[clientIndex].Receive(recvBuffer, recvBufferLen, 0);
				}

				if (result > 0)
				{
					std::uint32_t readSize = result;

					for (int i = 0; i < readSize; ++i)
					{
						mReceiveAccumulatorBuffer.push_back(static_cast<uint8_t>(recvBuffer[i]));
					}

					cout << "Bytes received: " << recvBuffer << endl;
				}
				else
				{
				}
			}
		}

		bool TryExtractPacket(std::vector<std::uint8_t>& outPacket, Duck::ECommand& outCommand)
		{
			outPacket.clear();

			while (mReceiveAccumulatorBuffer.size() >= Duck::ChatProtocol::HEADER_SIZE)
			{
				if (!IsHeaderAtBufferFront())
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				std::vector<std::uint8_t> parsedPacket;
				Duck::ECommand parsedCommand = Duck::ECommand::CHAT;
				std::size_t packetSize = 0;

				ETryParsePacketResult parseResult = TryParsePacket(parsedPacket, parsedCommand, packetSize);
				if (parseResult == ETryParsePacketResult::Incomplete)
				{
					return false;
				}

				if (parseResult == ETryParsePacketResult::Invalid)
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				for (std::size_t bufferIndex = 0; bufferIndex < packetSize; ++bufferIndex)
				{
					mReceiveAccumulatorBuffer.pop_front();
				}

				outPacket = std::move(parsedPacket);
				outCommand = parsedCommand;
				return true;
			}

			return false;
		}

		void StartConnectClient()
		{
			if (mIsConnectingClient)
			{
				return;
			}

			mIsConnectingClient = true;
			mConnectClientThread =
				std::thread(&TcpChatServer::ConnectingClient, this);
		}

		void StopConnectClient()
		{
			mIsConnectingClient = false;

			if (mListenSocket != INVALID_SOCKET)
			{
				closesocket(mListenSocket);
				mListenSocket = INVALID_SOCKET;
			}

			if (mConnectClientThread.joinable())
			{
				mConnectClientThread.join();
			}
		}

		// Getter
		bool IsRunning() const
		{
			return mIsRunning;
		}
		size_t GetDefaultBufferSize() const
		{
			return RECEIVE_BUFFER_SIZE;
		}


	private:
		enum class ETryParsePacketResult
		{
			Incomplete,
			Invalid,
			Success
		};

		bool IsHeaderAtBufferFront() const
		{
			return mReceiveAccumulatorBuffer[0] == Duck::ChatProtocol::HEADER[0]
				&& mReceiveAccumulatorBuffer[1] == Duck::ChatProtocol::HEADER[1]
				&& mReceiveAccumulatorBuffer[2] == Duck::ChatProtocol::HEADER[2]
				&& mReceiveAccumulatorBuffer[3] == Duck::ChatProtocol::HEADER[3];
		}

		ETryParsePacketResult TryParsePacket(
			std::vector<std::uint8_t>& outPacket,
			Duck::ECommand& outCommand,
			std::size_t& outPacketSize) const
		{
			const std::size_t HEADER_SIZE = Duck::ChatProtocol::HEADER_SIZE;
			const std::size_t COMMAND_INDEX = HEADER_SIZE;
			const std::size_t PAYLOAD_LENGTH_INDEX = COMMAND_INDEX + Duck::ChatProtocol::COMMAND_SIZE;
			const std::size_t PAYLOAD_START_INDEX = PAYLOAD_LENGTH_INDEX + Duck::ChatProtocol::PAYLOAD_LENGTH_SIZE;
			const std::size_t LENGTH_FIELD_END = PAYLOAD_START_INDEX;

			if (mReceiveAccumulatorBuffer.size() < LENGTH_FIELD_END)
			{
				return ETryParsePacketResult::Incomplete;
			}

			const std::size_t PAYLOAD_SIZE =
				static_cast<std::size_t>(mReceiveAccumulatorBuffer[PAYLOAD_LENGTH_INDEX])
				| (static_cast<std::size_t>(mReceiveAccumulatorBuffer[PAYLOAD_LENGTH_INDEX + 1]) << 8);
			const std::size_t CHECKSUM_INDEX = PAYLOAD_START_INDEX + PAYLOAD_SIZE;
			const std::size_t TAIL_INDEX = CHECKSUM_INDEX + Duck::ChatProtocol::CHECKSUM_SIZE;
			const std::size_t PACKET_SIZE = Duck::ChatProtocol::PACKET_MIN_SIZE + PAYLOAD_SIZE;

			if (mReceiveAccumulatorBuffer.size() < PACKET_SIZE)
			{
				return ETryParsePacketResult::Incomplete;
			}

			if (mReceiveAccumulatorBuffer[TAIL_INDEX] != Duck::ChatProtocol::TAIL[0]
				|| mReceiveAccumulatorBuffer[TAIL_INDEX + 1] != Duck::ChatProtocol::TAIL[1]
				|| mReceiveAccumulatorBuffer[TAIL_INDEX + 2] != Duck::ChatProtocol::TAIL[2]
				|| mReceiveAccumulatorBuffer[TAIL_INDEX + 3] != Duck::ChatProtocol::TAIL[3])
			{
				return ETryParsePacketResult::Invalid;
			}

			Duck::ECommand parsedCommand;
			switch (mReceiveAccumulatorBuffer[COMMAND_INDEX])
			{
			case static_cast<int>(Duck::ECommand::CHAT):
				parsedCommand = Duck::ECommand::CHAT;
				break;
			default:
				assert(false && "ECommand 처리 누락 또는 잘못된 명령");
				return ETryParsePacketResult::Invalid;
			}

			std::uint8_t calcCheckSum = 0;
			calcCheckSum += mReceiveAccumulatorBuffer[PAYLOAD_LENGTH_INDEX];
			calcCheckSum += mReceiveAccumulatorBuffer[PAYLOAD_LENGTH_INDEX + 1];
			calcCheckSum += static_cast<std::uint8_t>(parsedCommand);

			std::vector<std::uint8_t> parsedPayload;
			parsedPayload.reserve(PAYLOAD_SIZE);
			for (std::size_t payloadIndex = 0; payloadIndex < PAYLOAD_SIZE; ++payloadIndex)
			{
				const std::uint8_t payloadByte = mReceiveAccumulatorBuffer[PAYLOAD_START_INDEX + payloadIndex];
				parsedPayload.push_back(payloadByte);
				calcCheckSum += payloadByte;
			}

			if (calcCheckSum != mReceiveAccumulatorBuffer[CHECKSUM_INDEX])
			{
				return ETryParsePacketResult::Invalid;
			}

			outPacket = std::move(parsedPayload);
			outCommand = parsedCommand;
			outPacketSize = PACKET_SIZE;
			return ETryParsePacketResult::Success;
		}

		void ConnectingClient()
		{
			while (mIsConnectingClient)
			{
				TcpClient tempSocket = TcpClient(accept(mListenSocket, nullptr, nullptr));

				if (mClients[mClientCount].GetSocket() == INVALID_SOCKET)
				{
					if (!mIsConnectingClient)
					{
						break; // 서버 종료로 accept가 풀린 경우
					}

					cout << "accept failed: " << WSAGetLastError() << endl;
					WSACleanup();

					continue;
				}

				{
					std::scoped_lock<std::mutex> lock(sClientMutext);
					mClients.push_back(tempSocket);
				}
			}
		}

	protected:
		const char* DEFAULT_PORT = "27015";

	private:
		WSADATA mWsaData;
		bool mIsRunning = false;
		bool mIsConnectingClient = false;
		std::deque<std::uint8_t> mReceiveAccumulatorBuffer; // 계속 누적해서 쌓아두는 버퍼
		SOCKET mListenSocket;
		std::vector<Duck::TcpClient> mClients;
		size_t mClientCount = 0;

		std::thread mConnectClientThread;
		std::mutex sClientMutext;
	};
}
