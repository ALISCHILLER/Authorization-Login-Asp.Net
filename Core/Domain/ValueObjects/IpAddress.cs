using System;
using System.Net;
using System.Text.RegularExpressions;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار آدرس IP
    /// </summary>
    public class IpAddress
    {
        private const string Ipv4Pattern = @"^(\d{1,3}\.){3}\d{1,3}$";
        private const string Ipv6Pattern = @"^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$";

        /// <summary>
        /// آدرس IP
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// نوع آدرس IP
        /// </summary>
        public IpAddressType Type { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="ipAddress">آدرس IP</param>
        public IpAddress(string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentNullException(nameof(ipAddress));

            if (!IsValid(ipAddress, out var type))
                throw new ArgumentException("فرمت آدرس IP نامعتبر است");

            Value = Normalize(ipAddress);
            Type = type;
        }

        /// <summary>
        /// بررسی اعتبار آدرس IP
        /// </summary>
        public static bool IsValid(string ipAddress, out IpAddressType type)
        {
            type = IpAddressType.Unknown;

            if (string.IsNullOrWhiteSpace(ipAddress))
                return false;

            if (Regex.IsMatch(ipAddress, Ipv4Pattern))
            {
                if (IPAddress.TryParse(ipAddress, out var ip))
                {
                    type = IpAddressType.IPv4;
                    return true;
                }
            }
            else if (Regex.IsMatch(ipAddress, Ipv6Pattern))
            {
                if (IPAddress.TryParse(ipAddress, out var ip))
                {
                    type = IpAddressType.IPv6;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// نرمال‌سازی آدرس IP
        /// </summary>
        private static string Normalize(string ipAddress)
        {
            if (IPAddress.TryParse(ipAddress, out var ip))
                return ip.ToString();
            return ipAddress;
        }

        /// <summary>
        /// تبدیل ضمنی به رشته
        /// </summary>
        public static implicit operator string(IpAddress ipAddress) => ipAddress?.Value;

        /// <summary>
        /// تبدیل ضمنی از رشته
        /// </summary>
        public static implicit operator IpAddress(string ipAddress) => new(ipAddress);

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is IpAddress other)
                return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        public override int GetHashCode() => Value.ToLowerInvariant().GetHashCode();

        /// <summary>
        /// تبدیل به رشته
        /// </summary>
        public override string ToString() => Value;
    }

    /// <summary>
    /// نوع آدرس IP
    /// </summary>
    public enum IpAddressType
    {
        /// <summary>
        /// نامشخص
        /// </summary>
        Unknown,

        /// <summary>
        /// IPv4
        /// </summary>
        IPv4,

        /// <summary>
        /// IPv6
        /// </summary>
        IPv6
    }
} 