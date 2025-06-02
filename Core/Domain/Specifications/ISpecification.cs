using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Authorization_Login_Asp.Net.Core.Domain.Specifications
{
    /// <summary>
    /// رابط الگوی Specification برای پیاده‌سازی جستجوی پیشرفته
    /// </summary>
    public interface ISpecification<T>
    {
        /// <summary>
        /// شرط جستجو
        /// </summary>
        Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// لیست Include‌ها
        /// </summary>
        List<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// لیست مرتب‌سازی‌ها
        /// </summary>
        List<Expression<Func<T, object>>> OrderBy { get; }

        /// <summary>
        /// لیست مرتب‌سازی‌های نزولی
        /// </summary>
        List<Expression<Func<T, object>>> OrderByDescending { get; }

        /// <summary>
        /// تعداد آیتم‌های هر صفحه
        /// </summary>
        int Take { get; }

        /// <summary>
        /// تعداد آیتم‌های رد شده
        /// </summary>
        int Skip { get; }

        /// <summary>
        /// آیا صفحه‌بندی فعال است؟
        /// </summary>
        bool IsPagingEnabled { get; }

        /// <summary>
        /// فیلترهای پویا
        /// </summary>
        Dictionary<string, object> DynamicFilters { get; }

        /// <summary>
        /// گروه‌بندی
        /// </summary>
        Expression<Func<T, object>> GroupBy { get; }

        /// <summary>
        /// ترکیب با مشخصات دیگر
        /// </summary>
        /// <param name="other">مشخصات دیگر</param>
        /// <returns>مشخصات ترکیبی</returns>
        ISpecification<T> And(ISpecification<T> other);

        /// <summary>
        /// ترکیب با مشخصات دیگر به صورت OR
        /// </summary>
        /// <param name="other">مشخصات دیگر</param>
        /// <returns>مشخصات ترکیبی</returns>
        ISpecification<T> Or(ISpecification<T> other);

        /// <summary>
        /// معکوس کردن شرط
        /// </summary>
        /// <returns>مشخصات معکوس شده</returns>
        ISpecification<T> Not();

        /// <summary>
        /// افزودن فیلتر پویا
        /// </summary>
        /// <param name="key">کلید فیلتر</param>
        /// <param name="value">مقدار فیلتر</param>
        void AddDynamicFilter(string key, object value);

        /// <summary>
        /// حذف فیلتر پویا
        /// </summary>
        /// <param name="key">کلید فیلتر</param>
        void RemoveDynamicFilter(string key);

        /// <summary>
        /// پاک کردن تمام فیلترهای پویا
        /// </summary>
        void ClearDynamicFilters();
    }
}