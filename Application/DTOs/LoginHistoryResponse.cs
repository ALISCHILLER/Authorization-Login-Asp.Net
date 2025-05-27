using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// پاسخ درخواست تاریخچه ورود کاربر
    /// </summary>
    public class LoginHistoryResponse
    {
        /// <summary>
        /// لیست ورودهای کاربر
        /// </summary>
        public List<LoginHistoryItem> Items { get; set; }

        /// <summary>
        /// تعداد کل ورودها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// تعداد صفحات
        /// </summary>
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// آیتم تاریخچه ورود
    /// </summary>
    public class LoginHistoryItem
    {
        /// <summary>
        /// تاریخ و زمان ورود
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات مرورگر کاربر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی (در صورت دسترسی)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// وضعیت ورود (موفق/ناموفق)
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل ناموفق بودن (در صورت ناموفق بودن)
        /// </summary>
        public string FailureReason { get; set; }
    }
} 