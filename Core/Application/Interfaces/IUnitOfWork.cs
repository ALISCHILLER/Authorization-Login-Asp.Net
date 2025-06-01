using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس واحد کاری (Unit of Work) برای مدیریت تراکنش و ذخیره تغییرات در دیتابیس
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// ریپازیتوری کاربر
        /// </summary>
        IUserRepository Users { get; }

        /// <summary>
        /// ریپازیتوری نقش‌ها
        /// </summary>
        IRoleRepository Roles { get; }

        /// <summary>
        /// ریپازیتوری پرمیشن‌ها
        /// </summary>
        IPermissionRepository Permissions { get; }

        /// <summary>
        /// ریپازیتوری ارتباط نقش و پرمیشن
        /// </summary>
        IRolePermissionRepository RolePermissions { get; }

        /// <summary>
        /// ریپازیتوری رفرش توکن‌ها
        /// </summary>
        IRefreshTokenRepository RefreshTokens { get; }

        /// <summary>
        /// ذخیره تغییرات به صورت async (commit تراکنش)
        /// </summary>
        /// <returns>تعداد رکوردهای تغییر یافته</returns>
        Task<int> SaveChangesAsync();
    }
}
