using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly ILogger<SmsService> _logger;
        private readonly SmsOptions _smsOptions;
        private readonly ITracingService _tracingService;

        public SmsService(
            ILogger<SmsService> logger,
            IOptions<SmsOptions> smsOptions,
            ITracingService tracingService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smsOptions = smsOptions?.Value ?? throw new ArgumentNullException(nameof(smsOptions));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
        }

        public async Task SendSmsAsync(SmsRequest request)
        {
            using var activity = _tracingService.StartActivity("SmsService.SendSmsAsync");
            try
            {
                // در محیط توسعه، پیامک‌ها را لاگ می‌کنیم
                if (_smsOptions.UseDevelopmentMode)
                {
                    _logger.LogInformation(
                        "پیامک در محیط توسعه:\n" +
                        "به: {To}\n" +
                        "اولویت: {Priority}\n" +
                        "متن: {Message}\n" +
                        "متادیتا: {Metadata}",
                        request.To,
                        request.Priority,
                        request.Message,
                        request.Metadata);

                    return;
                }

                // ارسال واقعی پیامک
                // TODO: پیاده‌سازی ارسال پیامک با استفاده از سرویس‌های پیامک
                await Task.Delay(100); // شبیه‌سازی تأخیر شبکه

                _logger.LogInformation("پیامک با موفقیت به {To} ارسال شد", request.To);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ارسال پیامک به {To}", request.To);
                throw;
            }
        }

        public async Task SendTwoFactorCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
            await SendSmsAsync(new SmsRequest { To = phoneNumber, Message = message });
        }

        public async Task SendPasswordResetCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your password reset code is: {code}. This code will expire in 10 minutes.";
            await SendSmsAsync(new SmsRequest { To = phoneNumber, Message = message });
        }

        public async Task SendPhoneVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your phone verification code is: {code}. This code will expire in 5 minutes.";
            await SendSmsAsync(new SmsRequest { To = phoneNumber, Message = message });
        }

        public Task SendVerificationCodeAsync(string phoneNumber, string code)
        {
            throw new NotImplementedException();
        }

        public Task SendNewLoginNotificationAsync(string phoneNumber, string deviceInfo, string location)
        {
            throw new NotImplementedException();
        }

        public Task SendPasswordChangedNotificationAsync(string phoneNumber)
        {
            throw new NotImplementedException();
        }
    }
}