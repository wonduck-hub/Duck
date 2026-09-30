// DuckChatServer.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>

#pragma comment(lib, "Ws2_32.lib") // winsock 관련 해더를 가져올 때 VS에서 따로 설정 없이 코드 상으로 링크해야 하는 lib 파일을 설정하는 전처리문

using namespace std;

int main()
{
    WSADATA wsaData;

    int errResult;

    errResult = WSAStartup(MAKEWORD(2, 2), &wsaData);
    if (errResult != 0) 
    {
        cout << "WSAStartup failed: " << errResult << endl;
        return 1;
    }

    // 주소 및 연결 설정
    const char* DEFAULT_PORT = "27015";

    struct addrinfo* result = nullptr;
    struct addrinfo hints;

    ZeroMemory(&hints, sizeof(hints));
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;
    hints.ai_flags = AI_PASSIVE;

    // hints와 result는 각각 입력용 구조체와 실제 개체를 저장하는 개체이미지 타입이 같음(C 스타일 코드에서 종종 보임)
    errResult = getaddrinfo(nullptr, DEFAULT_PORT, &hints, &result);
    if (errResult != 0)
    {
        cout << "getaddrinfo failed: " << errResult << endl;
        WSACleanup();
        return 1;
    }

    SOCKET listenSocket = INVALID_SOCKET;

    // Create a SOCKET for the server to listen for client connections
    listenSocket = socket(result->ai_family, result->ai_socktype, result->ai_protocol);

    if (listenSocket == INVALID_SOCKET)
    {
        cout << "Error at socket(): " << WSAGetLastError() << endl;
        freeaddrinfo(result);
        WSACleanup();
        return 1;
    }

    // Setup the TCP listening socket
    errResult = bind(listenSocket, result->ai_addr, result->ai_addrlen);
    if (errResult == SOCKET_ERROR)
    {
        printf("bind failed with error: %d\n", WSAGetLastError());
        freeaddrinfo(result);
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    // 소켓 수신 대기
    // while 문에서 반복해서 호출하는 것이 아닌 listenSocket을 연결을 받을 준비가 된 상태로 전환하는 것
    if (listen(listenSocket, SOMAXCONN) == SOCKET_ERROR)
    {
        printf("Listen failed with error: %ld\n", WSAGetLastError());
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    // 소켓 연결을 하려면 SOCKET 개체로 clientSocket을 받아야 함
    // 연결할 클라이언트의 소켓을 코기화
    SOCKET clientSocket = INVALID_SOCKET;

    clientSocket = accept(listenSocket, nullptr, nullptr);
    if (clientSocket == INVALID_SOCKET)
    {
        cout << "Accept error: " << WSAGetLastError() << endl;
        closesocket(listenSocket);
        WSACleanup();
        return 1;
    }

    // 소켓 송수신 로직
    const size_t DEFAULT_BUFFER_SIZE = 512;
    char recvBuffer[DEFAULT_BUFFER_SIZE];
    int recvBufferLen = DEFAULT_BUFFER_SIZE;

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
}

// 프로그램 실행: Ctrl+F5 또는 디버그 > 디버깅하지 않고 시작 메뉴
// 프로그램 디버그: F5 키 또는 디버그 > 디버깅 시작 메뉴

// 시작을 위한 팁: 
//   1. [솔루션 탐색기] 창을 사용하여 파일을 추가/관리합니다.
//   2. [팀 탐색기] 창을 사용하여 소스 제어에 연결합니다.
//   3. [출력] 창을 사용하여 빌드 출력 및 기타 메시지를 확인합니다.
//   4. [오류 목록] 창을 사용하여 오류를 봅니다.
//   5. [프로젝트] > [새 항목 추가]로 이동하여 새 코드 파일을 만들거나, [프로젝트] > [기존 항목 추가]로 이동하여 기존 코드 파일을 프로젝트에 추가합니다.
//   6. 나중에 이 프로젝트를 다시 열려면 [파일] > [열기] > [프로젝트]로 이동하고 .sln 파일을 선택합니다.
