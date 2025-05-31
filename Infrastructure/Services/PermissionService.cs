using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت دسترسی‌ها
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly ILogger<PermissionService> _logger;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public PermissionService(
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            IRolePermissionRepository rolePermissionRepository,
            ILogger<PermissionService> logger,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));

            var cacheExpirationMinutes = configuration.GetValue<int>("CacheSettings:PermissionCacheExpirationMinutes", 30);
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes / 2));
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک منبع
        /// </summary>
        public async Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
        {
            try
            {
                var cacheKey = $"permission_{userId}_{resource}_{action}";
                if (_cache.TryGetValue(cacheKey, out bool hasPermission))
                {
                    return hasPermission;
                }

                var permissionName = $"{resource}.{action}";
                var hasAccess = await _permissionRepository.HasPermissionAsync(userId.ToString(), permissionName);

                _cache.Set(cacheKey, hasAccess, _cacheOptions);
                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی دسترسی کاربر {UserId} به منبع {Resource} با عملیات {Action}", 
                    userId, resource, action);
                throw new PermissionServiceException("خطا در بررسی دسترسی", ex);
            }
        }

        /// <summary>
        /// دریافت لیست دسترسی‌های کاربر
        /// </summary>
        public async Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"user_permissions_{userId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<string> permissions))
                {
                    return permissions;
                }

                var userPermissions = await _permissionRepository.GetByUserIdAsync(userId.ToString());
                var permissionNames = userPermissions.Select(p => p.Name).ToList();

                _cache.Set(cacheKey, permissionNames, _cacheOptions);
                return permissionNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های کاربر {UserId}", userId);
                throw new PermissionServiceException("خطا در دریافت دسترسی‌های کاربر", ex);
            }
        }

        /// <summary>
        /// افزودن دسترسی به کاربر
        /// </summary>
        public async Task AddPermissionAsync(Guid userId, string resource, string action)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"کاربر با شناسه {userId} یافت نشد");
                }

                var permissionName = $"{resource}.{action}";
                var permission = await _permissionRepository.GetByNameAsync(permissionName);
                if (permission == null)
                {
                    throw new KeyNotFoundException($"دسترسی {permissionName} یافت نشد");
                }

                // افزودن دسترسی به نقش کاربر
                var userRoles = await _roleRepository.GetByUserIdAsync(userId);
                foreach (var role in userRoles)
                {
                    if (!await _rolePermissionRepository.ExistsAsync(role.Id, permission.Id))
                    {
                        await _rolePermissionRepository.AddPermissionToRoleAsync(role.Id, permission.Id);
                    }
                }

                // پاک کردن کش
                _cache.Remove($"user_permissions_{userId}");
                _cache.Remove($"permission_{userId}_{resource}_{action}");

                _logger.LogInformation("دسترسی {Permission} به کاربر {UserId} اضافه شد", permissionName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن دسترسی {Resource}.{Action} به کاربر {UserId}", 
                    resource, action, userId);
                throw new PermissionServiceException("خطا در افزودن دسترسی", ex);
            }
        }

        /// <summary>
        /// حذف دسترسی از کاربر
        /// </summary>
        public async Task RemovePermissionAsync(Guid userId, string resource, string action)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new KeyNotFoundException($"کاربر با شناسه {userId} یافت نشد");
                }

                var permissionName = $"{resource}.{action}";
                var permission = await _permissionRepository.GetByNameAsync(permissionName);
                if (permission == null)
                {
                    throw new KeyNotFoundException($"دسترسی {permissionName} یافت نشد");
                }

                // حذف دسترسی از نقش‌های کاربر
                var userRoles = await _roleRepository.GetByUserIdAsync(userId);
                foreach (var role in userRoles)
                {
                    if (await _rolePermissionRepository.ExistsAsync(role.Id, permission.Id))
                    {
                        await _rolePermissionRepository.RemovePermissionFromRoleAsync(role.Id, permission.Id);
                    }
                }

                // پاک کردن کش
                _cache.Remove($"user_permissions_{userId}");
                _cache.Remove($"permission_{userId}_{resource}_{action}");

                _logger.LogInformation("دسترسی {Permission} از کاربر {UserId} حذف شد", permissionName, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف دسترسی {Resource}.{Action} از کاربر {UserId}", 
                    resource, action, userId);
                throw new PermissionServiceException("خطا در حذف دسترسی", ex);
            }
        }

        /// <summary>
        /// بررسی دسترسی‌های نقش
        /// </summary>
        public async Task<bool> HasRolePermissionAsync(string role, string resource, string action)
        {
            try
            {
                var cacheKey = $"role_permission_{role}_{resource}_{action}";
                if (_cache.TryGetValue(cacheKey, out bool hasPermission))
                {
                    return hasPermission;
                }

                var permissionName = $"{resource}.{action}";
                var roleEntity = await _roleRepository.GetByNameAsync(role);
                if (roleEntity == null)
                {
                    throw new KeyNotFoundException($"نقش {role} یافت نشد");
                }

                var permission = await _permissionRepository.GetByNameAsync(permissionName);
                if (permission == null)
                {
                    throw new KeyNotFoundException($"دسترسی {permissionName} یافت نشد");
                }

                var hasAccess = await _rolePermissionRepository.ExistsAsync(roleEntity.Id, permission.Id);

                _cache.Set(cacheKey, hasAccess, _cacheOptions);
                return hasAccess;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی دسترسی نقش {Role} به منبع {Resource} با عملیات {Action}", 
                    role, resource, action);
                throw new PermissionServiceException("خطا در بررسی دسترسی نقش", ex);
            }
        }

        /// <summary>
        /// دریافت لیست دسترسی‌های نقش
        /// </summary>
        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string role)
        {
            try
            {
                var cacheKey = $"role_permissions_{role}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<string> permissions))
                {
                    return permissions;
                }

                var roleEntity = await _roleRepository.GetByNameAsync(role);
                if (roleEntity == null)
                {
                    throw new KeyNotFoundException($"نقش {role} یافت نشد");
                }

                var rolePermissions = await _rolePermissionRepository.GetRolePermissionsAsync(roleEntity.Id);
                var permissionNames = rolePermissions.Select(p => p.Name).ToList();

                _cache.Set(cacheKey, permissionNames, _cacheOptions);
                return permissionNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های نقش {Role}", role);
                throw new PermissionServiceException("خطا در دریافت دسترسی‌های نقش", ex);
            }
        }

        /// <summary>
        /// دریافت تمام دسترسی‌های موجود در سیستم
        /// </summary>
        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            try
            {
                const string cacheKey = "all_permissions";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                permissions = await _permissionRepository.GetAllAsync();
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت لیست تمام دسترسی‌ها");
                throw new PermissionServiceException("خطا در دریافت لیست دسترسی‌ها", ex);
            }
        }

        /// <summary>
        /// دریافت دسترسی با استفاده از شناسه
        /// </summary>
        public async Task<Permission> GetPermissionByIdAsync(Guid id)
        {
            try
            {
                var cacheKey = $"permission_{id}";
                if (_cache.TryGetValue(cacheKey, out Permission permission))
                {
                    return permission;
                }

                permission = await _permissionRepository.GetByIdAsync(id);
                if (permission != null)
                {
                    _cache.Set(cacheKey, permission, _cacheOptions);
                }
                return permission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی با شناسه {PermissionId}", id);
                throw new PermissionServiceException("خطا در دریافت دسترسی", ex);
            }
        }

        /// <summary>
        /// دریافت لیست دسترسی‌های یک نقش با استفاده از شناسه نقش
        /// </summary>
        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
        {
            try
            {
                var cacheKey = $"role_permissions_{roleId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                permissions = await _rolePermissionRepository.GetRolePermissionsAsync(roleId);
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های نقش با شناسه {RoleId}", roleId);
                throw new PermissionServiceException("خطا در دریافت دسترسی‌های نقش", ex);
            }
        }

        /// <summary>
        /// دریافت لیست دسترسی‌های یک کاربر با استفاده از شناسه کاربر
        /// </summary>
        public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(Guid userId)
        {
            try
            {
                var cacheKey = $"user_permissions_{userId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new PermissionServiceException($"کاربر با شناسه {userId} یافت نشد");
                }

                var role = await _roleRepository.GetByIdAsync(user.RoleId);
                if (role == null)
                {
                    throw new PermissionServiceException($"نقش برای کاربر {userId} یافت نشد");
                }

                permissions = await GetPermissionsByRoleIdAsync(role.Id);
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت دسترسی‌های کاربر با شناسه {UserId}", userId);
                throw new PermissionServiceException("خطا در دریافت دسترسی‌های کاربر", ex);
            }
        }
    }

    /// <summary>
    /// خطای سرویس دسترسی‌ها
    /// </summary>
    public class PermissionServiceException : Exception
    {
        public PermissionServiceException(string message) : base(message) { }
        public PermissionServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}