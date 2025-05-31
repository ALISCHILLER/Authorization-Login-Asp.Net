using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ صفحه‌بندی شده تاریخچه ورود
    /// </summary>
    public class LoginHistoryResponse
    {
        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// تعداد در هر صفحه
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌های تاریخچه ورود
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// لیست آیتم‌های تاریخچه ورود
        /// </summary>
        public List<LoginHistoryItem> Items { get; set; } = new();
    }

    /// <summary>
    /// آیتم تاریخچه ورود
    /// </summary>
    public class LoginHistoryItem
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات مرورگر
        /// </summary>
        public string UserAgent { get; set; }

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

        /// <summary>
        /// زمان ورود
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// زمان خروج
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// مدت زمان نشست (به ثانیه)
        /// </summary>
        public int? SessionDuration { get; set; }

        /// <summary>
        /// آیا ورود موفقیت‌آمیز بود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل شکست (در صورت ناموفق بودن)
        /// </summary>
        public string FailureReason { get; set; }
    }
} 