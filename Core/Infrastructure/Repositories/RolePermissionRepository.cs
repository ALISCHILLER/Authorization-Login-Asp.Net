using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
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
    /// پیاده‌سازی مخزن ارتباط نقش-دسترسی
    /// این کلاس عملیات مربوط به ارتباط بین نقش‌ها و دسترسی‌ها را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class RolePermissionRepository : RelationshipRepository<RolePermission, Guid, Role, Permission>, IRolePermissionRepository
    {
        /// <summary>
        /// سازنده کلاس مخزن ارتباط نقش-دسترسی
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        /// <param name="logger">لاگر برای لاگ کردن خطاها</param>
        public RolePermissionRepository(
            ApplicationDbContext context,
            ILogger<RolePermissionRepository> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// دریافت تمام ارتباطات نقش-دسترسی
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست تمام ارتباطات نقش-دسترسی</returns>
        public async Task<IEnumerable<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت ارتباطات یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات نقش مورد نظر</returns>
        public async Task<IEnumerable<RolePermission>> GetByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await GetBySourceAsync(
                roleId,
                rp => rp.RoleId == roleId && !rp.IsDeleted,
                rp => rp.Permission,
                cancellationToken);
        }

        /// <summary>
        /// دریافت ارتباطات یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات دسترسی مورد نظر</returns>
        public async Task<IEnumerable<RolePermission>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await GetByTargetAsync(
                permissionId,
                rp => rp.PermissionId == permissionId && !rp.IsDeleted,
                rp => rp.Role,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود ارتباط بین نقش و دسترسی
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر ارتباط وجود داشته باشد</returns>
        public async Task<bool> HasPermissionAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await HasRelationshipAsync(
                roleId,
                permissionId,
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted,
                cancellationToken);
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>ارتباط ایجاد شده</returns>
        /// <exception cref="InvalidOperationException">در صورت وجود ارتباط تکراری</exception>
        public async Task<bool> AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };

            return await AddRelationshipAsync(
                rolePermission,
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted,
                cancellationToken);
        }

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر عملیات موفقیت‌آمیز باشد</returns>
        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await RemoveRelationshipAsync(
                rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted,
                cancellationToken);
        }

        /// <summary>
        /// حذف تمام دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد دسترسی‌های حذف شده</returns>
        public async Task<int> RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var rolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);

            if (!rolePermissions.Any())
                return 0;

            _dbSet.RemoveRange(rolePermissions);
            return await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// افزودن چند دسترسی به نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionIds">شناسه‌های دسترسی‌ها</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات ایجاد شده</returns>
        /// <exception cref="InvalidOperationException">در صورت وجود دسترسی تکراری</exception>
        public async Task<IEnumerable<RolePermission>> AddPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new ArgumentException("لیست دسترسی‌ها نمی‌تواند خالی باشد", nameof(permissionIds));

            var existingPermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync(cancellationToken);

            if (existingPermissions.Any())
                throw new InvalidOperationException($"دسترسی‌های زیر قبلاً به نقش اضافه شده‌اند: {string.Join(", ", existingPermissions)}");

            var rolePermissions = permissionIds.Select(permissionId => new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _dbSet.AddRangeAsync(rolePermissions, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// حذف چند دسترسی از نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionIds">شناسه‌های دسترسی‌ها</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد دسترسی‌های حذف شده</returns>
        public async Task<int> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default)
        {
            if (permissionIds == null || !permissionIds.Any())
                throw new ArgumentException("لیست دسترسی‌ها نمی‌تواند خالی باشد", nameof(permissionIds));

            var rolePermissions = await _dbSet
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync(cancellationToken);

            if (!rolePermissions.Any())
                return 0;

            _dbSet.RemoveRange(rolePermissions);
            return await SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            var rolePermissions = await GetByRoleAsync(roleId, cancellationToken);
            return rolePermissions.Select(rp => rp.Permission);
        }

        /// <summary>
        /// دریافت نقش‌های یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست نقش‌های دسترسی مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetRolesWithPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            var rolePermissions = await GetByPermissionAsync(permissionId, cancellationToken);
            return rolePermissions.Select(rp => rp.Role);
        }

        /// <summary>
        /// بروزرسانی دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionIds">شناسه‌های دسترسی‌ها</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر عملیات موفقیت‌آمیز باشد</returns>
        public async Task<bool> UpdateRolePermissionsAsync(
            Guid roleId, 
            IEnumerable<Guid> permissionIds, 
            CancellationToken cancellationToken = default)
        {
            return await UpdateRelationshipsAsync(
                roleId,
                permissionIds,
                permissionId => new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                },
                rp => rp.RoleId == roleId && !rp.IsDeleted,
                rp => rp.PermissionId,
                cancellationToken);
        }

        /// <summary>
        /// پاکسازی دسترسی‌های حذف شده نقش‌ها
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد دسترسی‌های حذف شده</returns>
        public async Task<int> CleanupDeletedRolePermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await CleanupDeletedRelationshipsAsync(
                rp => rp.IsDeleted && rp.DeletedAt < DateTime.UtcNow.AddDays(-30),
                cancellationToken);
        }
    }
}