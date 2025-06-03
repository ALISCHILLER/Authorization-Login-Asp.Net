// استفاده از رابط‌های تعریف شده در لایه کاربرد
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Cache;
// استفاده از کلاس‌های پایگاه داده در لایه زیرساخت
// استفاده از Entity Framework Core برای عملیات پایگاه داده
using Microsoft.EntityFrameworkCore;
// استفاده از کلاس‌های پایه سیستم
using System;
// استفاده از کلاس‌های مجموعه‌ها
using System.Collections.Generic;
// استفاده از کلاس‌های LINQ
using System.Linq;
// استفاده از کلاس‌های مدیریت نخ‌ها
using System.Threading;
// استفاده از کلاس‌های عملیات ناهمگام
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن نقش برای انجام عملیات روی مدل Role
    /// </summary>
    public class RoleRepository : CachedRepository<Role>, IRoleRepository
    {
        public RoleRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<RoleRepository> logger) : base(context, cacheService, logger)
        {
        }

        /// <summary>
        /// دریافت همه نقش‌ها
        /// </summary>
        public async Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.AllRoles();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var roles = await _dbSet
                        .Include(r => r.Permissions)
                        .Include(r => r.Users)
                        .Where(r => !r.IsDeleted)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} نقش بازیابی شد", roles.Count);
                    return roles;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت نقش بر اساس شناسه
        /// </summary>
        public async Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ValidateId(id, nameof(id));
            return await GetCachedAsync(
                id,
                async (roleId) => await _dbSet
                    .Include(r => r.Permissions)
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken),
                CacheKeys.Role,
                cancellationToken);
        }

        /// <summary>
        /// دریافت نقش بر اساس نام
        /// </summary>
        public async Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            return await GetCachedAsync(
                name,
                async (roleName) => await _dbSet
                    .Include(r => r.Permissions)
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Name == roleName && !r.IsDeleted, cancellationToken),
                CacheKeys.RoleByName,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود نقش با نام مشخص
        /// </summary>
        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            var cacheKey = $"role:exists:name:{name}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _dbSet.AnyAsync(r => r.Name == name && !r.IsDeleted, cancellationToken),
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// حذف نقش
        /// </summary>
        public override async Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role), "نقش نمی‌تواند خالی باشد");

            role.IsDeleted = true;
            role.DeletedAt = DateTime.UtcNow;
            await _dbSet.Update(role).ReloadAsync(cancellationToken);

            // حذف کش‌های مرتبط
            await _cacheService.RemoveAsync(CacheKeys.Role(role.Id), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RoleByName(role.Name), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RolePermissions(role.Id), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.AllRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ActiveRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.SystemRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.UserRoles(), cancellationToken);

            _logger.LogInformation("نقش با شناسه {RoleId} با موفقیت حذف شد", role.Id);
        }

        /// <summary>
        /// فعال‌سازی نقش
        /// </summary>
        public async Task ActivateAsync(Role role, CancellationToken cancellationToken = default)
        {
            ValidateEntity(role, nameof(role));
            role.IsActive = true;
            role.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(role).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(role, cancellationToken);
            _logger.LogInformation("نقش با شناسه {RoleId} با موفقیت فعال شد", role.Id);
        }

        /// <summary>
        /// غیرفعال‌سازی نقش
        /// </summary>
        public async Task DeactivateAsync(Role role, CancellationToken cancellationToken = default)
        {
            ValidateEntity(role, nameof(role));
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(role).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(role, cancellationToken);
            _logger.LogInformation("نقش با شناسه {RoleId} با موفقیت غیرفعال شد", role.Id);
        }

        /// <summary>
        /// به‌روزرسانی اطلاعات نقش
        /// </summary>
        public override async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role), "نقش نمی‌تواند خالی باشد");

            role.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(role).ReloadAsync(cancellationToken);

            // به‌روزرسانی کش‌های مرتبط
            await _cacheService.RemoveAsync(CacheKeys.Role(role.Id), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RoleByName(role.Name), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.RolePermissions(role.Id), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.AllRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ActiveRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.SystemRoles(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.UserRoles(), cancellationToken);

            _logger.LogInformation("نقش با شناسه {RoleId} با موفقیت به‌روزرسانی شد", role.Id);
        }

        /// <summary>
        /// دریافت نقش‌های یک نوع خاص
        /// </summary>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های نوع مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default)
        {
            var roleNames = type switch
            {
                RoleType.Admin => new[] { "Admin" },
                RoleType.User => new[] { "User" },
                RoleType.Operator => new[] { "Operator" },
                RoleType.ContentManager => new[] { "ContentManager" },
                RoleType.Support => new[] { "Support" },
                _ => Array.Empty<string>()
            };

            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => roleNames.Contains(r.Name))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های فعال
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های فعال</returns>
        public async Task<IEnumerable<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.ActiveRoles();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var roles = await _dbSet
                        .Include(r => r.Permissions)
                        .Where(r => !r.IsDeleted && r.IsActive)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} نقش فعال بازیابی شد", roles.Count);
                    return roles;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            var cacheKey = CacheKeys.UserRoles(userId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var user = await _context.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, cancellationToken);

                    return user?.Roles ?? new List<Role>();
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود نقش در یک نوع خاص
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر نقش در نوع مورد نظر وجود داشته باشد</returns>
        public async Task<bool> ExistsInTypeAsync(string name, RoleType type, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نقش نمی‌تواند خالی باشد", nameof(name));

            var roleNames = type switch
            {
                RoleType.Admin => new[] { "Admin" },
                RoleType.User => new[] { "User" },
                RoleType.Operator => new[] { "Operator" },
                RoleType.ContentManager => new[] { "ContentManager" },
                RoleType.Support => new[] { "Support" },
                _ => Array.Empty<string>()
            };

            return await _dbSet.AnyAsync(r => r.Name == name && roleNames.Contains(r.Name), cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های سیستمی
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های سیستمی</returns>
        public async Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.SystemRoles();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var roles = await _dbSet
                        .Include(r => r.Permissions)
                        .Where(r => !r.IsDeleted && r.IsSystem)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} نقش سیستمی بازیابی شد", roles.Count);
                    return roles;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های کاربری
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربری</returns>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.UserRoles();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var roles = await _dbSet
                        .Include(r => r.Permissions)
                        .Where(r => !r.IsDeleted && !r.IsSystem)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} نقش کاربری بازیابی شد", roles.Count);
                    return roles;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های یک کاربر به همراه دسترسی‌های آن‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر با دسترسی‌های آن‌ها</returns>
        public async Task<IEnumerable<Role>> GetUserRolesWithPermissionsAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new ArgumentException("شناسه کاربر باید یک GUID معتبر باشد", nameof(userId));

            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Users.Any(u => u.Id == userGuid))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی تعلق کاربر به یک نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به نقش تعلق داشته باشد</returns>
        public async Task<bool> IsUserInRoleAsync(Guid userId, string roleName, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            ValidateName(roleName, nameof(roleName));

            return await _dbSet
                .AnyAsync(r => r.Name == roleName && r.Users.Any(u => u.Id == userId), cancellationToken);
        }

        /// <summary>
        /// بررسی تعلق کاربر به چند نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه نقش‌ها تعلق داشته باشد</returns>
        public async Task<bool> IsUserInAllRolesAsync(Guid userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            if (roleNames == null || !roleNames.Any())
                throw new ArgumentException("لیست نقش‌ها نمی‌تواند خالی باشد", nameof(roleNames));

            var userRoles = await _dbSet
                .Where(r => r.Users.Any(u => u.Id == userId))
                .Select(r => r.Name)
                .ToListAsync(cancellationToken);

            return roleNames.All(roleName => userRoles.Contains(roleName));
        }

        /// <summary>
        /// بررسی تعلق کاربر به حداقل یکی از نقش‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از نقش‌ها تعلق داشته باشد</returns>
        public async Task<bool> IsUserInAnyRoleAsync(Guid userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            if (roleNames == null || !roleNames.Any())
                throw new ArgumentException("لیست نقش‌ها نمی‌تواند خالی باشد", nameof(roleNames));

            return await _dbSet
                .AnyAsync(r => roleNames.Contains(r.Name) && r.Users.Any(u => u.Id == userId), cancellationToken);
        }

        /// <summary>
        /// افزودن کاربر به نقش
        /// </summary>
        public async Task AddUserToRoleAsync(Guid userId, string roleName)
        {
            ValidateId(userId, nameof(userId));
            ValidateName(roleName, nameof(roleName));

            var role = await GetByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"نقش {roleName} یافت نشد", nameof(roleName));

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new ArgumentException($"کاربر با شناسه {userId} یافت نشد", nameof(userId));

            if (!await IsUserInRoleAsync(userId, roleName))
            {
                var userRole = new UserRole
                {
                    UserId = userId,
                    RoleId = role.Id,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.UserRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
                await InvalidateEntityCacheAsync(role);
            }
        }

        /// <summary>
        /// حذف کاربر از نقش
        /// </summary>
        public async Task RemoveUserFromRoleAsync(Guid userId, string roleName)
        {
            ValidateId(userId, nameof(userId));
            ValidateName(roleName, nameof(roleName));

            var role = await GetByNameAsync(roleName);
            if (role == null)
                throw new ArgumentException($"نقش {roleName} یافت نشد", nameof(roleName));

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
                await InvalidateEntityCacheAsync(role);
            }
        }

        /// <summary>
        /// بررسی یکتا بودن نام نقش
        /// </summary>
        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeRoleId = null, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            return !await _dbSet.AnyAsync(r => 
                r.Name == name && 
                !r.IsDeleted && 
                (!excludeRoleId.HasValue || r.Id != excludeRoleId.Value), 
                cancellationToken);
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        public async Task<bool> AddPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            ValidateId(roleId, nameof(roleId));
            ValidateId(permissionId, nameof(permissionId));

            var role = await _dbSet
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                return false;

            var permission = await _context.Permissions.FindAsync(new object[] { permissionId }, cancellationToken);
            if (permission == null)
                return false;

            if (!role.Permissions.Any(p => p.Id == permissionId))
            {
                role.Permissions.Add(permission);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (result)
                {
                    await InvalidateEntityCacheAsync(role, cancellationToken);
                }
                return result;
            }

            return true;
        }

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        public async Task<bool> RemovePermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            ValidateId(roleId, nameof(roleId));
            ValidateId(permissionId, nameof(permissionId));

            var role = await _dbSet
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

            if (role == null)
                return false;

            var permission = role.Permissions.FirstOrDefault(p => p.Id == permissionId);
            if (permission != null)
            {
                role.Permissions.Remove(permission);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (result)
                {
                    await InvalidateEntityCacheAsync(role, cancellationToken);
                }
                return result;
            }

            return true;
        }

        /// <summary>
        /// دریافت دسترسی‌های نقش
        /// </summary>
        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            ValidateId(roleId, nameof(roleId));
            var cacheKey = CacheKeys.RolePermissions(roleId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var role = await _dbSet
                        .Include(r => r.Permissions)
                        .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

                    return role?.Permissions ?? new List<Permission>();
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی نقش
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionName, CancellationToken cancellationToken = default)
        {
            ValidateId(roleId, nameof(roleId));
            ValidateName(permissionName, nameof(permissionName));

            return await _dbSet
                .Include(r => r.Permissions)
                .AnyAsync(r => 
                    r.Id == roleId && 
                    !r.IsDeleted && 
                    r.Permissions.Any(p => 
                        p.Name == permissionName && 
                        p.IsActive), 
                    cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های دارای یک دسترسی
        /// </summary>
        public async Task<IEnumerable<Role>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            ValidateId(permissionId, nameof(permissionId));
            return await _dbSet
                .Include(r => r.Permissions)
                .Where(r => r.Permissions.Any(p => p.Id == permissionId) && !r.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// باطل کردن کش‌های مرتبط با نقش
        /// </summary>
        protected override async Task InvalidateEntityCacheAsync(Role role, CancellationToken cancellationToken = default)
        {
            var cacheKeys = CacheKeyHelper.GetEntityCacheKeys<Role>(role.Id, role.Name);
            await InvalidateCacheAsync(cacheKeys, cancellationToken);
        }
    }
}