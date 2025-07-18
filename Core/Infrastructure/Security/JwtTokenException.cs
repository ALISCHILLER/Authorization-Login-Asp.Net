using System;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    public class JwtTokenException : Exception
    {
        public JwtTokenException(string message) : base(message) { }
        public JwtTokenException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
