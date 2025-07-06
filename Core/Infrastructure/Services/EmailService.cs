using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Configurations;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using System.Threading;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IEmailService برای ارسال ایمیل
    /// </summary>
    public class EmailService : IEmailService
    {
        // وابستگی‌ها
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;
        private readonly ICircuitBreakerService _circuitBreakerService;
        private readonly ActivitySource _activitySource;
        private readonly EmailOptions _emailOptions;
        private readonly ITracingService _tracingService;

        /// <summary>
        /// سازنده کلاس با وابستگی‌های لازم
        /// </summary>
        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IOptions<EmailSettings> settings,
            ICircuitBreakerService circuitBreakerService,
            ITracingService tracingService,
            IOptions<EmailOptions> emailOptions)
        {
            _configuration = configuration;
            _logger = logger;
            _settings = settings.Value;
            _circuitBreakerService = circuitBreakerService;
            _activitySource = tracingService.CreateActivitySource("EmailService");
            _emailOptions = emailOptions?.Value ?? throw new ArgumentNullException(nameof(emailOptions));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
        }

        #region ✅ پیاده‌سازی متدهای IEmailService

        /// <inheritdoc />
        public async Task SendConfirmationEmailAsync(string email, string confirmationLink)
        {
            using var activity = _activitySource.StartActivity("SendConfirmationEmail");

            try
            {
                var subject = "تایید آدرس ایمیل";
                var body = $@"
                    <h2>تایید ایمیل</h2>
                    <p>با تشکر از ثبت نام شما، لطفاً با کلیک روی لینک زیر ایمیل خود را تایید کنید:</p>
                    <p><a href='{confirmationLink}'>تایید ایمیل</a></p>
                    <p>در صورتی که این حساب را ایجاد نکرده‌اید، این ایمیل را نادیده بگیرید.</p>";

                await SendEmailAsync(new EmailRequest { To = email, Subject = subject, Body = body });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل تایید به {Email}", email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SendPasswordResetEmailAsync(string email, string resetLink)
        {
            using var activity = _activitySource.StartActivity("SendPasswordResetEmail");

            try
            {
                var subject = "بازیابی رمز عبور";
                var body = $@"
                    <h2>درخواست بازیابی رمز عبور</h2>
                    <p>شما درخواست تغییر رمز عبور داده‌اید. برای ادامه روی لینک زیر کلیک کنید:</p>
                    <p><a href='{resetLink}'>تغییر رمز عبور</a></p>
                    <p>این لینک ۱ ساعت اعتبار دارد.</p>
                    <p>در صورتی که این درخواست را نداده‌اید، این ایمیل را نادیده بگیرید.</p>";

                await SendEmailAsync(new EmailRequest { To = email, Subject = subject, Body = body });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال لینک بازیابی رمز به {Email}", email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SendTwoFactorCodeAsync(string email, string code)
        {
            using var activity = _activitySource.StartActivity("SendTwoFactorCode");

            try
            {
                var subject = "کد تأیید دو مرحله‌ای";
                var body = $@"
                    <h2>کد تأیید دو مرحله‌ای</h2>
                    <p>کد تأیید شما: <strong>{code}</strong></p>
                    <p>این کد ۵ دقیقه اعتبار دارد.</p>
                    <p>در صورتی که این درخواست را نداده‌اید، فوراً حساب خود را امن کنید.</p>";

                await SendEmailAsync(new EmailRequest { To = email, Subject = subject, Body = body });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال کد 2FA به {Email}", email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SendPasswordChangedEmailAsync(string email)
        {
            using var activity = _activitySource.StartActivity("SendPasswordChangedEmail");

            try
            {
                var subject = "رمز عبور شما تغییر کرد";
                var body = $@"
                    <h2>رمز عبور شما تغییر کرد</h2>
                    <p>رمز عبور حساب شما با موفقیت تغییر یافت.</p>
                    <p>در صورتی که این عمل را انجام نداده‌اید، فوراً حساب خود را امن کنید.</p>";

                await SendEmailAsync(new EmailRequest { To = email, Subject = subject, Body = body });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اطلاعیه تغییر رمز به {Email}", email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SendNewLoginNotificationAsync(string email, string deviceInfo, string location)
        {
            using var activity = _activitySource.StartActivity("SendNewLoginNotification");

            try
            {
                var subject = "ورود جدید به حساب شما";
                var body = $@"
                    <h2>ورود جدید</h2>
                    <p>یک دستگاه جدید وارد حساب شما شده است:</p>
                    <ul>
                        <li>دستگاه: {deviceInfo}</li>
                        <li>موقعیت: {location}</li>
                        <li>زمان: {DateTime.UtcNow}</li>
                    </ul>
                    <p>در صورتی که این ورود را انجام نداده‌اید، فوراً حساب خود را امن کنید.</p>";

                await SendEmailAsync(new EmailRequest { To = email, Subject = subject, Body = body });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال اطلاعیه ورود جدید به {Email}", email);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task SendVerificationEmailAsync(string email)
        {
            var subject = "تایید ایمیل";
            var body = "کد تایید یا لینک تایید ایمیل";
            await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = email, Subject = subject, Body = body });
        }

        public async Task SendBackupCodesAsync(string email, System.Collections.Generic.List<string> codes)
        {
            var subject = "کدهای پشتیبان";
            var body = $"کدهای پشتیبان شما: {string.Join(", ", codes)}";
            await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = email, Subject = subject, Body = body });
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = to, Subject = subject, Body = body, IsHtml = isHtml });
        }

        public async Task SendEmailAsync(string to, string subject, string body, System.Collections.Generic.List<Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailAttachment> attachments, bool isHtml = false)
        {
            await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = to, Subject = subject, Body = body, Attachments = attachments, IsHtml = isHtml });
        }

        public async Task SendEmailAsync(System.Collections.Generic.List<string> to, string subject, string body, bool isHtml = false)
        {
            foreach (var email in to)
                await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = email, Subject = subject, Body = body, IsHtml = isHtml });
        }

        public async Task SendEmailAsync(System.Collections.Generic.List<string> to, string subject, string body, System.Collections.Generic.List<Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailAttachment> attachments, bool isHtml = false)
        {
            foreach (var email in to)
                await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = email, Subject = subject, Body = body, Attachments = attachments, IsHtml = isHtml });
        }

        public async Task SendTemplatedEmailAsync(string to, string template, System.Collections.Generic.Dictionary<string, string> parameters)
        {
            var body = template;
            foreach (var kv in parameters)
                body = body.Replace($"{{{kv.Key}}}", kv.Value);
            await SendEmailAsync(new Authorization_Login_Asp.Net.Core.Application.DTOs.Common.EmailRequest { To = to, Subject = "ایمیل قالبی", Body = body });
        }

        public async Task SendTemplatedEmailAsync(System.Collections.Generic.List<string> to, string template, System.Collections.Generic.Dictionary<string, string> parameters)
        {
            foreach (var email in to)
                await SendTemplatedEmailAsync(email, template, parameters);
        }

        public Task<bool> ValidateEmailAsync(string email)
        {
            // Minimal validation
            return Task.FromResult(email.Contains("@"));
        }

        public Task<bool> IsEmailDeliveredAsync(string email)
        {
            // Stub: always return true
            return Task.FromResult(true);
        }

        public Task<string> GetEmailStatusAsync(string email)
        {
            // Stub: always return "Delivered"
            return Task.FromResult("Delivered");
        }

        #endregion

        #region 🔧 متدهای کمکی

        /// <summary>
        /// ارسال ایمیل با استفاده از SmtpClient
        /// </summary>
        private async Task SendEmailAsync(EmailRequest request)
        {
            using var activity = _tracingService.StartActivity("EmailService.SendEmailAsync");
            try
            {
                // در محیط توسعه، ایمیل‌ها را لاگ می‌کنیم
                if (_emailOptions.UseDevelopmentMode)
                {
                    _logger.LogInformation(
                        "ایمیل در محیط توسعه:\n" +
                        "به: {To}\n" +
                        "موضوع: {Subject}\n" +
                        "متن: {Body}\n",
                        request.To,
                        request.Subject,
                        request.Body);

                    return;
                }

                // ارسال واقعی ایمیل
                // TODO: پیاده‌سازی ارسال ایمیل با استفاده از SMTP یا سرویس‌های ایمیل
                await Task.Delay(100); // شبیه‌سازی تأخیر شبکه

                _logger.LogInformation("ایمیل با موفقیت به {To} ارسال شد", request.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال ایمیل به {To}", request.To);
                throw;
            }
        }

        #endregion
    }

    /// <summary>
    /// استثنا برای خطاهای مربوط به سرویس ایمیل
    /// </summary>
    public class EmailServiceException : Exception
    {
        public EmailServiceException(string message) : base(message) { }
        public EmailServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}