using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// درخواست تمدید توکن
    /// </summary>
    public class RefreshTokenRequest : AuthRequest
    {
        /// <summary>
        /// توکن رفرش
        /// </summary>
        [Required(ErrorMessage = "توکن رفرش الزامی است")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// توکن دسترسی منقضی شده
        /// </summary>
        [Required(ErrorMessage = "توکن دسترسی الزامی است")]
        public string ExpiredAccessToken { get; set; }
    }
} 