using System.ComponentModel.DataAnnotations;
using Authorization_Login_Asp.Net.Core.Infrastructure.Security;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Configurations
{
    /// <summary>
    /// تنظیمات اصلی برنامه
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// تنظیمات JWT
        /// </summary>
        [Required]
        public JwtSettings JwtSettings { get; set; }

        /// <summary>
        /// تنظیمات امنیتی
        /// </summary>
        [Required]
        public SecuritySettings SecuritySettings { get; set; }

        /// <summary>
        /// تنظیمات ایمیل
        /// </summary>
        [Required]
        public EmailSettings EmailSettings { get; set; }

        /// <summary>
        /// تنظیمات SMS
        /// </summary>
        [Required]
        public SmsSettings SmsSettings { get; set; }

        /// <summary>
        /// تنظیمات لاگینگ
        /// </summary>
        [Required]
        public LoggingSettings LoggingSettings { get; set; }

        /// <summary>
        /// تنظیمات کش
        /// </summary>
        [Required]
        public CacheSettings CacheSettings { get; set; }

        /// <summary>
        /// تنظیمات پایگاه داده
        /// </summary>
        [Required]
        public DatabaseSettings DatabaseSettings { get; set; }
    }

    /// <summary>
    /// تنظیمات امنیتی
    /// </summary>
    public class SecuritySettings
    {
        /// <summary>
        /// تنظیمات رمز عبور
        /// </summary>
        [Required]
        public PasswordSettings PasswordSettings { get; set; }

        /// <summary>
        /// تنظیمات احراز هویت دو مرحله‌ای
        /// </summary>
        [Required]
        public TwoFactorSettings TwoFactorSettings { get; set; }

        /// <summary>
        /// تنظیمات محدودیت نرخ درخواست
        /// </summary>
        [Required]
        public RateLimitSettings RateLimitSettings { get; set; }

        /// <summary>
        /// تنظیمات CORS
        /// </summary>
        [Required]
        public CorsSettings CorsSettings { get; set; }
    }

    /// <summary>
    /// تنظیمات رمز عبور
    /// </summary>
    public class PasswordSettings
    {
        /// <summary>
        /// حداقل طول رمز عبور
        /// </summary>
        [Range(8, 32)]
        public int MinLength { get; set; } = 8;

        /// <summary>
        /// حداکثر طول رمز عبور
        /// </summary>
        [Range(8, 32)]
        public int MaxLength { get; set; } = 32;

        /// <summary>
        /// آیا باید حروف بزرگ داشته باشد
        /// </summary>
        public bool RequireUppercase { get; set; } = true;

        /// <summary>
        /// آیا باید حروف کوچک داشته باشد
        /// </summary>
        public bool RequireLowercase { get; set; } = true;

        /// <summary>
        /// آیا باید اعداد داشته باشد
        /// </summary>
        public bool RequireDigit { get; set; } = true;

        /// <summary>
        /// آیا باید کاراکترهای خاص داشته باشد
        /// </summary>
        public bool RequireSpecialCharacter { get; set; } = true;

        /// <summary>
        /// تعداد تکرار مجاز کاراکترها
        /// </summary>
        [Range(1, 5)]
        public int MaxRepeatingCharacters { get; set; } = 3;

        /// <summary>
        /// تعداد تکرار مجاز کلمات رایج
        /// </summary>
        [Range(1, 5)]
        public int MaxCommonWords { get; set; } = 3;
    }

    /// <summary>
    /// تنظیمات احراز هویت دو مرحله‌ای
    /// </summary>
    public class TwoFactorSettings
    {
        /// <summary>
        /// مدت زمان اعتبار کد (دقیقه)
        /// </summary>
        [Range(1, 30)]
        public int CodeValidityMinutes { get; set; } = 5;

        /// <summary>
        /// تعداد کدهای بازیابی
        /// </summary>
        [Range(5, 20)]
        public int RecoveryCodeCount { get; set; } = 10;

        /// <summary>
        /// طول کدهای بازیابی
        /// </summary>
        [Range(8, 16)]
        public int RecoveryCodeLength { get; set; } = 10;

        /// <summary>
        /// آیا باید QR Code تولید شود
        /// </summary>
        public bool GenerateQrCode { get; set; } = true;

        /// <summary>
        /// آیا باید کدهای بازیابی تولید شوند
        /// </summary>
        public bool GenerateRecoveryCodes { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات محدودیت نرخ درخواست
    /// </summary>
    public class RateLimitSettings
    {
        /// <summary>
        /// حداکثر تعداد تلاش مجاز
        /// </summary>
        [Range(1, 10)]
        public int MaxAttempts { get; set; } = 5;

        /// <summary>
        /// مدت زمان محدودیت (دقیقه)
        /// </summary>
        [Range(1, 60)]
        public int WindowMinutes { get; set; } = 15;

        /// <summary>
        /// مدت زمان لیست سیاه (دقیقه)
        /// </summary>
        [Range(1, 1440)]
        public int BlacklistDurationMinutes { get; set; } = 60;

        /// <summary>
        /// آیا باید IP را محدود کرد
        /// </summary>
        public bool EnableIpRateLimit { get; set; } = true;

        /// <summary>
        /// آیا باید نام کاربری را محدود کرد
        /// </summary>
        public bool EnableUsernameRateLimit { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات CORS
    /// </summary>
    public class CorsSettings
    {
        /// <summary>
        /// آیا CORS فعال است
        /// </summary>
        public bool EnableCors { get; set; } = true;

        /// <summary>
        /// نام پالیسی CORS
        /// </summary>
        public string PolicyName { get; set; } = "DefaultCorsPolicy";

        /// <summary>
        /// دامنه‌های مجاز
        /// </summary>
        public string[] AllowedOrigins { get; set; } = new[] { "http://localhost:3000" };

        /// <summary>
        /// متدهای مجاز
        /// </summary>
        public string[] AllowedMethods { get; set; } = new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };

        /// <summary>
        /// هدرهای مجاز
        /// </summary>
        public string[] AllowedHeaders { get; set; } = new[] { "Authorization", "Content-Type", "X-Requested-With" };

        /// <summary>
        /// آیا اعتبارسنجی اعتبارنامه‌ها مجاز است
        /// </summary>
        public bool AllowCredentials { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات ایمیل
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// آدرس SMTP سرور
        /// </summary>
        [Required]
        public string SmtpServer { get; set; }

        /// <summary>
        /// پورت SMTP
        /// </summary>
        [Range(1, 65535)]
        public int SmtpPort { get; set; } = 587;

        /// <summary>
        /// نام کاربری SMTP
        /// </summary>
        [Required]
        public string SmtpUsername { get; set; }

        /// <summary>
        /// رمز عبور SMTP
        /// </summary>
        [Required]
        public string SmtpPassword { get; set; }

        /// <summary>
        /// آدرس ایمیل فرستنده
        /// </summary>
        [Required]
        [EmailAddress]
        public string FromEmail { get; set; }

        /// <summary>
        /// نام فرستنده
        /// </summary>
        [Required]
        public string FromName { get; set; }

        /// <summary>
        /// آیا از SSL استفاده شود
        /// </summary>
        public bool EnableSsl { get; set; } = true;

        /// <summary>
        /// آیا از TLS استفاده شود
        /// </summary>
        public bool EnableTls { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات SMS
    /// </summary>
    public class SmsSettings
    {
        /// <summary>
        /// کلید API
        /// </summary>
        [Required]
        public string ApiKey { get; set; }

        /// <summary>
        /// شناسه کاربری
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// رمز عبور
        /// </summary>
        [Required]
        public string Password { get; set; }

        /// <summary>
        /// شماره فرستنده
        /// </summary>
        [Required]
        public string SenderNumber { get; set; }

        /// <summary>
        /// آدرس API
        /// </summary>
        [Required]
        public string ApiUrl { get; set; }

        /// <summary>
        /// آیا از HTTPS استفاده شود
        /// </summary>
        public bool UseHttps { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات لاگینگ
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// مسیر فایل لاگ
        /// </summary>
        [Required]
        public string LogFilePath { get; set; }

        /// <summary>
        /// حداکثر اندازه فایل لاگ (مگابایت)
        /// </summary>
        [Range(1, 100)]
        public int MaxFileSize { get; set; } = 10;

        /// <summary>
        /// حداکثر تعداد فایل‌های لاگ
        /// </summary>
        [Range(1, 100)]
        public int MaxFileCount { get; set; } = 10;

        /// <summary>
        /// آیا لاگ‌ها به صورت JSON ذخیره شوند
        /// </summary>
        public bool UseJsonFormat { get; set; } = true;

        /// <summary>
        /// سطح لاگینگ
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// آیا لاگ‌های امنیتی ذخیره شوند
        /// </summary>
        public bool EnableSecurityLogging { get; set; } = true;

        /// <summary>
        /// آیا لاگ‌های عملکرد ذخیره شوند
        /// </summary>
        public bool EnablePerformanceLogging { get; set; } = true;
    }

    /// <summary>
    /// تنظیمات کش
    /// </summary>
    public class CacheSettings
    {
        /// <summary>
        /// مدت زمان اعتبار کش (دقیقه)
        /// </summary>
        [Range(1, 1440)]
        public int DefaultExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// حداکثر اندازه کش (مگابایت)
        /// </summary>
        [Range(1, 1000)]
        public int MaxSize { get; set; } = 100;

        /// <summary>
        /// آیا کش توزیع شده استفاده شود
        /// </summary>
        public bool UseDistributedCache { get; set; } = false;

        /// <summary>
        /// رشته اتصال به Redis
        /// </summary>
        public string RedisConnectionString { get; set; }

        /// <summary>
        /// نام نمونه Redis
        /// </summary>
        public string RedisInstanceName { get; set; }
    }

    /// <summary>
    /// تنظیمات پایگاه داده
    /// </summary>
    public class DatabaseSettings
    {
        /// <summary>
        /// رشته اتصال به پایگاه داده
        /// </summary>
        [Required]
        public string ConnectionString { get; set; }

        /// <summary>
        /// تعداد اتصالات همزمان
        /// </summary>
        [Range(1, 100)]
        public int MaxPoolSize { get; set; } = 100;

        /// <summary>
        /// زمان انتظار برای اتصال (ثانیه)
        /// </summary>
        [Range(1, 60)]
        public int ConnectionTimeout { get; set; } = 30;

        /// <summary>
        /// آیا از دستورات ذخیره شده استفاده شود
        /// </summary>
        public bool UseStoredProcedures { get; set; } = true;

        /// <summary>
        /// آیا از تراکنش‌ها استفاده شود
        /// </summary>
        public bool UseTransactions { get; set; } = true;
    }
}