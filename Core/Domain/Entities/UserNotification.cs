using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    public class UserNotification : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? AdditionalData { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Notification Notification { get; set; } = null!;
    }
} 