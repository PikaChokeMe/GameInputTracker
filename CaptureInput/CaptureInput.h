#pragma once

namespace HookCoreErrors
{
	namespace SetCallBack
	{
		const int SUCCESS = 1;
		const int ALREADY_SET = -2;
		const int NOT_IMPLEMENTED = -3;
		const int ARGUMENT_ERROR = -4;
	}
	namespace FilterMessage
	{
		const int SUCCESS = 1;
		const int FAILED = -2;
		const int NOT_IMPLEMENTED = -3;
	}
}

typedef void (CALLBACK *HookProc)(int code, WPARAM w, LPARAM l);

int SetUserHookCallback(HookProc userProc, UINT hookID);
bool InitializeHook(UINT hookID, UINT threadID);
void UninitializeHook(UINT hookID);
void Dispose(UINT hookID);
int FilterMessage(UINT hookID, int message);
bool GetScrollState(WPARAM wparam, LPARAM lparam, int& delta);