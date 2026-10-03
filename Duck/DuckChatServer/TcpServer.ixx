module;
#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>

export module TcpServer;

using namespace std;

export class TcpServer
{
public :
	TcpServer() = default;
	~TcpServer() = default;
	bool startServer() 
	{
		int errResult;

		errResult = WSAStartup(MAKEWORD(2, 2), &mWsaData);
		if (errResult != 0)
		{
			cout << "WSAStartup failed: " << errResult << endl;
			return false;
		}

		struct addrinfo* result = nullptr;
		struct addrinfo hints;

		ZeroMemory(&hints, sizeof(hints));
		hints.ai_family = AF_INET;
		hints.ai_socktype = SOCK_STREAM;
		hints.ai_protocol = IPPROTO_TCP;
		hints.ai_flags = AI_PASSIVE;

		// hints와 result는 각각 입력용 구조체와 실제 개체, 개체이미지 타입이 같음(C 스타일 코드에서 종종 보임)
		errResult = getaddrinfo(nullptr, DEFAULT_PORT, &hints, &result);
		if (errResult != 0)
		{
			cout << "getaddrinfo failed: " << errResult << endl;
			WSACleanup();
			return false;
		}

		SOCKET listenSocket = INVALID_SOCKET;

		listenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
		if (listenSocket == INVALID_SOCKET)
		{
			cout << "socket failed: " << WSAGetLastError() << endl;
			closesocket(listenSocket);
			WSACleanup();
			return false;
		}

		// Setup the TCP listening socket
		errResult = bind(listenSocket, result->ai_addr, (int)result->ai_addrlen);
		if (errResult != 0)
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
		const int recvBufferLen = DEFAULT_BUFFER_SIZE;
		char recvBuffer[DEFAULT_BUFFER_SIZE];

		while (true)
		{
			errResult = recv(clientSocket, recvBuffer, recvBufferLen, 0);
			if (errResult > 0)
			{
				cout << "Bytes received: " << recvBuffer << endl;
			}
			else
			{
				return 1;
			}
		}

		return true;
	}
	bool GetIsRunning() const
	{
		return mIsRunning;
	}
	size_t GetDefaultBufferSize() const
	{
		return DEFAULT_BUFFER_SIZE;
	}
	

private:

protected:
	const char* DEFAULT_PORT = "27015";
	static const size_t DEFAULT_BUFFER_SIZE = 512;

private:
	WSADATA mWsaData;
	bool mIsRunning = false;
};
