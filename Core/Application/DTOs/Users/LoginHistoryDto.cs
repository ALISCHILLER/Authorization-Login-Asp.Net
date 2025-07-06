using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class LoginHistoryDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime LoginTime { get; set; }
        public string IpAddress { get; set; }
        public string DeviceInfo { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
