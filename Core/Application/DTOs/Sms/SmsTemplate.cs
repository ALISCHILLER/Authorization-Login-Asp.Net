using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Sms
{
    public class SmsTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public enum SmsStatus
    {
        Pending,
        Sent,
        Delivered,
        Failed,
        Rejected
    }
} 