using System;

namespace Authorization_Login_Asp.Net.Domain.Interfaces
{
    /// <summary>
    /// رابط برای موجودیت‌هایی که نیاز به ثبت تاریخچه تغییرات دارند
    /// </summary>
    public interface IAuditable
    {
        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        int? CreatedBy { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// شناسه کاربر به‌روزرسانی کننده
        /// </summary>
        int? UpdatedBy { get; set; }
    }
} 