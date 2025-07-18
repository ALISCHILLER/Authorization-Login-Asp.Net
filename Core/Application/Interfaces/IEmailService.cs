using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, Guid userId);
        Task SendBackupCodesAsync(string email, List<string> codes);
        Task SendEmailAsync(EmailRequest request);
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendEmailAsync(string to, string subject, string body, List<EmailAttachment> attachments, bool isHtml = false);
        Task SendEmailAsync(List<string> to, string subject, string body, bool isHtml = false);
        Task SendEmailAsync(List<string> to, string subject, string body, List<EmailAttachment> attachments, bool isHtml = false);
        Task SendTemplatedEmailAsync(string to, string template, Dictionary<string, string> parameters);
        Task SendTemplatedEmailAsync(List<string> to, string template, Dictionary<string, string> parameters);
        Task<bool> ValidateEmailAsync(string email);
        Task<bool> IsEmailDeliveredAsync(string email);
        Task<string> GetEmailStatusAsync(string email);
        Task SendTwoFactorCodeAsync(string email, string code);
    }
}
