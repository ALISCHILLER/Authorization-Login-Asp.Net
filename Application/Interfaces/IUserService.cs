using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس کاربر برای انجام عملیات کسب‌وکار مرتبط با کاربران
    /// </summary>
    public interface IUserService
    {
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
        /// <returns>اطلاعات احراز هویت شامل توکن</returns>
        Task<AuthResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// گرفتن اطلاعات کاربر جاری با شناسه
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>شیء کاربر یا null</returns>
        Task<User> GetCurrentUserAsync(Guid userId);

        /// <summary>
        /// تغییر رمز عبور کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="currentPassword">رمز عبور فعلی</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns>تسک</returns>
        Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);

        /// <summary>
        /// خروج کاربر و لغو توکن رفرش (در صورت استفاده)
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task LogoutAsync(Guid userId);

        /// <summary>
        /// به‌روزرسانی اطلاعات پروفایل کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="profileData">اطلاعات جدید پروفایل</param>
        /// <returns>اطلاعات به‌روز شده کاربر</returns>
        Task<User> UpdateProfileAsync(Guid userId, UpdateProfileRequest profileData);

        /// <summary>
        /// آپلود تصویر پروفایل کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="imageData">داده‌های تصویر</param>
        /// <returns>آدرس تصویر آپلود شده</returns>
        Task<string> UploadProfileImageAsync(Guid userId, byte[] imageData);

        /// <summary>
        /// درخواست بازیابی رمز عبور
        /// </summary>
        /// <param name="email">ایمیل کاربر</param>
        /// <returns>تسک</returns>
        Task RequestPasswordResetAsync(string email);

        /// <summary>
        /// تأیید کد بازیابی رمز عبور
        /// </summary>
        /// <param name="email">ایمیل کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns>تسک</returns>
        Task ConfirmPasswordResetAsync(string email, string code, string newPassword);

        /// <summary>
        /// ارسال کد تأیید حساب کاربری
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>تسک</returns>
        Task SendVerificationCodeAsync(Guid userId);

        /// <summary>
        /// تأیید حساب کاربری با کد ارسال شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>تسک</returns>
        Task VerifyAccountAsync(Guid userId, string code);

        /// <summary>
        /// اختصاص نقش به کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>تسک</returns>
        Task AssignRoleAsync(Guid userId, string roleName);

        /// <summary>
        /// حذف نقش از کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="roleName">نام نقش</param>
        /// <returns>تسک</returns>
        Task RemoveRoleAsync(Guid userId, string roleName);

        /// <summary>
        /// دریافت لیست نقش‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست نام نقش‌ها</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(Guid userId);

        /// <summary>
        /// ارسال کد تأیید شماره تلفن
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <returns>تسک</returns>
        Task SendPhoneVerificationCodeAsync(Guid userId, string phoneNumber);

        /// <summary>
        /// تأیید شماره تلفن با کد ارسال شده
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>تسک</returns>
        Task VerifyPhoneNumberAsync(Guid userId, string code);

        /// <summary>
        /// به‌روزرسانی تنظیمات امنیتی کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="settings">تنظیمات جدید</param>
        /// <returns>تسک</returns>
        Task UpdateSecuritySettingsAsync(Guid userId, SecuritySettingsRequest settings);

        /// <summary>
        /// مدیریت دستگاه‌های متصل کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست دستگاه‌های متصل</returns>
        Task<IEnumerable<UserDevice>> GetConnectedDevicesAsync(Guid userId);

        /// <summary>
        /// حذف یک دستگاه از لیست دستگاه‌های متصل
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="deviceId">شناسه دستگاه</param>
        /// <returns>تسک</returns>
        Task RemoveDeviceAsync(Guid userId, string deviceId);

        /// <summary>
        /// ارسال اعلان به کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="notification">اطلاعات اعلان</param>
        /// <returns>تسک</returns>
        Task SendNotificationAsync(Guid userId, NotificationRequest notification);

        /// <summary>
        /// دریافت لیست اعلان‌های کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست اعلان‌ها</returns>
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(Guid userId);

        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="type">نوع احراز هویت دو مرحله‌ای</param>
        /// <returns>اطلاعات مورد نیاز برای راه‌اندازی</returns>
        Task<TwoFactorSetupResponse> EnableTwoFactorAsync(Guid userId, TwoFactorType type);

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>تسک</returns>
        Task DisableTwoFactorAsync(Guid userId, string code);

        /// <summary>
        /// تأیید کد احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>نتیجه تأیید</returns>
        Task<bool> VerifyTwoFactorCodeAsync(Guid userId, string code);

        /// <summary>
        /// تولید کدهای بازیابی جدید
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <returns>لیست کدهای بازیابی جدید</returns>
        Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId);

        /// <summary>
        /// استفاده از کد بازیابی
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="code">کد بازیابی</param>
        /// <returns>نتیجه استفاده از کد</returns>
        Task<bool> UseRecoveryCodeAsync(Guid userId, string code);

        /// <summary>
        /// قفل کردن حساب کاربری
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="duration">مدت زمان قفل شدن (به دقیقه)</param>
        /// <returns>تسک</returns>
        Task LockAccountAsync(Guid userId, int duration);

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
        /// <returns>وضعیت قفل بودن</returns>
        Task<AccountLockStatus> GetAccountLockStatusAsync(Guid userId);

        /// <summary>
        /// دریافت تاریخچه ورودهای کاربر
        /// </summary>
        /// <param name="userId">شناسه کاربر</param>
        /// <param name="page">شماره صفحه</param>
        /// <param name="pageSize">تعداد در هر صفحه</param>
        /// <returns>لیست ورودها</returns>
        Task<LoginHistoryResponse> GetLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10);

        Task<User> GetUserByIdAsync(Guid id);
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid id);
        Task<bool> ValidateUserCredentialsAsync(string email, string password);
        Task<AuthResult> ValidateTwoFactorAsync(TwoFactorDto model);
        Task<AuthResult> RefreshTokenAsync(RefreshTokenDto model);
        Task<AuthResult> EnableTwoFactorAsync(string userId);
        Task<AuthResult> DisableTwoFactorAsync(string userId);
        Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid userId);
        Task AddUserDeviceAsync(Guid userId, UserDevice device);
        Task RemoveUserDeviceAsync(Guid userId, Guid deviceId);
        Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId);
    }

    public class AuthResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public User User { get; set; }
        public bool RequiresTwoFactor { get; set; }
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
