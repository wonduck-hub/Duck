module;
#include <iostream>
#include <WinSock2.h>
#include <ws2tcpip.h>
#include <assert.h>

export module TcpClient;

namespace Duck
{
	export class TcpClient
	{
	public:
		TcpClient(SOCKET socket)
		{
			mSocket = socket;
		}
		virtual ~TcpClient()
		{

		}

		int Receive(char buffer[], size_t bufferLen, int flage)
		{
			return recv(mSocket, buffer, bufferLen, 0);
		}

		// Getter
		SOCKET GetSocket()
		{
			return mSocket;
		}

	private:

	public:

	private:
		SOCKET mSocket;

	};
}
