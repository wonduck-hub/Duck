// DuckChatServer.cpp : 이 파일에는 'main' 함수가 포함됩니다. 거기서 프로그램 실행이 시작되고 종료됩니다.
//

#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <assert.h>

#pragma comment(lib, "Ws2_32.lib") // winsock 관련 해더를 가져올 때 VS에서 따로 설정 없이 코드 상으로 링크해야 하는 lib 파일을 설정하는 전처리문

import TcpServer;

using namespace std;

int main()
{
	Duck::TcpChatServer server;
	if (!server.initServer())
	{
		cout << "Failed to start server." << endl;
		return 1;
	}

	server.StartConnectClient();

	Duck::ECommand commandOut;
	std::vector<uint8_t> payloadOut;
	while (true)
	{
		// TODO: Receive를 하는 스레드를 별도로 만들어 관리 필요
		server.ReceiveData();

		while (server.TryExtractPacket(payloadOut, commandOut))
		{
			cout << "Extract Success: " << static_cast<uint8_t>(commandOut) << endl;

			switch (commandOut)
			{
			case Duck::ECommand::CHAT:
				cout << payloadOut.size() << endl;
				break;
			default:
				assert(false && "ECommand 처리 누락 또는 잘못된 명령");
				break;
			}
		}
	}

	return 0;
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
