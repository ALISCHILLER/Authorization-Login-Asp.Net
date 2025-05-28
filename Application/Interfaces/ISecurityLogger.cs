using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// رابط لاگر امنیتی برای ثبت رویدادهای امنیتی
    /// </summary>
    public interface ISecurityLogger
    {
        /// <summary>
        /// ثبت تلاش ورود ناموفق
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">اطلاعات مرورگر</param>
        /// <param name="reason">دلیل ناموفق بودن</param>
        Task LogFailedLoginAttemptAsync(string username, string ipAddress, string userAgent, string reason);

        /// <summary>
        /// ثبت تغییر رمز عبور
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        Task LogPasswordChangeAsync(Guid userId, string ipAddress);

        /// <summary>
        /// ثبت تغییر ایمیل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="oldEmail">ایمیل قبلی</param>
        /// <param name="newEmail">ایمیل جدید</param>
        /// <param name="ipAddress">آدرس IP</param>
        Task LogEmailChangeAsync(Guid userId, string oldEmail, string newEmail, string ipAddress);

        /// <summary>
        /// ثبت تغییر شماره تلفن
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="oldPhone">شماره تلفن قبلی</param>
        /// <param name="newPhone">شماره تلفن جدید</param>
        /// <param name="ipAddress">آدرس IP</param>
        Task LogPhoneChangeAsync(Guid userId, string oldPhone, string newPhone, string ipAddress);

        /// <summary>
        /// ثبت فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="method">روش فعال‌سازی</param>
        Task LogTwoFactorActivationAsync(Guid userId, string ipAddress, string method);

        /// <summary>
        /// ثبت غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        Task LogTwoFactorDeactivationAsync(Guid userId, string ipAddress);

        /// <summary>
        /// ثبت تلاش دسترسی غیرمجاز
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="resource">منبع</param>
        /// <param name="action">عملیات</param>
        Task LogUnauthorizedAccessAttemptAsync(Guid userId, string ipAddress, string resource, string action);
    }
} 