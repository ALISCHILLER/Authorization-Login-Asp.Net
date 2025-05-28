using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Application.Interfaces
{
    /// <summary>
    /// اینترفیس سرویس پیامک
    /// </summary>
    public interface ISmsService
    {
        /// <summary>
        /// ارسال کد تأیید
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="code">کد تأیید</param>
        /// <returns>تسک</returns>
        Task SendVerificationCodeAsync(string phoneNumber, string code);

        /// <summary>
        /// ارسال اعلان ورود جدید
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <param name="deviceInfo">اطلاعات دستگاه</param>
        /// <param name="location">موقعیت</param>
        /// <returns>تسک</returns>
        Task SendNewLoginNotificationAsync(string phoneNumber, string deviceInfo, string location);

        /// <summary>
        /// ارسال اعلان تغییر رمز عبور
        /// </summary>
        /// <param name="phoneNumber">شماره تلفن</param>
        /// <returns>تسک</returns>
        Task SendPasswordChangedNotificationAsync(string phoneNumber);
        Task SendTwoFactorCodeAsync(string phoneNumber, string code);
    }
} 