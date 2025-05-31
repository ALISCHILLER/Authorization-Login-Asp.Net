using FluentValidation;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Domain.Enums;

namespace Authorization_Login_Asp.Net.Application.Validators
{
    /// <summary>
    /// اعتبارسنجی مدل پایه کاربر
    /// </summary>
    public class UserDtoValidator : AbstractValidator<UserDto>
    {
        public UserDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("شناسه کاربر الزامی است.");

            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("نام کاربری الزامی است.")
                .MaximumLength(50)
                .WithMessage("نام کاربری نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("ایمیل الزامی است.")
                .EmailAddress()
                .WithMessage("فرمت ایمیل معتبر نیست.")
                .MaximumLength(100)
                .WithMessage("ایمیل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("نقش کاربر الزامی است.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15)
                .WithMessage("شماره تلفن نمی‌تواند بیشتر از ۱۵ کاراکتر باشد.")
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("فرمت شماره تلفن معتبر نیست.");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500)
                .WithMessage("آدرس تصویر پروفایل نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("آدرس تصویر پروفایل باید یک URL معتبر باشد.");
        }
    }

    /// <summary>
    /// اعتبارسنجی مدل ایجاد کاربر
    /// </summary>
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("نام کاربری الزامی است.")
                .MaximumLength(50)
                .WithMessage("نام کاربری نمی‌تواند بیشتر از ۵۰ کاراکتر باشد.")
                .Matches(@"^[a-zA-Z0-9_]+$")
                .WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و کاراکتر _ باشد.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("ایمیل الزامی است.")
                .EmailAddress()
                .WithMessage("فرمت ایمیل معتبر نیست.")
                .MaximumLength(100)
                .WithMessage("ایمیل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("رمز عبور الزامی است.")
                .MinimumLength(8)
                .WithMessage("رمز عبور باید حداقل ۸ کاراکتر باشد.")
                .Matches(@"[A-Z]")
                .WithMessage("رمز عبور باید حداقل شامل یک حرف بزرگ باشد.")
                .Matches(@"[a-z]")
                .WithMessage("رمز عبور باید حداقل شامل یک حرف کوچک باشد.")
                .Matches(@"[0-9]")
                .WithMessage("رمز عبور باید حداقل شامل یک عدد باشد.")
                .Matches(@"[!@#$%^&*(),.?""':{}|<>]")
                .WithMessage("رمز عبور باید حداقل شامل یک کاراکتر خاص باشد.");

            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.RoleId)
                .NotEmpty()
                .WithMessage("نقش کاربر الزامی است.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15)
                .WithMessage("شماره تلفن نمی‌تواند بیشتر از ۱۵ کاراکتر باشد.")
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("فرمت شماره تلفن معتبر نیست.");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500)
                .WithMessage("آدرس تصویر پروفایل نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("آدرس تصویر پروفایل باید یک URL معتبر باشد.");

            RuleFor(x => x.TwoFactorType)
                .IsInEnum()
                .When(x => x.EnableTwoFactor)
                .WithMessage("روش احراز هویت دو مرحله‌ای نامعتبر است.");
        }
    }

    /// <summary>
    /// اعتبارسنجی مدل به‌روزرسانی کاربر
    /// </summary>
    public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
    {
        public UpdateUserDtoValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .WithMessage("نام کامل نمی‌تواند بیشتر از ۱۰۰ کاراکتر باشد.");

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(15)
                .WithMessage("شماره تلفن نمی‌تواند بیشتر از ۱۵ کاراکتر باشد.")
                .Matches(@"^\+?[0-9]{10,15}$")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
                .WithMessage("فرمت شماره تلفن معتبر نیست.");

            RuleFor(x => x.ProfileImageUrl)
                .MaximumLength(500)
                .WithMessage("آدرس تصویر پروفایل نمی‌تواند بیشتر از ۵۰۰ کاراکتر باشد.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .When(x => !string.IsNullOrEmpty(x.ProfileImageUrl))
                .WithMessage("آدرس تصویر پروفایل باید یک URL معتبر باشد.");

            RuleFor(x => x.TwoFactorType)
                .IsInEnum()
                .When(x => x.EnableTwoFactor == true)
                .WithMessage("روش احراز هویت دو مرحله‌ای نامعتبر است.");
        }
    }

    /// <summary>
    /// اعتبارسنجی مدل تغییر رمز عبور
    /// </summary>
    public class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
    {
        public ChangePasswordDtoValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("رمز عبور فعلی الزامی است.");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("رمز عبور جدید الزامی است.")
                .MinimumLength(8)
                .WithMessage("رمز عبور جدید باید حداقل ۸ کاراکتر باشد.")
                .Matches(@"[A-Z]")
                .WithMessage("رمز عبور جدید باید حداقل شامل یک حرف بزرگ باشد.")
                .Matches(@"[a-z]")
                .WithMessage("رمز عبور جدید باید حداقل شامل یک حرف کوچک باشد.")
                .Matches(@"[0-9]")
                .WithMessage("رمز عبور جدید باید حداقل شامل یک عدد باشد.")
                .Matches(@"[!@#$%^&*(),.?""':{}|<>]")
                .WithMessage("رمز عبور جدید باید حداقل شامل یک کاراکتر خاص باشد.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("رمز عبور جدید نمی‌تواند با رمز عبور فعلی یکسان باشد.");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty()
                .WithMessage("تکرار رمز عبور جدید الزامی است.")
                .Equal(x => x.NewPassword)
                .WithMessage("تکرار رمز عبور جدید با رمز عبور جدید مطابقت ندارد.");
        }
    }

    /// <summary>
    /// اعتبارسنجی مدل تنظیمات امنیتی کاربر
    /// </summary>
    public class UserSecuritySettingsDtoValidator : AbstractValidator<UserSecuritySettingsDto>
    {
        public UserSecuritySettingsDtoValidator()
        {
            RuleFor(x => x.MaxFailedLoginAttempts)
                .GreaterThan(0)
                .WithMessage("تعداد حداکثر تلاش‌های ناموفق ورود باید بزرگتر از صفر باشد.")
                .LessThanOrEqualTo(10)
                .WithMessage("تعداد حداکثر تلاش‌های ناموفق ورود نمی‌تواند بیشتر از ۱۰ باشد.");

            RuleFor(x => x.AccountLockoutDurationMinutes)
                .GreaterThan(0)
                .WithMessage("مدت زمان قفل شدن حساب باید بزرگتر از صفر باشد.")
                .LessThanOrEqualTo(1440)
                .WithMessage("مدت زمان قفل شدن حساب نمی‌تواند بیشتر از ۲۴ ساعت باشد.");

            RuleFor(x => x.RefreshTokenExpiryDays)
                .GreaterThan(0)
                .WithMessage("مدت زمان اعتبار توکن رفرش باید بزرگتر از صفر باشد.")
                .LessThanOrEqualTo(30)
                .WithMessage("مدت زمان اعتبار توکن رفرش نمی‌تواند بیشتر از ۳۰ روز باشد.");

            RuleFor(x => x.RecoveryCodesCount)
                .GreaterThan(0)
                .WithMessage("تعداد کدهای بازیابی باید بزرگتر از صفر باشد.")
                .LessThanOrEqualTo(10)
                .WithMessage("تعداد کدهای بازیابی نمی‌تواند بیشتر از ۱۰ باشد.");

            RuleFor(x => x.PasswordExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .When(x => x.PasswordExpiryDate.HasValue)
                .WithMessage("تاریخ انقضای رمز عبور باید در آینده باشد.");
        }
    }
} 