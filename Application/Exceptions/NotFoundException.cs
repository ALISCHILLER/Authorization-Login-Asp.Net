using System;

namespace Authorization_Login_Asp.Net.Application.Exceptions
{
    /// <summary>
    /// استثنا برای مواردی که داده یا موجودیت مورد نظر پیدا نشد
    /// </summary>
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("مورد درخواست شده پیدا نشد.") { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// استثنا برای مواردی که دسترسی غیرمجاز است (Unauthorized)
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException() : base("دسترسی غیرمجاز است.") { }

        public UnauthorizedException(string message) : base(message) { }

        public UnauthorizedException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// استثنا برای مواردی که عملیات ممنوع است (Forbidden)
    /// </summary>
    public class ForbiddenException : Exception
    {
        public ForbiddenException() : base("دسترسی به این عملیات ممنوع است.") { }

        public ForbiddenException(string message) : base(message) { }

        public ForbiddenException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// استثنا برای مواردی که درخواست نامعتبر است (مثل ورودی‌های نادرست)
    /// </summary>
    public class BadRequestException : Exception
    {
        public BadRequestException() : base("درخواست نامعتبر است.") { }

        public BadRequestException(string message) : base(message) { }

        public BadRequestException(string message, Exception innerException) : base(message, innerException) { }
    }
}
