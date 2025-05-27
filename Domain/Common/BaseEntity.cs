using System;

namespace Authorization_Login_Asp.Net.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای تمام موجودیت‌ها
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// آیا رکورد حذف شده است؟
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; set; }
    }
} 