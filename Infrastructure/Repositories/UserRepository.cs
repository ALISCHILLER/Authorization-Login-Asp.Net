using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Persistence.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن (Repository) کاربران با استفاده از Entity Framework Core
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly DbContext _context;
        private readonly DbSet<User> _users;

        /// <summary>
        /// سازنده با تزریق DbContext
        /// </summary>
        /// <param name="context">شیء DbContext پروژه</param>
        public UserRepository(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _users = _context.Set<User>();
        }

        /// <summary>
        /// دریافت همه کاربران به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیستی از همه کاربران</returns>
        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _users.ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس شناسه (Id) به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _users.FindAsync(new object[] { id }, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل (برای فرایند ورود و اعتبارسنجی) به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="email">آدرس ایمیل کاربر</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var loweredEmail = email.ToLowerInvariant();
            return await _users.SingleOrDefaultAsync(u => u.Email.Value == loweredEmail, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="username">نام کاربری یکتا</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>شیء کاربر یا null در صورت عدم وجود</returns>
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _users.SingleOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        /// <summary>
        /// افزودن کاربر جدید به مخزن به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="user">شیء کاربر برای افزودن</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _users.AddAsync(user, cancellationToken);
        }

        /// <summary>
        /// بروزرسانی اطلاعات یک کاربر موجود در کانتکست (تا ذخیره تغییرات)
        /// </summary>
        /// <param name="user">شیء کاربری که باید بروزرسانی شود</param>
        public void Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _users.Update(user);
        }

        /// <summary>
        /// حذف یک کاربر از کانتکست (تا ذخیره تغییرات)
        /// </summary>
        /// <param name="user">شیء کاربری که باید حذف شود</param>
        public void Remove(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _users.Remove(user);
        }

        /// <summary>
        /// ذخیره تغییرات انجام شده در مخزن به صورت غیرهمزمان و با امکان لغو عملیات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد رکوردهای تغییر یافته در دیتابیس</returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
