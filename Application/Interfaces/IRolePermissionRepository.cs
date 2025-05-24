using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس ریپازیتوری برای مدیریت ارتباط نقش‌ها و پرمیشن‌ها (RolePermission)
    /// </summary>
    public interface IRolePermissionRepository
    {
        /// <summary>
        /// دریافت تمام RolePermissionها
        /// </summary>
        /// <returns>لیست RolePermissionها</returns>
        Task<IEnumerable<RolePermission>> GetAllAsync();

        /// <summary>
        /// دریافت RolePermission بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه RolePermission</param>
        /// <returns>شیء RolePermission یا null</returns>
        Task<RolePermission> GetByIdAsync(Guid id);

        /// <summary>
        /// دریافت RolePermissionها بر اساس شناسه نقش (RoleId)
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <returns>لیست RolePermissionهای مرتبط</returns>
        Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId);

        /// <summary>
        /// دریافت RolePermissionها بر اساس شناسه پرمیشن (PermissionId)
        /// </summary>
        /// <param name="permissionId">شناسه پرمیشن</param>
        /// <returns>لیست RolePermissionهای مرتبط</returns>
        Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId);

        /// <summary>
        /// افزودن RolePermission جدید
        /// </summary>
        /// <param name="rolePermission">شیء RolePermission</param>
        /// <returns>تسک</returns>
        Task AddAsync(RolePermission rolePermission);

        /// <summary>
        /// حذف RolePermission
        /// </summary>
        /// <param name="rolePermission">شیء RolePermission</param>
        void Remove(RolePermission rolePermission);

        /// <summary>
        /// به‌روزرسانی RolePermission
        /// </summary>
        /// <param name="rolePermission">شیء RolePermission</param>
        void Update(RolePermission rolePermission);

        /// <summary>
        /// بررسی وجود RolePermission با ترکیب RoleId و PermissionId (برای جلوگیری از تکراری بودن)
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه پرمیشن</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsAsync(Guid roleId, Guid permissionId);
    }
}
