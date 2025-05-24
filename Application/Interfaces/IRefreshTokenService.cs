using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.DTOs;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس برای مدیریت منطق تجاری مرتبط با رفرش توکن‌ها
    /// </summary>
    public interface IRefreshTokenService
    {
        /// <summary>
        /// ایجاد و ذخیره یک رفرش توکن جدید برای یک کاربر مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>رشته توکن رفرش ایجاد شده</returns>
        Task<string> GenerateRefreshTokenAsync(Guid userId);

        /// <summary>
        /// اعتبارسنجی رفرش توکن و ایجاد توکن جدید (Token Rotation)
        /// </summary>
        /// <param name="refreshToken">رشته توکن رفرش فعلی</param>
        /// <returns>آبجکت پاسخ احراز هویت شامل توکن جدید و اطلاعات کاربر</returns>
        Task<AuthResponse> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// لغو (ری‌ووک) رفرش توکن مشخص (مثلا هنگام خروج از حساب)
        /// </summary>
        /// <param name="refreshToken">رشته توکن رفرش برای لغو</param>
        /// <returns>تسک ناهمزمان</returns>
        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// حذف همه رفرش توکن‌های فعال متعلق به یک کاربر (خروج از همه دستگاه‌ها)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک ناهمزمان</returns>
        Task RevokeAllTokensForUserAsync(Guid userId);
    }
}
