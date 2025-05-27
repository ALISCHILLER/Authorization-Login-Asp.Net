using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Authorization_Login_Asp.Net.Domain.Specifications
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
        /// عبارت OrderBy
        /// </summary>
        Expression<Func<T, object>> OrderBy { get; }

        /// <summary>
        /// عبارت OrderByDescending
        /// </summary>
        Expression<Func<T, object>> OrderByDescending { get; }

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
    }
} 