using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس تاریخچه ورود کاربران
    /// </summary>
    public interface ILoginHistoryService
    {
        /// <summary>
        /// ثبت ورود موفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">اطلاعات مرورگر</param>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        /// <returns>تاریخچه ورود ثبت شده</returns>
        Task<LoginHistory> LogSuccessfulLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo);

        /// <summary>
        /// ثبت ورود ناموفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">اطلاعات مرورگر</param>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        /// <param name="failureReason">دلیل عدم موفقیت</param>
        /// <returns>تاریخچه ورود ثبت شده</returns>
        Task<LoginHistory> LogFailedLoginAsync(Guid userId, string ipAddress, string userAgent, DeviceInfo deviceInfo, string failureReason);

        /// <summary>
        /// ثبت خروج کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task LogLogoutAsync(Guid userId);

        /// <summary>
        /// دریافت تاریخچه ورودهای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>لیست تاریخچه ورود</returns>
        Task<(List<LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// دریافت آخرین ورود موفق کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>آخرین تاریخچه ورود موفق</returns>
        Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId);

        /// <summary>
        /// دریافت تعداد ورودهای ناموفق کاربر در بازه زمانی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="timeWindowMinutes">بازه زمانی به دقیقه</param>
        /// <returns>تعداد ورودهای ناموفق</returns>
        Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15);
    }

    /// <summary>
    /// اطلاعات دستگاه کاربر
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// نام دستگاه
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// نام مرورگر
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// کشور
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// شهر
        /// </summary>
        public string City { get; set; }
    }
} 