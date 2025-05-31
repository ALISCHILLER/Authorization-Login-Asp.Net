using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Application.DTOs;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Common;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Domain.Enums;
using Authorization_Login_Asp.Net.Domain.ValueObjects;
using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;
        private readonly IImageService _imageService;
        private readonly ITracingService _tracingService;

        public UserService(
            IUserRepository userRepository,
            ILoginHistoryRepository loginHistoryRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
            IImageService imageService,
            ITracingService tracingService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _loginHistoryRepository = loginHistoryRepository ?? throw new ArgumentNullException(nameof(loginHistoryRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
        }

        #region احراز هویت و مجوزدهی
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.Register",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("RegisterUser", ActivityKind.Server);
                        activity?.SetTag("user.email", request.Email);
                        activity?.SetTag("user.username", request.Username);

                        if (await _userRepository.ExistsByEmailAsync(request.Email))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "ایمیل قبلاً ثبت شده است");
                            throw new ApplicationException("ایمیل قبلاً ثبت شده است");
                        }

                        var user = new User
                        {
                            Username = request.Username,
                            Email = new Email(request.Email),
                            FullName = request.FullName,
                            Role = RoleType.User,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        user.SetPassword(request.Password);

                        await _userRepository.AddAsync(user);
                        await _userRepository.SaveChangesAsync();

                        // Generate confirmation token and send email
                        var confirmationToken = Guid.NewGuid().ToString();
                        await _emailService.SendConfirmationEmailAsync(user.Email.Value, confirmationToken);

                        // Generate JWT token
                        var token = _jwtService.GenerateToken(user.Id, user.Username, user.Role.ToString());

                        await _logger.LogInformationAsync($"کاربر جدید با ایمیل {user.Email.Value} ثبت نام کرد");

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        return new AuthResponse
                        {
                            Token = token,
                            User = new UserDto
                            {
                                Id = user.Id,
                                Username = user.Username,
                                Email = user.Email.Value,
                                FullName = user.FullName,
                                Role = user.Role.ToString()
                            }
                        };
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در ثبت نام کاربر");
                        throw;
                    }
                });
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.Login",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("UserLogin", ActivityKind.Server);
                        activity?.SetTag("user.email", request.Email);
                        activity?.SetTag("login.ip", request.IpAddress);
                        activity?.SetTag("login.userAgent", request.UserAgent);

                        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر با این ایمیل یافت نشد");
                            throw new ApplicationException("کاربر با این ایمیل یافت نشد");
                        }

                        // Create login history entry
                        var loginHistory = new LoginHistory
                        {
                            UserId = user.Id,
                            LoginTime = DateTime.UtcNow,
                            IpAddress = request.IpAddress,
                            UserAgent = request.UserAgent,
                            Location = request.Location,
                            IsSuccessful = false
                        };

                        if (!user.VerifyPassword(request.Password))
                        {
                            user.IncrementFailedLoginAttempts();
                            loginHistory.FailureReason = "رمز عبور اشتباه است";
                            await _userRepository.SaveChangesAsync(cancellationToken);
                            await _loginHistoryRepository.AddAsync(loginHistory, cancellationToken);
                            await _loginHistoryRepository.SaveChangesAsync(cancellationToken);
                            activity?.SetStatus(ActivityStatusCode.Error, "رمز عبور اشتباه است");
                            throw new ApplicationException("رمز عبور اشتباه است");
                        }

                        if (user.IsAccountLocked())
                        {
                            loginHistory.FailureReason = "حساب کاربری قفل شده است";
                            await _loginHistoryRepository.AddAsync(loginHistory, cancellationToken);
                            await _loginHistoryRepository.SaveChangesAsync(cancellationToken);
                            activity?.SetStatus(ActivityStatusCode.Error, "حساب کاربری قفل شده است");
                            throw new ApplicationException("حساب کاربری شما قفل شده است. لطفاً بعداً تلاش کنید");
                        }

                        if (!user.IsEmailVerified)
                        {
                            loginHistory.FailureReason = "ایمیل تأیید نشده است";
                            await _loginHistoryRepository.AddAsync(loginHistory, cancellationToken);
                            await _loginHistoryRepository.SaveChangesAsync(cancellationToken);
                            activity?.SetStatus(ActivityStatusCode.Error, "ایمیل تأیید نشده است");
                            throw new ApplicationException("لطفاً ابتدا ایمیل خود را تأیید کنید");
                        }

                        // Generate tokens with expiration
                        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);
                        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                        var accessToken = _jwtService.GenerateToken(user.Id, user.Username, user.Role.ToString());
                        var refreshToken = _jwtService.GenerateRefreshToken(user);

                        // Get user permissions
                        var claims = _jwtService.GenerateClaims(user);
                        var permissions = claims.Where(c => c.Type == "permission").Select(c => c.Value).ToList();

                        // Update user's last login time and reset failed attempts
                        user.LastLoginAt = DateTime.UtcNow;
                        user.ResetFailedLoginAttempts();
                        loginHistory.IsSuccessful = true;

                        await _userRepository.SaveChangesAsync(cancellationToken);
                        await _loginHistoryRepository.AddAsync(loginHistory, cancellationToken);
                        await _loginHistoryRepository.SaveChangesAsync(cancellationToken);

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("login.success", true);
                        activity?.SetTag("user.id", user.Id.ToString());
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.permissions", string.Join(",", permissions));
                        activity?.SetTag("token.access.expires_at", accessTokenExpiresAt.ToString("o"));
                        activity?.SetTag("token.refresh.expires_at", refreshTokenExpiresAt.ToString("o"));
                        activity?.SetTag("user.2fa_enabled", user.TwoFactorEnabled);
                        activity?.SetTag("user.email_verified", user.IsEmailVerified);
                        activity?.SetTag("user.phone_verified", user.IsPhoneVerified);
                        if (user.TwoFactorEnabled)
                        {
                            activity?.SetTag("user.2fa_type", user.TwoFactorType.ToString());
                        }

                        await _logger.LogInformationAsync($"کاربر {user.Email.Value} با موفقیت وارد شد");

                        return new AuthResponse
                        {
                            UserId = user.Id,
                            Username = user.Username,
                            Email = user.Email.Value,
                            FullName = user.FullName,
                            Role = user.Role.ToString(),
                            IsEmailVerified = user.IsEmailVerified,
                            IsPhoneVerified = user.IsPhoneVerified,
                            TwoFactorEnabled = user.TwoFactorEnabled,
                            TwoFactorType = user.TwoFactorType,
                            Permissions = permissions,
                            AccessToken = accessToken,
                            AccessTokenExpiresAt = accessTokenExpiresAt,
                            RefreshToken = refreshToken,
                            RefreshTokenExpiresAt = refreshTokenExpiresAt
                        };
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در ورود کاربر");
                        throw;
                    }
                });
        }

        public async Task<AuthResult> ValidateTwoFactorAsync(TwoFactorDto model)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.ValidateTwoFactor",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("ValidateTwoFactor", ActivityKind.Server);
                        activity?.SetTag("user.id", model.UserId.ToString());

                        var user = await _userRepository.GetByIdAsync(model.UserId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                        }

                        if (!_jwtService.ValidateTwoFactorCode(user, model.Code))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کد تایید اشتباه است");
                            return new AuthResult { Succeeded = false, Message = "کد تایید اشتباه است" };
                        }

                        var token = _jwtService.GenerateToken(user);
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("auth.success", true);

                        return new AuthResult
                        {
                            Succeeded = true,
                            Message = "کد تایید شد",
                            User = user,
                            Token = token
                        };
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در تایید کد احراز هویت دو مرحله‌ای");
                        return new AuthResult { Succeeded = false, Message = "خطا در تایید کد" };
                    }
                });
        }

        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto model)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.RefreshToken",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("RefreshToken", ActivityKind.Server);
                        activity?.SetTag("user.id", model.UserId.ToString());

                        var user = await _userRepository.GetByIdAsync(model.UserId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                        }

                        if (!_jwtService.ValidateRefreshToken(user, model.RefreshToken))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "توکن رفرش نامعتبر است");
                            return new AuthResult { Succeeded = false, Message = "توکن رفرش نامعتبر است" };
                        }

                        var accessTokenExpiresAt = DateTime.UtcNow.AddHours(1);
                        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);
                        var accessToken = _jwtService.GenerateToken(user);
                        var refreshToken = _jwtService.GenerateRefreshToken(user);

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("auth.success", true);
                        activity?.SetTag("token.access.expires_at", accessTokenExpiresAt.ToString("o"));
                        activity?.SetTag("token.refresh.expires_at", refreshTokenExpiresAt.ToString("o"));
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.2fa_enabled", user.TwoFactorEnabled);

                        return new AuthResult
                        {
                            Succeeded = true,
                            Message = "توکن با موفقیت تمدید شد",
                            User = user,
                            Token = accessToken,
                            RefreshToken = refreshToken,
                            AccessTokenExpiresAt = accessTokenExpiresAt,
                            RefreshTokenExpiresAt = refreshTokenExpiresAt
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در تمدید توکن");
                        return new AuthResult { Succeeded = false, Message = "خطا در تمدید توکن" };
                    }
                });
        }

        public async Task LogoutAsync(Guid userId)
        {
            await _tracingService.ExecuteInActivityAsync(
                "UserService.Logout",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("Logout", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var previousRefreshToken = user.RefreshToken;
                        user.InvalidateRefreshToken();
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("logout.success", true);
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("token.refresh.invalidated", true);
                        activity?.SetTag("user.2fa_enabled", user.TwoFactorEnabled);
                        activity?.SetTag("user.last_login", user.LastLoginAt?.ToString("o"));

                        await _logger.LogInformationAsync($"کاربر {user.Email.Value} با موفقیت خارج شد");
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در خروج کاربر");
                        throw;
                    }
                });
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.ValidateCredentials",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("ValidateCredentials", ActivityKind.Server);
                        activity?.SetTag("user.username", username);

                        var user = await _userRepository.GetByUsernameAsync(username);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return false;
                        }

                        var isValid = user.VerifyPassword(password);
                        activity?.SetStatus(isValid ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
                        activity?.SetTag("validation.success", isValid);
                        activity?.SetTag("user.id", user.Id.ToString());
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.2fa_enabled", user.TwoFactorEnabled);
                        activity?.SetTag("user.email_verified", user.IsEmailVerified);
                        activity?.SetTag("user.phone_verified", user.IsPhoneVerified);
                        activity?.SetTag("user.account_locked", user.IsAccountLocked());

                        if (!isValid)
                        {
                            activity?.SetTag("validation.failure_reason", "رمز عبور اشتباه است");
                        }

                        return isValid;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در بررسی اعتبارنامه‌های کاربر");
                        return false;
                    }
                });
        }
        #endregion

        #region مدیریت کاربران
        public async Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت اطلاعات کاربر");
                throw;
            }
        }

        public async Task<UserResponse> GetUserByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت اطلاعات کاربر");
                throw;
            }
        }

        public async Task<UserResponse> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(email);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت اطلاعات کاربر");
                throw;
            }
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                return users.Select(MapToUserResponse);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت لیست کاربران");
                throw;
            }
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            try
            {
                if (await _userRepository.ExistsByEmailAsync(request.Email))
                {
                    throw new ApplicationException("ایمیل قبلاً ثبت شده است");
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = Email.From(request.Email),
                    FullName = $"{request.FirstName} {request.LastName}",
                    PhoneNumber = request.PhoneNumber,
                    Role = RoleType.User
                };

                user.SetPassword(request.Password);
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کاربر جدید با ایمیل {user.Email.Value} ایجاد شد");

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در ایجاد کاربر");
                throw;
            }
        }

        public async Task<UserResponse> UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.Username = request.Username ?? user.Username;
                if (request.Email != null)
                {
                    user.Email = Email.From(request.Email);
                }
                user.FullName = $"{request.FirstName ?? user.FirstName} {request.LastName ?? user.LastName}";
                user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
                user.IsActive = request.IsActive ?? user.IsActive;
                user.IsEmailVerified = request.IsEmailVerified ?? user.IsEmailVerified;

                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"اطلاعات کاربر {user.Email.Value} به‌روز شد");

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی کاربر");
                throw;
            }
        }

        public async Task<bool> DeleteUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                await _userRepository.DeleteAsync(user);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کاربر {user.Email.Value} حذف شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف کاربر");
                return false;
            }
        }

        public async Task<bool> ActivateUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = true;
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کاربر {user.Email.Value} فعال شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در فعال‌سازی کاربر");
                return false;
            }
        }

        public async Task<bool> DeactivateUserAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.IsActive = false;
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کاربر {user.Email.Value} غیرفعال شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در غیرفعال‌سازی کاربر");
                return false;
            }
        }

        public async Task<bool> IsUsernameUniqueAsync(string username)
        {
            try
            {
                return !await _userRepository.ExistsByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی یکتا بودن نام کاربری");
                return false;
            }
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            try
            {
                return !await _userRepository.ExistsByEmailAsync(email);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی یکتا بودن ایمیل");
                return false;
            }
        }
        #endregion

        #region مدیریت پروفایل
        public async Task<UserResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest profileData)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.FullName = profileData.FullName;
                user.PhoneNumber = profileData.PhoneNumber;

                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"پروفایل کاربر {user.Email.Value} به‌روز شد");

                return MapToUserResponse(user);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی پروفایل");
                throw;
            }
        }

        public async Task<string> UploadProfileImageAsync(Guid userId, byte[] imageData)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                // آپلود تصویر و دریافت آدرس
                var imageUrl = await _imageService.UploadImageAsync(imageData);
                user.ProfileImageUrl = imageUrl;

                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"تصویر پروفایل کاربر {user.Email.Value} به‌روز شد");

                return imageUrl;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در آپلود تصویر پروفایل");
                throw;
            }
        }

        public async Task ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            await _tracingService.ExecuteInActivityAsync(
                "UserService.ChangePassword",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("ChangePassword", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        if (!user.VerifyPassword(currentPassword))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "رمز عبور فعلی اشتباه است");
                            throw new ApplicationException("رمز عبور فعلی اشتباه است");
                        }

                        user.SetPassword(newPassword);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("password.changed", true);
                        activity?.SetTag("password.changed_at", DateTime.UtcNow.ToString("o"));

                        await _logger.LogInformationAsync($"رمز عبور کاربر {user.Email.Value} تغییر کرد");
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در تغییر رمز عبور");
                        throw;
                    }
                });
        }

        public async Task<bool> ResetPasswordAsync(Guid id, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.SetPassword(newPassword);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"رمز عبور کاربر {user.Email.Value} بازنشانی شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بازنشانی رمز عبور");
                return false;
            }
        }
        #endregion

        #region احراز هویت دو مرحله‌ای
        public async Task<AuthResult> EnableTwoFactorAsync(Guid userId)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.EnableTwoFactor",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("EnableTwoFactor", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                        }

                        var (secret, qrCode) = _jwtService.GenerateTwoFactorSecret(user);
                        user.SetTwoFactorSecret(secret);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("2fa.enabled", true);
                        activity?.SetTag("2fa.type", user.TwoFactorType.ToString());
                        activity?.SetTag("2fa.secret_generated", true);

                        await _logger.LogInformationAsync($"احراز هویت دو مرحله‌ای برای کاربر {user.Email.Value} فعال شد");

                        return new AuthResult
                        {
                            Succeeded = true,
                            Message = "احراز هویت دو مرحله‌ای فعال شد",
                            User = user,
                            RequiresTwoFactor = true
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در فعال‌سازی احراز هویت دو مرحله‌ای");
                        return new AuthResult { Succeeded = false, Message = "خطا در فعال‌سازی احراز هویت دو مرحله‌ای" };
                    }
                });
        }

        public async Task<AuthResult> DisableTwoFactorAsync(Guid userId, string code)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.DisableTwoFactor",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("DisableTwoFactor", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                        }

                        if (!_jwtService.ValidateTwoFactorCode(user, code))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کد تایید اشتباه است");
                            return new AuthResult { Succeeded = false, Message = "کد تایید اشتباه است" };
                        }

                        var previousType = user.TwoFactorType;
                        user.DisableTwoFactor();
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("2fa.disabled", true);
                        activity?.SetTag("2fa.previous_type", previousType?.ToString());
                        activity?.SetTag("2fa.recovery_codes_cleared", true);

                        await _logger.LogInformationAsync($"احراز هویت دو مرحله‌ای برای کاربر {user.Email.Value} غیرفعال شد");

                        return new AuthResult
                        {
                            Succeeded = true,
                            Message = "احراز هویت دو مرحله‌ای غیرفعال شد",
                            User = user
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای");
                        return new AuthResult { Succeeded = false, Message = "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای" };
                    }
                });
        }

        public async Task<IEnumerable<string>> GenerateRecoveryCodesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                var recoveryCodes = _jwtService.GenerateRecoveryCodes();
                user.SetRecoveryCodes(recoveryCodes);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کدهای بازیابی جدید برای کاربر {user.Email.Value} تولید شدند");

                return recoveryCodes;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تولید کدهای بازیابی");
                throw;
            }
        }

        public async Task<bool> UseRecoveryCodeAsync(Guid userId, string code)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                if (!user.ValidateRecoveryCode(code))
                {
                    return false;
                }

                user.UseRecoveryCode(code);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کد بازیابی برای کاربر {user.Email.Value} استفاده شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در استفاده از کد بازیابی");
                return false;
            }
        }
        #endregion

        #region مدیریت نقش‌ها
        public async Task<RoleType> GetUserRoleAsync(Guid userId)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.GetUserRole",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("GetUserRole", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.roles", string.Join(",", user.Roles));

                        return user.Role;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در دریافت نقش‌های کاربر");
                        throw;
                    }
                });
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.AssignRole",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("AssignRole", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());
                        activity?.SetTag("role.name", roleName);

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return false;
                        }

                        if (user.Roles.Contains(roleName))
                        {
                            activity?.SetStatus(ActivityStatusCode.Ok);
                            activity?.SetTag("role.already_assigned", true);
                            return true;
                        }

                        user.Roles.Add(roleName);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("role.assigned", true);
                        activity?.SetTag("user.roles", string.Join(",", user.Roles));

                        await _logger.LogInformationAsync($"نقش {roleName} به کاربر {user.Email.Value} اختصاص داده شد");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در اختصاص نقش به کاربر");
                        return false;
                    }
                });
        }

        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.RemoveRole",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("RemoveRole", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());
                        activity?.SetTag("role.name", roleName);

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            return false;
                        }

                        if (!user.Roles.Contains(roleName))
                        {
                            activity?.SetStatus(ActivityStatusCode.Ok);
                            activity?.SetTag("role.not_assigned", true);
                            return true;
                        }

                        user.Roles.Remove(roleName);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("role.removed", true);
                        activity?.SetTag("user.roles", string.Join(",", user.Roles));

                        await _logger.LogInformationAsync($"نقش {roleName} از کاربر {user.Email.Value} حذف شد");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در حذف نقش از کاربر");
                        return false;
                    }
                });
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid userId)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.GetUserClaims",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("GetUserClaims", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var claims = _jwtService.GenerateClaims(user);
                        
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("claims.count", claims.Count());
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.permissions", string.Join(",", claims.Where(c => c.Type == "permission").Select(c => c.Value)));

                        return claims;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در دریافت کلیم‌های کاربر");
                        throw;
                    }
                });
        }
        #endregion

        #region مدیریت دستگاه‌ها
        public async Task AddUserDeviceAsync(Guid userId, UserDevice device)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.AddDevice(device);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"دستگاه جدید به کاربر {user.Email.Value} اضافه شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در افزودن دستگاه");
                throw;
            }
        }

        public async Task RemoveUserDeviceAsync(Guid userId, Guid deviceId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.RemoveDevice(deviceId);
                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"دستگاه از کاربر {user.Email.Value} حذف شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف دستگاه");
                throw;
            }
        }

        public async Task<IEnumerable<UserDevice>> GetUserDevicesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return user.GetActiveDevices();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت دستگاه‌های کاربر");
                throw;
            }
        }
        #endregion

        #region امنیت حساب کاربری
        public async Task LockAccountAsync(Guid userId, int duration)
        {
            await _tracingService.ExecuteInActivityAsync(
                "UserService.LockAccount",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("LockAccount", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());
                        activity?.SetTag("lock.duration_minutes", duration);

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var previousLockStatus = user.IsAccountLocked();
                        var lockoutEnd = DateTime.UtcNow.AddMinutes(duration);
                        user.LockAccount(duration);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("account.locked", true);
                        activity?.SetTag("account.lockout_end", lockoutEnd.ToString("o"));
                        activity?.SetTag("account.previously_locked", previousLockStatus);
                        activity?.SetTag("account.failed_attempts", user.FailedLoginAttempts);

                        await _logger.LogInformationAsync($"حساب کاربر {user.Email.Value} برای {duration} دقیقه قفل شد");
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در قفل کردن حساب");
                        throw;
                    }
                });
        }

        public async Task UnlockAccountAsync(Guid userId)
        {
            await _tracingService.ExecuteInActivityAsync(
                "UserService.UnlockAccount",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("UnlockAccount", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var wasLocked = user.IsAccountLocked();
                        var previousFailedAttempts = user.FailedLoginAttempts;
                        user.ResetFailedLoginAttempts();
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("account.unlocked", true);
                        activity?.SetTag("account.was_locked", wasLocked);
                        activity?.SetTag("account.previous_failed_attempts", previousFailedAttempts);
                        activity?.SetTag("account.current_failed_attempts", user.FailedLoginAttempts);

                        await _logger.LogInformationAsync($"قفل حساب کاربر {user.Email.Value} باز شد");
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در باز کردن قفل حساب");
                        throw;
                    }
                });
        }

        public async Task<AccountLockStatus> GetAccountLockStatusAsync(Guid userId)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.GetAccountLockStatus",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("GetAccountLockStatus", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var lockStatus = user.LockStatus;
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("account.locked", lockStatus.IsLocked);
                        activity?.SetTag("account.lockout_end", lockStatus.LockoutEnd?.ToString("o"));
                        activity?.SetTag("account.failed_attempts", lockStatus.FailedAttempts);
                        activity?.SetTag("account.remaining_attempts", lockStatus.RemainingAttempts);
                        activity?.SetTag("user.role", user.Role.ToString());
                        activity?.SetTag("user.2fa_enabled", user.TwoFactorEnabled);

                        return lockStatus;
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در دریافت وضعیت قفل حساب");
                        throw;
                    }
                });
        }

        public async Task<LoginHistoryResponse> GetLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.GetLoginHistory",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("GetLoginHistory", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());
                        activity?.SetTag("page", page);
                        activity?.SetTag("pageSize", pageSize);

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var (items, totalCount) = await _loginHistoryRepository.GetUserLoginHistoryAsync(
                            userId,
                            page,
                            pageSize,
                            includeUser: false);

                        activity?.SetTag("totalCount", totalCount);
                        activity?.SetStatus(ActivityStatusCode.Ok);

                        return new LoginHistoryResponse
                        {
                            Items = items.Select(MapToLoginHistoryDto),
                            TotalCount = totalCount,
                            PageNumber = page,
                            PageSize = pageSize
                        };
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در دریافت تاریخچه ورودها");
                        throw;
                    }
                });
        }

        public async Task UpdateLastLoginAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.LastLoginAt = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();

                // ثبت تاریخچه ورود موفق
                var loginHistory = new LoginHistory
                {
                    UserId = user.Id,
                    LoginTime = DateTime.UtcNow,
                    IsSuccessful = true
                };

                await _loginHistoryRepository.AddAsync(loginHistory);
                await _loginHistoryRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"آخرین زمان ورود کاربر {user.Email.Value} به‌روز شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی آخرین زمان ورود");
                throw;
            }
        }
        #endregion

        #region متدهای کمکی
        private UserResponse MapToUserResponse(User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email.Value,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                Roles = new List<string> { user.Role.ToString() }
            };
        }

        private LoginHistoryDto MapToLoginHistoryDto(LoginHistory history)
        {
            return new LoginHistoryDto
            {
                Id = history.Id,
                UserId = history.UserId,
                LoginTime = history.LoginTime,
                LogoutTime = history.LogoutTime,
                IpAddress = history.IpAddress,
                UserAgent = history.UserAgent,
                Location = history.Location,
                IsSuccessful = history.IsSuccessful,
                FailureReason = history.FailureReason,
                SessionDuration = history.SessionDuration
            };
        }
        #endregion
    }
}