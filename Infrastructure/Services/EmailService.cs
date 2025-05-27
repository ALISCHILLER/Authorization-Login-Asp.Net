using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;
        private readonly string _fromEmail;
        private readonly string _fromName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            var smtpSettings = _configuration.GetSection("AppSettings:EmailSettings");
            _fromEmail = smtpSettings["FromEmail"];
            _fromName = smtpSettings["FromName"];

            _smtpClient = new SmtpClient
            {
                Host = smtpSettings["SmtpServer"],
                Port = int.Parse(smtpSettings["SmtpPort"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    smtpSettings["SmtpUsername"],
                    smtpSettings["SmtpPassword"]
                )
            };
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmail, _fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                message.To.Add(to);

                await _smtpClient.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw new EmailServiceException("Failed to send email", ex);
            }
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