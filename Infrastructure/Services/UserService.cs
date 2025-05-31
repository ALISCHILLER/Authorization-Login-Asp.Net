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
using Microsoft.EntityFrameworkCore;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Authorization_Login_Asp.Net.Application.Exceptions;
using AutoMapper;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Context;
using Authorization_Login_Asp.Net.Application.Exceptions.Domain;
using Authorization_Login_Asp.Net.Application.Validators;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private static readonly ActivitySource _activitySource = new ActivitySource("UserService");
        private readonly IUserRepository _userRepository;
        private readonly ILoginHistoryRepository _loginHistoryRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;
        private readonly IImageService _imageService;
        private readonly ITracingService _tracingService;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly ITwoFactorService _twoFactorService;

        public UserService(
            IUserRepository userRepository,
            ILoginHistoryRepository loginHistoryRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
            IImageService imageService,
            ITracingService tracingService,
            IRoleRepository roleRepository,
            IMapper mapper,
            ITwoFactorService twoFactorService)
        {
            _userRepository = userRepository;
            _loginHistoryRepository = loginHistoryRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _imageService = imageService;
            _tracingService = tracingService;
            _roleRepository = roleRepository;
            _mapper = mapper;
            _twoFactorService = twoFactorService;
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
                        if (await _userRepository.ExistsByEmailAsync(request.Email))
                        {
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

                        var token = await _jwtService.GenerateTokenAsync(user);

                        await _logger.LogInformationAsync($"کاربر جدید با ایمیل {user.Email.Value} ثبت نام کرد");

                        return new AuthResponse
                        {
                            Token = token,
                            User = new UserDto
                            {
                                Id = user.Id,
                                Username = user.Username,
                                Email = user.Email.Value,
                                FullName = user.FullName,
                                RoleType = user.Role
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

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.Login",
                async () =>
                {
                    try
                    {
                        using var activity = _tracingService.StartActivity("Login", ActivityKind.Server);
                        activity?.SetTag("email", request.Email);

                        var user = await _userRepository.GetByEmailAsync(request.Email);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "User not found");
                            throw new UnauthorizedException("نام کاربری یا رمز عبور اشتباه است");
                        }

                        if (!user.VerifyPassword(request.Password))
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "Invalid password");
                            throw new UnauthorizedException("نام کاربری یا رمز عبور اشتباه است");
                        }

                        var (accessToken, refreshToken) = await _jwtService.GenerateTokensAsync(user);

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("user.id", user.Id.ToString());
                        activity?.SetTag("user.roles", string.Join(",", user.GetRoles().Select(r => r.Name).ToList()));

                        var userDto = _mapper.Map<UserDto>(user);
                        var response = new AuthResponse
                        {
                            User = userDto,
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
                            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
                            RequiresTwoFactor = user.TwoFactorEnabled,
                            TwoFactorType = user.TwoFactorType,
                            IsEmailVerified = user.IsEmailVerified,
                            IsPhoneVerified = user.IsPhoneVerified
                        };

                        return response;
                    }
                    catch (Exception ex)
                    {
                        Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
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
                
                if (request.IsActive != null)
                    user.IsActive = request.IsActive;
                    
                if (request.IsEmailVerified != null)
                    user.IsEmailVerified = request.IsEmailVerified;

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

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                if (!user.VerifyPassword(currentPassword))
                    return false;

                user.SetPassword(newPassword);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تغییر رمز عبور");
                return false;
            }
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
                        activity?.SetTag("user.roles", string.Join(",", user.GetRoles().Select(r => r.Name).ToList()));

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

                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "نقش یافت نشد");
                            return false;
                        }

                        if (user.HasRole(role))
                        {
                            activity?.SetStatus(ActivityStatusCode.Ok, "کاربر قبلاً این نقش را دارد");
                            return true;
                        }

                        user.AddRole(role);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        await _logger.LogInformationAsync($"نقش {roleName} به کاربر {user.Email.Value} اختصاص داده شد");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در اختصاص نقش به کاربر");
                        throw;
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

                        var role = await _roleRepository.GetByNameAsync(roleName);
                        if (role == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "نقش یافت نشد");
                            return false;
                        }

                        if (!user.HasRole(role))
                        {
                            activity?.SetStatus(ActivityStatusCode.Ok, "کاربر این نقش را ندارد");
                            return true;
                        }

                        user.RemoveRole(role);
                        await _userRepository.SaveChangesAsync();

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        await _logger.LogInformationAsync($"نقش {roleName} از کاربر {user.Email.Value} حذف شد");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در حذف نقش از کاربر");
                        throw;
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

                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Username),
                            new Claim(ClaimTypes.Email, user.Email.Value),
                            new Claim("fullName", user.FullName)
                        };

                        // Add roles
                        foreach (var role in user.GetRoles())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.Name));
                        }

                        // Add permissions from roles
                        var permissions = user.GetRoles()
                            .SelectMany(r => r.Permissions)
                            .Select(p => p.Name)
                            .Distinct();

                        foreach (var permission in permissions)
                        {
                            claims.Add(new Claim("permission", permission));
                        }

                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("claims.count", claims.Count);
                        activity?.SetTag("user.roles", string.Join(",", user.GetRoles().Select(r => r.Name).ToList()));
                        activity?.SetTag("user.permissions", string.Join(",", permissions.Select(p => p.ToString()).ToList()));

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
        public async Task LockAccountAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("کاربر یافت نشد");
            }

            user.LockAccount(30); // Lock for 30 minutes
            await _userRepository.SaveChangesAsync();
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

                        var history = await _loginHistoryRepository.GetUserLoginHistoryAsync(userId, page, pageSize);
                        
                        activity?.SetTag("totalCount", history.TotalCount);
                        activity?.SetStatus(ActivityStatusCode.Ok);

                        var loginHistoryItems = history.Items.Select(h => new LoginHistoryItem
                        {
                            Id = h.Id,
                            Username = user.Username,
                            LoginTime = h.LoginTime,
                            LogoutTime = h.LogoutTime,
                            IpAddress = h.IpAddress,
                            UserAgent = h.UserAgent,
                            Location = h.Location,
                            IsSuccessful = h.IsSuccessful,
                            FailureReason = h.FailureReason,
                            SessionDuration = h.SessionDuration.HasValue ? TimeSpan.FromSeconds(h.SessionDuration.Value) : null,
                            DeviceName = h.DeviceName,
                            DeviceType = h.DeviceType,
                            DeviceId = h.DeviceId.ToString(),
                            Browser = h.Browser,
                            OperatingSystem = h.OperatingSystem
                        }).ToList();

                        return new LoginHistoryResponse
                        {
                            Items = loginHistoryItems,
                            TotalCount = history.TotalCount,
                            PageNumber = page,
                            PageSize = pageSize
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                        await _logger.LogErrorAsync(ex, "خطا در دریافت تاریخچه ورود");
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
                    IsSuccessful = true,
                    DeviceId = Guid.NewGuid(),
                    IpAddress = "Unknown",
                    UserAgent = "Unknown"
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

        public async Task UpdateLastLoginAsync(Guid id, string ipAddress, string userAgent, string deviceId)
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
                    IsSuccessful = true,
                    DeviceId = string.IsNullOrEmpty(deviceId) ? Guid.NewGuid() : 
                              Guid.TryParse(deviceId, out var parsedGuid) ? parsedGuid : Guid.NewGuid(),
                    IpAddress = string.IsNullOrEmpty(ipAddress) ? "Unknown" : ipAddress,
                    UserAgent = string.IsNullOrEmpty(userAgent) ? "Unknown" : userAgent,
                    Browser = string.IsNullOrEmpty(userAgent) ? "Unknown" : GetBrowserName(userAgent)
                };

                await _loginHistoryRepository.AddAsync(loginHistory);
                await _loginHistoryRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"آخرین ورود کاربر {user.Email.Value} به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی آخرین ورود");
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
                ProfileImageUrl = user.ProfileImageUrl,
                RoleType = user.Role,
                Roles = user.GetRoles().Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    DisplayName = r.DisplayName,
                    Description = r.Description,
                    IsSystem = r.IsSystem,
                    IsActive = r.IsActive,
                    Permissions = r.Permissions.Select(p => p.Name)
                })
            };
        }

        private RoleDto MapToRoleDto(Role role)
        {
            if (role == null)
                return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                DisplayName = role.DisplayName,
                Description = role.Description,
                IsSystem = role.IsSystem,
                IsActive = role.IsActive,
                Permissions = role.Permissions.Select(p => p.Name)
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
                SessionDuration = history.SessionDuration.HasValue ? TimeSpan.FromSeconds(history.SessionDuration.Value) : null,
                DeviceId = history.DeviceId.ToString(),
                DeviceName = history.DeviceName,
                Browser = history.Browser,
                OperatingSystem = history.OperatingSystem
            };
        }

        private string GetBrowserName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            userAgent = userAgent.ToLower();
            
            if (userAgent.Contains("firefox"))
                return "Firefox";
            if (userAgent.Contains("chrome"))
                return "Chrome";
            if (userAgent.Contains("safari"))
                return "Safari";
            if (userAgent.Contains("edge"))
                return "Edge";
            if (userAgent.Contains("opera"))
                return "Opera";
            if (userAgent.Contains("ie") || userAgent.Contains("internet explorer"))
                return "Internet Explorer";
            
            return "Other";
        }
        #endregion

        public async Task<bool> AddDeviceAsync(Guid userId, UserDevice device)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.AddDevice(device);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در افزودن دستگاه");
                return false;
            }
        }

        public async Task AddLoginHistoryAsync(LoginHistoryDto historyDto)
        {
            var loginHistory = _mapper.Map<LoginHistory>(historyDto);
            await _loginHistoryRepository.AddAsync(loginHistory);
            await _loginHistoryRepository.SaveChangesAsync();
        }

        public async Task AddToRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    await _logger.LogWarningAsync($"کاربر با شناسه {userId} یافت نشد");
                    return;
                }

                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null)
                {
                    await _logger.LogWarningAsync($"نقش {roleName} یافت نشد");
                    return;
                }

                if (!await _roleRepository.IsUserInRoleAsync(userId, roleName))
                {
                    await _roleRepository.AddUserToRoleAsync(userId, roleName);
                    await _logger.LogInformationAsync($"نقش {roleName} به کاربر {user.Email.Value} اضافه شد");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در افزودن نقش به کاربر");
                throw;
            }
        }

        public async Task RemoveFromRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    await _logger.LogWarningAsync($"کاربر با شناسه {userId} یافت نشد");
                    return;
                }

                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role == null)
                {
                    await _logger.LogWarningAsync($"نقش {roleName} یافت نشد");
                    return;
                }

                if (await _roleRepository.IsUserInRoleAsync(userId, roleName))
                {
                    await _roleRepository.RemoveUserFromRoleAsync(userId, roleName);
                    await _logger.LogInformationAsync($"نقش {roleName} از کاربر {user.Email.Value} حذف شد");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف نقش از کاربر");
                throw;
            }
        }

        public async Task UpdateAsync(User user)
        {
            try
            {
                if (user == null)
                    throw new ArgumentNullException(nameof(user));

                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی کاربر");
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("کاربر یافت نشد");

            await _userRepository.DeleteAsync(user);
            await _userRepository.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string username)
        {
            try
            {
                return await _userRepository.ExistsByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی وجود کاربر");
                return false;
            }
        }

        public async Task<bool> ExistsEmailAsync(string email)
        {
            try
            {
                return await _userRepository.ExistsByEmailAsync(email);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی وجود ایمیل");
                return false;
            }
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                // Generate token logic here
                return Guid.NewGuid().ToString("N");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تولید توکن تایید ایمیل");
                return null;
            }
        }

        public async Task<IEnumerable<string>> GenerateNewRecoveryCodesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                var codes = _jwtService.GenerateRecoveryCodes();
                user.SetRecoveryCodes(codes);
                await _userRepository.SaveChangesAsync();
                return codes;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تولید کدهای بازیابی جدید");
                return null;
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                // Generate token logic here
                return Guid.NewGuid().ToString("N");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تولید توکن بازنشانی رمز عبور");
                return null;
            }
        }

        public async Task<string> GeneratePhoneConfirmationTokenAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return null;

                // Generate token logic here
                return Guid.NewGuid().ToString("N");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تولید توکن تایید شماره تلفن");
                return null;
            }
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            try
            {
                return await _userRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت کاربر با ایمیل");
                return null;
            }
        }

        public async Task<User> GetByIdAsync(Guid userId)
        {
            try
            {
                return await _userRepository.GetByIdAsync(userId);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت کاربر با شناسه");
                return null;
            }
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            try
            {
                return await _userRepository.GetByUsernameAsync(username);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت کاربر با نام کاربری");
                return null;
            }
        }

        public async Task<IEnumerable<UserDevice>> GetDevicesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user?.GetActiveDevices() ?? Enumerable.Empty<UserDevice>();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت دستگاه‌ها");
                return Enumerable.Empty<UserDevice>();
            }
        }

        public async Task<IEnumerable<LoginHistoryDto>> GetLoginHistoryAsync(Guid userId)
        {
            try
            {
                var history = await _loginHistoryRepository.GetUserLoginHistoryAsync(userId);
                return history.Select(h => new LoginHistoryDto
                {
                    Id = h.Id,
                    UserId = h.UserId,
                    LoginTime = h.LoginTime,
                    LogoutTime = h.LogoutTime,
                    IpAddress = h.IpAddress,
                    UserAgent = h.UserAgent,
                    Location = h.Location,
                    IsSuccessful = h.IsSuccessful,
                    FailureReason = h.FailureReason
                });
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت تاریخچه ورود");
                return Enumerable.Empty<LoginHistoryDto>();
            }
        }

        public async Task<IEnumerable<Role>> GetRolesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user?.GetRoles() ?? Enumerable.Empty<Role>();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت نقش‌ها");
                return Enumerable.Empty<Role>();
            }
        }

        public async Task<IEnumerable<User>> GetUsersInRoleAsync(string roleName)
        {
            try
            {
                return await _userRepository.GetUsersInRoleAsync(roleName);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت کاربران در نقش");
                return Enumerable.Empty<User>();
            }
        }

        public async Task<bool> HasValidTwoFactorTokenAsync(Guid userId, string token)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                return _twoFactorService.ValidateToken(user.TwoFactorSecret, token);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در اعتبارسنجی توکن دو مرحله‌ای");
                return false;
            }
        }

        public async Task<bool> IsEmailConfirmedAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null && user.IsEmailVerified;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی تایید ایمیل");
                return false;
            }
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null && user.HasRole(roleName);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی نقش");
                return false;
            }
        }

        public async Task<bool> IsLockedOutAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null && user.IsAccountLocked();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی قفل حساب");
                return false;
            }
        }

        public async Task<bool> IsPhoneConfirmedAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null && user.IsPhoneVerified;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی تایید شماره تلفن");
                return false;
            }
        }

        public async Task<bool> LockAccountAsync(Guid userId, string reason)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.LockAccount(30); // Lock for 30 minutes by default
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در قفل کردن حساب");
                return false;
            }
        }

        public async Task<bool> RedeemRecoveryCodeAsync(Guid userId, string code)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                var result = user.UseRecoveryCode(code);
                if (result)
                {
                    await _userRepository.SaveChangesAsync();
                }
                return result;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در استفاده از کد بازیابی");
                return false;
            }
        }

        public async Task<bool> RemoveDeviceAsync(Guid userId, Guid deviceId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.RemoveDevice(deviceId);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف دستگاه");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(Guid userId, string token, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                // Validate token logic here
                user.SetPassword(newPassword);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بازنشانی رمز عبور");
                return false;
            }
        }

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(username);
                return user != null && user.VerifyPassword(password);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در اعتبارسنجی اعتبارنامه‌ها");
                return false;
            }
        }

        public async Task<bool> ValidateDeviceAsync(Guid userId, string deviceId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                if (!Guid.TryParse(deviceId, out var deviceGuid))
                    return false;

                return user.GetActiveDevices().Any(d => d.Id == deviceGuid);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در اعتبارسنجی دستگاه");
                return false;
            }
        }

        public async Task<bool> VerifyTwoFactorTokenAsync(Guid userId, string token)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                return _twoFactorService.ValidateToken(user.TwoFactorSecret, token);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در اعتبارسنجی توکن دو مرحله‌ای");
                return false;
            }
        }

        public async Task<bool> CheckPasswordAsync(Guid userId, string password)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user != null && user.VerifyPassword(password);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی رمز عبور");
                return false;
            }
        }

        public async Task<bool> ConfirmEmailAsync(Guid userId, string token)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                // Validate token logic here
                user.IsEmailVerified = true;
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تایید ایمیل");
                return false;
            }
        }

        public async Task<bool> ConfirmPhoneAsync(Guid userId, string token)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                // Validate token logic here
                user.IsPhoneVerified = true;
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تایید شماره تلفن");
                return false;
            }
        }

        public async Task<User> CreateAsync(User user)
        {
            try
            {
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در ایجاد کاربر");
                throw;
            }
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                using var activity = _activitySource.StartActivity("GetAllUsers");
                activity?.SetTag("operation", "get_all_users");

                var users = await _userRepository.GetAllAsync();
                
                activity?.SetStatus(ActivityStatusCode.Ok);
                activity?.SetTag("users.count", users.Count());
                
                return users;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت لیست کاربران");
                throw;
            }
        }

        public async Task<bool> HasRole(Guid userId, string roleName)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            return user?.HasRole(roleName) ?? false;
        }
    }
}