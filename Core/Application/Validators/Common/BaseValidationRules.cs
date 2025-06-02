using FluentValidation;

namespace Authorization_Login_Asp.Net.Core.Application.Validators.Common
{
    /// <summary>
    /// کلاس پایه برای قوانین مشترک اعتبارسنجی
    /// </summary>
    public static class BaseValidationRules
    {
        /// <summary>
        /// اعمال قوانین اعتبارسنجی رمز عبور
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("رمز عبور الزامی است.")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
                .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.")
                .Matches("[A-Z]").WithMessage("رمز عبور باید حداقل شامل یک حرف بزرگ باشد.")
                .Matches("[a-z]").WithMessage("رمز عبور باید حداقل شامل یک حرف کوچک باشد.")
                .Matches("[0-9]").WithMessage("رمز عبور باید حداقل شامل یک عدد باشد.")
                .Matches("[^a-zA-Z0-9]").WithMessage("رمز عبور باید حداقل شامل یک کاراکتر خاص باشد.");
        }

        /// <summary>
        /// اعمال قوانین اعتبارسنجی ایمیل
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("ایمیل الزامی است.")
                .EmailAddress().WithMessage("فرمت ایمیل معتبر نیست.")
                .MaximumLength(100).WithMessage("ایمیل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");
        }

        /// <summary>
        /// اعمال قوانین اعتبارسنجی نام کاربری
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyUsernameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("نام کاربری الزامی است.")
                .MinimumLength(3).WithMessage("نام کاربری باید حداقل ۳ کاراکتر باشد.")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")
                .Matches("^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد.");
        }

        /// <summary>
        /// اعمال قوانین اعتبارسنجی نام کامل
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyFullNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("نام کامل الزامی است.")
                .MaximumLength(100).WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");
        }

        /// <summary>
        /// اعمال قوانین اعتبارسنجی آدرس IP
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyIpAddressRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("آدرس IP الزامی است.")
                .Matches(@"^(\d{1,3}\.){3}\d{1,3}$").WithMessage("فرمت آدرس IP معتبر نیست.")
                .MaximumLength(45).WithMessage("آدرس IP نمی‌تواند بیشتر از ۴۵ کاراکتر باشد.");
        }

        /// <summary>
        /// اعمال قوانین اعتبارسنجی اطلاعات دستگاه
        /// </summary>
        public static IRuleBuilderOptions<T, string> ApplyDeviceInfoRules<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("اطلاعات دستگاه الزامی است.")
                .MaximumLength(500).WithMessage("اطلاعات دستگاه نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.");
        }
    }
} 