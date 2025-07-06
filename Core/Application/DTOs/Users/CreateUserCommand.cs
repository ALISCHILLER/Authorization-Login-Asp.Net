using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class CreateUserCommand
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
} 