#include "stdafx.h"
#include "Winsock.h"
#include <WinSock2.h>
#include <ws2bth.h>
#include <bthsdpdef.h>
#include <bluetoothapis.h>

bool Winsock::g_isInitialzedForCurrentProcess = false;

Winsock::Winsock(int af, int type, int protocol)
{
	/* Add network programming using Winsock here */
	SOCKET s = socket(af, type, protocol);

	const DWORD lastError = ::GetLastError();

	if (s == INVALID_SOCKET)
	{
		printf("Failed to get bluetooth socket! %d\n", lastError);

		return;
	}

	WSAPROTOCOL_INFO protocolInfo;

	int protocolInfoSize = sizeof(protocolInfo);

	if (getsockopt(s, SOL_SOCKET, SO_PROTOCOL_INFO, (char*)&protocolInfo, &protocolInfoSize) != 0)
	{
		return;
	}

	_socket = s;

	_isValidSocket = true;
}

Winsock::~Winsock()
{
	// remove servcie
	if (0 != WSASetService(&_service, RNRSERVICE_DELETE, 0))
	{
		printf("%d\n", GetLastError());
	}

	closesocket(_socket);
}

bool Winsock::IsValid()
{
	return _isValidSocket;
}

void Winsock::Bind()
{
	SOCKADDR_BTH address;

	address.addressFamily = AF_BTH;

	address.btAddr = 0;

	address.serviceClassId = GUID_NULL;

	address.port = BT_PORT_ANY;

	sockaddr *pAddr = (sockaddr*)&address;

	if (bind(_socket, pAddr, sizeof(SOCKADDR_BTH)) != 0)
	{
		printf("%d\n", GetLastError());

		return;
	}

	printf("\nBinding Successful....\n");

	int length = sizeof(SOCKADDR_BTH);
	getsockname(_socket, (sockaddr*)&address, &length);
	wprintf(L"Local Bluetooth device is %04x%08x \nServer channel = %d\n",
		GET_NAP(address.btAddr), GET_SAP(address.btAddr), address.port);
}

void Winsock::Listen()
{
	if (listen(_socket, 10) != 0)
	{
		printf("%d\n", GetLastError());
	}
}

void Winsock::RegisterServcie(LPWSTR const name, LPWSTR const desp, GUID uuid)
{
	memset(&_service, 0, sizeof(_service));
	_service.dwSize = sizeof(_service);
	_service.lpszServiceInstanceName = name;
	_service.lpszComment = desp;
	GUID serviceID = uuid;
	_service.lpServiceClassId = &serviceID;
	_service.dwNumberOfCsAddrs = 1; // This member is ignored for queries.
	_service.dwNameSpace = NS_BTH;
	
	SOCKADDR_BTH address;
	sockaddr *pAddr = (sockaddr*)&address;

	int length = sizeof(SOCKADDR_BTH);

	getsockname(_socket, pAddr, &length);

	CSADDR_INFO csAddr;
	memset(&csAddr, 0, sizeof(csAddr));
	csAddr.LocalAddr.iSockaddrLength = sizeof(SOCKADDR_BTH);
	csAddr.LocalAddr.lpSockaddr = pAddr;
	csAddr.iSocketType = SOCK_STREAM;
	csAddr.iProtocol = BTHPROTO_RFCOMM;
	_service.lpcsaBuffer = &csAddr;

	if (0 != WSASetService(&_service, RNRSERVICE_REGISTER, 0))
	{
		printf("Service registration failed....");
		printf("%d\n", GetLastError());
	}
	else
	{
		printf("\nService registration Successful....\n");
	}
}

void Winsock::OnConnected(std::function<void(SOCKET)> handler)
{
	_onConnectedHanlder = handler;
}

void Winsock::Accept()
{
	printf("\nBefore accept.........");

	SOCKADDR_BTH sab;

	int ilen = sizeof(sab);

	SOCKET s = accept(_socket, (sockaddr*)&sab, &ilen);

	if (s == INVALID_SOCKET)
	{
		wprintf(L"Socket bind, error %d\n", WSAGetLastError());
	}

	wprintf(L"\nConnection came from %04x%08x to channel %d\n", GET_NAP(sab.btAddr), GET_SAP(sab.btAddr), sab.port);

	wprintf(L"\nAfter Accept\n");

	if (_onConnectedHanlder != NULL)
	{
		_onConnectedHanlder(s);
	}
}

bool Winsock::InitialzedForCurrentProcess()
{
	if (g_isInitialzedForCurrentProcess)
	{
		return true;
	}

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

		return false;
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

		return false;
	}
	else {
		printf("The Winsock 2.2 dll was found okay\n");
	}

	g_isInitialzedForCurrentProcess = true;

	return true;
}

void Winsock::UnInitialze()
{
	WSACleanup();
}


