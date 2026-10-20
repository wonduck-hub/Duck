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
#include <atomic>

export module TcpServer;

import DuckPacket;
import TcpClient;

using namespace std;

namespace Duck
{
	export class TcpChatServer final
	{
	public:
		TcpChatServer()
		{
			mClients.reserve(256);
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

			EndConnectClient();
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

		void EndConnectClient()
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

		std::vector<DuckPacket> GetPackets()
		{
			std::vector<DuckPacket> result = std::vector<DuckPacket>();

			size_t clientCount = 0;
			{
				std::scoped_lock<std::mutex> lock(mClientMutext);
				clientCount = mClients.size();
			}

			for (int clientIndex = 0; clientIndex < clientCount; ++clientIndex)
			{
				Duck::DuckPacket outTempProtocol;
				if (mClients[clientIndex]->TryExtractPacket(outTempProtocol))
				{
					result.push_back(outTempProtocol);
				}
			}

			return result;
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

		void ConnectingClient()
		{
			while (mIsConnectingClient)
			{
				SOCKET clientSocket = accept(mListenSocket, nullptr, nullptr);

				if (clientSocket == INVALID_SOCKET)
				{
					if (!mIsConnectingClient)
					{
						break; // 서버 종료로 accept가 풀린 경우
					}

					cout << "accept failed: " << WSAGetLastError() << endl;
					continue;
				}

				{
					std::scoped_lock<std::mutex> lock(mClientMutext);
					mClients.emplace_back(std::make_unique<Duck::TcpClient>(clientSocket));
					mClients.back()->StartReceive();
				}
			}
		}

	protected:


	private:
		WSADATA mWsaData;
		std::atomic_bool mIsRunning = false;
		std::atomic_bool mIsConnectingClient = false;
		SOCKET mListenSocket = INVALID_SOCKET;

		std::vector<std::unique_ptr<Duck::TcpClient>> mClients;
		std::mutex mClientMutext;
		std::thread mConnectClientThread;

		const char* DEFAULT_PORT = "27015";
		static constexpr std::uint32_t RECEIVE_ACCUMULATOR_BUFFER = 4096;
		static constexpr std::uint32_t RECEIVE_BUFFER_SIZE = 1024;
	};
}
