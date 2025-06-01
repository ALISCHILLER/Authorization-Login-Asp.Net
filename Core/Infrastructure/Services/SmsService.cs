using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _senderId;
        private readonly string _apiUrl;
        private readonly bool _isProduction;

        public SmsService(
            IConfiguration configuration,
            ILogger<SmsService> logger,
            HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = _configuration["SmsSettings:ApiKey"];
            _senderId = _configuration["SmsSettings:SenderId"];
            _apiUrl = _configuration["SmsSettings:ApiUrl"];
            _isProduction = _configuration.GetValue("SmsSettings:IsProduction", false);
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                if (!_isProduction)
                {
                    _logger.LogInformation("SMS (Development Mode) to {PhoneNumber}: {Message}", phoneNumber, message);
                    return;
                }

                var request = new
                {
                    apiKey = _apiKey,
                    senderId = _senderId,
                    phoneNumber = FormatPhoneNumber(phoneNumber),
                    message,
                    timestamp = DateTime.UtcNow
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(_apiUrl, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<SmsResponse>(responseContent);

                if (!result.Success)
                {
                    throw new Exception($"SMS API Error: {result.ErrorMessage}");
                }

                _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
                throw new SmsServiceException($"Failed to send SMS: {ex.Message}", ex);
            }
        }

        public async Task SendTwoFactorCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPasswordResetCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your password reset code is: {code}. This code will expire in 10 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }

        public async Task SendPhoneVerificationCodeAsync(string phoneNumber, string code)
        {
            var message = $"Your phone verification code is: {code}. This code will expire in 5 minutes.";
            await SendSmsAsync(phoneNumber, message);
        }

        private string FormatPhoneNumber(string phoneNumber)
        {
            // Remove any non-digit characters
            var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());

            // Ensure the number starts with the country code
            if (!digits.StartsWith("98"))
            {
                digits = "98" + digits;
            }

            return digits;
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

        private class SmsResponse
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public string MessageId { get; set; }
        }
    }

    public class SmsServiceException : Exception
    {
        public SmsServiceException(string message) : base(message) { }
        public SmsServiceException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}