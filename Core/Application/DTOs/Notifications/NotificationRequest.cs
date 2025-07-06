using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Notifications
{
    public class NotificationRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public Guid? UserId { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public bool IsPersistent { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class NotificationTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string Priority { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 