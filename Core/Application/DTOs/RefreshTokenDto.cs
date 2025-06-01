using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// مدل درخواست تمدید توکن
    /// </summary>
    public class RefreshTokenDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }
    }
}