using System;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// استثنای مربوط به تداخل در عملیات
    /// </summary>
    public class ConflictException : DomainException
    {
        /// <summary>
        /// نوع موجودیت
        /// </summary>
        public string EntityType { get; }

        /// <summary>
        /// مقدار تکراری
        /// </summary>
        public object ConflictingValue { get; }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای Conflict
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="conflictingValue">مقدار تکراری</param>
        public ConflictException(string message, string entityType = null, object conflictingValue = null) 
            : base(message, DomainErrorCodes.General.DuplicateEntry)
        {
            EntityType = entityType;
            ConflictingValue = conflictingValue;

            if (entityType != null)
                AddData("EntityType", entityType);
            if (conflictingValue != null)
                AddData("ConflictingValue", conflictingValue);
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای Conflict با پیام پیش‌فرض
        /// </summary>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="conflictingValue">مقدار تکراری</param>
        public ConflictException(string entityType, object conflictingValue)
            : this($"{entityType} با مقدار {conflictingValue} قبلاً ثبت شده است", entityType, conflictingValue)
        {
        }
    }
} 