using System;
using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Infrastructure.Configurations
{
    /// <summary>
    /// تنظیمات سرویس ردیابی توزیع شده
    /// </summary>
    public class TracingSettings
    {
        /// <summary>
        /// نام سرویس
        /// </summary>
        public string ServiceName { get; set; } = "Authorization.Login.Service";

        /// <summary>
        /// نسخه سرویس
        /// </summary>
        public string ServiceVersion { get; set; } = "1.0.0";

        /// <summary>
        /// فعال بودن ردیابی توزیع شده
        /// </summary>
        public bool EnableTracing { get; set; } = true;

        /// <summary>
        /// نسبت نمونه‌برداری (بین 0 تا 1)
        /// </summary>
        public double SamplingRatio { get; set; } = 1.0;

        /// <summary>
        /// فعال بودن خروجی کنسول
        /// </summary>
        public bool EnableConsoleExporter { get; set; } = true;

        /// <summary>
        /// فعال بودن خروجی Jaeger
        /// </summary>
        public bool EnableJaegerExporter { get; set; } = false;

        /// <summary>
        /// آدرس سرور Jaeger
        /// </summary>
        public string JaegerEndpoint { get; set; } = "http://localhost:6831";

        /// <summary>
        /// تنظیمات اضافی برای Jaeger
        /// </summary>
        public JaegerSettings JaegerSettings { get; set; } = new JaegerSettings();
    }

    /// <summary>
    /// تنظیمات اضافی برای Jaeger
    /// </summary>
    public class JaegerSettings
    {
        /// <summary>
        /// نام سرویس در Jaeger
        /// </summary>
        public string ServiceName { get; set; } = "Authorization.Login.Service";

        /// <summary>
        /// نام محیط (Environment)
        /// </summary>
        public string Environment { get; set; } = "Development";

        /// <summary>
        /// زمان انتظار برای اتصال به سرور (به میلی‌ثانیه)
        /// </summary>
        public int ConnectionTimeout { get; set; } = 5000;

        /// <summary>
        /// زمان انتظار برای ارسال داده‌ها (به میلی‌ثانیه)
        /// </summary>
        public int ExportTimeout { get; set; } = 30000;

        /// <summary>
        /// حداکثر اندازه بسته داده‌ها (به بایت)
        /// </summary>
        public int MaxPacketSize { get; set; } = 65000;
    }
}