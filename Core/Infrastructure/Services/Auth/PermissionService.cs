using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Auth
{
    /// <summary>
    /// سرویس یکپارچه مدیریت دسترسی‌ها
    /// این سرویس شامل تمام عملیات مربوط به مدیریت دسترسی‌ها و نقش‌های کاربران است
    /// </summary>
    public class PermissionService : BasePermissionService, IPermissionService
    {
        private readonly AppDbContext _context;

        public PermissionService(
            ILogger<PermissionService> logger,
            IMemoryCache cache,
            AppDbContext context)
            : base(logger, cache)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک عملیات خاص
        /// </summary>
        public async Task<bool> HasPermissionAsync(string userId, string permissionName)
        {
            return await base.HasPermissionAsync(userId, permissionName, async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return new List<Permission>();

                var permissions = new List<Permission>();
                foreach (var role in user.Roles)
                {
                    permissions.AddRange(role.Permissions);
                }
                return permissions;
            });
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش خاص
        /// </summary>
        public async Task<bool> HasRoleAsync(string userId, UserRole role)
        {
            return await base.HasRoleAsync(userId, role, async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return new List<Role>();

                return user.Roles;
            });
        }

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش و عملیات خاص
        /// </summary>
        public async Task<bool> HasRolePermissionAsync(string userId, UserRole role, string permissionName)
        {
            return await base.HasRolePermissionAsync(userId, role, permissionName,
                async () =>
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return new List<Role>();

                    return user.Roles;
                },
                async () =>
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null) return new List<Permission>();

                    var permissions = new List<Permission>();
                    foreach (var userRole in user.Roles)
                    {
                        permissions.AddRange(userRole.Permissions);
                    }
                    return permissions;
                });
        }

        /// <summary>
        /// دریافت تمام دسترسی‌های کاربر
        /// </summary>
        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(string userId)
        {
            return await base.GetUserPermissionsAsync(userId, async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return new List<Permission>();

                var permissions = new List<Permission>();
                foreach (var role in user.Roles)
                {
                    permissions.AddRange(role.Permissions);
                }
                return permissions;
            });
        }

        /// <summary>
        /// دریافت تمام نقش‌های کاربر
        /// </summary>
        public async Task<IEnumerable<Role>> GetUserRolesAsync(string userId)
        {
            return await base.GetUserRolesAsync(userId, async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return new List<Role>();

                return user.Roles;
            });
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        public async Task AddPermissionToRoleAsync(string roleId, string permissionName)
        {
            await ExecuteWithLoggingAsync($"افزودن دسترسی {permissionName} به نقش {roleId}", async () =>
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    throw new DomainException($"نقش با شناسه {roleId} یافت نشد");
                }

                var permission = await _context.Permissions.FirstOrDefaultAsync(p => p.Name == permissionName);
                if (permission == null)
                {
                    throw new DomainException($"دسترسی {permissionName} یافت نشد");
                }

                if (!role.Permissions.Contains(permission))
                {
                    role.Permissions.Add(permission);
                    await _context.SaveChangesAsync();
                    ClearAllUserPermissionCache();
                }
            });
        }

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        public async Task RemovePermissionFromRoleAsync(string roleId, string permissionName)
        {
            await ExecuteWithLoggingAsync($"حذف دسترسی {permissionName} از نقش {roleId}", async () =>
            {
                var role = await _context.Roles.FindAsync(roleId);
                if (role == null)
                {
                    throw new DomainException($"نقش با شناسه {roleId} یافت نشد");
                }

                var permission = role.Permissions.FirstOrDefault(p => p.Name == permissionName);
                if (permission != null)
                {
                    role.Permissions.Remove(permission);
                    await _context.SaveChangesAsync();
                    ClearAllUserPermissionCache();
                }
            });
        }

        /// <summary>
        /// افزودن نقش به کاربر
        /// </summary>
        public async Task AddRoleToUserAsync(string userId, UserRole role)
        {
            await ExecuteWithLoggingAsync($"افزودن نقش {role} به کاربر {userId}", async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new DomainException($"کاربر با شناسه {userId} یافت نشد");
                }

                var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Name == role.ToString());
                if (roleEntity == null)
                {
                    throw new DomainException($"نقش {role} یافت نشد");
                }

                if (!user.Roles.Contains(roleEntity))
                {
                    user.Roles.Add(roleEntity);
                    await _context.SaveChangesAsync();
                    ClearUserPermissionCache(userId);
                }
            });
        }

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        public async Task RemoveRoleFromUserAsync(string userId, UserRole role)
        {
            await ExecuteWithLoggingAsync($"حذف نقش {role} از کاربر {userId}", async () =>
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    throw new DomainException($"کاربر با شناسه {userId} یافت نشد");
                }

                var roleEntity = user.Roles.FirstOrDefault(r => r.Name == role.ToString());
                if (roleEntity != null)
                {
                    user.Roles.Remove(roleEntity);
                    await _context.SaveChangesAsync();
                    ClearUserPermissionCache(userId);
                }
            });
        }
    }
} 