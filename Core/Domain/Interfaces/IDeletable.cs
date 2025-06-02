using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    /// <summary>
    /// رابط برای موجودیت‌هایی که از حذف نرم پشتیبانی می‌کنند
    /// </summary>
    public interface IDeletable
    {
        /// <summary>
        /// وضعیت حذف شده بودن
        /// </summary>
        bool IsDeleted { get; }

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        DateTime? DeletedAt { get; }

        /// <summary>
        /// شناسه کاربر حذف کننده
        /// </summary>
        Guid? DeletedBy { get; }
    }
}