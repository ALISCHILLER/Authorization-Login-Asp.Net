using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Events;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای تمام موجودیت‌ها
    /// </summary>
    public abstract class BaseEntity : IAuditable, IEquatable<BaseEntity>
    {
        private readonly List<IDomainEvent> _domainEvents = new();

        /// <summary>
        /// شناسه یکتا
        /// </summary>
        public Guid Id { get; protected set; }

        /// <summary>
        /// تاریخ ایجاد
        /// </summary>
        public DateTime CreatedAt { get; protected set; }

        /// <summary>
        /// تاریخ آخرین به‌روزرسانی
        /// </summary>
        public DateTime? UpdatedAt { get; protected set; }

        /// <summary>
        /// آیا رکورد حذف شده است؟
        /// </summary>
        public bool IsDeleted { get; protected set; }

        /// <summary>
        /// تاریخ حذف
        /// </summary>
        public DateTime? DeletedAt { get; protected set; }

        /// <summary>
        /// شناسه کاربر ایجاد کننده
        /// </summary>
        public Guid? CreatedBy { get; protected set; }

        /// <summary>
        /// شناسه کاربر به‌روزرسانی کننده
        /// </summary>
        public Guid? UpdatedBy { get; protected set; }

        /// <summary>
        /// رویدادهای دامنه
        /// </summary>
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        // IAuditable implementation
        DateTime IAuditable.CreatedAt 
        { 
            get => CreatedAt; 
            set => CreatedAt = value; 
        }

        int? IAuditable.CreatedBy 
        { 
            get => CreatedBy.HasValue ? (int)CreatedBy.Value.GetHashCode() : null; 
            set => CreatedBy = value.HasValue ? new Guid(value.Value.ToString().PadLeft(32, '0')) : null; 
        }

        DateTime? IAuditable.UpdatedAt 
        { 
            get => UpdatedAt; 
            set => UpdatedAt = value; 
        }

        int? IAuditable.UpdatedBy 
        { 
            get => UpdatedBy.HasValue ? (int)UpdatedBy.Value.GetHashCode() : null; 
            set => UpdatedBy = value.HasValue ? new Guid(value.Value.ToString().PadLeft(32, '0')) : null; 
        }

        /// <summary>
        /// سازنده پیش‌فرض
        /// </summary>
        protected BaseEntity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsDeleted = false;
        }

        /// <summary>
        /// افزودن رویداد دامنه
        /// </summary>
        /// <param name="domainEvent">رویداد دامنه</param>
        protected void AddDomainEvent(IDomainEvent domainEvent)
        {
            if (domainEvent == null)
                throw new ArgumentNullException(nameof(domainEvent));
                
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        /// پاک کردن رویدادهای دامنه
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        /// <summary>
        /// به‌روزرسانی موجودیت
        /// </summary>
        /// <param name="updatedBy">شناسه کاربر به‌روزرسانی کننده</param>
        public void Update(Guid? updatedBy = null)
        {
            UpdatedAt = DateTime.UtcNow;
            UpdatedBy = updatedBy;
        }

        /// <summary>
        /// حذف منطقی موجودیت
        /// </summary>
        /// <param name="deletedBy">شناسه کاربر حذف کننده</param>
        public virtual void Delete(Guid? deletedBy = null)
        {
            IsDeleted = true;
            DeletedAt = DateTime.UtcNow;
            UpdatedBy = deletedBy;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// بازیابی موجودیت حذف شده
        /// </summary>
        /// <param name="restoredBy">شناسه کاربر بازیابی کننده</param>
        public virtual void Restore(Guid? restoredBy = null)
        {
            IsDeleted = false;
            DeletedAt = null;
            UpdatedBy = restoredBy;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// مقایسه با موجودیت دیگر
        /// </summary>
        /// <param name="other">موجودیت دیگر</param>
        /// <returns>نتیجه مقایسه</returns>
        public bool Equals(BaseEntity other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        /// <param name="obj">شیء دیگر</param>
        /// <returns>نتیجه مقایسه</returns>
        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BaseEntity)obj);
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        /// <returns>هش کد</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// عملگر برابری
        /// </summary>
        /// <param name="left">موجودیت سمت چپ</param>
        /// <param name="right">موجودیت سمت راست</param>
        /// <returns>نتیجه مقایسه</returns>
        public static bool operator ==(BaseEntity left, BaseEntity right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        /// <summary>
        /// عملگر نابرابری
        /// </summary>
        /// <param name="left">موجودیت سمت چپ</param>
        /// <param name="right">موجودیت سمت راست</param>
        /// <returns>نتیجه مقایسه</returns>
        public static bool operator !=(BaseEntity left, BaseEntity right)
        {
            return !(left == right);
        }
    }
}