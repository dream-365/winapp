// RFCOMMServer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "Winsock.h"
#include <WinSock2.h>
#include <ws2bth.h>
#include <bthsdpdef.h>
#include <bluetoothapis.h>

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "irprops.lib")

int main()
{
	if (!Winsock::InitialzedForCurrentProcess())
	{
		return 1;
	}

	Winsock winsock = Winsock(AF_BTH, SOCK_STREAM, BTHPROTO_RFCOMM);

	if (!winsock.IsValid())
	{
		return 1;
	}

	winsock.Bind();

	winsock.Listen();

	winsock.RegisterServcie(_T("RFCOMM Server Demo Instance"), _T("Pushing data to PC"), OBEXObjectPushServiceClass_UUID);

	winsock.OnConnected([](SOCKET s){
		char buffer[1024] = { 0 };

		memset(buffer, 0, sizeof(buffer));

		int r = 0;

		do
		{
			r = recv(s, (char*)buffer, sizeof(buffer), 0);

			printf("result:%d, %s\n", r, buffer);

		} while (r != 0);

		closesocket(s);
	});

	winsock.Accept();

	WSACleanup();

    return 0;
}

