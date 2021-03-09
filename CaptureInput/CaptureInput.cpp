#include "stdafx.h"
#include <windows.h>
#include "CaptureInput.h"
#include "MessageFilter.h"
#include <type_traits>

HookProc UserMouseHookCallback = NULL;
HookProc UserKeyboardHookCallback = NULL;
HHOOK hookMouse = NULL;
HHOOK hookKeyboard = NULL;

//
// Store the application instance of this module to pass to
// hook initialization. This is set in DLLMain().
//
HINSTANCE g_appInstance = NULL;

MessageFilter mouseFilter;
MessageFilter keyboardFilter;

static LRESULT CALLBACK InternalMouseHookCallback(int code, WPARAM wparam, LPARAM lparam);
static LRESULT CALLBACK InternalKeyboardHookCallback(int code, WPARAM wparam, LPARAM lparam);

int SetUserHookCallback(HookProc userProc, UINT hookID)
{
	if (userProc == NULL)
	{
		return HookCoreErrors::SetCallBack::ARGUMENT_ERROR;
	}

	if (hookID == WH_MOUSE_LL)
	{
		if (UserMouseHookCallback != NULL)
		{
			return HookCoreErrors::SetCallBack::ALREADY_SET;
		}

		UserMouseHookCallback = userProc;
		mouseFilter.Clear();
		return HookCoreErrors::SetCallBack::SUCCESS;
	}
	else if (hookID == WH_KEYBOARD_LL) 
	{
		if (UserKeyboardHookCallback != NULL)
		{
			return HookCoreErrors::SetCallBack::ALREADY_SET;
		}

		UserKeyboardHookCallback = userProc;
		keyboardFilter.Clear();
		return HookCoreErrors::SetCallBack::SUCCESS;
	}

	return HookCoreErrors::SetCallBack::NOT_IMPLEMENTED;
}

bool InitializeHook(UINT hookID, UINT threadID)
{
	if (g_appInstance == NULL)
	{
		return false;
	}

	if (hookID == WH_MOUSE_LL)
	{
		if (UserMouseHookCallback == NULL)
		{
			return false;
		}

		hookMouse = SetWindowsHookEx(hookID, (HOOKPROC)InternalMouseHookCallback, g_appInstance, threadID);
		return hookMouse != NULL;
	}
	else if (hookID == WH_KEYBOARD_LL)
	{
		if (UserKeyboardHookCallback == NULL)
		{
			return false;
		}

		hookKeyboard = SetWindowsHookEx(hookID, (HOOKPROC)InternalKeyboardHookCallback, g_appInstance, threadID);
		return hookKeyboard != NULL;
	}

	return true;
}

void UninitializeHook(UINT hookID)
{
	if (hookID == WH_MOUSE_LL)
	{
		if (hookMouse != NULL)
		{
			UnhookWindowsHookEx(hookMouse);
		}
		hookMouse = NULL;
	}
	else if (hookID == WH_KEYBOARD_LL)
	{
		if (hookKeyboard != NULL)
		{
			UnhookWindowsHookEx(hookKeyboard);
		}
		hookKeyboard = NULL;
	}
}

void Dispose(UINT hookID)
{
	if (hookID == WH_MOUSE_LL)
	{
		UserMouseHookCallback = NULL;
	}
	else if (hookID == WH_KEYBOARD_LL)
	{
		UserKeyboardHookCallback = NULL;
	}
}

int FilterMessage(UINT hookID, int message)
{
	if (hookID == WH_MOUSE_LL)
	{
		if (mouseFilter.AddMessage(message))
		{
			return HookCoreErrors::FilterMessage::SUCCESS;
		}
		else
		{
			return HookCoreErrors::FilterMessage::FAILED;
		}
	}

	return HookCoreErrors::FilterMessage::NOT_IMPLEMENTED;
}

static LRESULT CALLBACK InternalMouseHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code < 0)
	{
		return CallNextHookEx(hookMouse, code, wparam, lparam);
	}

	if (UserMouseHookCallback != NULL && !mouseFilter.IsFiltered((int)wparam))
	{
		UserMouseHookCallback(code, wparam, lparam);
	}

	return CallNextHookEx(hookMouse, code, wparam, lparam);
}

static LRESULT CALLBACK InternalKeyboardHookCallback(int code, WPARAM wparam, LPARAM lparam)
{
	if (code < 0)
	{
		return CallNextHookEx(hookKeyboard, code, wparam, lparam);
	}

	if (UserKeyboardHookCallback != NULL && !keyboardFilter.IsFiltered((int)wparam))
	{
		UserKeyboardHookCallback(code, wparam, lparam);
	}

	return CallNextHookEx(hookKeyboard, code, wparam, lparam);
}

bool GetScrollState(WPARAM wparam, LPARAM lparam, int& delta)
{
	MSLLHOOKSTRUCT* mouseInfo = reinterpret_cast<MSLLHOOKSTRUCT*>(lparam);

	if (mouseInfo == NULL)
	{
		return false;
	}

	delta = static_cast<std::make_signed_t<WORD>>(HIWORD(mouseInfo->mouseData));

	return true;
}

bool GetKeyboardReading(WPARAM wparam, LPARAM lparam, int& vkCode)
{
	KBDLLHOOKSTRUCT* pKeyboardStruct = (KBDLLHOOKSTRUCT*)lparam;

	if (pKeyboardStruct == NULL)
	{
		return false;
	}

	vkCode = pKeyboardStruct->vkCode;
	return true;
}
