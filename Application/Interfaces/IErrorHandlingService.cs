using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت خطاها
    /// </summary>
    public interface IErrorHandlingService
    {
        /// <summary>
        /// ثبت خطا
        /// </summary>
        Task LogErrorAsync(Exception exception, string message = null);

        /// <summary>
        /// ثبت خطای بحرانی
        /// </summary>
        Task LogCriticalAsync(Exception exception, string message = null);

        /// <summary>
        /// ثبت خطای کاربری
        /// </summary>
        Task LogUserErrorAsync(string userId, string message, Exception exception = null);

        /// <summary>
        /// ثبت خطای سیستم
        /// </summary>
        Task LogSystemErrorAsync(string component, string message, Exception exception = null);

        /// <summary>
        /// ثبت خطای امنیتی
        /// </summary>
        Task LogSecurityErrorAsync(string userId, string message, Exception exception = null);
        Task HandleExceptionAsync(Exception ex, HttpContext context);
    }
} 