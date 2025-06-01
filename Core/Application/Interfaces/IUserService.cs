using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس کاربر برای انجام عملیات کسب‌وکار مرتبط با کاربران
    /// </summary>
    public interface IUserService
    {
        #region احراز هویت و مجوزدهی
        /// <summary>
        /// ثبت‌نام کاربر جدید
        /// </summary>
        /// <param name="request">مدل درخواست ثبت‌نام</param>
        /// <returns>اطلاعات احراز هویت (توکن‌ها و کاربر)</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// ورود کاربر و دریافت توکن احراز هویت
        /// </summary>
        /// <param name="request">مدل درخواست ورود</param>
        /// <param name="cancellationToken">توکن لغو عملیات</param>
        /// <returns>اطلاعات احراز هویت شامل توکن</returns>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// تأیید کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="model">مدل احراز هویت دو مرحله‌ای</param>
        /// <returns>نتیجه تأیید</returns>
        Task<AuthResult> ValidateTwoFactorAsync(TwoFactorDto model);

        /// <summary>
        /// تمدید توکن با استفاده از توکن رفرش
        /// </summary>
        /// <param name="model">مدل توکن رفرش</param>
        /// <returns>نتیجه تمدید توکن</returns>
        Task<AuthResult> RefreshTokenAsync(RefreshTokenDto model);

        /// <summary>
        /// خروج کاربر و لغو توکن رفرش
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task LogoutAsync(Guid userId);

        /// <summary>
        /// بررسی اعتبارنامه‌های کاربر
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <param name="password">رمز عبور</param>
        /// <returns>نتیجه بررسی اعتبارنامه‌ها</returns>
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        #endregion

        #region مدیریت کاربران
        /// <summary>
        /// دریافت کاربر با شناسه
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>اطلاعات کاربر</returns>
        Task<User> GetByIdAsync(Guid userId);

        /// <summary>
        /// دریافت کاربر با ایمیل
        /// </summary>
        /// <param name="email">ایمیل کاربر</param>
        /// <returns>اطلاعات کاربر</returns>
        Task<User> GetByEmailAsync(string email);

        /// <summary>
        /// دریافت کاربر با نام کاربری
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <returns>اطلاعات کاربر</returns>
        Task<User> GetByUsernameAsync(string username);

        /// <summary>
        /// دریافت لیست تمام کاربران
        /// </summary>
        /// <returns>لیست کاربران</returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// ایجاد کاربر جدید
        /// </summary>
        /// <param name="user">اطلاعات کاربر</param>
        /// <returns>اطلاعات کاربر جدید</returns>
        Task<User> CreateAsync(User user);

        /// <summary>
        /// به‌روزرسانی اطلاعات کاربر
        /// </summary>
        /// <param name="user">اطلاعات کاربر</param>
        /// <returns>اطلاعات کاربر به‌روز شده</returns>
        Task UpdateAsync(User user);

        /// <summary>
        /// حذف کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه حذف</returns>
        Task DeleteAsync(Guid userId);

        /// <summary>
        /// فعال‌سازی کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه فعال‌سازی</returns>
        Task<bool> ActivateUserAsync(Guid id);

        /// <summary>
        /// غیرفعال‌سازی کاربر
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>نتیجه غیرفعال‌سازی</returns>
        Task<bool> DeactivateUserAsync(Guid id);

        /// <summary>
        /// بررسی یکتا بودن نام کاربری
        /// </summary>
        /// <param name="username">نام کاربری</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> IsUsernameUniqueAsync(string username);

        /// <summary>
        /// بررسی یکتا بودن ایمیل
        /// </summary>
        /// <param name="email">ایمیل</param>
        /// <returns>نتیجه بررسی</returns>
        Task<bool> IsEmailUniqueAsync(string email);
        #endregion

        #region مدیریت پروفایل
        /// <summary>
        /// به‌روزرسانی اطلاعات پروفایل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="request">مدل درخواست به‌روزرسانی</param>
        /// <returns>اطلاعات به‌روز شده پروفایل</returns>
        Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);

        /// <summary>
        /// آپلود تصویر پروفایل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="imageData">داده‌های تصویر</param>
        /// <returns>آدرس تصویر</returns>
        Task<string> UploadProfileImageAsync(Guid userId, byte[] imageData);

        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="currentPassword">رمز عبور فعلی</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns>تسک</returns>
        Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// بازنشانی رمز عبور
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="token">توکن بازنشانی</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns>نتیجه بازنشانی</returns>
        Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword);
        #endregion

        #region احراز هویت دو مرحله‌ای
        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نتیجه فعال‌سازی</returns>
        Task<AuthResult> EnableTwoFactorAsync(Guid userId);

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>نتیجه غیرفعال‌سازی</returns>
        Task<AuthResult> DisableTwoFactorAsync(Guid userId, string code);

        /// <summary>
        /// تولید کدهای بازیابی جدید
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست کدهای بازیابی</returns>
        Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId);

        /// <summary>
        /// استفاده از کد بازیابی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد بازیابی</param>
        /// <returns>نتیجه استفاده</returns>
        Task<bool> UseRecoveryCodeAsync(Guid userId, string code);
        #endregion

        #region مدیریت نقش‌ها
        /// <summary>
        /// دریافت نقش کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>نقش کاربر</returns>
        Task<RoleType> GetUserRoleAsync(Guid userId);

        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>نتیجه اختصاص</returns>
        Task<bool> AssignRoleToUserAsync(Guid userId, string roleName);

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>نتیجه حذف</returns>
        Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName);

        /// <summary>
        /// دریافت کلیم‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست کلیم‌ها</returns>
        Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid userId);
        #endregion

        #region مدیریت دستگاه‌ها
        /// <summary>
        /// افزودن دستگاه جدید
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="device">اطلاعات دستگاه</param>
        /// <returns>تسک</returns>
        Task<bool> AddDeviceAsync(Guid userId, UserDevice device);

        /// <summary>
        /// حذف دستگاه
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="deviceId">شناسه دستگاه</param>
        /// <returns>تسک</returns>
        Task<bool> RemoveDeviceAsync(Guid userId, Guid deviceId);

        /// <summary>
        /// دریافت لیست دستگاه‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست دستگاه‌ها</returns>
        Task<IEnumerable<UserDevice>> GetDevicesAsync(Guid userId);
        #endregion

        #region امنیت حساب کاربری
        /// <summary>
        /// قفل کردن حساب کاربری
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="reason">دلیل قفل</param>
        /// <returns>تسک</returns>
        Task<bool> LockAccountAsync(Guid userId, string reason);

        /// <summary>
        /// باز کردن قفل حساب کاربری
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task UnlockAccountAsync(Guid userId);

        /// <summary>
        /// بررسی وضعیت قفل بودن حساب
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>وضعیت قفل</returns>
        Task<AccountLockStatus> GetAccountLockStatusAsync(Guid userId);

        /// <summary>
        /// دریافت تاریخچه ورودها
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد در هر صفحه</param>
        /// <returns>لیست ورودها</returns>
        Task<LoginHistoryResponse> GetLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10);

        /// <summary>
        /// به‌روزرسانی آخرین زمان ورود
        /// </summary>
        /// <param name="id">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task UpdateLastLoginAsync(Guid id);
        #endregion

        #region امنیت حساب کاربری
        /// <summary>
        /// بررسی وضعیت قفل بودن حساب
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>وضعیت قفل</returns>
        Task<bool> IsLockedOutAsync(Guid userId);
        #endregion

        #region امنیت حساب کاربری
        /// <summary>
        /// بررسی وضعیت تایید ایمیل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>وضعیت تایید ایمیل</returns>
        Task<bool> IsEmailConfirmedAsync(Guid userId);

        /// <summary>
        /// بررسی وضعیت تایید تلفن
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>وضعیت تایید تلفن</returns>
        Task<bool> IsPhoneConfirmedAsync(Guid userId);

        /// <summary>
        /// تولید توکن تایید ایمیل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>توکن تایید ایمیل</returns>
        Task<string> GenerateEmailConfirmationTokenAsync(Guid userId);

        /// <summary>
        /// تولید توکن تایید تلفن
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>توکن تایید تلفن</returns>
        Task<string> GeneratePhoneConfirmationTokenAsync(Guid userId);

        /// <summary>
        /// تایید ایمیل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="token">توکن تایید</param>
        /// <returns>نتیجه تایید</returns>
        Task<bool> ConfirmEmailAsync(Guid userId, string token);

        /// <summary>
        /// تایید تلفن
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="token">توکن تایید</param>
        /// <returns>نتیجه تایید</returns>
        Task<bool> ConfirmPhoneAsync(Guid userId, string token);
        #endregion

        // User management methods
        Task<IEnumerable<UserResponse>> GetAllUsersAsync();
        Task<UserResponse> GetUserByIdAsync(Guid id);
        Task<UserResponse> CreateUserAsync(CreateUserRequest request);
        Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task<bool> DeleteUserAsync(Guid id);
    }

    /// <summary>
    /// نتیجه عملیات احراز هویت
    /// </summary>
    public class AuthResult
    {
        /// <summary>
        /// آیا عملیات موفقیت‌آمیز بود
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// پیام نتیجه عملیات
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// اطلاعات کاربر
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// آیا نیاز به احراز هویت دو مرحله‌ای است
        /// </summary>
        public bool RequiresTwoFactor { get; set; }

        /// <summary>
        /// توکن دسترسی
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// توکن رفرش
        /// </summary>
        public string RefreshToken { get; set; }
        public DateTime AccessTokenExpiresAt { get; internal set; }
        public DateTime RefreshTokenExpiresAt { get; internal set; }
    }
}
