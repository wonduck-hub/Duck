// DuckChatClient.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>

#pragma comment(lib, "Ws2_32.lib") // winsock 관련 해더를 가져올 때 VS에서 따로 설정 없이 코드 상으로 링크해야 하는 lib 파일을 설정하는 전처리문

using namespace std;

int main()
{
	WSADATA wsaData;
	struct addrinfo* result = nullptr;
	struct addrinfo hints;

	ZeroMemory(&hints, sizeof(addrinfo));
	hints.ai_family = AF_INET;
	hints.ai_socktype = SOCK_STREAM;
	hints.ai_protocol = IPPROTO_TCP;

	const char* DEFAULT_PORT = "27015";

	int errResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
	if (errResult != 0)
	{
		cout << "WSAStartup failed: " << errResult << endl;
		return 1;
	}

	errResult = getaddrinfo("localhost", DEFAULT_PORT, &hints, &result);
	if (errResult != 0)
	{
		cout << "getaddrinfo failed: " << errResult << endl;
		WSACleanup();
		return 1;
	}

	SOCKET connectSocket = INVALID_SOCKET;

	connectSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);
	if (connectSocket == INVALID_SOCKET)
	{
		cout << "Error at socket(): " << WSAGetLastError() << endl;
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	errResult = connect(connectSocket, result->ai_addr, (int)result->ai_addrlen);
	if (errResult == SOCKET_ERROR)
	{
		cout << "Error at connect(): " << WSAGetLastError() << endl;
		closesocket(connectSocket);
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}

	// 소켓 송수신 로직
	const size_t DEFAULT_BUFFER_SIZE = 512;
	const char* sendBuffer = "Hello server!";

	errResult = send(connectSocket, sendBuffer, (int)strlen(sendBuffer), 0);
	if (errResult == SOCKET_ERROR)
	{
		cout << "Error at send(): " << WSAGetLastError() << endl;
		closesocket(connectSocket);
		freeaddrinfo(result);
		WSACleanup();
		return 1;
	}
	
	return 0;
}

// Run program: Ctrl + F5 or Debug > Start Without Debugging menu
// Debug program: F5 or Debug > Start Debugging menu

// Tips for Getting Started: 
//   1. Use the Solution Explorer window to add/manage files
//   2. Use the Team Explorer window to connect to source control
//   3. Use the Output window to see build output and other messages
//   4. Use the Error List window to view errors
//   5. Go to Project > Add New Item to create new code files, or Project > Add Existing Item to add existing code files to the project
//   6. In the future, to open this project again, go to File > Open > Project and select the .sln file
