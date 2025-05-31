using System;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    public class LoginHistoryItem
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Location { get; set; }
        public bool IsSuccessful { get; set; }
        public string FailureReason { get; set; }
        public TimeSpan? SessionDuration { get; set; }
        public string DeviceName { get; set; }
        public string DeviceType { get; set; }
        public string DeviceId { get; set; }
        public string Browser { get; set; }
        public string OperatingSystem { get; set; }
    }
} 