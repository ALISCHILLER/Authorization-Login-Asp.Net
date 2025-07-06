namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    public class SecurityError : BaseException
    {
        public SecurityError(string message) : base(message) { }
        public SecurityError(string message, Exception innerException) : base(message, innerException) { }
    }

    public class ValidationError : BaseException
    {
        public ValidationError(string message) : base(message) { }
        public ValidationError(string message, Exception innerException) : base(message, innerException) { }
    }

    public class PerformanceError : BaseException
    {
        public PerformanceError(string message) : base(message) { }
        public PerformanceError(string message, Exception innerException) : base(message, innerException) { }
    }

    public class SystemError : BaseException
    {
        public SystemError(string message) : base(message) { }
        public SystemError(string message, Exception innerException) : base(message, innerException) { }
    }
} 