using System.ComponentModel;

namespace Authorization_Login_Asp.Net.Core.Domain.Enums
{
    /// <summary>
    /// انواع رویدادهای امنیتی سیستم
    /// این enum برای ثبت و پیگیری رویدادهای امنیتی مختلف استفاده می‌شود
    /// </summary>
    public enum SecurityEventType
    {
        #region Authentication Events (1-19)
        /// <summary>
        /// ورود موفق به سیستم
        /// </summary>
        [Description("ورود موفق")]
        SuccessfulLogin = 1,

        /// <summary>
        /// تلاش ناموفق برای ورود
        /// </summary>
        [Description("تلاش ناموفق ورود")]
        FailedLoginAttempt = 2,

        /// <summary>
        /// خروج از سیستم
        /// </summary>
        [Description("خروج از سیستم")]
        Logout = 3,

        /// <summary>
        /// ورود با احراز هویت دو مرحله‌ای
        /// </summary>
        [Description("ورود با احراز هویت دو مرحله‌ای")]
        TwoFactorLogin = 4,

        /// <summary>
        /// تلاش ناموفق برای احراز هویت دو مرحله‌ای
        /// </summary>
        [Description("تلاش ناموفق احراز هویت دو مرحله‌ای")]
        FailedTwoFactorAttempt = 5,
        #endregion

        #region Account Management Events (20-39)
        /// <summary>
        /// تغییر رمز عبور
        /// </summary>
        [Description("تغییر رمز عبور")]
        PasswordChange = 20,

        /// <summary>
        /// درخواست بازنشانی رمز عبور
        /// </summary>
        [Description("درخواست بازنشانی رمز عبور")]
        PasswordResetRequest = 21,

        /// <summary>
        /// بازنشانی رمز عبور
        /// </summary>
        [Description("بازنشانی رمز عبور")]
        PasswordReset = 22,

        /// <summary>
        /// قفل شدن حساب کاربری
        /// </summary>
        [Description("قفل شدن حساب")]
        AccountLocked = 23,

        /// <summary>
        /// بازگشایی حساب کاربری
        /// </summary>
        [Description("بازگشایی حساب")]
        AccountUnlocked = 24,

        /// <summary>
        /// تغییر اطلاعات پروفایل
        /// </summary>
        [Description("تغییر اطلاعات پروفایل")]
        ProfileUpdate = 25,
        #endregion

        #region Security Settings Events (40-59)
        /// <summary>
        /// فعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        [Description("فعال‌سازی احراز هویت دو مرحله‌ای")]
        TwoFactorEnabled = 40,

        /// <summary>
        /// غیرفعال‌سازی احراز هویت دو مرحله‌ای
        /// </summary>
        [Description("غیرفعال‌سازی احراز هویت دو مرحله‌ای")]
        TwoFactorDisabled = 41,

        /// <summary>
        /// تغییر تنظیمات امنیتی
        /// </summary>
        [Description("تغییر تنظیمات امنیتی")]
        SecuritySettingsChanged = 42,

        /// <summary>
        /// تغییر روش احراز هویت دو مرحله‌ای
        /// </summary>
        [Description("تغییر روش احراز هویت دو مرحله‌ای")]
        TwoFactorMethodChanged = 43,
        #endregion

        #region Verification Events (60-79)
        /// <summary>
        /// تأیید ایمیل
        /// </summary>
        [Description("تأیید ایمیل")]
        EmailVerified = 60,

        /// <summary>
        /// تأیید شماره تلفن
        /// </summary>
        [Description("تأیید شماره تلفن")]
        PhoneVerified = 61,

        /// <summary>
        /// تغییر ایمیل
        /// </summary>
        [Description("تغییر ایمیل")]
        EmailChanged = 62,

        /// <summary>
        /// تغییر شماره تلفن
        /// </summary>
        [Description("تغییر شماره تلفن")]
        PhoneChanged = 63,
        #endregion

        #region Session Events (80-89)
        /// <summary>
        /// منقضی شدن نشست
        /// </summary>
        [Description("منقضی شدن نشست")]
        SessionExpired = 80,

        /// <summary>
        /// تمدید نشست
        /// </summary>
        [Description("تمدید نشست")]
        SessionRenewed = 81,

        /// <summary>
        /// خاتمه اجباری نشست
        /// </summary>
        [Description("خاتمه اجباری نشست")]
        SessionTerminated = 82,
        #endregion

        #region Other Events (90-99)
        /// <summary>
        /// سایر رویدادهای امنیتی
        /// </summary>
        [Description("سایر رویدادها")]
        Other = 99
        #endregion
    }
} 