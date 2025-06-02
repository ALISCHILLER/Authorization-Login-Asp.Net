using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس مدیریت دسترسی‌های کاربر
    /// این اینترفیس عملیات مربوط به مدیریت دسترسی‌های کاربر را تعریف می‌کند
    /// </summary>
    public interface IUserAuthorizationService
    {
        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        Task<bool> AssignRoleAsync(Guid userId, string role);

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        Task<bool> RemoveRoleAsync(Guid userId, string role);

        /// <summary>
        /// بررسی دسترسی کاربر به یک نقش
        /// </summary>
        Task<bool> IsInRoleAsync(Guid userId, string role);

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// بررسی دسترسی کاربر به یک پرمیشن
        /// </summary>
        Task<bool> HasPermissionAsync(Guid userId, string permission);

        /// <summary>
        /// دریافت پرمیشن‌های کاربر
        /// </summary>
        Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId);

        /// <summary>
        /// اختصاص پرمیشن به کاربر
        /// </summary>
        Task<bool> AssignPermissionAsync(Guid userId, string permission);

        /// <summary>
        /// حذف پرمیشن از کاربر
        /// </summary>
        Task<bool> RemovePermissionAsync(Guid userId, string permission);
    }
} 