using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس ریپازیتوری برای مدیریت پرمیشن‌ها (Permission)
    /// </summary>
    public interface IPermissionRepository
    {
        /// <summary>
        /// دریافت تمام پرمیشن‌ها
        /// </summary>
        /// <returns>لیست پرمیشن‌ها</returns>
        Task<IEnumerable<Permission>> GetAllAsync();

        /// <summary>
        /// دریافت پرمیشن بر اساس شناسه
        /// </summary>
        /// <param name="id">شناسه پرمیشن</param>
        /// <returns>شیء پرمیشن یا null</returns>
        Task<Permission> GetByIdAsync(Guid id);

        /// <summary>
        /// دریافت پرمیشن‌ها بر اساس نوع پرمیشن (PermissionType)
        /// </summary>
        /// <param name="permissionType">نوع پرمیشن</param>
        /// <returns>لیست پرمیشن‌ها</returns>
        Task<IEnumerable<Permission>> GetByTypeAsync(int permissionType);

        /// <summary>
        /// افزودن پرمیشن جدید
        /// </summary>
        /// <param name="permission">شیء پرمیشن</param>
        /// <returns>تسک</returns>
        Task AddAsync(Permission permission);

        /// <summary>
        /// حذف پرمیشن
        /// </summary>
        /// <param name="permission">شیء پرمیشن</param>
        void Remove(Permission permission);

        /// <summary>
        /// به‌روزرسانی پرمیشن
        /// </summary>
        /// <param name="permission">شیء پرمیشن</param>
        void Update(Permission permission);

        /// <summary>
        /// بررسی وجود پرمیشن با نام مشخص (برای جلوگیری از تکراری بودن)
        /// </summary>
        /// <param name="name">نام پرمیشن</param>
        /// <returns>True اگر وجود داشته باشد</returns>
        Task<bool> ExistsByNameAsync(string name);
        Task<IEnumerable<Permission>?> GetPermissionsByRoleIdAsync(int roleId);
        Task AssignPermissionToRoleAsync(int roleId, int permissionId);
        Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
    }
}
