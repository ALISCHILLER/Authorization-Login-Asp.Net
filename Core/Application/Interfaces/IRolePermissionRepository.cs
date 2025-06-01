using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مخزن ارتباط نقش-دسترسی
    /// این اینترفیس عملیات مربوط به ارتباط بین نقش‌ها و دسترسی‌ها را در پایگاه داده تعریف می‌کند
    /// </summary>
    public interface IRolePermissionRepository : IRepository<RolePermission>
    {
        /// <summary>
        /// دریافت تمام ارتباطات نقش-دسترسی
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست تمام ارتباطات نقش-دسترسی</returns>
        Task<IEnumerable<RolePermission>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت ارتباطات یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات نقش مورد نظر</returns>
        Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت ارتباطات یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات دسترسی مورد نظر</returns>
        Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود ارتباط بین نقش و دسترسی
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر ارتباط وجود داشته باشد</returns>
        Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// افزودن دسترسی به نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>ارتباط ایجاد شده</returns>
        Task<RolePermission> AddPermissionToRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف دسترسی از نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر عملیات موفقیت‌آمیز باشد</returns>
        Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف تمام دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد دسترسی‌های حذف شده</returns>
        Task<int> RemoveAllPermissionsFromRoleAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// افزودن چند دسترسی به نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionIds">شناسه‌های دسترسی‌ها</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست ارتباطات ایجاد شده</returns>
        Task<IEnumerable<RolePermission>> AddPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف چند دسترسی از نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="permissionIds">شناسه‌های دسترسی‌ها</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد دسترسی‌های حذف شده</returns>
        Task<int> RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های یک نقش
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های یک دسترسی
        /// </summary>
        /// <param name="permissionId">شناسه دسترسی</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست نقش‌های دسترسی مورد نظر</returns>
        Task<IEnumerable<Role>> GetPermissionRolesAsync(Guid permissionId, CancellationToken cancellationToken = default);
    }
}
