using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message) { }
        public DomainException(string message, Exception inner) : base(message, inner) { }
    }
}
