using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس پایه برای ریپازیتوری‌ها؛ این کلاس متدهای عمومی (مثلا افزودن، حذف، به‌روزرسانی، بازیابی و فیلتر کردن موجودیت‌ها) را تعریف می‌کند.
    /// </summary>
    /// <typeparam name="T">نوع موجودیت (مثلا User، Role و غیره)</typeparam>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// افزودن یک موجودیت جدید به مخزن (با ذخیره‌سازی ناهمگام)؛ این متد یک موجودیت را به مخزن اضافه می‌کند و در صورت موفقیت، موجودیت (با شناسه‌های تولید شده) را برمی‌گرداند.
        /// </summary>
        /// <param name="entity">موجودیت مورد نظر</param>
        /// <returns>موجودیت افزوده شده (با شناسه‌های تولید شده)</returns>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// حذف یک موجودیت از مخزن (با ذخیره‌سازی ناهمگام)؛ این متد یک موجودیت را از مخزن حذف می‌کند و وضعیت حذف (موفق یا ناموفق) را برمی‌گرداند.
        /// </summary>
        /// <param name="entity">موجودیت مورد نظر</param>
        /// <returns>وضعیت حذف (موفق یا ناموفق)</returns>
        Task<bool> RemoveAsync(T entity);

        /// <summary>
        /// به‌روزرسانی یک موجودیت در مخزن (با ذخیره‌سازی ناهمگام)؛ این متد یک موجودیت را با مقادیر به‌روز شده در مخزن به‌روزرسانی می‌کند و وضعیت به‌روزرسانی (موفق یا ناموفق) را برمی‌گرداند.
        /// </summary>
        /// <param name="entity">موجودیت مورد نظر (با مقادیر به‌روز شده)</param>
        /// <returns>وضعیت به‌روزرسانی (موفق یا ناموفق)</returns>
        Task<bool> UpdateAsync(T entity);

        /// <summary>
        /// بازیابی یک موجودیت بر اساس شناسه (با ذخیره‌سازی ناهمگام)؛ این متد یک موجودیت را بر اساس شناسه (GUID) بازیابی می‌کند و در صورت عدم وجود، مقدار null را برمی‌گرداند.
        /// </summary>
        /// <param name="id">شناسه (GUID) موجودیت</param>
        /// <returns>موجودیت مورد نظر (یا null در صورت عدم وجود)</returns>
        Task<T> GetByIdAsync(Guid id);

        /// <summary>
        /// بازیابی تمام موجودیت‌ها (با ذخیره‌سازی ناهمگام)؛ این متد لیست تمام موجودیت‌های موجود در مخزن را برمی‌گرداند.
        /// </summary>
        /// <returns>لیست تمام موجودیت‌ها</returns>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// بازیابی موجودیت‌ها بر اساس یک شرط (با ذخیره‌سازی ناهمگام)؛ این متد لیست موجودیت‌هایی را که با شرط (به صورت Expression) مطابقت دارند، برمی‌گرداند.
        /// </summary>
        /// <param name="predicate">شرط (به صورت Expression) برای فیلتر کردن موجودیت‌ها</param>
        /// <returns>لیست موجودیت‌های مطابق شرط</returns>
        Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// بازیابی یک موجودیت بر اساس شرط (با ذخیره‌سازی ناهمگام)؛ این متد اولین موجودیتی را که با شرط (به صورت Expression) مطابقت دارد، برمی‌گرداند و در صورت عدم وجود، مقدار null را برمی‌گرداند.
        /// </summary>
        /// <param name="predicate">شرط (به صورت Expression) برای فیلتر کردن موجودیت‌ها</param>
        /// <returns>موجودیت مورد نظر (یا null در صورت عدم وجود)</returns>
        Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// بررسی وجود یک موجودیت بر اساس شرط (با ذخیره‌سازی ناهمگام)؛ این متد وضعیت وجود (بله یا خیر) را بر اساس شرط (به صورت Expression) برمی‌گرداند.
        /// </summary>
        /// <param name="predicate">شرط (به صورت Expression) برای بررسی وجود موجودیت</param>
        /// <returns>وضعیت وجود (بله یا خیر)</returns>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// ذخیره‌سازی تغییرات (با ذخیره‌سازی ناهمگام)؛ این متد معمولاً در انتهای عملیات‌های چند مرحله‌ای فراخوانی می‌شود و وضعیت ذخیره‌سازی (موفق یا ناموفق) را برمی‌گرداند.
        /// </summary>
        /// <returns>وضعیت ذخیره‌سازی (موفق یا ناموفق)</returns>
        Task<bool> SaveChangesAsync();
    }
}
