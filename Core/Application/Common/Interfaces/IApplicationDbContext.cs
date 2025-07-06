using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Interfaces
{
    /// <summary>
    /// رابط دسترسی به پایگاه داده برنامه
    /// این رابط برای مدیریت دسترسی به جداول پایگاه داده و عملیات‌های پایه استفاده می‌شود
    /// </summary>
    public interface IApplicationDbContext
    {
        #region جداول پایگاه داده
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
        /// دسترسی به جدول نشست‌های کاربر
        /// </summary>
        DbSet<UserSession> UserSessions { get; }

        /// <summary>
        /// دسترسی به جدول تاریخچه تغییرات
        /// </summary>
        DbSet<AuditLog> AuditLogs { get; }

        /// <summary>
        /// دسترسی به جدول تنظیمات برنامه
        /// </summary>
        DbSet<AppSetting> AppSettings { get; }

        /// <summary>
        /// دسترسی به جدول اعلان‌ها
        /// </summary>
        DbSet<Notification> Notifications { get; }
        #endregion

        #region عملیات پایگاه داده
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
        Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی اتصال به پایگاه داده
        /// </summary>
        /// <returns>آیا اتصال برقرار است؟</returns>
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// اجرای یک کوئری SQL خام
        /// </summary>
        /// <typeparam name="T">نوع نتیجه</typeparam>
        /// <param name="sql">کوئری SQL</param>
        /// <param name="parameters">پارامترهای کوئری</param>
        /// <returns>نتیجه کوئری</returns>
        Task<T> ExecuteSqlRawAsync<T>(string sql, object[] parameters = null);

        /// <summary>
        /// اجرای یک کوئری SQL خام بدون نتیجه
        /// </summary>
        /// <param name="sql">کوئری SQL</param>
        /// <param name="parameters">پارامترهای کوئری</param>
        /// <returns>تعداد رکوردهای تأثیرپذیر</returns>
        Task<int> ExecuteSqlRawNonQueryAsync(string sql, object[] parameters = null);

        /// <summary>
        /// دریافت اطلاعات پایگاه داده
        /// </summary>
        DatabaseFacade Database { get; }
        #endregion

        #region مدیریت تغییرات
        /// <summary>
        /// ثبت تغییرات در تاریخچه
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="action">عملیات انجام شده</param>
        /// <param name="entityName">نام موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <param name="changes">تغییرات اعمال شده</param>
        Task LogChangesAsync(Guid userId, string action, string entityName, string entityId, string changes);

        /// <summary>
        /// دریافت تاریخچه تغییرات یک موجودیت
        /// </summary>
        /// <param name="entityName">نام موجودیت</param>
        /// <param name="entityId">شناسه موجودیت</param>
        /// <returns>لیست تغییرات</returns>
        Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityName, string entityId);
        #endregion
    }

    /// <summary>
    /// رابط تراکنش پایگاه داده
    /// </summary>
    public interface IDbContextTransaction : IDisposable, IAsyncDisposable
    {
        /// <summary>
        /// شناسه تراکنش
        /// </summary>
        Guid TransactionId { get; }

        /// <summary>
        /// تأیید تراکنش
        /// </summary>
        void Commit();

        /// <summary>
        /// تأیید تراکنش به صورت ناهمزمان
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// برگرداندن تراکنش
        /// </summary>
        void Rollback();

        /// <summary>
        /// برگرداندن تراکنش به صورت ناهمزمان
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// ایجاد نقطه ذخیره‌سازی
        /// </summary>
        void CreateSavepoint(string name);

        /// <summary>
        /// ایجاد نقطه ذخیره‌سازی به صورت ناهمزمان
        /// </summary>
        Task CreateSavepointAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// برگرداندن به نقطه ذخیره‌سازی
        /// </summary>
        void RollbackToSavepoint(string name);

        /// <summary>
        /// برگرداندن به نقطه ذخیره‌سازی به صورت ناهمزمان
        /// </summary>
        Task RollbackToSavepointAsync(string name, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف نقطه ذخیره‌سازی
        /// </summary>
        void ReleaseSavepoint(string name);

        /// <summary>
        /// حذف نقطه ذخیره‌سازی به صورت ناهمزمان
        /// </summary>
        Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default);
    }
} 