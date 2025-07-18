using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities; // Added using directive for User namespace

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    // UserNotification now inherits from BaseEntity for Id, CreatedAt, etc.
    public class UserNotification : BaseEntity
    {
        public Guid UserId { get; set; }
        public Guid NotificationId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string? AdditionalData { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Notification Notification { get; set; } = null!;

        // Expose Notification properties for easier access
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
    }
}