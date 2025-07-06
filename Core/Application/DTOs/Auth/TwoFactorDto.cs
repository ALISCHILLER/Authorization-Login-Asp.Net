using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class TwoFactorDto
    {
        public string TwoFactorType { get; set; }
        public bool IsEnabled { get; set; }
    }
} 