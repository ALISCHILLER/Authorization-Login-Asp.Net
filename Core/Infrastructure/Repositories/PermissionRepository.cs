using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;
using Authorization_Login_Asp.Net.Core.Infrastructure.Cache;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن دسترسی‌ها
    /// این کلاس عملیات مربوط به دسترسی‌ها را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class PermissionRepository : CachedRepository<Permission>, IPermissionRepository
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(30);

        /// <summary>
        /// سازنده کلاس مخزن دسترسی‌ها
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        /// <param name="rolePermissionRepository">مخزن ارتباط نقش-دسترسی</param>
        /// <param name="cacheService">خدمات کش</param>
        /// <param name="logger">لاگر برای لاگ کردن خطاها</param>
        public PermissionRepository(
            ApplicationDbContext context,
            IRolePermissionRepository rolePermissionRepository,
            ICacheService cacheService,
            ILogger<PermissionRepository> logger) : base(context, cacheService, logger)
        {
            _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        }

        /// <summary>
        /// دریافت دسترسی با شناسه
        /// </summary>
        /// <param name="id">شناسه دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>دسترسی مورد نظر در صورت وجود</returns>
        /// <exception cref="ArgumentException">در صورت نامعتبر بودن شناسه دسترسی</exception>
        public async Task<Permission> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            ValidateId(id, nameof(id));
            return await GetCachedAsync(
                id,
                async (permissionId) => await _dbSet
                    .Include(p => p.Roles)
                    .FirstOrDefaultAsync(p => p.Id == permissionId && !p.IsDeleted, cancellationToken),
                CacheKeys.Permission,
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی با نام
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>دسترسی مورد نظر در صورت وجود</returns>
        /// <exception cref="ArgumentException">در صورت خالی بودن نام دسترسی</exception>
        public async Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            return await GetCachedAsync(
                name,
                async (permissionName) => await _dbSet
                    .Include(p => p.Roles)
                    .FirstOrDefaultAsync(p => p.Name == permissionName && !p.IsDeleted, cancellationToken),
                CacheKeys.PermissionByName,
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک گروه خاص
        /// </summary>
        /// <param name="group">نام گروه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های گروه مورد نظر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن نام گروه</exception>
        public async Task<IEnumerable<Permission>> GetByGroupAsync(string group, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(p => p.Group == group && !p.IsDeleted)
                    .OrderBy(p => p.Name)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های گروه {Group}", group);
                throw;
            }
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نوع خاص
        /// </summary>
        /// <param name="type">نوع دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نوع مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetByTypeAsync(PermissionType type, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(p => p.Type == type)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های فعال
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های فعال</returns>
        public async Task<IEnumerable<Permission>> GetActivePermissionsAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = CacheKeys.ActivePermissions();
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var permissions = await _dbSet
                        .Include(p => p.Roles)
                        .Where(p => !p.IsDeleted && p.IsActive)
                        .ToListAsync(cancellationToken);

                    _logger.LogInformation("تعداد {Count} دسترسی فعال بازیابی شد", permissions.Count);
                    return permissions;
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نقش خاص
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه نقش</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه نقش</exception>
        public async Task<IEnumerable<Permission>> GetByRoleIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentNullException(nameof(roleId), "شناسه نقش نمی‌تواند خالی باشد");

            if (!Guid.TryParse(roleId, out Guid roleGuid))
                throw new FormatException("فرمت شناسه نقش نامعتبر است");

            return await _dbSet
                .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleGuid))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های چند نقش
        /// </summary>
        /// <param name="roleIds">شناسه‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش‌های مورد نظر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن لیست شناسه‌ها</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت هر یک از شناسه‌ها</exception>
        public async Task<IEnumerable<Permission>> GetByRoleIdsAsync(IEnumerable<string> roleIds, CancellationToken cancellationToken = default)
        {
            if (roleIds == null || !roleIds.Any())
                throw new ArgumentNullException(nameof(roleIds), "لیست شناسه‌های نقش نمی‌تواند خالی باشد");

            var roleGuids = new List<Guid>();
            foreach (var roleId in roleIds)
            {
                if (!Guid.TryParse(roleId, out Guid roleGuid))
                    throw new FormatException($"فرمت شناسه نقش '{roleId}' نامعتبر است");
                roleGuids.Add(roleGuid);
            }

            return await _dbSet
                .Where(p => p.RolePermissions.Any(rp => roleGuids.Contains(rp.RoleId)))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی وجود دسترسی با نام
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر دسترسی وجود داشته باشد</returns>
        /// <exception cref="ArgumentException">در صورت خالی بودن نام دسترسی</exception>
        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            var cacheKey = $"permission:exists:name:{name}";
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await _dbSet.AnyAsync(p => p.Name == name && !p.IsDeleted, cancellationToken),
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود دسترسی در یک گروه
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="group">نام گروه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر دسترسی در گروه وجود داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن نام دسترسی یا گروه</exception>
        public async Task<bool> ExistsInGroupAsync(string name, string group, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name), "نام دسترسی نمی‌تواند خالی باشد");

            if (string.IsNullOrWhiteSpace(group))
                throw new ArgumentNullException(nameof(group), "نام گروه نمی‌تواند خالی باشد");

            return await _dbSet
                .AnyAsync(p => p.Name == name && p.Group == group, cancellationToken);
        }

        /// <summary>
        /// دریافت تمام گروه‌های دسترسی
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نام گروه‌های دسترسی</returns>
        public async Task<IEnumerable<string>> GetAllGroupsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Select(p => p.Group)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت تعداد دسترسی‌های هر گروه
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>دیکشنری شامل نام گروه و تعداد دسترسی‌های آن</returns>
        public async Task<IDictionary<string, int>> GetGroupCountsAsync(CancellationToken cancellationToken = default)
        {
            var groups = await _dbSet
                .GroupBy(p => p.Group)
                .Select(g => new { Group = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            return groups.ToDictionary(g => g.Group, g => g.Count);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های کاربر مورد نظر</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<IEnumerable<Permission>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .Where(p => p.RolePermissions
                    .Any(rp => rp.Role.Users
                        .Any(u => u.Id == userGuid)))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک عملیات خاص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionName">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا نام دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentNullException(nameof(permissionName), "نام دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => p.Name == permissionName &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به چند عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            var userPermissions = await _dbSet
                .Where(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)))
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);

            return permissionNames.All(name => userPermissions.Contains(name));
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        public async Task<IEnumerable<Permission>?> GetPermissionsByRoleIdAsync(int roleId)
        {
            try
            {
                return await _dbSet
                    .Where(p => p.RolePermissions.Any(rp => rp.RoleId == roleId) && !p.IsDeleted)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های نقش {RoleId}", roleId);
                throw;
            }
        }

        /// <summary>
        /// بررسی یکتا بودن نام دسترسی
        /// </summary>
        public async Task<bool> IsNameUniqueAsync(string name, Guid? excludePermissionId = null, CancellationToken cancellationToken = default)
        {
            ValidateName(name, nameof(name));
            return !await _dbSet.AnyAsync(p => 
                p.Name == name && 
                !p.IsDeleted && 
                (!excludePermissionId.HasValue || p.Id != excludePermissionId.Value), 
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نقش
        /// </summary>
        public async Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            ValidateId(roleId, nameof(roleId));
            var cacheKey = CacheKeys.RolePermissions(roleId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var role = await _context.Roles
                        .Include(r => r.Permissions)
                        .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted, cancellationToken);

                    return role?.Permissions ?? new List<Permission>();
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک کاربر
        /// </summary>
        public async Task<IEnumerable<Permission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            var cacheKey = CacheKeys.UserPermissions(userId);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var user = await _context.Users
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
        public async Task<bool> HasUserPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default)
        {
            ValidateId(userId, nameof(userId));
            ValidateName(permissionName, nameof(permissionName));

            var userPermissions = await GetByUserAsync(userId, cancellationToken);
            return userPermissions.Any(p => p.Name == permissionName);
        }

        /// <summary>
        /// بررسی دسترسی نقش
        /// </summary>
        public async Task<bool> HasRoleAsync(Guid permissionId, string roleName, CancellationToken cancellationToken = default)
        {
            ValidateId(permissionId, nameof(permissionId));
            ValidateName(roleName, nameof(roleName));

            return await _dbSet
                .Include(p => p.Roles)
                .AnyAsync(p => 
                    p.Id == permissionId && 
                    !p.IsDeleted && 
                    p.Roles.Any(r => 
                        r.Name == roleName && 
                        r.IsActive), 
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به چند عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            var userPermissions = await _dbSet
                .Where(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)))
                .Select(p => p.Name)
                .ToListAsync(cancellationToken);

            return permissionNames.All(name => userPermissions.Contains(name));
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن شناسه کاربر یا لیست نام‌های دسترسی</exception>
        /// <exception cref="FormatException">در صورت نامعتبر بودن فرمت شناسه کاربر</exception>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId), "شناسه کاربر نمی‌تواند خالی باشد");

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentNullException(nameof(permissionNames), "لیست نام‌های دسترسی نمی‌تواند خالی باشد");

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException("فرمت شناسه کاربر نامعتبر است");

            return await _dbSet
                .AnyAsync(p => permissionNames.Contains(p.Name) &&
                    p.RolePermissions.Any(rp => rp.Role.Users.Any(u => u.Id == userGuid)),
                    cancellationToken);
        }

        /// <summary>
        /// <returns>درست اگر دسترسی با موفقیت افزوده شد</returns>
        /// <exception cref="ArgumentNullException">در صورت خالی بودن دسترسی</exception>
        public override async Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission), "دسترسی نمی‌تواند خالی باشد");

            permission.CreatedAt = DateTime.UtcNow;
            permission.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(permission, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // به‌روزرسانی کش‌های مرتبط
            await _cacheService.RemoveAsync(CacheKeys.PermissionByName(permission.Name), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.AllPermissions(), cancellationToken);
            await _cacheService.RemoveAsync(CacheKeys.ActivePermissions(), cancellationToken);

            _logger.LogInformation("دسترسی جدید با شناسه {PermissionId} با موفقیت افزوده شد", permission.Id);
        }
    }
}