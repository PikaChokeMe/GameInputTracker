using System;
using System.Collections.Generic;
using System.Text;

namespace CaptureInputDotNet
{
    class HookTypeNotImplementedException : HookException
    {
		public HookTypeNotImplementedException()
		{
		}

		public HookTypeNotImplementedException(string message) : base(message)
		{
		}

		public HookTypeNotImplementedException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
