using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن کاربر برای انجام عملیات روی مدل User
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// دریافت همه کاربران
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.ConnectedDevices)
                .Include(u => u.RefreshTokens)
                .Include(u => u.RecoveryCodes)
                .Include(u => u.LoginHistory)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس شناسه
        /// </summary>
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.ConnectedDevices)
                .Include(u => u.RefreshTokens)
                .Include(u => u.RecoveryCodes)
                .Include(u => u.LoginHistory)
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل
        /// </summary>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.ConnectedDevices)
                .Include(u => u.RefreshTokens)
                .Include(u => u.RecoveryCodes)
                .Include(u => u.LoginHistory)
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری
        /// </summary>
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .Include(u => u.ConnectedDevices)
                .Include(u => u.RefreshTokens)
                .Include(u => u.RecoveryCodes)
                .Include(u => u.LoginHistory)
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            await _context.Users.AddAsync(user, cancellationToken);
        }

        /// <summary>
        /// بروزرسانی اطلاعات کاربر
        /// </summary>
        public void Update(User user)
        {
            _context.Users.Update(user);
        }

        /// <summary>
        /// حذف کاربر از کانتکست
        /// </summary>
        public void Remove(User user)
        {
            _context.Users.Remove(user);
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی وجود کاربر با ایمیل مشخص
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Email.Value == email, cancellationToken);
        }

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری مشخص
        /// </summary>
        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username, cancellationToken);
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت تاریخچه ورودهای کاربر
        /// </summary>
        public async Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            return await _context.LoginHistory
                .Where(lh => lh.UserId == userId)
                .OrderByDescending(lh => lh.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت تعداد کل رکوردهای تاریخچه ورود کاربر
        /// </summary>
        public async Task<int> GetLoginHistoryCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.LoginHistory
                .CountAsync(lh => lh.UserId == userId, cancellationToken);
        }
    }
}
