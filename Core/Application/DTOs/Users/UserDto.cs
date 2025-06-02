using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    /// <summary>
    /// اطلاعات کاربر
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// شناسه کاربر
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// نام کاربری
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// نام کامل
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// آیا کاربر فعال است
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// آیا ایمیل تأیید شده است
        /// </summary>
        public bool IsEmailVerified { get; set; }

        /// <summary>
        /// آیا شماره تلفن تأیید شده است
        /// </summary>
        public bool IsPhoneVerified { get; set; }

        /// <summary>
        /// تاریخ آخرین ورود
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public List<RoleDto> Roles { get; set; }

        /// <summary>
        /// دسترسی‌های کاربر
        /// </summary>
        public List<PermissionDto> Permissions { get; set; }

        /// <summary>
        /// تنظیمات امنیتی کاربر
        /// </summary>
        public UserSecuritySettingsDto SecuritySettings { get; set; }
    }

    /// <summary>
    /// درخواست ایجاد کاربر جدید
    /// </summary>
    public class CreateUserRequest
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [Required(ErrorMessage = "ایمیل الزامی است")]
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "رمز عبور باید حداقل 6 کاراکتر باشد")]
        public string Password { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [Required(ErrorMessage = "نام الزامی است")]
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Required(ErrorMessage = "شماره تلفن الزامی است")]
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public List<string> Roles { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی کاربر
    /// </summary>
    public class UpdateUserRequest
    {
        /// <summary>
        /// نام کاربری
        /// </summary>
        [StringLength(50, MinimumLength = 3, ErrorMessage = "نام کاربری باید بین 3 تا 50 کاراکتر باشد")]
        public string Username { get; set; }

        /// <summary>
        /// ایمیل
        /// </summary>
        [EmailAddress(ErrorMessage = "فرمت ایمیل نامعتبر است")]
        public string Email { get; set; }

        /// <summary>
        /// نام
        /// </summary>
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }

        /// <summary>
        /// آیا کاربر فعال است
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// نقش‌های کاربر
        /// </summary>
        public List<string> Roles { get; set; }
    }

    /// <summary>
    /// درخواست به‌روزرسانی پروفایل
    /// </summary>
    public class UpdateProfileRequest
    {
        /// <summary>
        /// نام
        /// </summary>
        [StringLength(50, ErrorMessage = "نام نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string FirstName { get; set; }

        /// <summary>
        /// نام خانوادگی
        /// </summary>
        [StringLength(50, ErrorMessage = "نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")]
        public string LastName { get; set; }

        /// <summary>
        /// شماره تلفن
        /// </summary>
        [Phone(ErrorMessage = "فرمت شماره تلفن نامعتبر است")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// آدرس تصویر پروفایل
        /// </summary>
        public string ProfileImageUrl { get; set; }
    }

    /// <summary>
    /// تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsDto
    {
        /// <summary>
        /// آیا احراز هویت دو مرحله‌ای فعال است
        /// </summary>
        public bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// نوع احراز هویت دو مرحله‌ای
        /// </summary>
        public string TwoFactorType { get; set; }

        /// <summary>
        /// تاریخ آخرین تغییر رمز عبور
        /// </summary>
        public DateTime? LastPasswordChange { get; set; }

        /// <summary>
        /// تعداد تلاش‌های ناموفق ورود
        /// </summary>
        public int FailedLoginAttempts { get; set; }

        /// <summary>
        /// زمان پایان قفل شدن حساب
        /// </summary>
        public DateTime? AccountLockoutEnd { get; set; }

        /// <summary>
        /// وضعیت قفل بودن حساب
        /// </summary>
        public string LockStatus { get; set; }
    }
} 