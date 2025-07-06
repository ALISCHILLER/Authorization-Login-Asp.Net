using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای Value Objects
    /// </summary>
    public abstract class ValueObject
    {
        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        /// <param name="obj">شیء دیگر</param>
        /// <returns>نتیجه مقایسه</returns>
        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType())
                return false;
            var other = (ValueObject)obj;
            return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        /// <returns>هش کد</returns>
        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        /// <summary>
        /// عملگر برابری
        /// </summary>
        /// <param name="left">Value Object سمت چپ</param>
        /// <param name="right">Value Object سمت راست</param>
        /// <returns>نتیجه مقایسه</returns>
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
                return false;
            return ReferenceEquals(left, null) || left.Equals(right);
        }

        /// <summary>
        /// عملگر نابرابری
        /// </summary>
        /// <param name="left">Value Object سمت چپ</param>
        /// <param name="right">Value Object سمت راست</param>
        /// <returns>نتیجه مقایسه</returns>
        public static bool operator !=(ValueObject left, ValueObject right)
        {
            return !(left == right);
        }

        /// <summary>
        /// دریافت مقادیر پراپرتی‌ها برای مقایسه
        /// </summary>
        /// <returns>مجموعه مقادیر پراپرتی‌ها</returns>
        /// <remarks>
        /// این متد باید در کلاس‌های مشتق شده پیاده‌سازی شود.
        /// برای پیاده‌سازی صحیح، تمام پراپرتی‌هایی که در مقایسه دو Value Object نقش دارند را برگردانید.
        /// مثال:
        /// protected override IEnumerable<object> GetEqualityComponents()
        /// {
        ///     yield return PropertyA;
        ///     yield return PropertyB;
        /// }
        /// </remarks>
        protected abstract IEnumerable<object> GetEqualityComponents();

        /// <summary>
        /// کپی کردن Value Object
        /// </summary>
        /// <returns>کپی از Value Object</returns>
        /// <remarks>
        /// این متد یک کپی عمیق از Value Object ایجاد می‌کند.
        /// در صورت نیاز به منطق خاص برای کپی کردن، این متد را در کلاس مشتق شده override کنید.
        /// </remarks>
        public virtual ValueObject Copy()
        {
            var copy = MemberwiseClone() as ValueObject;
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                if (!property.CanWrite) continue;
                
                var value = property.GetValue(this);
                if (value is ValueObject valueObject)
                {
                    property.SetValue(copy, valueObject.Copy());
                }
                else if (value is ICloneable cloneable)
                {
                    property.SetValue(copy, cloneable.Clone());
                }
            }
            
            return copy;
        }

        /// <summary>
        /// دریافت مقادیر تمام پراپرتی‌های عمومی
        /// </summary>
        /// <returns>مجموعه مقادیر پراپرتی‌ها</returns>
        protected virtual IEnumerable<object> GetProperties()
        {
            return GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.GetValue(this));
        }
    }
} 