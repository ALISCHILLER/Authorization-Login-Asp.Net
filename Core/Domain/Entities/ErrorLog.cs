using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class ErrorLog : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string Source { get; set; }
        public string? AdditionalData { get; set; }
    }

    public class SystemError : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string? StackTrace { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
    }

    public class SecurityError : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class ValidationError : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public string? UserId { get; set; }
    }

    public class PerformanceError : BaseAuditableEntity
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Context { get; set; }
        public long Duration { get; set; }
    }
}
