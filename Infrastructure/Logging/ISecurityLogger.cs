using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Logging
{
    /// <summary>
    /// رابط لاگر امنیتی برای ثبت رویدادهای امنیتی
    /// </summary>
    public interface ISecurityLogger
    {
        /// <summary>
        /// ثبت تلاش ورود ناموفق
        /// </summary>
        Task LogFailedLoginAttemptAsync(string username, string ipAddress, string userAgent, string reason);

        /// <summary>
        /// ثبت تغییر رمز عبور
        /// </summary>
        Task LogPasswordChangeAsync(Guid userId, string ipAddress);

        /// <summary>
        /// ثبت تغییر ایمیل
        /// </summary>
        Task LogEmailChangeAsync(Guid userId, string oldEmail, string newEmail, string ipAddress);

        /// <summary>
        /// ثبت تغییر شماره تلفن
        /// </summary>
        Task LogPhoneChangeAsync(Guid userId, string oldPhone, string newPhone, string ipAddress);

        /// <summary>
        /// ثبت فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        Task LogTwoFactorActivationAsync(Guid userId, string ipAddress, string method);

        /// <summary>
        /// ثبت غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        Task LogTwoFactorDeactivationAsync(Guid userId, string ipAddress);

        /// <summary>
        /// ثبت تلاش دسترسی غیرمجاز
        /// </summary>
        Task LogUnauthorizedAccessAttemptAsync(Guid userId, string ipAddress, string resource, string action);
    }
} 