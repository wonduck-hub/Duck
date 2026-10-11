module;
#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <queue>

export module TcpServer;

import ChatProtocol;

using namespace std;

namespace Duck
{
	export class TcpServer final
	{
		enum
		{
			RECEIVE_ACCUMULATOR_BUFFER = 4096,
			RECEIVE_BUFFER_SIZE = 1024
		};

	public:
		TcpServer() = default;
		virtual ~TcpServer() = default;
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
						mReceiveAccumulator.push(static_cast<uint8_t>(recvBuffer[i]));
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
			while (!mReceiveAccumulator.empty())
			{
				mReceiveAccumulator.pop();
			}
		}

		bool TryGetPacket(std::vector<std::uint8_t>& outPacket)
		{
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
		std::queue<std::uint8_t> mReceiveAccumulator; // 계속 누적해서 쌓아두는 버퍼
	};
}
