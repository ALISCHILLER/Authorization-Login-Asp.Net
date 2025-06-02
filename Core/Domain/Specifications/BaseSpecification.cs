using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Authorization_Login_Asp.Net.Core.Domain.Common;

namespace Authorization_Login_Asp.Net.Core.Domain.Specifications
{
    /// <summary>
    /// کلاس پایه برای مشخصات‌های دامنه
    /// </summary>
    /// <typeparam name="T">نوع موجودیت</typeparam>
    public abstract class BaseSpecification<T> where T : BaseEntity
    {
        /// <summary>
        /// شرط اصلی
        /// </summary>
        public Expression<Func<T, bool>> Criteria { get; private set; }

        /// <summary>
        /// لیست شامل‌ها
        /// </summary>
        public List<Expression<Func<T, object>>> Includes { get; } = new();

        /// <summary>
        /// لیست مرتب‌سازی‌ها
        /// </summary>
        public List<Expression<Func<T, object>>> OrderBy { get; private set; } = new();

        /// <summary>
        /// لیست مرتب‌سازی‌های نزولی
        /// </summary>
        public List<Expression<Func<T, object>>> OrderByDescending { get; private set; } = new();

        /// <summary>
        /// عبارت گروه‌بندی
        /// </summary>
        public Expression<Func<T, object>> GroupBy { get; private set; }

        /// <summary>
        /// تعداد آیتم‌های هر صفحه
        /// </summary>
        public int Take { get; private set; }

        /// <summary>
        /// تعداد آیتم‌های رد شده
        /// </summary>
        public int Skip { get; private set; }

        /// <summary>
        /// آیا صفحه‌بندی فعال است
        /// </summary>
        public bool IsPagingEnabled { get; private set; }

        /// <summary>
        /// فیلترهای پویا
        /// </summary>
        public Dictionary<string, object> DynamicFilters { get; } = new Dictionary<string, object>();

        /// <summary>
        /// سازنده
        /// </summary>
        protected BaseSpecification()
        {
        }

        /// <summary>
        /// سازنده با شرط
        /// </summary>
        /// <param name="criteria">شرط</param>
        protected BaseSpecification(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        /// <summary>
        /// افزودن شرط
        /// </summary>
        /// <param name="criteria">شرط</param>
        protected virtual void AddCriteria(Expression<Func<T, bool>> criteria)
        {
            Criteria = Criteria == null ? criteria : Criteria.And(criteria);
        }

        /// <summary>
        /// افزودن شامل
        /// </summary>
        /// <param name="includeExpression">عبارت شامل</param>
        protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// افزودن مرتب‌سازی صعودی
        /// </summary>
        /// <param name="orderByExpression">عبارت مرتب‌سازی</param>
        protected virtual void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy.Add(orderByExpression);
        }

        /// <summary>
        /// افزودن مرتب‌سازی نزولی
        /// </summary>
        /// <param name="orderByDescendingExpression">عبارت مرتب‌سازی نزولی</param>
        protected virtual void AddOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending.Add(orderByDescendingExpression);
        }

        /// <summary>
        /// تنظیم گروه‌بندی
        /// </summary>
        /// <param name="groupByExpression">عبارت گروه‌بندی</param>
        protected virtual void SetGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        /// <summary>
        /// اعمال صفحه‌بندی
        /// </summary>
        /// <param name="skip">تعداد آیتم‌های رد شده</param>
        /// <param name="take">تعداد آیتم‌های هر صفحه</param>
        protected virtual void ApplyPaging(int skip, int take)
        {
            Skip = skip;
            Take = take;
            IsPagingEnabled = true;
        }

        /// <summary>
        /// ترکیب با مشخصات دیگر
        /// </summary>
        public virtual ISpecification<T> And(ISpecification<T> other)
        {
            var combinedCriteria = Criteria == null ? other.Criteria : Criteria.And(other.Criteria);
            var result = new CompositeSpecification<T>(combinedCriteria);
            
            // ترکیب Include‌ها
            result.Includes.AddRange(Includes);
            result.Includes.AddRange(other.Includes);

            // ترکیب مرتب‌سازی‌ها
            result.OrderBy.AddRange(OrderBy);
            result.OrderBy.AddRange(other.OrderBy);
            result.OrderByDescending.AddRange(OrderByDescending);
            result.OrderByDescending.AddRange(other.OrderByDescending);

            // ترکیب فیلترهای پویا
            foreach (var filter in DynamicFilters)
                result.AddDynamicFilter(filter.Key, filter.Value);
            foreach (var filter in other.DynamicFilters)
                result.AddDynamicFilter(filter.Key, filter.Value);

            return result;
        }

        /// <summary>
        /// ترکیب با مشخصات دیگر به صورت OR
        /// </summary>
        public virtual ISpecification<T> Or(ISpecification<T> other)
        {
            var combinedCriteria = Criteria == null ? other.Criteria : Criteria.Or(other.Criteria);
            var result = new CompositeSpecification<T>(combinedCriteria);
            
            // ترکیب Include‌ها
            result.Includes.AddRange(Includes);
            result.Includes.AddRange(other.Includes);

            // ترکیب مرتب‌سازی‌ها
            result.OrderBy.AddRange(OrderBy);
            result.OrderBy.AddRange(other.OrderBy);
            result.OrderByDescending.AddRange(OrderByDescending);
            result.OrderByDescending.AddRange(other.OrderByDescending);

            // ترکیب فیلترهای پویا
            foreach (var filter in DynamicFilters)
                result.AddDynamicFilter(filter.Key, filter.Value);
            foreach (var filter in other.DynamicFilters)
                result.AddDynamicFilter(filter.Key, filter.Value);

            return result;
        }

        /// <summary>
        /// معکوس کردن شرط
        /// </summary>
        public virtual ISpecification<T> Not()
        {
            var result = new CompositeSpecification<T>(Criteria.Not());
            result.Includes.AddRange(Includes);
            result.OrderBy.AddRange(OrderBy);
            result.OrderByDescending.AddRange(OrderByDescending);
            foreach (var filter in DynamicFilters)
                result.AddDynamicFilter(filter.Key, filter.Value);
            return result;
        }

        /// <summary>
        /// افزودن فیلتر پویا
        /// </summary>
        public virtual void AddDynamicFilter(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            DynamicFilters[key] = value;
        }

        /// <summary>
        /// حذف فیلتر پویا
        /// </summary>
        public virtual void RemoveDynamicFilter(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            DynamicFilters.Remove(key);
        }

        /// <summary>
        /// پاک کردن تمام فیلترهای پویا
        /// </summary>
        public virtual void ClearDynamicFilters()
        {
            DynamicFilters.Clear();
        }
    }

    /// <summary>
    /// کلاس کمکی برای ترکیب مشخصات
    /// </summary>
    internal class CompositeSpecification<T> : BaseSpecification<T>
    {
        public CompositeSpecification(Expression<Func<T, bool>> criteria) : base(criteria)
        {
        }
    }

    /// <summary>
    /// کلاس‌های کمکی برای ترکیب عبارات لامبدا
    /// </summary>
    public static class ExpressionExtensions
    {
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.AndAlso(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.OrElse(
                Expression.Invoke(first, parameter),
                Expression.Invoke(second, parameter)
            );
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }

        public static Expression<Func<T, bool>> Not<T>(this Expression<Func<T, bool>> expression)
        {
            var parameter = Expression.Parameter(typeof(T));
            var body = Expression.Not(Expression.Invoke(expression, parameter));
            return Expression.Lambda<Func<T, bool>>(body, parameter);
        }
    }
}