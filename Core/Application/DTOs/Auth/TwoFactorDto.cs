using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class TwoFactorDto
    {
        public TwoFactorType TwoFactorType { get; set; }
        public bool IsEnabled { get; set; }
    }
}