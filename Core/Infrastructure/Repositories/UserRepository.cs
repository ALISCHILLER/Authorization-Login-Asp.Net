using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Cache;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن کاربر برای انجام عملیات روی مدل User
    /// </summary>
    public class UserRepository : CachedRepository<User>, IUserRepository
    {
        public UserRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<UserRepository> logger) : base(context, cacheService, logger)
        {
        }

        /// <summary>
        /// دریافت همه کاربران
        /// </summary>
        public async Task<IEnumerable<User>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.AllUsers();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var users = await _dbSet
                        .Include(u => u.Roles)
                        .Include(u => u.LoginHistory)
                        .Where(u => !u.IsDeleted)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} کاربر بازیابی شد", users.Count);
                    return users;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس شناسه
        /// </summary>
        public async Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ValidateId(id, nameof(id));
            return await GetCachedAsync(
                id,
                async (userId) => await _dbSet
                    .Include(u => u.Roles)
                    .Include(u => u.LoginHistory)
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken),
                CacheKeys.User,
                cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل
        /// </summary>
        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            return await GetCachedAsync(
                email,
                async (userEmail) => await _dbSet
                    .Include(u => u.Roles)
                    .Include(u => u.LoginHistory)
                    .FirstOrDefaultAsync(u => u.Email == userEmail && !u.IsDeleted, cancellationToken),
                CacheKeys.UserByEmail,
                cancellationToken);
        }

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری
        /// </summary>
        public async Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));

            return await GetCachedAsync(
                username,
                async (userUsername) => await _dbSet
                    .Include(u => u.Roles)
                    .Include(u => u.LoginHistory)
                    .FirstOrDefaultAsync(u => u.Username == userUsername && !u.IsDeleted, cancellationToken),
                CacheKeys.UserByUsername,
                cancellationToken);
        }

        /// <summary>
        /// افزودن کاربر جدید
        /// </summary>
        public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            await _dbSet.AddAsync(user, cancellationToken);
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

            _dbSet.Remove(user);
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
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            var cacheKey = $"user:exists:email:{email}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _dbSet.AnyAsync(u => u.Email == email && !u.IsDeleted, cancellationToken),
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری مشخص
        /// </summary>
        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));

            var cacheKey = $"user:exists:username:{username}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken),
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// حذف کاربر
        /// </summary>
        public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// فعال‌سازی کاربر
        /// </summary>
        public async Task ActivateAsync(User user, CancellationToken cancellationToken = default)
        {
            ValidateEntity(user, nameof(user));
            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(user).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(user, cancellationToken);
            _logger.LogInformation("کاربر با شناسه {UserId} با موفقیت فعال شد", user.Id);
        }

        /// <summary>
        /// غیرفعال‌سازی کاربر
        /// </summary>
        public async Task DeactivateAsync(User user, CancellationToken cancellationToken = default)
        {
            ValidateEntity(user, nameof(user));
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(user).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(user, cancellationToken);
            _logger.LogInformation("کاربر با شناسه {UserId} با موفقیت غیرفعال شد", user.Id);
        }

        /// <summary>
        /// قفل کردن کاربر
        /// </summary>
        public async Task LockAsync(User user, string reason, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("دلیل قفل کردن نمی‌تواند خالی باشد", nameof(reason));

            user.IsLocked = true;
            user.LockedAt = DateTime.UtcNow;
            user.LockReason = reason;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// باز کردن قفل کاربر
        /// </summary>
        public async Task UnlockAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

            user.IsLocked = false;
            user.LockedAt = null;
            user.LockReason = null;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// تغییر رمز عبور کاربر
        /// </summary>
        public async Task ChangePasswordAsync(User user, string newPasswordHash, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("رمز عبور جدید نمی‌تواند خالی باشد", nameof(newPasswordHash));

            user.PasswordHash = newPasswordHash;
            user.PasswordChangedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        public async Task UpdateProfileAsync(User user, CancellationToken cancellationToken = default)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user), "کاربر نمی‌تواند خالی باشد");

            user.UpdatedAt = DateTime.UtcNow;
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

            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == roleName))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(u => u.Roles)
                    .Where(u => u.Roles.Any(r => r.Id == roleId) && !u.IsDeleted)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت کاربران با نقش {RoleId}", roleId);
                throw;
            }
        }

        /// <summary>
        /// دریافت کاربران فعال
        /// </summary>
        public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.ActiveUsers();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var users = await _dbSet
                        .Include(u => u.Roles)
                        .Include(u => u.LoginHistory)
                        .Where(u => !u.IsDeleted && u.IsActive)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} کاربر فعال بازیابی شد", users.Count);
                    return users;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی یکتا بودن ایمیل
        /// </summary>
        public async Task<bool> IsEmailUniqueAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("ایمیل نمی‌تواند خالی باشد", nameof(email));

            return !await _dbSet.AnyAsync(u => 
                u.Email == email && 
                !u.IsDeleted && 
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value), 
                cancellationToken);
        }

        /// <summary>
        /// بررسی یکتا بودن نام کاربری
        /// </summary>
        public async Task<bool> IsUsernameUniqueAsync(string username, Guid? excludeUserId = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("نام کاربری نمی‌تواند خالی باشد", nameof(username));

            return !await _dbSet.AnyAsync(u => 
                u.Username == username && 
                !u.IsDeleted && 
                (!excludeUserId.HasValue || u.Id != excludeUserId.Value), 
                cancellationToken);
        }

        public async Task<bool> UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = passwordHash;
                user.LastPasswordChange = DateTime.UtcNow;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی رمز عبور کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> UpdateLastLoginAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet.FindAsync(new object[] { userId }, cancellationToken);
                if (user == null)
                {
                    return false;
                }

                user.LastLoginAt = DateTime.UtcNow;
                user.LoginCount++;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی آخرین ورود کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> AddToRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    return false;
                }

                var role = await _context.Roles.FindAsync(new object[] { roleId }, cancellationToken);
                if (role == null)
                {
                    return false;
                }

                if (!user.Roles.Any(r => r.Id == roleId))
                {
                    user.Roles.Add(role);
                    return await _context.SaveChangesAsync(cancellationToken) > 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن نقش {RoleId} به کاربر {UserId}", roleId, userId);
                throw;
            }
        }

        public async Task<bool> RemoveFromRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _dbSet
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                if (user == null)
                {
                    return false;
                }

                var role = user.Roles.FirstOrDefault(r => r.Id == roleId);
                if (role != null)
                {
                    user.Roles.Remove(role);
                    return await _context.SaveChangesAsync(cancellationToken) > 0;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف نقش {RoleId} از کاربر {UserId}", roleId, userId);
                throw;
            }
        }

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            var cacheKey = CacheKeys.UserRoles(userId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var user = await _dbSet
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                    return user?.Roles ?? new List<Role>();
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های کاربر
        /// </summary>
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            var cacheKey = CacheKeys.UserPermissions(userId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var user = await _dbSet
                        .Include(u => u.Roles)
                            .ThenInclude(r => r.Permissions)
                        .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                    if (user == null)
                        return new List<Permission>();

                    var permissions = user.Roles
                        .Where(r => r.IsActive)
                        .SelectMany(r => r.Permissions)
                        .Where(p => p.IsActive)
                        .Distinct()
                        .ToList();

                    return permissions;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            var userPermissions = await GetUserPermissionsAsync(userId, cancellationToken);
            return userPermissions.Any(p => p.Name == permissionName);
        }

        /// <summary>
        /// باطل کردن کش‌های مرتبط با کاربر
        /// </summary>
        protected override async Task InvalidateEntityCacheAsync(User user, CancellationToken cancellationToken = default)
        {
            var cacheKeys = CacheKeyHelper.GetEntityCacheKeys<User>(user.Id, user.Username);
            cacheKeys.Add(CacheKeys.UserByEmail(user.Email));
            await InvalidateCacheAsync(cacheKeys, cancellationToken);
        }
    }
}
