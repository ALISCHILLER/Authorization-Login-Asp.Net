using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس توکن رفرش
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// تولید توکن رفرش جدید
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>توکن رفرش</returns>
        Task<string> GenerateRefreshTokenAsync(Guid userId);

        /// <summary>
        /// تمدید توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        /// <returns>توکن رفرش جدید</returns>
        Task<string> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// باطل کردن توکن رفرش
        /// </summary>
        /// <param name="refreshToken">توکن رفرش</param>
        /// <returns>تسک</returns>
        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// باطل کردن تمام توکن‌های رفرش کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task RevokeAllTokensForUserAsync(Guid userId);
    }
}
