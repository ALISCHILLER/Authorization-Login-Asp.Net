using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands.Login
{
    /// <summary>
    /// دستور ورود کاربر
    /// </summary>
    public class LoginCommand : IRequest<LoginResponse>
    {
        /// <summary>
        /// نام کاربری یا ایمیل
        /// </summary>
        [Required(ErrorMessage = "نام کاربری یا ایمیل الزامی است")]
        public string UsernameOrEmail { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        public string Password { get; set; }

        /// <summary>
        /// به خاطر سپردن من
        /// </summary>
        public bool RememberMe { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// توکن دستگاه
        /// </summary>
        public string DeviceToken { get; set; }
    }
} 