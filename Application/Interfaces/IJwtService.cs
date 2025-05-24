using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس JWT برای تولید و اعتبارسنجی توکن‌های JWT
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// تولید توکن JWT برای کاربر با اطلاعات کلیدی و ادعاها (Claims)
        /// </summary>
        /// <param name="userId">شناسه یکتا کاربر</param>
        /// <param name="username">نام کاربری کاربر</param>
        /// <param name="role">نقش کاربر</param>
        /// <param name="additionalClaims">لیست کلید-مقدار ادعاهای اضافی در توکن (اختیاری)</param>
        /// <returns>رشته توکن JWT تولید شده</returns>
        string GenerateToken(Guid userId, string username, string role, IDictionary<string, string> additionalClaims = null);

        /// <summary>
        /// اعتبارسنجی توکن JWT و بررسی صحت آن (امضا و انقضا)
        /// </summary>
        /// <param name="token">رشته توکن JWT</param>
        /// <returns>در صورت معتبر بودن، اطلاعات ادعاها (Claims) به صورت دیکشنری برگردانده می‌شود، در غیر این صورت null یا Exception</returns>
        IDictionary<string, string> ValidateToken(string token);

        /// <summary>
        /// استخراج شناسه کاربر از توکن JWT
        /// </summary>
        /// <param name="token">رشته توکن JWT</param>
        /// <returns>شناسه کاربر (Guid) در صورت اعتبارسنجی موفق، در غیر این صورت مقدار پیش‌فرض (Guid.Empty)</returns>
        Guid GetUserIdFromToken(string token);

        /// <summary>
        /// استخراج نقش کاربر از توکن JWT
        /// </summary>
        /// <param name="token">رشته توکن JWT</param>
        /// <returns>نقش کاربر به صورت رشته</returns>
        string GetUserRoleFromToken(string token);
    }
}
