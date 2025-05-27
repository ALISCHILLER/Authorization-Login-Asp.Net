namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس مدیریت ارسال پیامک
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// ارسال پیامک تأیید شماره تلفن
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="code">کد تأیید</param>
        Task SendVerificationCodeAsync(string phoneNumber, string code);

        /// <summary>
        /// ارسال کد یکبار مصرف برای احراز هویت دو مرحله‌ای
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="code">کد یکبار مصرف</param>
        Task SendTwoFactorCodeAsync(string phoneNumber, string code);

        /// <summary>
        /// ارسال پیامک اعلان ورود جدید
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        /// <param name="location">موقعیت مکانی</param>
        Task SendNewLoginNotificationAsync(string phoneNumber, string deviceInfo, string location);

        /// <summary>
        /// ارسال پیامک تغییر رمز عبور
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        Task SendPasswordChangedNotificationAsync(string phoneNumber);
    }
} 