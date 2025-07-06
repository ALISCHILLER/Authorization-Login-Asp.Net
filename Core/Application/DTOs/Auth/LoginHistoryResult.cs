using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    public class LoginHistoryResult
    {
        public List<LoginHistoryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}