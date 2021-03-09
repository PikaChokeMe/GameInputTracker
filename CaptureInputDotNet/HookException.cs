using System;
using System.Collections.Generic;
using System.Text;

namespace CaptureInputDotNet
{
    class HookException : ApplicationException
    {
        public HookException()
        {
        }

        public HookException(string message) : base(message)
        {
        }

        public HookException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
