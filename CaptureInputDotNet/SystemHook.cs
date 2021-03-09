using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace CaptureInputDotNet
{
    public abstract class SystemHook : IDisposable
    {
		protected delegate void HookProcessedHandler(int code, UIntPtr wparam, IntPtr lparam);
		private HookTypes _type = HookTypes.None;
		private HookProcessedHandler _processHandler = null;
		private bool _isHooked = false;

		public SystemHook(HookTypes type)
		{
			_type = type;

			_processHandler = new HookProcessedHandler(InternalHookCallback);
			SetCallBackResults result = SetUserHookCallback(_processHandler, _type);
			if (result != SetCallBackResults.Success)
			{
				this.Dispose();
				GenerateCallBackException(type, result);
			}
		}

		~SystemHook()
		{
			Trace.WriteLine("SystemHook (" + _type + ") WARNING: Finalizer called, " +
				"a reference has not been properly disposed.");

			Dispose(false);
		}

		protected abstract void HookCallback(int code, UIntPtr wparam, IntPtr lparam);


		[MethodImpl(MethodImplOptions.NoInlining)]
		private void InternalHookCallback(int code, UIntPtr wparam, IntPtr lparam)
		{
			try
			{
				HookCallback(code, wparam, lparam);
			}
			catch (Exception e)
			{
				//
				// While it is not generally a good idea to trap and discard all exceptions
				// in a base class, this is a special case. Remember, this is the entry point
				// for the C++ library to call into our .NET code. We don't want to return
				// .NET exceptions to C++. If it gets this far all we can do is drop them.
				//
				Debug.WriteLine("Exception during hook callback: " + e.Message + " " + e.ToString());
			}
		}

		public void InstallHook()
		{
			if (!InitializeHook(_type, 0))
			{
				throw new HookException("Hook failed to install.");
			}
			_isHooked = true;
		}

		public void UninstallHook()
		{
			_isHooked = false;
			UninitializeHook(_type);
		}

		protected HookTypes HookType
		{
			get
			{
				return _type;
			}
		}

		public bool IsHooked
		{
			get
			{
				return _isHooked;
			}
		}

		private void GenerateCallBackException(HookTypes type, SetCallBackResults result)
		{
			if (result == SetCallBackResults.Success)
			{
				return;
			}

			string msg;

			switch (result)
			{
				case SetCallBackResults.AlreadySet:
					msg = "A hook of type " + type + " is already registered. You can only register ";
					msg += "a single instance of each type of hook class. This can also occur when you forget ";
					msg += "to unregister or dispose a previous instance of the class.";

					throw new HookException(msg);

				case SetCallBackResults.ArgumentError:
					msg = "Failed to set hook callback due to an error in the arguments.";

					throw new ArgumentException(msg);

				case SetCallBackResults.NotImplemented:
					msg = "The hook type of type " + type + " is not implemented in the C++ layer. ";
					msg += "You must implement this hook type before you can use it. See the C++ function ";
					msg += "SetUserHookCallback.";

					throw new HookTypeNotImplementedException(msg);
			}

			msg = "Unrecognized exception during hook callback setup. Error code " + result + ".";
			throw new HookException(msg);
		}

		private void GenerateFilterMessageException(HookTypes type, FilterMessageResults result)
		{
			if (result == FilterMessageResults.Success)
			{
				return;
			}

			string msg;

			if (result == FilterMessageResults.NotImplemented)
			{
				msg = "The hook type of type " + type + " is not implemented in the C++ layer. ";
				msg += "You must implement this hook type before you can use it. See the C++ function ";
				msg += "FilterMessage.";

				throw new HookTypeNotImplementedException(msg);
			}

			//
			// All other errors are general errors.
			//
			msg = "Unrecognized exception during hook FilterMessage call. Error code " + result + ".";
			throw new HookException(msg);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);
			}

			UninstallHook();
			DisposeCppLayer(_type);
		}

		protected void GetScrollState(UIntPtr wparam, IntPtr lparam, ref int delta)
		{
			if (!InternalGetScrollState(wparam, lparam, ref delta))
			{
				throw new HookException("Failed to access mouse position.");
			}
		}

		protected void GetKeyboardReading(UIntPtr wparam, IntPtr lparam, ref int vkCode)
		{
			if (!InternalGetKeyboardReading(wparam, lparam, ref vkCode))
			{
				throw new HookException("Failed to access keyboard settings.");
			}
		}

		protected void FilterMessage(HookTypes hookType, int message)
		{
			FilterMessageResults result = InternalFilterMessage(hookType, message);
			if (result != FilterMessageResults.Success)
			{
				GenerateFilterMessageException(hookType, result);
			}
		}


		private enum SetCallBackResults
		{
			Success = 1,
			AlreadySet = -2,
			NotImplemented = -3,
			ArgumentError = -4
		};

		private enum FilterMessageResults
		{
			Success = 1,
			Failed = -2,
			NotImplemented = -3
		};

		[DllImport("CaptureInput.dll", EntryPoint = "SetUserHookCallback", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern SetCallBackResults SetUserHookCallback(HookProcessedHandler hookCallback, HookTypes hookType);

		[DllImport("CaptureInput.dll", EntryPoint = "InitializeHook", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern bool InitializeHook(HookTypes hookType, UInt32 threadID);

		[DllImport("CaptureInput.dll", EntryPoint = "UninitializeHook", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern void UninitializeHook(HookTypes hookType);

		[DllImport("CaptureInput.dll", EntryPoint = "GetScrollState", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern bool InternalGetScrollState(UIntPtr wparam, IntPtr lparam, ref int delta);

		[DllImport("CaptureInput.dll", EntryPoint = "GetKeyboardReading", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern bool InternalGetKeyboardReading(UIntPtr wparam, IntPtr lparam, ref int delta);

		[DllImport("CaptureInput.dll", EntryPoint = "Dispose", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern void DisposeCppLayer(HookTypes hookType);

		[DllImport("CaptureInput.dll", EntryPoint = "FilterMessage", SetLastError = true,
			 CharSet = CharSet.Unicode, ExactSpelling = true,
			 CallingConvention = CallingConvention.StdCall)]
		private static extern FilterMessageResults InternalFilterMessage(HookTypes hookType, int message);

	}
}
