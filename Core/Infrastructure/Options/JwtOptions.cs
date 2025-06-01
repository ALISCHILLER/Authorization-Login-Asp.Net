using System.ComponentModel.DataAnnotations;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Options
{
    /// <summary>
    /// تنظیمات JWT
    /// این کلاس تنظیمات مورد نیاز برای مدیریت توکن‌های JWT را تعریف می‌کند
    /// </summary>
    public class JwtOptions
    {
        /// <summary>
        /// کلید مخفی برای امضای توکن
        /// </summary>
        [Required(ErrorMessage = "کلید مخفی الزامی است")]
        [MinLength(32, ErrorMessage = "کلید مخفی باید حداقل 32 کاراکتر باشد")]
        public string SecretKey { get; set; }

        /// <summary>
        /// صادرکننده توکن
        /// </summary>
        [Required(ErrorMessage = "صادرکننده توکن الزامی است")]
        public string Issuer { get; set; }

        /// <summary>
        /// مخاطب توکن
        /// </summary>
        [Required(ErrorMessage = "مخاطب توکن الزامی است")]
        public string Audience { get; set; }

        /// <summary>
        /// مدت زمان اعتبار توکن (دقیقه)
        /// </summary>
        [Range(1, 1440, ErrorMessage = "مدت زمان اعتبار توکن باید بین 1 تا 1440 دقیقه باشد")]
        public int ExpiryMinutes { get; set; } = 60;

        /// <summary>
        /// مدت زمان اعتبار توکن بازیابی (روز)
        /// </summary>
        [Range(1, 30, ErrorMessage = "مدت زمان اعتبار توکن بازیابی باید بین 1 تا 30 روز باشد")]
        public int RefreshTokenExpiryDays { get; set; } = 7;

        /// <summary>
        /// حداکثر تعداد توکن‌های بازیابی فعال برای هر کاربر
        /// </summary>
        [Range(1, 10, ErrorMessage = "حداکثر تعداد توکن‌های بازیابی فعال باید بین 1 تا 10 باشد")]
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
        [Range(1, 1440, ErrorMessage = "مدت زمان اعتبار کش توکن‌ها باید بین 1 تا 1440 دقیقه باشد")]
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

        /// <summary>
        /// اعتبارسنجی تنظیمات
        /// </summary>
        /// <returns>نتیجه اعتبارسنجی</returns>
        public ValidationResult Validate()
        {
            var validationContext = new ValidationContext(this);
            var validationResults = new List<ValidationResult>();

            if (!Validator.TryValidateObject(this, validationContext, validationResults, true))
            {
                return new ValidationResult(
                    string.Join(", ", validationResults.Select(r => r.ErrorMessage)),
                    validationResults.SelectMany(r => r.MemberNames)
                );
            }

            return ValidationResult.Success;
        }
    }
}