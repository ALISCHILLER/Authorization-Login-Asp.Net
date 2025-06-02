using FluentValidation;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// کلاس پایه برای اعتبارسنج‌ها
    /// این کلاس شامل قوانین اعتبارسنجی مشترک است
    /// </summary>
    /// <typeparam name="T">نوع مدل مورد اعتبارسنجی</typeparam>
    public abstract class BaseValidator<T> : AbstractValidator<T>
    {
        /// <summary>
        /// سازنده کلاس پایه
        /// </summary>
        protected BaseValidator()
        {
            // قوانین اعتبارسنجی مشترک در اینجا تعریف می‌شوند
        }

        /// <summary>
        /// اعتبارسنجی آدرس ایمیل
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidateEmail(IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("ایمیل الزامی است")
                .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است")
                .MaximumLength(100).WithMessage("ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد");
        }

        /// <summary>
        /// اعتبارسنجی رمز عبور
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidatePassword(IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("رمز عبور الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل 8 کاراکتر باشد")
                .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"[0-9]").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("رمز عبور باید حداقل یک کاراکتر خاص داشته باشد");
        }

        /// <summary>
        /// اعتبارسنجی نام کاربری
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidateUsername(IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("نام کاربری الزامی است")
                .MinimumLength(4).WithMessage("نام کاربری باید حداقل 4 کاراکتر باشد")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد");
        }

        /// <summary>
        /// اعتبارسنجی نام فارسی
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <param name="propertyName">نام فیلد</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidatePersianName(IRuleBuilder<T, string> ruleBuilder, string propertyName)
        {
            return ruleBuilder
                .NotEmpty().WithMessage($"{propertyName} الزامی است")
                .MaximumLength(50).WithMessage($"{propertyName} نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage($"{propertyName} باید به فارسی وارد شود");
        }

        /// <summary>
        /// اعتبارسنجی شماره تلفن
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidatePhoneNumber(IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن نامعتبر است")
                .MaximumLength(11).WithMessage("شماره تلفن نمی‌تواند بیشتر از 11 کاراکتر باشد");
        }

        /// <summary>
        /// اعتبارسنجی آدرس IP
        /// </summary>
        /// <param name="ruleBuilder">سازنده قانون</param>
        /// <returns>سازنده قانون</returns>
        protected IRuleBuilderOptions<T, string> ValidateIpAddress(IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty().WithMessage("آدرس IP الزامی است")
                .MaximumLength(45).WithMessage("آدرس IP نمی‌تواند بیشتر از 45 کاراکتر باشد")
                .Matches(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")
                .WithMessage("فرمت آدرس IP نامعتبر است");
        }
    }
} 