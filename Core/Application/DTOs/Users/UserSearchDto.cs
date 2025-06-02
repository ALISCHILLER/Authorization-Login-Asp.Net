using System;
using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    /// <summary>
    /// کلاس درخواست جستجوی پیشرفته کاربران
    /// </summary>
    public class UserSearchDto : PaginationRequestDto
    {
        /// <summary>
        /// عبارت جستجو در نام کاربری، نام، نام خانوادگی و ایمیل
        /// </summary>
        [MaxLength(100, ErrorMessage = "عبارت جستجو نمی‌تواند بیشتر از 100 کاراکتر باشد")]
        public string SearchTerm { get; set; }

        /// <summary>
        /// نوع نقش کاربر
        /// </summary>
        public RoleType? RoleType { get; set; }

        /// <summary>
        /// وضعیت فعال بودن حساب کاربری
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// وضعیت تأیید ایمیل
        /// </summary>
        public bool? IsEmailVerified { get; set; }

        /// <summary>
        /// وضعیت تأیید شماره تلفن
        /// </summary>
        public bool? IsPhoneVerified { get; set; }

        /// <summary>
        /// وضعیت فعال بودن احراز هویت دو مرحله‌ای
        /// </summary>
        public bool? TwoFactorEnabled { get; set; }

        /// <summary>
        /// تاریخ شروع برای فیلتر تاریخ ایجاد
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// تاریخ پایان برای فیلتر تاریخ ایجاد
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// تاریخ شروع برای فیلتر آخرین ورود
        /// </summary>
        public DateTime? LastLoginFrom { get; set; }

        /// <summary>
        /// تاریخ پایان برای فیلتر آخرین ورود
        /// </summary>
        public DateTime? LastLoginTo { get; set; }

        /// <summary>
        /// فیلدهای اضافی برای مرتب‌سازی
        /// </summary>
        public enum SortableFields
        {
            Username,
            Email,
            FullName,
            CreatedAt,
            LastLoginAt,
            RoleType,
            IsActive
        }

        /// <summary>
        /// اعتبارسنجی تاریخ‌ها
        /// </summary>
        public bool ValidateDates()
        {
            if (CreatedFrom.HasValue && CreatedTo.HasValue && CreatedFrom > CreatedTo)
                return false;

            if (LastLoginFrom.HasValue && LastLoginTo.HasValue && LastLoginFrom > LastLoginTo)
                return false;

            return true;
        }
    }

    /// <summary>
    /// کلاس پاسخ جستجوی کاربران
    /// </summary>
    public class UserSearchResponseDto : PaginationResponseDto<UserDto>
    {
        /// <summary>
        /// تعداد کل کاربران فعال
        /// </summary>
        public int TotalActiveUsers { get; set; }

        /// <summary>
        /// تعداد کل کاربران غیرفعال
        /// </summary>
        public int TotalInactiveUsers { get; set; }

        /// <summary>
        /// تعداد کل کاربران با ایمیل تأیید نشده
        /// </summary>
        public int TotalUnverifiedEmailUsers { get; set; }

        /// <summary>
        /// تعداد کل کاربران با شماره تلفن تأیید نشده
        /// </summary>
        public int TotalUnverifiedPhoneUsers { get; set; }

        /// <summary>
        /// تعداد کل کاربران با احراز هویت دو مرحله‌ای فعال
        /// </summary>
        public int TotalTwoFactorEnabledUsers { get; set; }
    }
} 