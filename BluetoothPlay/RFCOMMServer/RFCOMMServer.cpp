// RFCOMMServer.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <WinSock2.h>
#include <ws2bth.h>
#include <bthsdpdef.h>
#include <bluetoothapis.h>

#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "irprops.lib")

int main()
{
	WORD wVersionRequested;

	WSADATA wsaData;
	int err;
	/* Use the MAKEWORD(lowbyte, highbyte) macro declared in Windef.h */
	wVersionRequested = MAKEWORD(2, 2);

	err = WSAStartup(wVersionRequested, &wsaData);

	if (err != 0) {
		/* Tell the user that we could not find a usable */
		/* Winsock DLL.                                  */
		printf("WSAStartup failed with error: %d\n", err);
		return 1;
	}


	/* Confirm that the WinSock DLL supports 2.2.*/
	/* Note that if the DLL supports versions greater    */
	/* than 2.2 in addition to 2.2, it will still return */
	/* 2.2 in wVersion since that is the version we      */
	/* requested.                                        */

	if (LOBYTE(wsaData.wVersion) != 2 || HIBYTE(wsaData.wVersion) != 2) {
		/* Tell the user that we could not find a usable */
		/* WinSock DLL.                                  */
		printf("Could not find a usable version of Winsock.dll\n");
		WSACleanup();
		return 1;
	}
	else
		printf("The Winsock 2.2 dll was found okay\n");
	/* The Winsock DLL is acceptable. Proceed to use it. */

	/* Add network programming using Winsock here */
	SOCKET s = socket(AF_BTH, SOCK_STREAM, BTHPROTO_RFCOMM);

	const DWORD lastError = ::GetLastError();

	if (s == INVALID_SOCKET)
	{
		printf("Failed to get bluetooth socket! %s\n", lastError);
		
		return 1;
	}

	WSAPROTOCOL_INFO protocolInfo;

	int protocolInfoSize = sizeof(protocolInfo);

	if (getsockopt(s, SOL_SOCKET, SO_PROTOCOL_INFO, (char*)&protocolInfo, &protocolInfoSize) != 0)
	{
		return 1;
	}

	SOCKADDR_BTH address;

	address.addressFamily = AF_BTH;

	address.btAddr = 0;

	address.serviceClassId = GUID_NULL;

	address.port = BT_PORT_ANY;

	sockaddr *pAddr = (sockaddr*)&address;

	if (bind(s, pAddr, sizeof(SOCKADDR_BTH)) != 0)
	{
		printf("%s\n", GetLastError());
	}
	else
	{
		printf("\nBinding Successful....\n");

		int length = sizeof(SOCKADDR_BTH);
		getsockname(s, (sockaddr*)&address, &length);
		wprintf(L"Local Bluetooth device is %04x%08x \nServer channel = %d\n",
			GET_NAP(address.btAddr), GET_SAP(address.btAddr), address.port);
	}

	int size = sizeof(SOCKADDR_BTH);

	if (getsockname(s, pAddr, &size) != 0)
	{
		printf("%s\n", GetLastError());
	}
	if (listen(s, 10) != 0)
	{
		printf("%s\n", GetLastError());
	}

	WSAQUERYSET service;
	memset(&service, 0, sizeof(service));
	service.dwSize = sizeof(service);
	service.lpszServiceInstanceName = _T("Accelerometer Data...");
	service.lpszComment = _T("Pushing data to PC");

	GUID serviceID = OBEXObjectPushServiceClass_UUID;

	service.lpServiceClassId = &serviceID;
	service.dwNumberOfCsAddrs = 1;
	service.dwNameSpace = NS_BTH;

	CSADDR_INFO csAddr;

	memset(&csAddr, 0, sizeof(csAddr));

	csAddr.LocalAddr.iSockaddrLength = sizeof(SOCKADDR_BTH);
	csAddr.LocalAddr.lpSockaddr = pAddr;
	csAddr.iSocketType = SOCK_STREAM;
	csAddr.iProtocol = BTHPROTO_RFCOMM;
	service.lpcsaBuffer = &csAddr;

	if (0 != WSASetService(&service, RNRSERVICE_REGISTER, 0))
	{
		printf("Service registration failed....");
		printf("%d\n", GetLastError());
	}
	else
	{
		printf("\nService registration Successful....\n");
	}

	printf("\nBefore accept.........");

	SOCKADDR_BTH sab2;

	int ilen = sizeof(sab2);

	SOCKET s2 = accept(s, (sockaddr*)&sab2, &ilen);

	if (s2 == INVALID_SOCKET)
	{
		wprintf(L"Socket bind, error %d\n", WSAGetLastError());
	}

	wprintf(L"\nConnection came from %04x%08x to channel %d\n",
		GET_NAP(sab2.btAddr), GET_SAP(sab2.btAddr), sab2.port);

	wprintf(L"\nAfter Accept\n");

	char buffer[1024] = { 0 };

	memset(buffer, 0, sizeof(buffer));

	int r = 0;

	do
	{
		 r = recv(s2, (char*)buffer, sizeof(buffer), 0);

		printf("result:%d, %s\n", r, buffer);

	} while (r != 0);

	closesocket(s2);

	// remove servcie
	if (0 != WSASetService(&service, RNRSERVICE_DELETE, 0))
	{
		printf("%s\n", GetLastError());
	}

	closesocket(s);

	/* then call WSACleanup when done using the Winsock dll */

	WSACleanup();

    return 0;
}

