using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Interfaces
{
    /// <summary>
    /// رابط دسترسی به پایگاه داده برنامه
    /// این رابط برای مدیریت دسترسی به جداول پایگاه داده و عملیات‌های پایه استفاده می‌شود
    /// </summary>
    public interface IApplicationDbContext
    {
        /// <summary>
        /// دسترسی به جدول کاربران
        /// </summary>
        DbSet<User> Users { get; }

        /// <summary>
        /// دسترسی به جدول نقش‌ها
        /// </summary>
        DbSet<Role> Roles { get; }

        /// <summary>
        /// دسترسی به جدول ارتباط کاربران و نقش‌ها
        /// </summary>
        DbSet<UserRole> UserRoles { get; }

        /// <summary>
        /// دسترسی به جدول دسترسی‌ها
        /// </summary>
        DbSet<Permission> Permissions { get; }

        /// <summary>
        /// دسترسی به جدول ارتباط نقش‌ها و دسترسی‌ها
        /// </summary>
        DbSet<RolePermission> RolePermissions { get; }

        /// <summary>
        /// دسترسی به جدول تاریخچه ورود
        /// </summary>
        DbSet<LoginHistory> LoginHistories { get; }

        /// <summary>
        /// دسترسی به جدول دستگاه‌های کاربر
        /// </summary>
        DbSet<UserDevice> UserDevices { get; }

        /// <summary>
        /// دسترسی به جدول توکن‌های رفرش
        /// </summary>
        DbSet<RefreshToken> RefreshTokens { get; }

        /// <summary>
        /// دسترسی به جدول کدهای بازیابی احراز هویت دو مرحله‌ای
        /// </summary>
        DbSet<TwoFactorRecoveryCode> TwoFactorRecoveryCodes { get; }

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>تعداد رکوردهای تأثیرپذیر</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// شروع یک تراکنش جدید
        /// </summary>
        /// <returns>تراکنش جدید</returns>
        Task<IDbContextTransaction> BeginTransactionAsync();

        /// <summary>
        /// بررسی اتصال به پایگاه داده
        /// </summary>
        /// <returns>آیا اتصال برقرار است؟</returns>
        Task<bool> CanConnectAsync();
    }
} 