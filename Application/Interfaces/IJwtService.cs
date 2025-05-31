using System;
using System.Collections.Generic;
using System.Security.Claims;
using Authorization_Login_Asp.Net.Domain.Entities;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس JWT
    /// این اینترفیس عملیات مربوط به مدیریت توکن‌های JWT و احراز هویت دو مرحله‌ای را تعریف می‌کند
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// تولید توکن JWT
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="username">نام کاربری</param>
        /// <param name="role">نقش کاربر</param>
        /// <param name="additionalClaims">کلیم‌های اضافی (اختیاری)</param>
        /// <returns>توکن JWT تولید شده</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن نام کاربری یا نقش</exception>
        string GenerateToken(Guid userId, string username, string role, IDictionary<string, string> additionalClaims = null);

        /// <summary>
        /// اعتبارسنجی توکن JWT
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>دیکشنری شامل کلیم‌های توکن</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن توکن</exception>
        /// <exception cref="SecurityTokenException">در صورت نامعتبر بودن توکن</exception>
        IDictionary<string, string> ValidateToken(string token);

        /// <summary>
        /// دریافت شناسه کاربر از توکن
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>شناسه کاربر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن توکن</exception>
        /// <exception cref="SecurityTokenException">در صورت نامعتبر بودن توکن یا عدم وجود شناسه کاربر</exception>
        Guid GetUserIdFromToken(string token);

        /// <summary>
        /// دریافت نقش کاربر از توکن
        /// </summary>
        /// <param name="token">توکن JWT</param>
        /// <returns>نقش کاربر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن توکن</exception>
        /// <exception cref="SecurityTokenException">در صورت نامعتبر بودن توکن یا عدم وجود نقش کاربر</exception>
        string GetUserRoleFromToken(string token);

        /// <summary>
        /// تولید کلید محرمانه احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>تاپل شامل کلید محرمانه و کد QR برای اسکن در اپلیکیشن احراز هویت</returns>
        (string secret, string qrCode) GenerateTwoFactorSecret();

        /// <summary>
        /// اعتبارسنجی کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="secret">کلید محرمانه</param>
        /// <param name="code">کد وارد شده توسط کاربر</param>
        /// <returns>نتیجه اعتبارسنجی (درست اگر کد معتبر باشد)</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن کلید محرمانه یا کد</exception>
        bool ValidateTwoFactorCode(string secret, string code);

        /// <summary>
        /// تولید کدهای بازیابی برای احراز هویت دو مرحله‌ای
        /// </summary>
        /// <returns>لیست کدهای بازیابی یکبار مصرف</returns>
        IEnumerable<string> GenerateRecoveryCodes();

        /// <summary>
        /// تولید کلیم‌های کاربر برای توکن JWT
        /// </summary>
        /// <param name="user">اطلاعات کاربر</param>
        /// <returns>لیست کلیم‌های کاربر شامل شناسه، نام کاربری، نقش و سایر اطلاعات</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن اطلاعات کاربر</exception>
        IEnumerable<Claim> GenerateClaims(User user);

        /// <summary>
        /// تولید توکن رفرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <returns>توکن رفرش</returns>
        Task<string> GenerateRefreshTokenAsync(Guid userId, string ipAddress);

        /// <summary>
        /// اعتبارسنجی توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<bool> ValidateRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// باطل کردن توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        /// <param name="reason">دلیل باطل شدن</param>
        Task RevokeRefreshTokenAsync(string refreshToken, string reason = null);

        /// <summary>
        /// باطل کردن تمام توکن‌های رفرش کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        Task RevokeAllRefreshTokensAsync(Guid userId);
    }
}
