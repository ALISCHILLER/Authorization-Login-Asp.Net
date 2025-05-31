using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مخزن دسترسی‌ها
    /// این اینترفیس عملیات مربوط به دسترسی‌ها را در پایگاه داده تعریف می‌کند
    /// </summary>
    public interface IPermissionRepository : IRepository<Permission>
    {
        /// <summary>
        /// دریافت دسترسی با نام مشخص
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>دسترسی مورد نظر در صورت وجود</returns>
        Task<Permission> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های یک گروه خاص
        /// </summary>
        /// <param name="group">نام گروه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های گروه مورد نظر</returns>
        Task<IEnumerable<Permission>> GetByGroupAsync(string group, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های یک نوع خاص
        /// </summary>
        /// <param name="type">نوع دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نوع مورد نظر</returns>
        Task<IEnumerable<Permission>> GetByTypeAsync(PermissionType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های فعال
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های فعال</returns>
        Task<IEnumerable<Permission>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های یک نقش خاص
        /// </summary>
        /// <param name="roleId">شناسه نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش مورد نظر</returns>
        Task<IEnumerable<Permission>> GetByRoleIdAsync(string roleId, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های چند نقش
        /// </summary>
        /// <param name="roleIds">شناسه‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های نقش‌های مورد نظر</returns>
        Task<IEnumerable<Permission>> GetByRoleIdsAsync(IEnumerable<string> roleIds, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود دسترسی با نام مشخص
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر دسترسی وجود داشته باشد</returns>
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود دسترسی در یک گروه
        /// </summary>
        /// <param name="name">نام دسترسی</param>
        /// <param name="group">نام گروه</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر دسترسی در گروه وجود داشته باشد</returns>
        Task<bool> ExistsInGroupAsync(string name, string group, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تمام گروه‌های دسترسی
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نام گروه‌های دسترسی</returns>
        Task<IEnumerable<string>> GetAllGroupsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت تعداد دسترسی‌های هر گروه
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>دیکشنری شامل نام گروه و تعداد دسترسی‌های آن</returns>
        Task<IDictionary<string, int>> GetGroupCountsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت دسترسی‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست دسترسی‌های کاربر مورد نظر</returns>
        Task<IEnumerable<Permission>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی دسترسی کاربر به یک عملیات خاص
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionName">نام دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر دسترسی داشته باشد</returns>
        Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی دسترسی کاربر به چند عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه دسترسی‌ها دسترسی داشته باشد</returns>
        Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی دسترسی کاربر به حداقل یکی از عملیات
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="permissionNames">نام‌های دسترسی</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از دسترسی‌ها دسترسی داشته باشد</returns>
        Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default);
        Task<IEnumerable<Permission>?> GetPermissionsByRoleIdAsync(int roleId);
    }
}
