using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public class BaseException : Exception
    {
        public BaseException(string message) : base(message) { }
        public BaseException(string message, Exception inner) : base(message, inner) { }
    }
}
