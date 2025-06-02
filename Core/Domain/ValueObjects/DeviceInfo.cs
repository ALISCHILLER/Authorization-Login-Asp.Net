using System;

namespace Authorization_Login_Asp.Net.Core.Domain.ValueObjects
{
    /// <summary>
    /// کلاس مقدار اطلاعات دستگاه
    /// </summary>
    public class DeviceInfo
    {
        /// <summary>
        /// شناسه دستگاه
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// نام دستگاه
        /// </summary>
        public string DeviceName { get; private set; }

        /// <summary>
        /// نوع دستگاه
        /// </summary>
        public DeviceType Type { get; private set; }

        /// <summary>
        /// سیستم عامل
        /// </summary>
        public string OperatingSystem { get; private set; }

        /// <summary>
        /// مرورگر
        /// </summary>
        public string Browser { get; private set; }

        /// <summary>
        /// نسخه مرورگر
        /// </summary>
        public string BrowserVersion { get; private set; }

        /// <summary>
        /// سازنده
        /// </summary>
        /// <param name="deviceId">شناسه دستگاه</param>
        /// <param name="deviceName">نام دستگاه</param>
        /// <param name="type">نوع دستگاه</param>
        /// <param name="operatingSystem">سیستم عامل</param>
        /// <param name="browser">مرورگر</param>
        /// <param name="browserVersion">نسخه مرورگر</param>
        public DeviceInfo(
            string deviceId,
            string deviceName,
            DeviceType type,
            string operatingSystem,
            string browser,
            string browserVersion)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
                throw new ArgumentNullException(nameof(deviceId));

            if (string.IsNullOrWhiteSpace(deviceName))
                throw new ArgumentNullException(nameof(deviceName));

            if (string.IsNullOrWhiteSpace(operatingSystem))
                throw new ArgumentNullException(nameof(operatingSystem));

            if (string.IsNullOrWhiteSpace(browser))
                throw new ArgumentNullException(nameof(browser));

            DeviceId = deviceId;
            DeviceName = deviceName;
            Type = type;
            OperatingSystem = operatingSystem;
            Browser = browser;
            BrowserVersion = browserVersion;
        }

        /// <summary>
        /// مقایسه با شیء دیگر
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is DeviceInfo other)
                return DeviceId.Equals(other.DeviceId, StringComparison.OrdinalIgnoreCase);
            return false;
        }

        /// <summary>
        /// محاسبه هش کد
        /// </summary>
        public override int GetHashCode() => DeviceId.ToLowerInvariant().GetHashCode();

        /// <summary>
        /// تبدیل به رشته
        /// </summary>
        public override string ToString() => $"{DeviceName} ({Type}) - {OperatingSystem} - {Browser} {BrowserVersion}";
    }

    /// <summary>
    /// نوع دستگاه
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// نامشخص
        /// </summary>
        Unknown,

        /// <summary>
        /// دسکتاپ
        /// </summary>
        Desktop,

        /// <summary>
        /// موبایل
        /// </summary>
        Mobile,

        /// <summary>
        /// تبلت
        /// </summary>
        Tablet,

        /// <summary>
        /// تلویزیون
        /// </summary>
        TV,

        /// <summary>
        /// ساعت هوشمند
        /// </summary>
        SmartWatch,

        /// <summary>
        /// کنسول بازی
        /// </summary>
        GameConsole
    }
} 