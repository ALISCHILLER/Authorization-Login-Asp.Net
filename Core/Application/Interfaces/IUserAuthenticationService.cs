using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس احراز هویت کاربر
    /// این اینترفیس عملیات مربوط به احراز هویت کاربر را تعریف می‌کند
    /// </summary>
    public interface IUserAuthenticationService
    {
        /// <summary>
        /// ثبت‌نام کاربر جدید
        /// </summary>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// ورود کاربر
        /// </summary>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// خروج کاربر
        /// </summary>
        Task LogoutAsync(Guid userId);

        /// <summary>
        /// تأیید کد احراز هویت دو مرحله‌ای
        /// </summary>
        Task<AuthResult> ValidateTwoFactorAsync(TwoFactorDto model);

        /// <summary>
        /// درخواست بازنشانی رمز عبور
        /// </summary>
        Task<bool> RequestPasswordResetAsync(string email);

        /// <summary>
        /// بازنشانی رمز عبور
        /// </summary>
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    }
} 