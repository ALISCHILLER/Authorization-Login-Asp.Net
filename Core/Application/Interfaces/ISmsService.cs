using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Sms;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
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
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendSmsAsync(List<string> phoneNumbers, string message);
        Task SendTemplatedSmsAsync(string phoneNumber, string templateId, Dictionary<string, string> templateData);
        Task SendTemplatedSmsAsync(List<string> phoneNumbers, string templateId, Dictionary<string, string> templateData);
        Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
        Task<bool> IsSmsDeliveredAsync(string messageId);
        Task<SmsStatus> GetSmsStatusAsync(string messageId);
        Task<decimal> GetBalanceAsync();
        Task<List<SmsTemplate>> GetTemplatesAsync();
        Task<bool> AddTemplateAsync(SmsTemplate template);
        Task<bool> UpdateTemplateAsync(SmsTemplate template);
        Task<bool> DeleteTemplateAsync(string templateId);
    }
}