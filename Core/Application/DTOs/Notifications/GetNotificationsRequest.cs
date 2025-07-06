using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications
{
    public class GetNotificationsRequest
    {
        public Guid UserId { get; set; }
        public bool? IsRead { get; set; }
        public NotificationType? NotificationType { get; set; }
        public NotificationPriority? NotificationPriority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool IncludeRead { get; set; } = false;
    }
}