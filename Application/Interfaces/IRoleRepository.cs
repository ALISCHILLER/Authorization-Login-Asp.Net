using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مخزن نقش‌ها
    /// این اینترفیس عملیات مربوط به نقش‌ها را در پایگاه داده تعریف می‌کند
    /// </summary>
    public interface IRoleRepository : IRepository<Role>
    {
        /// <summary>
        /// دریافت تمام نقش‌ها به صورت لیست
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>لیست نقش‌ها</returns>
        Task<IEnumerable<Role>> GetAllAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت یک نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>نقش یا null اگر یافت نشد</returns>
        Task<Role> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش با نام مشخص
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>نقش مورد نظر در صورت وجود</returns>
        Task<Role> GetByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// افزودن یک نقش جدید
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task AddAsync(Role role, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task DeleteAsync(Role role, CancellationToken cancellationToken = default);

        /// <summary>
        /// بروزرسانی اطلاعات نقش
        /// </summary>
        /// <param name="role">شیء نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task UpdateAsync(Role role, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف نقش بر اساس آی‌دی
        /// </summary>
        /// <param name="id">آی‌دی نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تسک غیرهمزمان</returns>
        Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود نقش بر اساس نام
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>درست اگر نقش وجود داشته باشد</returns>
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// ذخیره تغییرات
        /// </summary>
        /// <param name="cancellationToken">امکان لغو عملیات</param>
        /// <returns>تعداد رکوردهای تغییر یافته</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های یک نوع خاص
        /// </summary>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های نوع مورد نظر</returns>
        Task<IEnumerable<Role>> GetByTypeAsync(RoleType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های فعال
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های فعال</returns>
        Task<IEnumerable<Role>> GetActiveAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های یک کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر مورد نظر</returns>
        Task<IEnumerable<Role>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود نقش در یک نوع خاص
        /// </summary>
        /// <param name="name">نام نقش</param>
        /// <param name="type">نوع نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر نقش در نوع مورد نظر وجود داشته باشد</returns>
        Task<bool> ExistsInTypeAsync(string name, RoleType type, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های سیستمی
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های سیستمی</returns>
        Task<IEnumerable<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های کاربری
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربری</returns>
        Task<IEnumerable<Role>> GetUserRolesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت نقش‌های یک کاربر به همراه دسترسی‌های آن‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>لیست نقش‌های کاربر با دسترسی‌های آن‌ها</returns>
        Task<IEnumerable<Role>> GetUserRolesWithPermissionsAsync(string userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی تعلق کاربر به یک نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به نقش تعلق داشته باشد</returns>
        Task<bool> IsUserInRoleAsync(string userId, string roleName, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی تعلق کاربر به چند نقش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به همه نقش‌ها تعلق داشته باشد</returns>
        Task<bool> IsUserInAllRolesAsync(string userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی تعلق کاربر به حداقل یکی از نقش‌ها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleNames">نام‌های نقش‌ها</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>درست اگر کاربر به حداقل یکی از نقش‌ها تعلق داشته باشد</returns>
        Task<bool> IsUserInAnyRoleAsync(string userId, IEnumerable<string> roleNames, CancellationToken cancellationToken = default);
    }
}
