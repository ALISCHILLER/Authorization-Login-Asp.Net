using System;

namespace Authorization_Login_Asp.Net.Domain.Interfaces
{
    /// <summary>
    /// رابط برای موجودیت‌هایی که از حذف نرم پشتیبانی می‌کنند
    /// </summary>
    public interface IDeletable
    {
        /// <summary>
        /// وضعیت حذف شده بودن
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        DateTime? DeletedAt { get; set; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        int? DeletedBy { get; set; }
    }
} 