using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Application.Services.Interfaces;
using Application.DTOs;
using Application.Results;
using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Services;

namespace Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
        }

        // ثبت نام کاربر جدید
        public async Task<Result<UserDto>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // بررسی تکراری نبودن ایمیل
                if (await _userRepository.ExistsByEmailAsync(request.Email))
                {
                    return Result<UserDto>.Failure("ایمیل قبلاً ثبت شده است");
                }

                // ایجاد کاربر جدید
                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FullName = request.FullName,
                    Role = Role.User
                };

                // هش کردن رمز عبور
                user.SetPassword(request.Password);

                // ذخیره کاربر در دیتابیس
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                // ارسال ایمیل تایید
                await _emailService.SendConfirmationEmailAsync(user.Email, user.Id);

                // لاگ کردن ثبت نام موفق
                await _logger.LogInformationAsync($"کاربر جدید با ایمیل {user.Email} ثبت نام کرد");

                return Result<UserDto>.Success(new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در ثبت نام کاربر");
                return Result<UserDto>.Failure("خطا در ثبت نام کاربر");
            }
        }

        // ورود کاربر
        public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // پیدا کردن کاربر با ایمیل
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return Result<AuthResponse>.Failure("ایمیل یا رمز عبور اشتباه است");
                }

                // بررسی رمز عبور
                if (!user.VerifyPassword(request.Password))
                {
                    // افزایش تعداد تلاش‌های ناموفق
                    user.IncrementFailedLoginAttempts();
                    await _userRepository.SaveChangesAsync();

                    // لاگ کردن تلاش ناموفق
                    await _logger.LogWarningAsync($"تلاش ناموفق برای ورود با ایمیل {request.Email}");

                    return Result<AuthResponse>.Failure("ایمیل یا رمز عبور اشتباه است");
                }

                // بررسی قفل بودن حساب
                if (user.IsLocked)
                {
                    return Result<AuthResponse>.Failure("حساب کاربری قفل شده است");
                }

                // بررسی تایید ایمیل
                if (!user.IsEmailConfirmed)
                {
                    return Result<AuthResponse>.Failure("لطفاً ابتدا ایمیل خود را تایید کنید");
                }

                // تولید توکن JWT
                var token = _jwtService.GenerateToken(user);

                // بازنشانی تعداد تلاش‌های ناموفق
                user.ResetFailedLoginAttempts();
                await _userRepository.SaveChangesAsync();

                // لاگ کردن ورود موفق
                await _logger.LogInformationAsync($"کاربر {user.Email} با موفقیت وارد شد");

                return Result<AuthResponse>.Success(new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در ورود کاربر");
                return Result<AuthResponse>.Failure("خطا در ورود کاربر");
            }
        }

        // تایید ایمیل
        public async Task<Result> ConfirmEmailAsync(string email, string token)
        {
            try
            {
                // پیدا کردن کاربر با ایمیل
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return Result.Failure("کاربر یافت نشد");
                }

                // بررسی اعتبار توکن
                if (!_jwtService.ValidateToken(token))
                {
                    return Result.Failure("توکن نامعتبر است");
                }

                // تایید ایمیل
                user.ConfirmEmail();
                await _userRepository.SaveChangesAsync();

                // لاگ کردن تایید ایمیل
                await _logger.LogInformationAsync($"ایمیل {email} تایید شد");

                return Result.Success();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در تایید ایمیل");
                return Result.Failure("خطا در تایید ایمیل");
            }
        }

        // درخواست بازنشانی رمز عبور
        public async Task<Result> ForgotPasswordAsync(string email)
        {
            try
            {
                // پیدا کردن کاربر با ایمیل
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    return Result.Failure("کاربر یافت نشد");
                }

                // تولید توکن بازنشانی رمز عبور
                var token = _jwtService.GeneratePasswordResetToken(user);

                // ارسال ایمیل بازنشانی رمز عبور
                await _emailService.SendPasswordResetEmailAsync(email, token);

                // لاگ کردن درخواست بازنشانی رمز عبور
                await _logger.LogInformationAsync($"درخواست بازنشانی رمز عبور برای {email} ارسال شد");

                return Result.Success();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در ارسال درخواست بازنشانی رمز عبور");
                return Result.Failure("خطا در ارسال درخواست بازنشانی رمز عبور");
            }
        }

        // بازنشانی رمز عبور
        public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
        {
            try
            {
                // پیدا کردن کاربر با ایمیل
                var user = await _userRepository.GetByEmailAsync(request.Email);
                if (user == null)
                {
                    return Result.Failure("کاربر یافت نشد");
                }

                // بررسی اعتبار توکن
                if (!_jwtService.ValidateToken(request.Token))
                {
                    return Result.Failure("توکن نامعتبر است");
                }

                // تغییر رمز عبور
                user.SetPassword(request.NewPassword);
                await _userRepository.SaveChangesAsync();

                // لاگ کردن تغییر رمز عبور
                await _logger.LogInformationAsync($"رمز عبور کاربر {request.Email} تغییر کرد");

                return Result.Success();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در بازنشانی رمز عبور");
                return Result.Failure("خطا در بازنشانی رمز عبور");
            }
        }

        // تغییر رمز عبور
        public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            try
            {
                // پیدا کردن کاربر با شناسه
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure("کاربر یافت نشد");
                }

                // بررسی رمز عبور فعلی
                if (!user.VerifyPassword(request.CurrentPassword))
                {
                    return Result.Failure("رمز عبور فعلی اشتباه است");
                }

                // تغییر رمز عبور
                user.SetPassword(request.NewPassword);
                await _userRepository.SaveChangesAsync();

                // لاگ کردن تغییر رمز عبور
                await _logger.LogInformationAsync($"رمز عبور کاربر {user.Email} تغییر کرد");

                return Result.Success();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در تغییر رمز عبور");
                return Result.Failure("خطا در تغییر رمز عبور");
            }
        }

        // فعال‌سازی احراز هویت دو مرحله‌ای
        public async Task<Result<TwoFactorResponse>> EnableTwoFactorAsync(string userId)
        {
            try
            {
                // پیدا کردن کاربر با شناسه
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<TwoFactorResponse>.Failure("کاربر یافت نشد");
                }

                // تولید کد QR برای احراز هویت دو مرحله‌ای
                var (secret, qrCode) = _jwtService.GenerateTwoFactorSecret(user);

                // ذخیره کلید مخفی
                user.SetTwoFactorSecret(secret);
                await _userRepository.SaveChangesAsync();

                // لاگ کردن فعال‌سازی احراز هویت دو مرحله‌ای
                await _logger.LogInformationAsync($"احراز هویت دو مرحله‌ای برای کاربر {user.Email} فعال شد");

                return Result<TwoFactorResponse>.Success(new TwoFactorResponse
                {
                    Secret = secret,
                    QrCode = qrCode
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در فعال‌سازی احراز هویت دو مرحله‌ای");
                return Result<TwoFactorResponse>.Failure("خطا در فعال‌سازی احراز هویت دو مرحله‌ای");
            }
        }

        // غیرفعال‌سازی احراز هویت دو مرحله‌ای
        public async Task<Result> DisableTwoFactorAsync(string userId, string code)
        {
            try
            {
                // پیدا کردن کاربر با شناسه
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result.Failure("کاربر یافت نشد");
                }

                // بررسی کد تایید
                if (!_jwtService.ValidateTwoFactorCode(user, code))
                {
                    return Result.Failure("کد تایید اشتباه است");
                }

                // غیرفعال‌سازی احراز هویت دو مرحله‌ای
                user.DisableTwoFactor();
                await _userRepository.SaveChangesAsync();

                // لاگ کردن غیرفعال‌سازی احراز هویت دو مرحله‌ای
                await _logger.LogInformationAsync($"احراز هویت دو مرحله‌ای برای کاربر {user.Email} غیرفعال شد");

                return Result.Success();
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای");
                return Result.Failure("خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای");
            }
        }

        // تایید کد احراز هویت دو مرحله‌ای
        public async Task<Result<AuthResponse>> VerifyTwoFactorAsync(string userId, string code)
        {
            try
            {
                // پیدا کردن کاربر با شناسه
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return Result<AuthResponse>.Failure("کاربر یافت نشد");
                }

                // بررسی کد تایید
                if (!_jwtService.ValidateTwoFactorCode(user, code))
                {
                    return Result<AuthResponse>.Failure("کد تایید اشتباه است");
                }

                // تولید توکن JWT
                var token = _jwtService.GenerateToken(user);

                // لاگ کردن تایید موفق
                await _logger.LogInformationAsync($"کد احراز هویت دو مرحله‌ای برای کاربر {user.Email} تایید شد");

                return Result<AuthResponse>.Success(new AuthResponse
                {
                    Token = token,
                    User = new UserDto
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        Role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                // لاگ کردن خطا
                await _logger.LogErrorAsync(ex, "خطا در تایید کد احراز هویت دو مرحله‌ای");
                return Result<AuthResponse>.Failure("خطا در تایید کد احراز هویت دو مرحله‌ای");
            }
        }
    }
} 