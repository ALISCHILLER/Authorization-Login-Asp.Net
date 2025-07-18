using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Domain.Enums;

namespace Authorization_Login_Asp.Net.Core.Domain.Exceptions
{
    /// <summary>
    /// استثنای مربوط به خطاهای امنیتی
    /// این کلاس برای خطاهای مربوط به امنیت سیستم مانند تلاش‌های نفوذ، فعالیت‌های مشکوک و غیره استفاده می‌شود
    /// </summary>
    public class SecurityDomainException : DomainException
    {
        /// <summary>
        /// سطح ریسک امنیتی
        /// </summary>
        public SecurityRiskLevel RiskLevel { get; }

        /// <summary>
        /// آدرس IP (در صورت وجود)
        /// </summary>
        public string? IpAddress { get; }

        /// <summary>
        /// User Agent (در صورت وجود)
        /// </summary>
        public string? UserAgent { get; }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای امنیتی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="riskLevel">سطح ریسک امنیتی</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="additionalData">اطلاعات اضافی</param>
        public SecurityDomainException(
            string message,
            SecurityRiskLevel riskLevel = SecurityRiskLevel.Medium,
            string? errorCode = null,
            string? ipAddress = null,
            string? userAgent = null,
            IDictionary<string, object>? additionalData = null)
            : base(message, errorCode, additionalData)
        {
            RiskLevel = riskLevel;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            if (AdditionalData != null)
                AdditionalData["RiskLevel"] = riskLevel;
            if (!string.IsNullOrEmpty(ipAddress) && AdditionalData != null)
                AdditionalData["IpAddress"] = ipAddress;
            if (!string.IsNullOrEmpty(userAgent) && AdditionalData != null)
                AdditionalData["UserAgent"] = userAgent;
        }

        /// <summary>
        /// ایجاد یک نمونه جدید از استثنای امنیتی با استثنای داخلی
        /// </summary>
        /// <param name="message">پیام خطا</param>
        /// <param name="innerException">استثنای داخلی</param>
        /// <param name="riskLevel">سطح ریسک امنیتی</param>
        /// <param name="errorCode">کد خطا</param>
        /// <param name="ipAddress">آدرس IP</param>
        /// <param name="userAgent">User Agent</param>
        /// <param name="additionalData">اطلاعات اضافی</param>
        public SecurityDomainException(
            string message,
            Exception innerException,
            SecurityRiskLevel riskLevel = SecurityRiskLevel.Medium,
            string? errorCode = null,
            string? ipAddress = null,
            string? userAgent = null,
            IDictionary<string, object>? additionalData = null)
            : base(message, innerException, errorCode, additionalData)
        {
            RiskLevel = riskLevel;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            if (AdditionalData != null)
                AdditionalData["RiskLevel"] = riskLevel;
            if (!string.IsNullOrEmpty(ipAddress) && AdditionalData != null)
                AdditionalData["IpAddress"] = ipAddress;
            if (!string.IsNullOrEmpty(userAgent) && AdditionalData != null)
                AdditionalData["UserAgent"] = userAgent;
        }

        /// <summary>
        /// تبدیل استثنا به رشته
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();

            var securityInfo = $" (Security Risk: {RiskLevel}";
            if (!string.IsNullOrEmpty(IpAddress))
                securityInfo += $", IP: {IpAddress}";
            if (!string.IsNullOrEmpty(UserAgent))
                securityInfo += $", UA: {UserAgent}";
            securityInfo += ")";

            result = result.Insert(result.IndexOf('\n'), securityInfo);

            return result;
        }
    }
}