module;
#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <queue>
#include <assert.h>

export module TcpServer;

import ChatProtocol;

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
		TcpChatServer() = default;
		virtual ~TcpChatServer() = default;
		bool startServer()
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

			SOCKET listenSocket = INVALID_SOCKET;

			listenSocket = socket(addressInfo->ai_family, addressInfo->ai_socktype, addressInfo->ai_protocol);
			if (listenSocket == INVALID_SOCKET)
			{
				cout << "socket failed: " << WSAGetLastError() << endl;
				closesocket(listenSocket);
				WSACleanup();
				return false;
			}

			// Setup the TCP listening socket
			result = bind(listenSocket, addressInfo->ai_addr, (int)addressInfo->ai_addrlen);
			if (result != 0)
			{
				cout << "bind failed: " << WSAGetLastError() << endl;
				closesocket(listenSocket);
				WSACleanup();
				return false;
			}

			// 수신 대기
			if (listen(listenSocket, SOMAXCONN) == SOCKET_ERROR)
			{
				cout << "listen failed: " << WSAGetLastError() << endl;
				closesocket(listenSocket);
				WSACleanup();
				return false;
			}

			// TODO: accept()를 호출하면 연결이 들어올 때까지 블로킹되므로, 
			// 별도의 스레드에서 accept()를 호출하도록 변경 필요
			// TODO: 여러 클라이언트 연결을 처리하기 위해, 
			// accept() 호출 후 새로운 스레드를 생성하여 클라이언트와 통신하도록 변경 필요
			SOCKET clientSocket = INVALID_SOCKET;

			clientSocket = accept(listenSocket, nullptr, nullptr);
			if (clientSocket == INVALID_SOCKET)
			{
				cout << "accept failed: " << WSAGetLastError() << endl;
				closesocket(listenSocket);
				WSACleanup();
				return false;
			}

			// 소켓 송수신 로직
			const int recvBufferLen = RECEIVE_BUFFER_SIZE;
			char recvBuffer[RECEIVE_BUFFER_SIZE];

			mIsRunning = true;
			while (mIsRunning)
			{
				result = recv(clientSocket, recvBuffer, recvBufferLen, 0);
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
					return 1;
				}
			}

			return true;
		}

		void SteopServer()
		{
			mIsRunning = false;
		}

		void ClearReceiveAccumulator()
		{
			while (!mReceiveAccumulatorBuffer.empty())
			{
				mReceiveAccumulatorBuffer.pop_front();
			}
		}

		bool TryGetPacket(std::vector<std::uint8_t>& outPacket, Duck::ECommand& outCommand)
		{
			const size_t bufferSize = mReceiveAccumulatorBuffer.size();
			if (bufferSize < Duck::ChatProtocol::PACKET_MIN_SIZE)
			{
				return false;
			}

			while (mReceiveAccumulatorBuffer.size() >= Duck::ChatProtocol::PACKET_MIN_SIZE)
			{
				if (!(mReceiveAccumulatorBuffer[0] == Duck::ChatProtocol::HEADER[0]
					&& mReceiveAccumulatorBuffer[1] == Duck::ChatProtocol::HEADER[1]
					&& mReceiveAccumulatorBuffer[2] == Duck::ChatProtocol::HEADER[2]
					&& mReceiveAccumulatorBuffer[3] == Duck::ChatProtocol::HEADER[3]))
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				const size_t COMMAND_INDEX = 4;
				const std::size_t PAYLOAD_SIZE =
					static_cast<std::size_t>(mReceiveAccumulatorBuffer[5])
					| (static_cast<std::size_t>(mReceiveAccumulatorBuffer[6]) << 8);
				const size_t PAYLOAD_START_INDEX = 7;
				const size_t CHECKSUM_INDEX = PAYLOAD_START_INDEX + PAYLOAD_SIZE;
				const size_t TAIL_INDEX = CHECKSUM_INDEX + 1;
				const std::size_t PACKET_SIZE = Duck::ChatProtocol::HEADER_SIZE + Duck::ChatProtocol::COMMAND_SIZE + Duck::ChatProtocol::PAYLOAD_LENGTH_SIZE
					+ PAYLOAD_SIZE + Duck::ChatProtocol::CHECKSUM_SIZE + Duck::ChatProtocol::TAIL_SIZE;

				if (mReceiveAccumulatorBuffer.size() < PACKET_SIZE)
				{
					return false; // 아직 완전한 패킷이 들어오지 않음
				}

				if (!(mReceiveAccumulatorBuffer[TAIL_INDEX] == Duck::ChatProtocol::TAIL[0]
					&& mReceiveAccumulatorBuffer[TAIL_INDEX + 1] == Duck::ChatProtocol::TAIL[1]
					&& mReceiveAccumulatorBuffer[TAIL_INDEX + 2] == Duck::ChatProtocol::TAIL[2]
					&& mReceiveAccumulatorBuffer[TAIL_INDEX + 3] == Duck::ChatProtocol::TAIL[3]))
				{
					mReceiveAccumulatorBuffer.pop_front();
					continue;
				}

				uint8_t calcCheckSum = 0;

				calcCheckSum += static_cast<std::size_t>(mReceiveAccumulatorBuffer[5]);
				calcCheckSum += static_cast<std::size_t>(mReceiveAccumulatorBuffer[6]) << 8;

				vector<uint8_t> tempPayload;
				tempPayload.reserve(PAYLOAD_SIZE);
				for (size_t payloadIndex = 0; payloadIndex < PAYLOAD_SIZE; ++payloadIndex)
				{
					tempPayload.push_back(mReceiveAccumulatorBuffer[PAYLOAD_START_INDEX + payloadIndex]);
					calcCheckSum += mReceiveAccumulatorBuffer[PAYLOAD_START_INDEX + payloadIndex];
				}

				if (calcCheckSum == mReceiveAccumulatorBuffer[CHECKSUM_INDEX])
				{
					switch (mReceiveAccumulatorBuffer[COMMAND_INDEX])
					{
					case static_cast<int>(Duck::ECommand::CHAT):
						outCommand = Duck::ECommand::CHAT;
						break;
					default:
						assert(false && "ECommand 처리 누락 또는 잘못된 명령");
						mReceiveAccumulatorBuffer.pop_front();
						continue;
					}

					outPacket = tempPayload;

					// 완전한 패킷을 찾았으므로 패킷을 pop하고 true 반환
					for (size_t bufferIndex = 0; bufferIndex < PACKET_SIZE; ++bufferIndex)
					{
						mReceiveAccumulatorBuffer.pop_front();
					}
					return true;
				}

				mReceiveAccumulatorBuffer.pop_front();
				outPacket.clear();
			}

			return false;
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

	protected:
		const char* DEFAULT_PORT = "27015";

	private:
		WSADATA mWsaData;
		bool mIsRunning = false;
		std::deque<std::uint8_t> mReceiveAccumulatorBuffer; // 계속 누적해서 쌓아두는 버퍼
	};
}
