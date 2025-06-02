using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Auth
{
    /// <summary>
    /// اطلاعات تاریخچه ورود
    /// </summary>
    public class LoginHistoryDto : BaseDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        [Required(ErrorMessage = "شناسه کاربر الزامی است")]
        public Guid UserId { get; set; }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        public UserDto User { get; set; }

        /// <summary>
        /// تاریخ و زمان ورود
        /// </summary>
        [Required(ErrorMessage = "تاریخ و زمان ورود الزامی است")]
        public DateTime LoginAt { get; set; }

        /// <summary>
        /// تاریخ و زمان خروج
        /// </summary>
        public DateTime? LogoutAt { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [Required(ErrorMessage = "آدرس IP الزامی است")]
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "فرمت آدرس IP نامعتبر است")]
        public string IpAddress { get; set; }

        /// <summary>
        /// اطلاعات دستگاه
        /// </summary>
        [Required(ErrorMessage = "اطلاعات دستگاه الزامی است")]
        public DeviceInfoDto DeviceInfo { get; set; }

        /// <summary>
        /// اطلاعات موقعیت مکانی
        /// </summary>
        public LocationDto Location { get; set; }

        /// <summary>
        /// آیا ورود موفق بوده است؟
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// پیام خطا در صورت ناموفق بودن ورود
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن دسترسی
        /// </summary>
        public DateTime? AccessTokenExpiresAt { get; set; }

        /// <summary>
        /// توکن تجدید
        /// </summary>
        public string RefreshToken { get; set; }

        /// <summary>
        /// تاریخ انقضای توکن تجدید
        /// </summary>
        public DateTime? RefreshTokenExpiresAt { get; set; }
    }

    /// <summary>
    /// درخواست دریافت تاریخچه ورود
    /// </summary>
    public class GetLoginHistoryRequestDto : BaseRequestDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// تاریخ شروع
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// تاریخ پایان
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// آدرس IP
        /// </summary>
        [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "فرمت آدرس IP نامعتبر است")]
        public string IpAddress { get; set; }

        /// <summary>
        /// آیا فقط ورودهای موفق را نمایش دهد؟
        /// </summary>
        public bool? OnlySuccessful { get; set; }

        /// <summary>
        /// شماره صفحه
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "شماره صفحه باید بزرگتر از صفر باشد")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// تعداد آیتم در هر صفحه
        /// </summary>
        [Range(1, 100, ErrorMessage = "تعداد آیتم در هر صفحه باید بین 1 تا 100 باشد")]
        public int PageSize { get; set; } = 10;
    }

    /// <summary>
    /// پاسخ دریافت تاریخچه ورود
    /// </summary>
    public class GetLoginHistoryResponseDto : BaseResponseDto
    {
        /// <summary>
        /// لیست تاریخچه ورود
        /// </summary>
        public IReadOnlyList<LoginHistoryDto> Items { get; set; }

        /// <summary>
        /// تعداد کل آیتم‌ها
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// تعداد صفحات
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// آیا صفحه بعدی وجود دارد؟
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// آیا صفحه قبلی وجود دارد؟
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }
} 