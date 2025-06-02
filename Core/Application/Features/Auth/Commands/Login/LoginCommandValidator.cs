using FluentValidation;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands.Login;

/// <summary>
/// اعتبارسنج دستور ورود کاربر
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("نام کاربری یا ایمیل الزامی است")
            .MinimumLength(3).WithMessage("نام کاربری یا ایمیل باید حداقل 3 کاراکتر باشد")
            .MaximumLength(100).WithMessage("نام کاربری یا ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور باید حداقل 6 کاراکتر باشد")
            .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از 100 کاراکتر باشد")
            .Matches("[A-Z]").WithMessage("رمز عبور باید حداقل شامل یک حرف بزرگ باشد")
            .Matches("[a-z]").WithMessage("رمز عبور باید حداقل شامل یک حرف کوچک باشد")
            .Matches("[0-9]").WithMessage("رمز عبور باید حداقل شامل یک عدد باشد")
            .Matches("[^a-zA-Z0-9]").WithMessage("رمز عبور باید حداقل شامل یک کاراکتر خاص باشد");

        RuleFor(x => x.IpAddress)
            .NotEmpty().WithMessage("آدرس IP الزامی است")
            .Matches(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$")
            .When(x => !string.IsNullOrEmpty(x.IpAddress))
            .WithMessage("آدرس IP نامعتبر است");

        RuleFor(x => x.DeviceToken)
            .MaximumLength(500).WithMessage("توکن دستگاه نمی‌تواند بیشتر از 500 کاراکتر باشد");
    }
} 