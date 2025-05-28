using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ احراز هویت
    /// </summary>
    public class AuthResponse
    {
        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        public UserDto User { get; set; }
    }
}
