using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Infrastructure.Security
{
    /// <summary>
    /// تنظیمات JWT
    /// </summary>
    public class JwtSettings
    {
        /// <summary>
        /// کلید مخفی برای امضای توکن
        /// </summary>
        [Required(ErrorMessage = "SecretKey is required")]
        [MinLength(32, ErrorMessage = "SecretKey must be at least 32 characters long")]
        public string SecretKey { get; set; }

        /// <summary>
        /// صادرکننده توکن
        /// </summary>
        [Required(ErrorMessage = "Issuer is required")]
        public string Issuer { get; set; }

        /// <summary>
        /// مخاطب توکن
        /// </summary>
        [Required(ErrorMessage = "Audience is required")]
        public string Audience { get; set; }

        /// <summary>
        /// مدت زمان اعتبار توکن (دقیقه)
        /// </summary>
        [Range(1, 1440, ErrorMessage = "ExpirationInMinutes must be between 1 and 1440")]
        public int ExpirationInMinutes { get; set; } = 60;

        /// <summary>
        /// مدت زمان اعتبار توکن بازیابی (روز)
        /// </summary>
        [Range(1, 30, ErrorMessage = "RefreshTokenExpirationDays must be between 1 and 30")]
        public int RefreshTokenExpirationDays { get; set; } = 7;

        /// <summary>
        /// حداکثر تعداد توکن‌های بازیابی فعال برای هر کاربر
        /// </summary>
        [Range(1, 10, ErrorMessage = "MaxActiveRefreshTokensPerUser must be between 1 and 10")]
        public int MaxActiveRefreshTokensPerUser { get; set; } = 5;

        /// <summary>
        /// آیا باید توکن‌های قدیمی را لغو کرد
        /// </summary>
        public bool RevokeOldTokens { get; set; } = true;

        /// <summary>
        /// آیا باید توکن‌ها را در کش ذخیره کرد
        /// </summary>
        public bool CacheTokens { get; set; } = true;

        /// <summary>
        /// مدت زمان اعتبار کش توکن‌ها (دقیقه)
        /// </summary>
        [Range(1, 1440, ErrorMessage = "TokenCacheExpirationMinutes must be between 1 and 1440")]
        public int TokenCacheExpirationMinutes { get; set; } = 60;

        /// <summary>
        /// آیا باید از الگوریتم‌های رمزنگاری قوی استفاده کرد
        /// </summary>
        public bool UseStrongEncryption { get; set; } = true;

        /// <summary>
        /// آیا باید از claims اضافی استفاده کرد
        /// </summary>
        public bool UseAdditionalClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از IP در claims استفاده کرد
        /// </summary>
        public bool IncludeIpInClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از User Agent در claims استفاده کرد
        /// </summary>
        public bool IncludeUserAgentInClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از Device ID در claims استفاده کرد
        /// </summary>
        public bool IncludeDeviceIdInClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از Location در claims استفاده کرد
        /// </summary>
        public bool IncludeLocationInClaims { get; set; } = false;

        /// <summary>
        /// آیا باید از Role در claims استفاده کرد
        /// </summary>
        public bool IncludeRoleInClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از Permissions در claims استفاده کرد
        /// </summary>
        public bool IncludePermissionsInClaims { get; set; } = true;

        /// <summary>
        /// آیا باید از Custom Claims استفاده کرد
        /// </summary>
        public bool UseCustomClaims { get; set; } = false;

        /// <summary>
        /// Custom Claims
        /// </summary>
        public Dictionary<string, string> CustomClaims { get; set; } = new Dictionary<string, string>();
    }
} 