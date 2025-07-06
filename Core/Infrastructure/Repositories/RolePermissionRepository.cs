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
            var rolePermission = RolePermission.Create(roleId, permissionId);
            await _dbSet.AddAsync(rolePermission, cancellationToken);
            return await _context.SaveChangesAsync(cancellationToken) > 0;
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
            var entity = await _dbSet.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted, cancellationToken);
            if (entity == null) return false;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }

        // --- Implementation of IRolePermissionRepository (Domain/Interfaces) ---
        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId)
        {
            return await _dbSet
                .Where(rp => rp.RoleId == roleId && !rp.IsDeleted)
                .Include(rp => rp.Permission)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionName)
        {
            return await _dbSet
                .Include(rp => rp.Permission)
                .AnyAsync(rp => rp.RoleId == roleId && !rp.IsDeleted && rp.Permission.Name == permissionName);
        }

        public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            if (await _dbSet.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted))
                return;
            var rolePermission = RolePermission.Create(roleId, permissionId);
            await _dbSet.AddAsync(rolePermission);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId && !rp.IsDeleted);
            if (entity == null) return;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(string permissionName)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .Where(rp => !rp.IsDeleted && rp.Permission.Name == permissionName)
                .Select(rp => rp.Role)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(Guid permissionId)
        {
            return await _dbSet
                .Include(rp => rp.Role)
                .Where(rp => !rp.IsDeleted && rp.PermissionId == permissionId)
                .Select(rp => rp.Role)
                .ToListAsync();
        }

        // حذف کامل متدهای قدیمی و ناسازگار (مانند RemoveAllPermissionsFromRoleAsync، AddPermissionsToRoleAsync، RemovePermissionsFromRoleAsync، UpdateRolePermissionsAsync، CleanupDeletedRolePermissionsAsync و ...)
        // فقط متدهای اینترفیس باقی بماند (در بالا پیاده‌سازی شد)
    }
}