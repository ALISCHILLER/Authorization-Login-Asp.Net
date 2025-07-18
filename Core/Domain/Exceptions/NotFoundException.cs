using System;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// استثنای مربوط به زمانی که یک موجودیت پیدا نشود
    /// </summary>
    public class NotFoundException : DomainException
    {
        /// <summary>
        /// نوع موجودیت
        /// </summary>
        public string? EntityType { get; }

        /// <summary>
        /// شناسه موجودیت
        /// </summary>
        public object? EntityId { get; }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای NotFound
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        public NotFoundException(string message, string? entityType = null, object? entityId = null) 
            : base(message, DomainErrorCodes.General.NotFound)
        {
            EntityType = entityType;
            EntityId = entityId;
            if (entityType != null && AdditionalData != null)
                AdditionalData["EntityType"] = entityType;
            if (entityId != null && AdditionalData != null)
                AdditionalData["EntityId"] = entityId;
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای NotFound با پیام پیش‌فرض
        /// </summary>
        /// <param name="entityType">نوع موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        public NotFoundException(string entityType, object entityId)
            : this($"{entityType} با شناسه {entityId} یافت نشد", entityType, entityId)
        {
        }
    }
}