using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base
{
    /// <summary>
    /// کلاس پایه برای سرویس‌های مدیریت دسترسی
    /// این کلاس شامل متدهای مشترک برای بررسی و مدیریت دسترسی‌های کاربران است
    /// </summary>
    public abstract class BasePermissionService : BaseService
    {
        protected BasePermissionService(
            ILogger logger,
            IMemoryCache cache,
            int cacheExpirationMinutes = 30)
            : base(logger, cache, cacheExpirationMinutes)
        {
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک عملیات خاص
        /// </summary>
        protected async Task<bool> HasPermissionAsync(
            string userId,
            string permissionName,
            Func<Task<IEnumerable<Permission>>> getPermissions)
        {
            return await ExecuteWithLoggingAsync($"بررسی دسترسی {permissionName} برای کاربر {userId}", async () =>
            {
                var cacheKey = $"UserPermissions_{userId}";
                var permissions = await GetOrSetCacheAsync(cacheKey, getPermissions);
                return permissions.Any(p => p.Name == permissionName && p.IsActive);
            });
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش خاص
        /// </summary>
        protected async Task<bool> HasRoleAsync(
            string userId,
            UserRole role,
            Func<Task<IEnumerable<Role>>> getRoles)
        {
            return await ExecuteWithLoggingAsync($"بررسی نقش {role} برای کاربر {userId}", async () =>
            {
                var cacheKey = $"UserRoles_{userId}";
                var roles = await GetOrSetCacheAsync(cacheKey, getRoles);
                return roles.Any(r => r.Name == role.ToString() && r.IsActive);
            });
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش و عملیات خاص
        /// </summary>
        protected async Task<bool> HasRolePermissionAsync(
            string userId,
            UserRole role,
            string permissionName,
            Func<Task<IEnumerable<Role>>> getRoles,
            Func<Task<IEnumerable<Permission>>> getPermissions)
        {
            return await ExecuteWithLoggingAsync(
                $"بررسی دسترسی {permissionName} برای نقش {role} و کاربر {userId}",
                async () =>
                {
                    var hasRole = await HasRoleAsync(userId, role, getRoles);
                    if (!hasRole) return false;

                    return await HasPermissionAsync(userId, permissionName, getPermissions);
                });
        }

        /// <summary>
        /// دریافت تمام دسترسی‌های کاربر
        /// </summary>
        protected async Task<IEnumerable<Permission>> GetUserPermissionsAsync(
            string userId,
            Func<Task<IEnumerable<Permission>>> getPermissions)
        {
            return await ExecuteWithLoggingAsync($"دریافت دسترسی‌های کاربر {userId}", async () =>
            {
                var cacheKey = $"UserPermissions_{userId}";
                return await GetOrSetCacheAsync(cacheKey, getPermissions);
            });
        }

        /// <summary>
        /// دریافت تمام نقش‌های کاربر
        /// </summary>
        protected async Task<IEnumerable<Role>> GetUserRolesAsync(
            string userId,
            Func<Task<IEnumerable<Role>>> getRoles)
        {
            return await ExecuteWithLoggingAsync($"دریافت نقش‌های کاربر {userId}", async () =>
            {
                var cacheKey = $"UserRoles_{userId}";
                return await GetOrSetCacheAsync(cacheKey, getRoles);
            });
        }

        /// <summary>
        /// پاک کردن کش دسترسی‌های کاربر
        /// </summary>
        protected void ClearUserPermissionCache(string userId)
        {
            RemoveFromCache($"UserPermissions_{userId}");
            RemoveFromCache($"UserRoles_{userId}");
        }

        /// <summary>
        /// پاک کردن کش دسترسی‌های تمام کاربران
        /// </summary>
        protected void ClearAllUserPermissionCache()
        {
            RemoveFromCacheByPattern("UserPermissions_");
            RemoveFromCacheByPattern("UserRoles_");
        }
    }
} 