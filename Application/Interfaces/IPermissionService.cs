using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت دسترسی‌ها
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// بررسی دسترسی کاربر به یک منبع
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string resource, string action);

        /// <summary>
        /// دریافت لیست دسترسی‌های کاربر
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// افزودن دسترسی به کاربر
        /// </summary>
        Task AddPermissionAsync(Guid userId, string resource, string action);

        /// <summary>
        /// حذف دسترسی از کاربر
        /// </summary>
        Task RemovePermissionAsync(Guid userId, string resource, string action);

        /// <summary>
        /// بررسی دسترسی‌های نقش
        /// </summary>
        Task<bool> HasRolePermissionAsync(string role, string resource, string action);

        /// <summary>
        /// دریافت لیست دسترسی‌های نقش
        /// </summary>
        Task<IEnumerable<string>> GetRolePermissionsAsync(string role);
    }
} 