using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    /// <summary>
    /// کلاس درخواست به‌روزرسانی پروفایل کاربر
    /// </summary>
    public class UpdateUserProfileRequestDto : BaseRequestDto
    {
        /// <summary>
        /// نام
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [MaxLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [MaxLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(50, ErrorMessage = "نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد")]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        [MaxLength(100, ErrorMessage = "ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string Email { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        [MaxLength(20, ErrorMessage = "شماره تلفن نمی‌تواند بیشتر از 20 کاراکتر باشد")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// جنسیت
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// آدرس
        /// </summary>
        [MaxLength(500, ErrorMessage = "آدرس نمی‌تواند بیشتر از 500 کاراکتر باشد")]
        public string Address { get; set; }

        /// <summary>
        /// کد پستی
        /// </summary>
        [RegularExpression(@"^\d{10}$", ErrorMessage = "کد پستی باید 10 رقم باشد")]
        public string PostalCode { get; set; }

        /// <summary>
        /// شهر
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام شهر نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string City { get; set; }

        /// <summary>
        /// استان
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام استان نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Province { get; set; }

        /// <summary>
        /// کشور
        /// </summary>
        [MaxLength(50, ErrorMessage = "نام کشور نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string Country { get; set; }

        /// <summary>
        /// توضیحات
        /// </summary>
        [MaxLength(1000, ErrorMessage = "توضیحات نمی‌تواند بیشتر از 1000 کاراکتر باشد")]
        public string Description { get; set; }

        /// <summary>
        /// زبان ترجیحی
        /// </summary>
        public string PreferredLanguage { get; set; }

        /// <summary>
        /// منطقه زمانی
        /// </summary>
        public string TimeZone { get; set; }
    }

    /// <summary>
    /// کلاس پاسخ پروفایل کاربر
    /// </summary>
    public class UserProfileResponseDto : BaseResponseDto
    {
        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// وضعیت تأیید ایمیل
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// تاریخ تولد
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// سن
        /// </summary>
        public int? Age { get; set; }

        /// <summary>
        /// جنسیت
        /// </summary>
        public Gender? Gender { get; set; }

        /// <summary>
        /// آدرس کامل
        /// </summary>
        public string FullAddress { get; set; }

        /// <summary>
        /// تاریخ عضویت
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// آدرس IP آخرین ورود
        /// </summary>
        public string LastLoginIp { get; set; }

        /// <summary>
        /// مرورگر آخرین ورود
        /// </summary>
        public string LastLoginUserAgent { get; set; }

        /// <summary>
        /// تعداد ورودهای ناموفق
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// وضعیت قفل شدن حساب
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        /// تاریخ انقضای قفل
        /// </summary>
        public DateTime? LockoutEnd { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public List<string> Roles { get; set; }

        /// <summary>
        /// دسترسی‌های کاربر
        /// </summary>
        public List<string> Permissions { get; set; }
    }
} 