using System;

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
        /// زمان ورود
        /// </summary>
        public DateTime LoginTime { get; set; }

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
        /// وضعیت موفقیت‌آمیز بودن ورود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل عدم موفقیت
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// زمان خروج
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// مدت زمان حضور کاربر (به ثانیه)
        /// </summary>
        public int? SessionDuration { get; set; }
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