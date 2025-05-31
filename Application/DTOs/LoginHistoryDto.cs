using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Application.DTOs
{
    /// <summary>
    /// مدل نمایش تاریخچه ورود
    /// </summary>
    public class LoginHistoryDto
    {
        /// <summary>
        /// شناسه
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// زمان ورود
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// زمان خروج
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات مرورگر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// مکان
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// وضعیت موفقیت‌آمیز بودن ورود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل عدم موفقیت
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// مدت زمان نشست
        /// </summary>
        public TimeSpan? SessionDuration { get; set; }

        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// نام مرورگر
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }
    }

    /// <summary>
    /// مدل درخواست ثبت ورود
    /// </summary>
    public class LoginHistoryRequestDto
    {
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
        /// دلیل عدم موفقیت (در صورت ناموفق بودن)
        /// </summary>
        public string FailureReason { get; set; }
    }
} 