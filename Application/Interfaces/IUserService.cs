using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس کاربر برای انجام عملیات کسب‌وکار مرتبط با کاربران
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// ثبت‌نام کاربر جدید
        /// </summary>
        /// <param name="request">مدل درخواست ثبت‌نام</param>
        /// <returns>اطلاعات احراز هویت (توکن‌ها و کاربر)</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// ورود کاربر و دریافت توکن احراز هویت
        /// </summary>
        /// <param name="request">مدل درخواست ورود</param>
        /// <returns>اطلاعات احراز هویت شامل توکن</returns>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// گرفتن اطلاعات کاربر جاری با شناسه
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>شیء کاربر یا null</returns>
        Task<User> GetCurrentUserAsync(Guid userId);

        /// <summary>
        /// تغییر رمز عبور کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="currentPassword">رمز عبور فعلی</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns>تسک</returns>
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// خروج کاربر و لغو توکن رفرش (در صورت استفاده)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task LogoutAsync(Guid userId);
    }
}
