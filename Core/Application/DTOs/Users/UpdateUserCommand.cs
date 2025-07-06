using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class UpdateUserCommand
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; } // اضافه کردن UserId برای استفاده در کنترلر
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
    }
}