using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class UserAuditLog : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string Action { get; set; } = null!;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? Location { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public string? AdditionalData { get; set; }

        // Navigation property
        public virtual User User { get; set; } = null!;
    }
} 