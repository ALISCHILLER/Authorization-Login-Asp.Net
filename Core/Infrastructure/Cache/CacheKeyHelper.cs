using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Cache
{
    /// <summary>
    /// کلاس کمکی برای مدیریت کلیدهای کش
    /// </summary>
    public static class CacheKeyHelper
    {
        /// <summary>
        /// دریافت کلیدهای کش مرتبط با یک موجودیت
        /// </summary>
        public static IEnumerable<string> GetEntityCacheKeys<T>(
            Guid id,
            string name,
            bool includeRelated = true) where T : class
        {
            var keys = new List<string>
            {
                CacheKeys.GetEntityKey<T>(id),
                CacheKeys.GetEntityByNameKey<T>(name)
            };

            if (includeRelated)
            {
                keys.AddRange(GetRelatedCacheKeys<T>(id));
            }

            return keys;
        }

        /// <summary>
        /// دریافت کلیدهای کش مرتبط با یک موجودیت
        /// </summary>
        private static IEnumerable<string> GetRelatedCacheKeys<T>(Guid id) where T : class
        {
            return typeof(T).Name switch
            {
                nameof(Role) => new[]
                {
                    CacheKeys.RolePermissions(id),
                    CacheKeys.UserRoles(),
                    CacheKeys.ActiveRoles(),
                    CacheKeys.SystemRoles(),
                    CacheKeys.UserRoles()
                },
                nameof(Permission) => new[]
                {
                    CacheKeys.RolePermissions(id),
                    CacheKeys.UserPermissions(id),
                    CacheKeys.ActivePermissions(),
                    CacheKeys.AllPermissions()
                },
                nameof(User) => new[]
                {
                    CacheKeys.UserRoles(id),
                    CacheKeys.UserPermissions(id),
                    CacheKeys.UserLoginHistory(id)
                },
                _ => Array.Empty<string>()
            };
        }

        /// <summary>
        /// دریافت کلیدهای کش برای لیست‌های عمومی
        /// </summary>
        public static IEnumerable<string> GetListCacheKeys<T>() where T : class
        {
            return typeof(T).Name switch
            {
                nameof(Role) => new[]
                {
                    CacheKeys.AllRoles(),
                    CacheKeys.ActiveRoles(),
                    CacheKeys.SystemRoles(),
                    CacheKeys.UserRoles()
                },
                nameof(Permission) => new[]
                {
                    CacheKeys.AllPermissions(),
                    CacheKeys.ActivePermissions()
                },
                _ => Array.Empty<string>()
            };
        }
    }
} 