namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مدیریت ارسال ایمیل
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// ارسال ایمیل تأیید حساب کاربری
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <param name="confirmationLink">لینک تأیید</param>
        Task SendConfirmationEmailAsync(string email, string confirmationLink);

        /// <summary>
        /// ارسال ایمیل بازیابی رمز عبور
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <param name="resetLink">لینک بازیابی</param>
        Task SendPasswordResetEmailAsync(string email, string resetLink);

        /// <summary>
        /// ارسال کد یکبار مصرف برای احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <param name="code">کد یکبار مصرف</param>
        Task SendTwoFactorCodeAsync(string email, string code);

        /// <summary>
        /// ارسال ایمیل تغییر رمز عبور
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        Task SendPasswordChangedEmailAsync(string email);

        /// <summary>
        /// ارسال ایمیل اعلان ورود جدید
        /// </summary>
        /// <param name="email">آدرس ایمیل</param>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        /// <param name="location">موقعیت مکانی</param>
        Task SendNewLoginNotificationAsync(string email, string deviceInfo, string location);
    }
} 