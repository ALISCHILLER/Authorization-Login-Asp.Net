using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس ریپازیتوری برای مدیریت عملیات مرتبط با رفرش توکن‌ها
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// افزودن یک رفرش توکن جدید به دیتابیس
        /// </summary>
        /// <param name="refreshToken">شیء رفرش توکن برای افزودن</param>
        /// <returns>تسک ناهمزمان</returns>
        Task AddAsync(RefreshToken refreshToken);

        /// <summary>
        /// دریافت رفرش توکن بر اساس مقدار رشته‌ای توکن (Token)
        /// </summary>
        /// <param name="token">رشته توکن رفرش</param>
        /// <returns>شیء رفرش توکن مرتبط یا null اگر پیدا نشود</returns>
        Task<RefreshToken?> GetByTokenAsync(string token);

        /// <summary>
        /// دریافت رفرش توکن بر اساس شناسه منحصربه‌فرد آن (Id)
        /// </summary>
        /// <param name="id">شناسه رفرش توکن</param>
        /// <returns>شیء رفرش توکن مرتبط یا null اگر پیدا نشود</returns>
        Task<RefreshToken?> GetByIdAsync(Guid id);

        /// <summary>
        /// دریافت تمام رفرش توکن‌های فعال متعلق به یک کاربر مشخص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیستی از رفرش توکن‌های فعال</returns>
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId);

        /// <summary>
        /// حذف یک رفرش توکن از دیتابیس
        /// </summary>
        /// <param name="refreshToken">شیء رفرش توکن که باید حذف شود</param>
        /// <returns>تسک ناهمزمان</returns>
        Task RemoveAsync(RefreshToken refreshToken);

        /// <summary>
        /// به‌روزرسانی اطلاعات یک رفرش توکن موجود در دیتابیس
        /// </summary>
        /// <param name="refreshToken">شیء رفرش توکنی که باید به‌روزرسانی شود</param>
        /// <returns>تسک ناهمزمان</returns>
        Task UpdateAsync(RefreshToken refreshToken);

        /// <summary>
        /// ذخیره تغییرات اعمال شده در دیتابیس (در صورت استفاده از Unit of Work)
        /// </summary>
        /// <returns>تسک ناهمزمان</returns>
        Task SaveChangesAsync();

        Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(Guid userId);
        Task<bool> IsTokenRevokedAsync(string token);
        Task RevokeTokenAsync(string token, string reason = null);
        Task RevokeAllUserTokensAsync(Guid userId);
    }
}
