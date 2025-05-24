using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس عمومی مخزن (Repository) برای انجام عملیات پایه CRUD روی موجودیت‌ها
    /// با قابلیت کار با انواع مختلف موجودیت‌ها (Generic)
    /// </summary>
    /// <typeparam name="T">نوع موجودیت (Entity) که این ریپازیتوری برای آن کار می‌کند</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// دریافت همه رکوردهای موجودیت به صورت لیست
        /// </summary>
        /// <returns>لیست تمام رکوردهای موجودیت</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// دریافت یک رکورد موجودیت بر اساس شناسه یکتا (Id)
        /// </summary>
        /// <param name="id">شناسه یکتا (معمولاً Guid یا کلید اصلی)</param>
        /// <returns>رکورد موجودیت یا null در صورت عدم وجود</returns>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// جستجو و دریافت لیستی از موجودیت‌ها بر اساس شرط دلخواه
        /// </summary>
        /// <param name="predicate">عبارت شرطی (Expression) برای فیلتر کردن رکوردها</param>
        /// <returns>لیستی از رکوردهای مطابق شرط</returns>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// افزودن موجودیت جدید به مخزن (برای ذخیره‌سازی بعدی در دیتابیس)
        /// </summary>
        /// <param name="entity">موجودیت جدید برای افزودن</param>
        /// <returns>تسک اجرای عملیات</returns>
        Task AddAsync(T entity);

        /// <summary>
        /// افزودن چند موجودیت به صورت گروهی
        /// </summary>
        /// <param name="entities">لیست موجودیت‌ها برای افزودن</param>
        /// <returns>تسک اجرای عملیات</returns>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// حذف موجودیت از مخزن (نشانه‌گذاری برای حذف در دیتابیس)
        /// </summary>
        /// <param name="entity">موجودیتی که باید حذف شود</param>
        void Remove(T entity);

        /// <summary>
        /// حذف گروهی چند موجودیت از مخزن
        /// </summary>
        /// <param name="entities">لیست موجودیت‌هایی که باید حذف شوند</param>
        void RemoveRange(IEnumerable<T> entities);

        /// <summary>
        /// به‌روزرسانی اطلاعات موجودیت موجود
        /// </summary>
        /// <param name="entity">موجودیت با اطلاعات به‌روز شده</param>
        void Update(T entity);

        /// <summary>
        /// ذخیره نهایی تغییرات انجام شده در دیتابیس
        /// این متد معمولاً باید پس از افزودن، حذف یا به‌روزرسانی فراخوانی شود
        /// </summary>
        /// <returns>تعداد رکوردهای تغییر یافته در دیتابیس</returns>
        Task<int> SaveChangesAsync();
    }
}
