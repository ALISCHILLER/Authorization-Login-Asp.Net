using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Interfaces
{
    /// <summary>
    /// Interface برای سرویس مدیریت هویت
    /// </summary>
    public interface IIdentityService
    {
        #region احراز هویت
        /// <summary>
        /// بررسی اعتبار رمز عبور
        /// </summary>
        Task<bool> ValidatePasswordAsync(User user, string password);

        /// <summary>
        /// ایجاد رمز عبور جدید
        /// </summary>
        Task<string> GeneratePasswordHashAsync(string password);

        /// <summary>
        /// بررسی وجود کاربر با نام کاربری یا ایمیل
        /// </summary>
        Task<bool> IsUserExistsAsync(string usernameOrEmail);

        /// <summary>
        /// احراز هویت کاربر
        /// </summary>
        Task<(bool Success, User User, string Error)> AuthenticateAsync(string usernameOrEmail, string password);

        /// <summary>
        /// بررسی نیاز به احراز هویت دو مرحله‌ای
        /// </summary>
        Task<bool> RequiresTwoFactorAsync(User user);
        #endregion

        #region مدیریت کاربر
        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        Task<(bool Success, User User, string Error)> CreateUserAsync(
            string username,
            string email,
            string password,
            string firstName,
            string lastName,
            bool isActive = true);

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        Task<(bool Success, string Error)> UpdateUserAsync(User user);

        /// <summary>
        /// غیرفعال کردن کاربر
        /// </summary>
        Task<(bool Success, string Error)> DeactivateUserAsync(User user);

        /// <summary>
        /// فعال کردن کاربر
        /// </summary>
        Task<(bool Success, string Error)> ActivateUserAsync(User user);

        /// <summary>
        /// حذف کاربر
        /// </summary>
        Task<(bool Success, string Error)> DeleteUserAsync(User user);

        /// <summary>
        /// دریافت کاربر با شناسه
        /// </summary>
        Task<User> GetUserByIdAsync(Guid userId);

        /// <summary>
        /// دریافت کاربر با نام کاربری یا ایمیل
        /// </summary>
        Task<User> GetUserByUsernameOrEmailAsync(string usernameOrEmail);
        #endregion

        #region مدیریت رمز عبور
        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        Task<(bool Success, string Error)> ChangePasswordAsync(User user, string currentPassword, string newPassword);

        /// <summary>
        /// بازنشانی رمز عبور
        /// </summary>
        Task<(bool Success, string Error)> ResetPasswordAsync(User user, string newPassword);

        /// <summary>
        /// ایجاد توکن بازنشانی رمز عبور
        /// </summary>
        Task<string> GeneratePasswordResetTokenAsync(User user);

        /// <summary>
        /// بررسی اعتبار توکن بازنشانی رمز عبور
        /// </summary>
        Task<bool> ValidatePasswordResetTokenAsync(User user, string token);
        #endregion

        #region مدیریت ایمیل
        /// <summary>
        /// تأیید ایمیل
        /// </summary>
        Task<(bool Success, string Error)> ConfirmEmailAsync(User user, string token);

        /// <summary>
        /// ایجاد توکن تأیید ایمیل
        /// </summary>
        Task<string> GenerateEmailConfirmationTokenAsync(User user);

        /// <summary>
        /// تغییر ایمیل
        /// </summary>
        Task<(bool Success, string Error)> ChangeEmailAsync(User user, string newEmail, string token);
        #endregion

        #region مدیریت نقش‌ها
        /// <summary>
        /// افزودن کاربر به نقش
        /// </summary>
        Task<(bool Success, string Error)> AddToRoleAsync(User user, string roleName);

        /// <summary>
        /// حذف کاربر از نقش
        /// </summary>
        Task<(bool Success, string Error)> RemoveFromRoleAsync(User user, string roleName);

        /// <summary>
        /// دریافت نقش‌های کاربر
        /// </summary>
        Task<IList<string>> GetRolesAsync(User user);

        /// <summary>
        /// بررسی عضویت کاربر در نقش
        /// </summary>
        Task<bool> IsInRoleAsync(User user, string roleName);
        #endregion

        #region قفل کردن حساب
        /// <summary>
        /// قفل کردن حساب کاربری
        /// </summary>
        Task<(bool Success, string Error)> LockoutAsync(User user, DateTimeOffset? endDate = null);

        /// <summary>
        /// باز کردن قفل حساب کاربری
        /// </summary>
        Task<(bool Success, string Error)> UnlockAsync(User user);

        /// <summary>
        /// بررسی قفل بودن حساب کاربری
        /// </summary>
        Task<bool> IsLockedOutAsync(User user);
        #endregion
    }
} 