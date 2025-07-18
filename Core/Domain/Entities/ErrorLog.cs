using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class ErrorLog : BaseEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string Source { get; set; }
        public string? AdditionalData { get; set; }
    }

    public class SystemError : BaseEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
    }

    public class SecurityError : BaseEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class ValidationError : BaseEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
    }

    public class PerformanceError : BaseEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public long Duration { get; set; }
    }
}
