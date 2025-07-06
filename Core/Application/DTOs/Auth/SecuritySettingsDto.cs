using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class SecuritySettingsDto
    {
        public bool TwoFactorEnabled { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
    }
} 