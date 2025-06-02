using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس اصلی سرویس کاربر
    /// این اینترفیس عملیات اصلی مدیریت کاربر را تعریف می‌کند
    /// </summary>
    public interface IUserService
    {
        #region عملیات اصلی کاربر
        /// <summary>
        /// دریافت کاربر بر اساس شناسه
        /// </summary>
        Task<User> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کاربر بر اساس ایمیل
        /// </summary>
        Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت کاربر بر اساس نام کاربری
        /// </summary>
        Task<User> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// دریافت لیست کاربران با فیلتر و صفحه‌بندی
        /// </summary>
        Task<(IEnumerable<User> Users, int TotalCount)> GetUsersAsync(
            UserFilterDto filter,
            int pageNumber = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        Task<User> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// حذف کاربر
        /// </summary>
        Task<bool> DeleteUserAsync(Guid id, CancellationToken cancellationToken = default);
        #endregion

        #region مدیریت وضعیت حساب کاربری
        /// <summary>
        /// فعال‌سازی حساب کاربری
        /// </summary>
        Task<bool> ActivateAccountAsync(Guid userId);

        /// <summary>
        /// غیرفعال‌سازی حساب کاربری
        /// </summary>
        Task<bool> DeactivateAccountAsync(Guid userId);

        /// <summary>
        /// قفل کردن حساب کاربری
        /// </summary>
        Task<bool> LockAccountAsync(Guid userId, TimeSpan? duration = null);

        /// <summary>
        /// باز کردن قفل حساب کاربری
        /// </summary>
        Task<bool> UnlockAccountAsync(Guid userId);

        /// <summary>
        /// بررسی وضعیت قفل بودن حساب
        /// </summary>
        Task<bool> IsAccountLockedAsync(Guid userId);
        #endregion

        #region بررسی‌های وجود کاربر
        /// <summary>
        /// بررسی وجود کاربر با ایمیل
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری
        /// </summary>
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);

        /// <summary>
        /// بررسی وجود کاربر با شماره تلفن
        /// </summary>
        Task<bool> ExistsByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);
        #endregion
    }
} 