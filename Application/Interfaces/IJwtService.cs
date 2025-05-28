using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// تولید توکن JWT
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="username">نام کاربری</param>
        /// <param name="role">نقش کاربر</param>
        /// <param name="additionalClaims">کلیم‌های اضافی</param>
        /// <returns>توکن JWT</returns>
        string GenerateToken(Guid userId, string username, string role, IDictionary<string, string> additionalClaims = null);

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>کلیم‌های توکن</returns>
        IDictionary<string, string> ValidateToken(string token);

        /// <summary>
        /// دریافت شناسه کاربر از توکن
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>شناسه کاربر</returns>
        Guid GetUserIdFromToken(string token);

        /// <summary>
        /// دریافت نقش کاربر از توکن
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نقش کاربر</returns>
        string GetUserRoleFromToken(string token);

        /// <summary>
        /// تولید کلید محرمانه احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>تاپل شامل کلید محرمانه و کد QR</returns>
        (string secret, string qrCode) GenerateTwoFactorSecret();

        /// <summary>
        /// اعتبارسنجی کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="secret">کلید محرمانه</param>
        /// <param name="code">کد</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        bool ValidateTwoFactorCode(string secret, string code);

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        /// <returns>لیست کدهای بازیابی</returns>
        IEnumerable<string> GenerateRecoveryCodes();

        /// <summary>
        /// تولید کلیم‌های کاربر
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <returns>لیست کلیم‌ها</returns>
        IEnumerable<Claim> GenerateClaims(Domain.Entities.User user);
    }
}
