using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
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
                .Include(u => u.UserDevices)
                .Include(u => u.RefreshTokens.Where(rt => rt.IsActive))
                .Include(u => u.RecoveryCodes.Where(rc => rc.IsActive))
                .Include(u => u.LoginHistory.OrderByDescending(lh => lh.LoginTime).Take(10))
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس شناسه
        /// </summary>
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(id));

            return await _context.Users
                .Include(u => u.UserDevices)
                .Include(u => u.RefreshTokens.Where(rt => rt.IsActive))
                .Include(u => u.RecoveryCodes.Where(rc => rc.IsActive))
                .Include(u => u.LoginHistory.OrderByDescending(lh => lh.LoginTime).Take(10))
                .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل
        /// </summary>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            return await _context.Users
                .Include(u => u.UserDevices)
                .Include(u => u.RefreshTokens.Where(rt => rt.IsActive))
                .Include(u => u.RecoveryCodes.Where(rc => rc.IsActive))
                .Include(u => u.LoginHistory.OrderByDescending(lh => lh.LoginTime).Take(10))
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری
        /// </summary>
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            return await _context.Users
                .Include(u => u.UserDevices)
                .Include(u => u.RefreshTokens.Where(rt => rt.IsActive))
                .Include(u => u.RecoveryCodes.Where(rc => rc.IsActive))
                .Include(u => u.LoginHistory.OrderByDescending(lh => lh.LoginTime).Take(10))
                .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        }

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _context.Users.AddAsync(user, cancellationToken);
        }

        /// <summary>
        /// بروزرسانی اطلاعات کاربر
        /// </summary>
        public void Update(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Entry(user).State = EntityState.Modified;
        }

        /// <summary>
        /// حذف کاربر از کانتکست
        /// </summary>
        public void Remove(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
        }

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw new InvalidOperationException("A concurrency error occurred while saving changes.", ex);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }
        }

        /// <summary>
        /// بررسی وجود کاربر با ایمیل مشخص
        /// </summary>
        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            return await _context.Users
                .AnyAsync(u => u.Email.Value == email, cancellationToken);
        }

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری مشخص
        /// </summary>
        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            return await _context.Users
                .AnyAsync(u => u.Username == username, cancellationToken);
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت تاریخچه ورودهای کاربر
        /// </summary>
        public async Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));
            if (page < 1)
                throw new ArgumentException("Page number must be greater than 0", nameof(page));
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

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
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _context.LoginHistory
                .CountAsync(lh => lh.UserId == userId, cancellationToken);
        }

        #region Login History Management
        /// <summary>
        /// افزودن رکورد جدید به تاریخچه ورود
        /// </summary>
        public async Task AddLoginHistoryAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            await _context.LoginHistory.AddAsync(loginHistory);
        }

        /// <summary>
        /// به‌روزرسانی رکورد تاریخچه ورود
        /// </summary>
        public async Task UpdateLoginHistoryAsync(LoginHistory loginHistory)
        {
            if (loginHistory == null)
                throw new ArgumentNullException(nameof(loginHistory));

            _context.Entry(loginHistory).State = EntityState.Modified;
        }

        /// <summary>
        /// دریافت کوئری تاریخچه ورود کاربر
        /// </summary>
        public IQueryable<LoginHistory> GetLoginHistoryQuery(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return _context.LoginHistory
                .Where(lh => lh.UserId == userId)
                .AsNoTracking();
        }

        /// <summary>
        /// دریافت آخرین ورود موفق کاربر
        /// </summary>
        public async Task<LoginHistory> GetLastSuccessfulLoginAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _context.LoginHistory
                .Where(lh => lh.UserId == userId && lh.IsSuccessful)
                .OrderByDescending(lh => lh.LoginTime)
                .FirstOrDefaultAsync();
        }
        #endregion

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            return await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
