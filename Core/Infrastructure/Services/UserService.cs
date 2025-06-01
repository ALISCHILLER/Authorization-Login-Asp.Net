using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Domain.Common;
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
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ActivitySource _activitySource;
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
            _activitySource = _tracingService.CreateActivitySource("UserService");
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
            using var activity = _tracingService.StartActivity("Login", ActivityKind.Server);
            activity?.SetTag("email", request.Email);

            return await _tracingService.ExecuteInActivityAsync(
                "UserService.Login",
                async () =>
                {
                    try
                    {
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

                        return new AuthResponse
                        {
                            AccessToken = accessToken,
                            RefreshToken = refreshToken,
                            UserId = user.Id,
                            Username = user.Username,
                            Email = user.Email.Value,
                            Roles = user.GetRoles().Select(r => r.Name).ToList()
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
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
                        var user = await _userRepository.GetByIdAsync(model.UserId);
                        if (user == null)
                        {
                            return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                        }

                        if (!await _jwtService.ValidateRefreshTokenAsync(user, model.RefreshToken))
                        {
                            return new AuthResult { Succeeded = false, Message = "توکن رفرش نامعتبر است" };
                        }

                        var (accessToken, refreshToken) = await _jwtService.GenerateTokensAsync(user);
                        return new AuthResult
                        {
                            Succeeded = true,
                            Token = accessToken,
                            RefreshToken = refreshToken,
                            User = user
                        };
                    }
                    catch (Exception ex)
                    {
                        await _logger.LogErrorAsync(ex, "خطا در تمدید توکن");
                        return new AuthResult { Succeeded = false, Message = "خطا در تمدید توکن" };
                    }
                });
        }

        public async Task LogoutAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    await _jwtService.RevokeAllUserTokensAsync(userId);
                    await _logger.LogInformationAsync($"کاربر {user.Email.Value} از سیستم خارج شد");
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در خروج کاربر");
                throw;
            }
        }

        public async Task<bool> ValidateUserCredentialsAsync(string username, string password)
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
        #endregion

        #region مدیریت کاربران
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
                    Activity activity = null; // ✅ اعلان خارج از try

                    try
                    {
                        activity = _tracingService.StartActivity("EnableTwoFactor", ActivityKind.Server); // ✅ بدون using
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
                    finally
                    {
                        activity?.Dispose(); // ✅ dispose دستی
                    }
                });
        }

        public async Task<AuthResult> DisableTwoFactorAsync(Guid userId, string code)
        {
            return await _tracingService.ExecuteInActivityAsync(
                "UserService.DisableTwoFactor",
                async () =>
                {
                    Activity activity = null; // ✅ اعلان خارج از try

                    try
                    {
                        activity = _tracingService.StartActivity("DisableTwoFactor", ActivityKind.Server); // ✅ بدون using
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
                    finally
                    {
                        activity?.Dispose(); // ✅ dispose دستی
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

                var recoveryCodes = _twoFactorService.GenerateRecoveryCode();
                user.SetRecoveryCodes(new[] { recoveryCodes });
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"کدهای بازیابی جدید برای کاربر {user.Email.Value} تولید شدند");

                return new[] { recoveryCodes };
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
                    return false;

                if (user.UseRecoveryCode(code))
                {
                    await _userRepository.SaveChangesAsync();
                    return true;
                }

                return false;
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
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                return user?.Role ?? RoleType.User;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت نقش کاربر");
                throw;
            }
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                await _roleRepository.AddUserToRoleAsync(userId, roleName);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در اختصاص نقش به کاربر");
                return false;
            }
        }

        public async Task<bool> RemoveRoleFromUserAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                await _roleRepository.RemoveUserFromRoleAsync(userId, roleName);
                await _userRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف نقش از کاربر");
                return false;
            }
        }

        public async Task<IEnumerable<Claim>> GetUserClaimsAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException("کاربر یافت نشد");

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email.Value),
                    new Claim("fullName", user.FullName),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                };

                return claims;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت کلیم‌های کاربر");
                throw;
            }
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
        public async Task<bool> LockAccountAsync(Guid userId, string reason)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    return false;

                user.LockAccount(reason);
                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"حساب کاربر {user.Email.Value} قفل شد. دلیل: {reason}");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در قفل کردن حساب کاربری");
                return false;
            }
        }

        public async Task UnlockAccountAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException("کاربر یافت نشد");

                user.UnlockAccount();
                await _userRepository.SaveChangesAsync();
                await _logger.LogInformationAsync($"قفل حساب کاربر {user.Email.Value} باز شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در باز کردن قفل حساب کاربری");
                throw;
            }
        }

        public async Task<AccountLockStatus> GetAccountLockStatusAsync(Guid userId)
        {
            using var activity = _tracingService.StartActivity("GetAccountLockStatus", ActivityKind.Server);
            activity?.SetTag("user.id", userId.ToString());

            return await _tracingService.ExecuteInActivityAsync(
                "UserService.GetAccountLockStatus",
                async () =>
                {
                    try
                    {
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
                    Activity activity = null; // ✅ تعریف خارج از try

                    try
                    {
                        activity = _tracingService.StartActivity("GetLoginHistory", ActivityKind.Server);
                        activity?.SetTag("user.id", userId.ToString());
                        activity?.SetTag("page", page);
                        activity?.SetTag("pageSize", pageSize);

                        var user = await _userRepository.GetByIdAsync(userId);
                        if (user == null)
                        {
                            activity?.SetStatus(ActivityStatusCode.Error, "کاربر یافت نشد");
                            throw new ApplicationException("کاربر یافت نشد");
                        }

                        var result = await _loginHistoryRepository.GetUserLoginHistoryAsync(userId, page, pageSize);

                        activity?.SetTag("totalCount", result.TotalCount);
                        activity?.SetStatus(ActivityStatusCode.Ok);

                        return new LoginHistoryResponse
                        {
                            Items = result.Items.Select(h => new LoginHistoryItem
                            {
                                Id = h.Id,
                                UserId = h.UserId,
                                LoginTime = h.LoginTime,
                                LogoutTime = h.LogoutTime,
                                IpAddress = h.IpAddress,
                                Location = h.Location,
                                IsSuccessful = h.IsSuccessful,
                                FailureReason = h.FailureReason,
                                SessionDuration = h.SessionDuration.HasValue
                                    ? TimeSpan.FromSeconds(h.SessionDuration.Value)
                                    : null,
                                DeviceId = h.DeviceId.ToString(),
                                DeviceName = h.DeviceName,
                                Browser = h.Browser,
                                OperatingSystem = h.OperatingSystem
                            }),
                            TotalCount = result.TotalCount,
                            PageNumber = page,
                            PageSize = pageSize
                        };
                    }
                    catch (Exception ex)
                    {
                        activity?.SetStatus(ActivityStatusCode.Error, ex.Message); // ✅ حالا وجود دارد
                        await _logger.LogErrorAsync(ex, "خطا در دریافت تاریخچه ورود");
                        throw;
                    }
                    finally
                    {
                        activity?.Dispose(); // ✅ dispose دستی
                    }
                });
        }

        public async Task UpdateLastLoginAsync(Guid id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user != null)
                {
                    user.LastLoginAt = DateTime.UtcNow;
                    await _userRepository.SaveChangesAsync();
                }
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

            if (userAgent.Contains("Chrome"))
                return "Chrome";
            if (userAgent.Contains("Firefox"))
                return "Firefox";
            if (userAgent.Contains("Safari"))
                return "Safari";
            if (userAgent.Contains("Edge"))
                return "Edge";
            if (userAgent.Contains("MSIE") || userAgent.Contains("Trident"))
                return "Internet Explorer";
            if (userAgent.Contains("Opera"))
                return "Opera";

            return "Unknown";
        }

        private string GetOperatingSystem(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            if (userAgent.Contains("Windows"))
                return "Windows";
            if (userAgent.Contains("Mac OS"))
                return "MacOS";
            if (userAgent.Contains("Linux"))
                return "Linux";
            if (userAgent.Contains("Android"))
                return "Android";
            if (userAgent.Contains("iOS"))
                return "iOS";

            return "Unknown";
        }

        private string GetDeviceType(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            if (userAgent.Contains("Mobile"))
                return "Mobile";
            if (userAgent.Contains("Tablet"))
                return "Tablet";
            if (userAgent.Contains("Desktop"))
                return "Desktop";

            // Try to infer from screen size or other characteristics
            if (userAgent.Contains("iPhone") || userAgent.Contains("Android"))
                return "Mobile";
            if (userAgent.Contains("iPad"))
                return "Tablet";

            return "Desktop"; // Default to Desktop if we can't determine
        }

        private string GetDeviceName(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            // Extract device name from user agent
            if (userAgent.Contains("iPhone"))
                return "iPhone";
            if (userAgent.Contains("iPad"))
                return "iPad";
            if (userAgent.Contains("Macintosh"))
                return "Mac";
            if (userAgent.Contains("Windows"))
                return "Windows PC";
            if (userAgent.Contains("Android"))
            {
                // Try to extract specific Android device name
                var match = System.Text.RegularExpressions.Regex.Match(userAgent, @"Android.*?; (.*?) Build");
                if (match.Success)
                    return match.Groups[1].Value.Trim();
                return "Android Device";
            }

            return "Unknown Device";
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
                await _logger.LogInformationAsync($"دستگاه جدید به کاربر {user.Email.Value} اضافه شد");
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

        public async Task<IEnumerable<UserDevice>> GetDevicesAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new ApplicationException("کاربر یافت نشد");

                return user.GetActiveDevices();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت دستگاه‌های کاربر");
                throw;
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
                return user?.IsPhoneVerified ?? false;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی تایید تلفن");
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
                await _logger.LogInformationAsync($"دستگاه از کاربر {user.Email.Value} حذف شد");
                return true;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف دستگاه");
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

                if (string.IsNullOrEmpty(deviceId) || !Guid.TryParse(deviceId, out var deviceGuid))
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
                return await _userRepository.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت لیست کاربران");
                throw;
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
                throw;
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
                throw;
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
                throw;
            }
        }

        public async Task<User> UpdateUserAsync(Guid id, UpdateUserRequest request)
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

                return user;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی کاربر");
                throw;
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

        public async Task UpdateAsync(User user)
        {
            try
            {
                _userRepository.Update(user);
                await _userRepository.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در به‌روزرسانی کاربر");
                throw;
            }
        }

        public async Task DeleteAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    await _userRepository.DeleteAsync(user);
                    await _userRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در حذف کاربر");
                throw;
            }
        }

        public Task<UserResponse> GetUserByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        Task<UserResponse> IUserService.UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            throw new NotImplementedException();
        }
    }
}