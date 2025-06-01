using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی پایه برای مخزن‌های موجودیت‌ها
    /// این کلاس عملیات پایه CRUD را برای تمام موجودیت‌ها پیاده‌سازی می‌کند
    /// </summary>
    /// <typeparam name="T">نوع موجودیت (مثلا User، Role و غیره)</typeparam>
    public class Repository<T> : IRepository<T> where T : class
    {
        /// <summary>
        /// کانتکست پایگاه داده
        /// </summary>
        protected readonly AppDbContext _context;

        /// <summary>
        /// مجموعه موجودیت‌ها در کانتکست
        /// </summary>
        protected readonly DbSet<T> _dbSet;

        /// <summary>
        /// سازنده کلاس مخزن
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public Repository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        /// <summary>
        /// افزودن یک موجودیت جدید به مخزن
        /// </summary>
        /// <param name="entity">موجودیت مورد نظر</param>
        /// <returns>موجودیت افزوده شده با شناسه‌های تولید شده</returns>
        /// <exception cref="ArgumentNullException">در صورتی که موجودیت null باشد</exception>
        public virtual async Task<T> AddAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _dbSet.AddAsync(entity);
            await SaveChangesAsync();
            return entity;
        }

        /// <summary>
        /// حذف یک موجودیت از مخزن
        /// </summary>
        /// <param name="entity">موجودیت مورد نظر</param>
        /// <returns>true در صورت موفقیت‌آمیز بودن عملیات حذف</returns>
        /// <exception cref="ArgumentNullException">در صورتی که موجودیت null باشد</exception>
        public virtual async Task<bool> RemoveAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbSet.Remove(entity);
            return await SaveChangesAsync();
        }

        /// <summary>
        /// به‌روزرسانی یک موجودیت در مخزن
        /// </summary>
        /// <param name="entity">موجودیت با مقادیر به‌روز شده</param>
        /// <returns>true در صورت موفقیت‌آمیز بودن عملیات به‌روزرسانی</returns>
        /// <exception cref="ArgumentNullException">در صورتی که موجودیت null باشد</exception>
        public virtual async Task<bool> UpdateAsync(T entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.Entry(entity).State = EntityState.Modified;
            return await SaveChangesAsync();
        }

        /// <summary>
        /// بازیابی یک موجودیت بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه یکتای موجودیت</param>
        /// <returns>موجودیت مورد نظر در صورت وجود، در غیر این صورت null</returns>
        public virtual async Task<T> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        /// <summary>
        /// بازیابی تمام موجودیت‌ها
        /// </summary>
        /// <returns>لیست تمام موجودیت‌های موجود در مخزن</returns>
        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        /// <summary>
        /// بازیابی موجودیت‌ها بر اساس یک شرط
        /// </summary>
        /// <param name="predicate">عبارت شرطی برای فیلتر کردن موجودیت‌ها</param>
        /// <returns>لیست موجودیت‌های مطابق با شرط</returns>
        /// <exception cref="ArgumentNullException">در صورتی که شرط null باشد</exception>
        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// بازیابی اولین موجودیت مطابق با شرط
        /// </summary>
        /// <param name="predicate">عبارت شرطی برای فیلتر کردن موجودیت‌ها</param>
        /// <returns>اولین موجودیت مطابق با شرط در صورت وجود، در غیر این صورت null</returns>
        /// <exception cref="ArgumentNullException">در صورتی که شرط null باشد</exception>
        public virtual async Task<T> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        /// <summary>
        /// بررسی وجود موجودیت بر اساس شرط
        /// </summary>
        /// <param name="predicate">عبارت شرطی برای بررسی وجود موجودیت</param>
        /// <returns>true در صورت وجود حداقل یک موجودیت مطابق با شرط</returns>
        /// <exception cref="ArgumentNullException">در صورتی که شرط null باشد</exception>
        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            return await _dbSet.AnyAsync(predicate);
        }

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        /// <returns>true در صورت موفقیت‌آمیز بودن عملیات ذخیره</returns>
        /// <exception cref="DbUpdateConcurrencyException">در صورت بروز خطای همزمانی</exception>
        /// <exception cref="DbUpdateException">در صورت بروز خطای بروزرسانی</exception>
        public virtual async Task<bool> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync() > 0;
            }
            catch (DbUpdateConcurrencyException)
            {
                // در صورت بروز خطای همزمانی، تمام موجودیت‌ها را از حالت تغییر خارج می‌کنیم
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
            catch (DbUpdateException)
            {
                // در صورت بروز خطای بروزرسانی، تمام موجودیت‌ها را از حالت تغییر خارج می‌کنیم
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    entry.State = EntityState.Detached;
                }
                throw;
            }
        }

        /// <summary>
        /// اضافه کردن موجودیت‌های مرتبط به کوئری
        /// </summary>
        /// <param name="query">کوئری اصلی</param>
        /// <param name="includes">عبارت‌های include برای موجودیت‌های مرتبط</param>
        /// <returns>کوئری با موجودیت‌های مرتبط</returns>
        protected IQueryable<T> Include(IQueryable<T> query, params Expression<Func<T, object>>[] includes)
        {
            if (includes != null)
            {
                query = includes.Aggregate(query, (current, include) => current.Include(include));
            }
            return query;
        }

        /// <summary>
        /// مرتب‌سازی نتایج کوئری
        /// </summary>
        /// <param name="query">کوئری اصلی</param>
        /// <param name="orderBy">عبارت مرتب‌سازی</param>
        /// <param name="isDescending">آیا به صورت نزولی مرتب شود؟</param>
        /// <returns>کوئری مرتب شده</returns>
        protected IQueryable<T> OrderBy(IQueryable<T> query, Expression<Func<T, object>> orderBy, bool isDescending = false)
        {
            if (orderBy != null)
            {
                query = isDescending
                    ? query.OrderByDescending(orderBy)
                    : query.OrderBy(orderBy);
            }
            return query;
        }

        /// <summary>
        /// صفحه‌بندی نتایج کوئری
        /// </summary>
        /// <param name="query">کوئری اصلی</param>
        /// <param name="pageNumber">شماره صفحه</param>
        /// <param name="pageSize">تعداد آیتم در هر صفحه</param>
        /// <returns>کوئری صفحه‌بندی شده</returns>
        protected IQueryable<T> Paginate(IQueryable<T> query, int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;
            if (pageSize < 1)
                pageSize = 10;

            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
    }
}