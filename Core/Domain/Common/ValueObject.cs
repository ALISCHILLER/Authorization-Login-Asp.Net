using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    /// <summary>
    /// کلاس پایه برای Value Objects
    /// </summary>
    public abstract class ValueObject : IEquatable<ValueObject>
    {
        /// <summary>
        /// مقایسه با Value Object دیگر
        /// </summary>
        /// <param name="other">Value Object دیگر</param>
        /// <returns>نتیجه مقایسه</returns>
        public sealed bool Equals(ValueObject other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            var thisProperties = GetProperties();
            var otherProperties = other.GetProperties();

            if (thisProperties is null || otherProperties is null)
                return false;

            return thisProperties.SequenceEqual(otherProperties);
        }

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        /// <param name="obj">شیء دیگر</param>
        /// <returns>نتیجه مقایسه</returns>
        public sealed override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ValueObject)obj);
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        /// <returns>هش کد</returns>
        public sealed override int GetHashCode()
        {
            var properties = GetProperties();
            if (properties is null)
                return 0;

            return properties
                .Select(x => x != null ? x.GetHashCode() : 0)
                .Aggregate((x, y) => x ^ y);
        }

        /// <summary>
        /// عملگر برابری
        /// </summary>
        /// <param name="left">Value Object سمت چپ</param>
        /// <param name="right">Value Object سمت راست</param>
        /// <returns>نتیجه مقایسه</returns>
        public static bool operator ==(ValueObject left, ValueObject right)
        {
            if (left is null && right is null) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
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
        protected abstract IEnumerable<object> GetProperties();

        /// <summary>
        /// کپی کردن Value Object
        /// </summary>
        /// <returns>کپی از Value Object</returns>
        public virtual ValueObject Copy()
        {
            return MemberwiseClone() as ValueObject;
        }
    }
} 