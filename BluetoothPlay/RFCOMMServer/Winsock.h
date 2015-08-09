#pragma once
#include <WinSock2.h>
#include <functional>

class Winsock
{
private:
	static bool g_isInitialzedForCurrentProcess;

	bool _isValidSocket = false;

	SOCKET _socket = NULL;

	std::function<void(SOCKET)> _onConnectedHanlder;

	WSAQUERYSET _service;

public:
	Winsock(int af, int type, int protocol);
	~Winsock();

	bool IsValid();

	void Bind();

	void Listen();

	void RegisterServcie(LPWSTR const name, LPWSTR const desp, GUID uuid);

	void OnConnected(std::function<void(SOCKET)> handler);

	void Accept();

	static bool InitialzedForCurrentProcess();
	static void UnInitialze();
};


