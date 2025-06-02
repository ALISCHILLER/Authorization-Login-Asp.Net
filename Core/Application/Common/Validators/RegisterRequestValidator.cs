using FluentValidation;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Validators
{
    /// <summary>
    /// اعتبارسنج برای مدل RegisterRequest
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("نام کاربری الزامی است")
                .MinimumLength(4).WithMessage("نام کاربری باید حداقل 4 کاراکتر باشد")
                .MaximumLength(50).WithMessage("نام کاربری نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("نام کاربری فقط می‌تواند شامل حروف انگلیسی، اعداد و آندرلاین باشد");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("ایمیل الزامی است")
                .EmailAddress().WithMessage("فرمت ایمیل نامعتبر است")
                .MaximumLength(100).WithMessage("ایمیل نمی‌تواند بیشتر از 100 کاراکتر باشد");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("رمز عبور الزامی است")
                .MinimumLength(8).WithMessage("رمز عبور باید حداقل 8 کاراکتر باشد")
                .MaximumLength(100).WithMessage("رمز عبور نمی‌تواند بیشتر از 100 کاراکتر باشد")
                .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
                .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
                .Matches(@"[0-9]").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد")
                .Matches(@"[^a-zA-Z0-9]").WithMessage("رمز عبور باید حداقل یک کاراکتر خاص داشته باشد");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("تکرار رمز عبور الزامی است")
                .Equal(x => x.Password).WithMessage("رمز عبور و تکرار آن باید یکسان باشند");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("نام الزامی است")
                .MaximumLength(50).WithMessage("نام نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام باید به فارسی وارد شود");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("نام خانوادگی الزامی است")
                .MaximumLength(50).WithMessage("نام خانوادگی نمی‌تواند بیشتر از 50 کاراکتر باشد")
                .Matches(@"^[\u0600-\u06FF\s]+$").WithMessage("نام خانوادگی باید به فارسی وارد شود");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("شماره تلفن الزامی است")
                .Matches(@"^09[0-9]{9}$").WithMessage("فرمت شماره تلفن نامعتبر است")
                .MaximumLength(11).WithMessage("شماره تلفن نمی‌تواند بیشتر از 11 کاراکتر باشد");
        }
    }
} 