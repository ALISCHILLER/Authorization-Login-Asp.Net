using System;

namespace Authorization_Login_Asp.Net.Application.Exceptions.Domain
{
    public class DomainException : Exception
    {
        public DomainException() : base()
        {
        }

        public DomainException(string message) : base(message)
        {
        }

        public DomainException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
} 