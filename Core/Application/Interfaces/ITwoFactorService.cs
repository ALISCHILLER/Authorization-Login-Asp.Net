using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// سرویس احراز هویت دو مرحله‌ای
    /// </summary>
    public interface ITwoFactorService
    {
        /// <summary>
        /// تولید کلید محرمانه
        /// </summary>
        /// <returns>کلید محرمانه</returns>
        string GenerateSecret();

        /// <summary>
        /// اعتبارسنجی توکن
        /// </summary>
        /// <param name="secret">کلید محرمانه</param>
        /// <param name="token">توکن</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        bool ValidateToken(string secret, string token);

        /// <summary>
        /// تولید کد QR
        /// </summary>
        /// <param name="secret">کلید محرمانه</param>
        /// <param name="email">ایمیل</param>
        /// <param name="issuer">منتقل کننده</param>
        /// <returns>کد QR</returns>
        string GenerateQrCodeUri(string secret, string email, string issuer);

        /// <summary>
        /// تولید کدهای بازیابی
        /// </summary>
        /// <returns>کد بازیابی</returns>
        string GenerateRecoveryCode();

        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="type">نوع احراز هویت دو مرحله‌ای</param>
        /// <returns>نتیجه فعال‌سازی</returns>
        Task<bool> EnableAsync(User user, TwoFactorType type);

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <returns>نتیجه غیرفعال‌سازی</returns>
        Task<bool> DisableAsync(User user);

        /// <summary>
        /// ارسال کد تایید
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <returns>نتیجه ارسال</returns>
        Task<bool> SendCodeAsync(User user);

        /// <summary>
        /// اعتبارسنجی کد تایید
        /// </summary>
        /// <param name="user">کاربر</param>
        /// <param name="code">کد تایید</param>
        /// <returns>نتیجه اعتبارسنجی</returns>
        Task<bool> VerifyCodeAsync(User user, string code);
    }
}