using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    /// <summary>
    /// DTO مربوط به تاریخچه ورود کاربر
    /// شامل اطلاعات جامعی از هر بار ورود کاربر به سیستم
    /// </summary>
    public class LoginHistoryItem
    {
        /// <summary>
        /// شناسه منحصر به فرد رکورد ورود
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// نام کاربری کاربری که وارد شده است
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// زمان ورود کاربر
        /// </summary>
        public DateTime LoginTime { get; set; }

        /// <summary>
        /// زمان خروج کاربر (اختیاری)
        /// </summary>
        public DateTime? LogoutTime { get; set; }

        /// <summary>
        /// آدرس IP کاربر
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات User-Agent مرورگر کاربر
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی کاربر (شهر، کشور و غیره)
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// وضعیت موفقیت‌آمیز بودن ورود
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// دلیل عدم موفقیت ورود (در صورت شکست)
        /// </summary>
        public string FailureReason { get; set; }

        /// <summary>
        /// مدت زمان جلسه ورود (تفاوت بین خروج و ورود)
        /// </summary>
        public TimeSpan? SessionDuration { get; set; }

        /// <summary>
        /// نام دستگاه کاربر (مانند "Windows Laptop")
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// نوع دستگاه (موبایل، دسکتاپ، API و غیره)
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// شناسه یکتا دستگاه کاربر
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// نام مرورگر استفاده شده (مانند Chrome، Firefox)
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// سیستم عامل دستگاه کاربر (مانند Windows، Android)
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// نام مرورگر استفاده شده
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// نسخه مرورگر استفاده شده
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// کشور کاربر
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// شهر کاربر
        /// </summary>
        public string City { get; set; }



        // ✅ این خاصیت را اضافه کن:
        public string DeviceInfo { get; set; }
    }
}