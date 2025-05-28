using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ تاریخچه ورود
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
        public int TotalPages { get; set; }

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage { get; set; }

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
        /// زمان ورود
        /// </summary>
        public System.DateTime LoginTime { get; set; }

        /// <summary>
        /// آیا ورود موفقیت‌آمیز بود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// پیام خطا (در صورت ناموفق بودن)
        /// </summary>
        public string ErrorMessage { get; set; }
    }
} 