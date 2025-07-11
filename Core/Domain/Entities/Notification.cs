using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Common; // Added for BaseEntity

namespace Authorization_Login_Asp.Net.Core.Domain.Entities
{
    /// <summary>
    /// موجودیت اعلان
    /// </summary>
    public class Notification : BaseEntity // Inherit from BaseEntity
    {
        // Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted, DeletedAt, DeletedBy are inherited.

        public Guid UserId { get; set; } // Assuming this is the recipient user
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // 'Type' string property might be redundant if NotificationType enum is used effectively.
        // Consider removing if NotificationType covers all needs. For now, keeping it.
        public string Type { get; set; } = string.Empty;

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public NotificationType NotificationType { get; set; }
        public NotificationPriority NotificationPriority { get; set; }
        public string ActionUrl { get; set; } = string.Empty;
        public string ActionText { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public virtual User? User { get; set; } // nullable برای جلوگیری از خطا

        protected Notification() { }

        public Notification(
            Guid userId,
            string title,
            string message,
            NotificationType type = NotificationType.Info,
            NotificationPriority priority = NotificationPriority.Normal,
            string actionUrl = "",
            string actionText = "",
            string icon = "",
            string color = "",
            DateTime? expiresAt = null)
            : base() // Calls BaseEntity constructor for Id and CreatedAt
        {
            // Id and CreatedAt are set by BaseEntity constructor
            UserId = userId;
            Title = title;
            Message = message;
            NotificationType = type;
            NotificationPriority = priority;
            ActionUrl = actionUrl;
            ActionText = actionText;
            Icon = icon;
            Color = color;
            ExpiresAt = expiresAt;
            IsRead = false;
            // CreatedBy would be set by the service/context if needed
        }

        /// <summary>
        /// علامت‌گذاری به عنوان خوانده شده
        /// </summary>
        public void MarkAsRead(string? updatedByUserId = null)
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
                MarkAsUpdated(updatedByUserId); // Record this change
            }
        }

        /// <summary>
        /// آیا منقضی شده است؟
        /// </summary>
        public bool IsExpired()
        {
            return ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات اعلان
        /// </summary>
        public void Update(
            string title = "",
            string message = "",
            NotificationType? type = null,
            NotificationPriority? priority = null,
            string actionUrl = "",
            string actionText = "",
            string icon = "",
            string color = "",
            DateTime? expiresAt = null,
            string? updatedByUserId = null)
        {
            Title = string.IsNullOrEmpty(title) ? Title : title;
            Message = string.IsNullOrEmpty(message) ? Message : message;
            NotificationType = type ?? NotificationType;
            NotificationPriority = priority ?? NotificationPriority;
            ActionUrl = string.IsNullOrEmpty(actionUrl) ? ActionUrl : actionUrl;
            ActionText = string.IsNullOrEmpty(actionText) ? ActionText : actionText;
            Icon = string.IsNullOrEmpty(icon) ? Icon : icon;
            Color = string.IsNullOrEmpty(color) ? Color : color;
            ExpiresAt = expiresAt ?? ExpiresAt;
            MarkAsUpdated(updatedByUserId);
        }
    }
}