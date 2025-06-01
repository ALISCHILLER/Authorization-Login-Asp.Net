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
    /// پیاده‌سازی مخزن ارتباط نقش-دسترسی
    /// این کلاس عملیات مربوط به ارتباط بین نقش‌ها و دسترسی‌ها را در پایگاه داده پیاده‌سازی می‌کند
    /// </summary>
    public class RolePermissionRepository : Repository<RolePermission>, IRolePermissionRepository
    {
        /// <summary>
        /// سازنده کلاس مخزن ارتباط نقش-دسترسی
        /// </summary>
        /// <param name="context">کانتکست پایگاه داده</param>
        public RolePermissionRepository(AppDbContext context) : base(context)
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
        public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت ارتباطات یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات دسترسی مورد نظر</returns>
        public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی وجود ارتباط بین نقش و دسترسی
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر ارتباط وجود داشته باشد</returns>
        public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
        }

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>ارتباط ایجاد شده</returns>
        /// <exception cref="InvalidOperationException">در صورت وجود ارتباط تکراری</exception>
        public async Task<RolePermission> AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (await ExistsAsync(roleId, permissionId, cancellationToken))
                throw new InvalidOperationException("این دسترسی قبلاً به نقش اضافه شده است");

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId
            };

            await _dbSet.AddAsync(rolePermission, cancellationToken);
            await SaveChangesAsync(cancellationToken);

            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);
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
            var rolePermission = await _dbSet
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId, cancellationToken);

            if (rolePermission == null)
                return false;

            _dbSet.Remove(rolePermission);
            return await SaveChangesAsync(cancellationToken) > 0;
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
                PermissionId = permissionId
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
            return await _dbSet
                .Where(rp => rp.RoleId == roleId)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// دریافت نقش‌های یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست نقش‌های دسترسی مورد نظر</returns>
        public async Task<IEnumerable<Role>> GetPermissionRolesAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(rp => rp.PermissionId == permissionId)
                .Include(rp => rp.Role)
                .Select(rp => rp.Role)
                .ToListAsync(cancellationToken);
        }
    }
}