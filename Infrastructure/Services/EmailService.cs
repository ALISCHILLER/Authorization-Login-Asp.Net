using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly EmailSettings _settings;
        private readonly ICircuitBreakerService _circuitBreakerService;
        private readonly ActivitySource _activitySource;

        public EmailService(
            IConfiguration configuration,
            ILogger<EmailService> logger,
            IOptions<EmailSettings> settings,
            ICircuitBreakerService circuitBreakerService,
            ITracingService tracingService)
        {
            _configuration = configuration;
            _logger = logger;
            _settings = settings.Value;
            _circuitBreakerService = circuitBreakerService;
            _activitySource = tracingService.CreateActivitySource("EmailService");
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var activity = _activitySource.StartActivity("SendEmail");
            activity?.SetTag("email.to", to);
            activity?.SetTag("email.subject", subject);

            await _circuitBreakerService.ExecuteWithCircuitBreakerAsync(async () =>
            {
                using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new System.Net.NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
                };

                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {To}", to);
                activity?.SetStatus(ActivityStatusCode.Ok);
            }, "EmailService");
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetLink)
        {
            var subject = "Reset Your Password";
            var body = $@"
                <h2>Password Reset Request</h2>
                <p>You have requested to reset your password. Click the link below to proceed:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <p>This link will expire in 1 hour.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendVerificationEmailAsync(string to, string verificationLink)
        {
            var subject = "Verify Your Email";
            var body = $@"
                <h2>Email Verification</h2>
                <p>Thank you for registering. Please click the link below to verify your email address:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you did not create an account, please ignore this email.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendTwoFactorCodeEmailAsync(string to, string code)
        {
            var subject = "Your Two-Factor Authentication Code";
            var body = $@"
                <h2>Two-Factor Authentication Code</h2>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>This code will expire in 5 minutes.</p>
                <p>If you did not request this code, please secure your account immediately.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task<bool> VerifyCodeAsync(string email, string code)
        {
            // In a real implementation, you would validate the code against a stored or recently sent code
            // For this example, we'll just return true
            return true;
        }

        public async Task SendSecurityAlertEmailAsync(string to, string alertType, string details)
        {
            var subject = $"Security Alert: {alertType}";
            var body = $@"
                <h2>Security Alert</h2>
                <p>We detected a security-related event on your account:</p>
                <p><strong>{alertType}</strong></p>
                <p>Details: {details}</p>
                <p>If this was not you, please secure your account immediately by:</p>
                <ul>
                    <li>Changing your password</li>
                    <li>Enabling two-factor authentication</li>
                    <li>Reviewing your recent account activity</li>
                </ul>
                <p>If you need assistance, please contact our support team.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendNewDeviceLoginEmailAsync(string to, string deviceInfo, string location)
        {
            var subject = "New Device Login Detected";
            var body = $@"
                <h2>New Device Login</h2>
                <p>A new device has logged into your account:</p>
                <p><strong>Device Information:</strong></p>
                <ul>
                    <li>Device: {deviceInfo}</li>
                    <li>Location: {location}</li>
                    <li>Time: {DateTime.UtcNow}</li>
                </ul>
                <p>If this was you, you can ignore this email.</p>
                <p>If this was not you, please secure your account immediately.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendPasswordChangedEmailAsync(string to)
        {
            var subject = "Password Changed";
            var body = $@"
                <h2>Password Changed</h2>
                <p>Your password was recently changed.</p>
                <p>If this was you, you can ignore this email.</p>
                <p>If this was not you, please secure your account immediately by:</p>
                <ul>
                    <li>Resetting your password</li>
                    <li>Enabling two-factor authentication</li>
                    <li>Reviewing your recent account activity</li>
                </ul>";

            await SendEmailAsync(to, subject, body);
        }
    }

    public class EmailServiceException : Exception
    {
        public EmailServiceException(string message) : base(message) { }
        public EmailServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}