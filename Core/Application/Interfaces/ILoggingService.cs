using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس لاگینگ برای ثبت رویدادها و خطاها
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// ثبت اطلاعات
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>تسک</returns>
        Task LogInformationAsync(string message);

        /// <summary>
        /// ثبت خطا
        /// </summary>
        /// <param name="exception">خطا</param>
        /// <param name="message">پیام</param>
        /// <returns>تسک</returns>
        Task LogErrorAsync(Exception exception, string message);

        /// <summary>
        /// ثبت هشدار
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>تسک</returns>
        Task LogWarningAsync(string message);

        /// <summary>
        /// ثبت خطای بحرانی
        /// </summary>
        /// <param name="exception">خطا</param>
        /// <param name="message">پیام</param>
        /// <returns>تسک</returns>
        Task LogCriticalAsync(Exception exception, string message);

        /// <summary>
        /// ثبت اطلاعات دیباگ
        /// </summary>
        /// <param name="message">پیام</param>
        /// <returns>تسک</returns>
        Task LogDebugAsync(string message);

        /// <summary>
        /// ثبت اطلاعات با جزئیات بیشتر
        /// </summary>
        /// <param name="message">پیام</param>
        /// <param name="args">پارامترهای پیام</param>
        /// <returns>تسک</returns>
        Task LogTraceAsync(string message, params object[] args);
    }
}