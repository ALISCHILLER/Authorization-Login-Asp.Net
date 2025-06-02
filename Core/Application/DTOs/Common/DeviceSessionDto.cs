using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    /// <summary>
    /// اطلاعات دستگاه
    /// </summary>
    public class DeviceDto
    {
        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// مدل دستگاه
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; set; }

        /// <summary>
        /// نسخه سیستم عامل
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        public string Browser { get; set; }

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// آیا دستگاه مورد اعتماد است
        /// </summary>
        public bool IsTrusted { get; set; }

        /// <summary>
        /// تاریخ آخرین فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// اطلاعات نشست
    /// </summary>
    public class SessionDto
    {
        /// <summary>
        /// شناسه نشست
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن رفرش
        /// </summary>
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// آیا نشست فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// تاریخ آخرین فعالیت
        /// </summary>
        public DateTime LastActivityAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndedAt { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        public DeviceDto Device { get; set; }
    }

    /// <summary>
    /// درخواست ایجاد نشست جدید
    /// </summary>
    public class CreateSessionRequest
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public string UserId { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        [Required(ErrorMessage = "توکن دسترسی الزامی است")]
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        [Required(ErrorMessage = "توکن رفرش الزامی است")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        [Required(ErrorMessage = "تاریخ انقضای توکن دسترسی الزامی است")]
        public DateTime AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن رفرش
        /// </summary>
        [Required(ErrorMessage = "تاریخ انقضای توکن رفرش الزامی است")]
        public DateTime RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfo DeviceInfo { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی نشست
    /// </summary>
    public class UpdateSessionRequest
    {
        /// <summary>
        /// شناسه نشست
        /// </summary>
        [Required(ErrorMessage = "شناسه نشست الزامی است")]
        public string SessionId { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        public DateTime? AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن رفرش
        /// </summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }

        /// <summary>
        /// آیا نشست فعال است
        /// </summary>
        public bool? IsActive { get; set; }
    }

    /// <summary>
    /// درخواست دریافت نشست‌ها
    /// </summary>
    public class GetSessionsRequest
    {
        /// <summary>
        /// شماره صفحه
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// وضعیت فعال بودن
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// پاسخ دریافت نشست‌ها
    /// </summary>
    public class GetSessionsResponse
    {
        /// <summary>
        /// لیست نشست‌ها
        /// </summary>
        public List<SessionDto> Sessions { get; set; }

        /// <summary>
        /// تعداد کل نشست‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد نشست‌های فعال
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// تعداد کل صفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// شماره صفحه فعلی
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// آیا صفحه بعدی وجود دارد
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// آیا صفحه قبلی وجود دارد
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// اطلاعات دستگاه
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// نام دستگاه
        /// </summary>
        [Required(ErrorMessage = "نام دستگاه الزامی است")]
        [StringLength(100, ErrorMessage = "نام دستگاه نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Name { get; set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        [Required(ErrorMessage = "نوع دستگاه الزامی است")]
        public string Type { get; set; }

        /// <summary>
        /// مدل دستگاه
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        [Required(ErrorMessage = "سیستم عامل الزامی است")]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// نسخه سیستم عامل
        /// </summary>
        public string OsVersion { get; set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        [Required(ErrorMessage = "مرورگر الزامی است")]
        public string Browser { get; set; }

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        public string BrowserVersion { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [Required(ErrorMessage = "آدرس IP الزامی است")]
        public string IpAddress { get; set; }

        /// <summary>
        /// موقعیت جغرافیایی
        /// </summary>
        public string Location { get; set; }
    }
} 