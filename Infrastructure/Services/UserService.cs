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

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly ISmsService _smsService;
        private readonly ILoggingService _logger;
        private readonly IImageService _imageService;

        public UserService(
            IUserRepository userRepository,
            IJwtService jwtService,
            IEmailService emailService,
            ISmsService smsService,
            ILoggingService logger,
            IImageService imageService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
            _emailService = emailService;
            _smsService = smsService;
            _logger = logger;
            _imageService = imageService;
        }

        #region احراز هویت و مجوزدهی
        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
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

                // Generate confirmation token and send email
                var confirmationToken = Guid.NewGuid().ToString();
                await _emailService.SendConfirmationEmailAsync(user.Email.Value, confirmationToken);

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Username, user.Role.ToString());

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
                        Role = user.Role.ToString()
                    }
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در ثبت نام کاربر");
                throw;
            }
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
                if (user == null)
                {
                    throw new ApplicationException("کاربر با این ایمیل یافت نشد");
                }

                // Create login history entry
                var loginHistory = new LoginHistory
                {
                    UserId = user.Id,
                    LoginAt = DateTime.UtcNow,
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
                    throw new ApplicationException("رمز عبور اشتباه است");
                }

                if (user.IsAccountLocked())
                {
                    loginHistory.FailureReason = "حساب کاربری قفل شده است";
                    throw new ApplicationException("حساب کاربری شما قفل شده است. لطفاً بعداً تلاش کنید");
                }

                if (!user.IsEmailVerified)
                {
                    loginHistory.FailureReason = "ایمیل تأیید نشده است";
                    throw new ApplicationException("لطفاً ابتدا ایمیل خود را تأیید کنید");
                }

                // Generate JWT token
                var token = _jwtService.GenerateToken(user.Id, user.Username, user.Role.ToString());

                // Update user's last login time and reset failed attempts
                user.LastLoginAt = DateTime.UtcNow;
                user.ResetFailedLoginAttempts();
                loginHistory.IsSuccessful = true;

                await _userRepository.SaveChangesAsync(cancellationToken);

                await _logger.LogInformationAsync($"کاربر {user.Email.Value} با موفقیت وارد شد");

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
                await _logger.LogErrorAsync(ex, "خطا در ورود کاربر");
                throw;
            }
        }

        public async Task<AuthResult> ValidateTwoFactorAsync(TwoFactorDto model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                }

                if (!_jwtService.ValidateTwoFactorCode(user, model.Code))
                {
                    return new AuthResult { Succeeded = false, Message = "کد تایید اشتباه است" };
                }

                var token = _jwtService.GenerateToken(user);
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
        }

        public async Task<AuthResult> RefreshTokenAsync(RefreshTokenDto model)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(model.UserId);
                if (user == null)
                {
                    return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                }

                if (!_jwtService.ValidateRefreshToken(user, model.RefreshToken))
                {
                    return new AuthResult { Succeeded = false, Message = "توکن رفرش نامعتبر است" };
                }

                var token = _jwtService.GenerateToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken(user);

                return new AuthResult
                {
                    Succeeded = true,
                    Message = "توکن با موفقیت تمدید شد",
                    User = user,
                    Token = token,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تمدید توکن");
                return new AuthResult { Succeeded = false, Message = "خطا در تمدید توکن" };
            }
        }

        public async Task LogoutAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user != null)
                {
                    user.InvalidateRefreshToken();
                    await _userRepository.SaveChangesAsync();
                    await _logger.LogInformationAsync($"کاربر {user.Email.Value} با موفقیت خارج شد");
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
                if (user == null)
                {
                    return false;
                }

                return user.VerifyPassword(password);
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در بررسی اعتبارنامه‌های کاربر");
                return false;
            }
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
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                if (!user.VerifyPassword(currentPassword))
                {
                    throw new ApplicationException("رمز عبور فعلی اشتباه است");
                }

                user.SetPassword(newPassword);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"رمز عبور کاربر {user.Email.Value} تغییر کرد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در تغییر رمز عبور");
                throw;
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
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                }

                var (secret, qrCode) = _jwtService.GenerateTwoFactorSecret(user);
                user.SetTwoFactorSecret(secret);
                await _userRepository.SaveChangesAsync();

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
                await _logger.LogErrorAsync(ex, "خطا در فعال‌سازی احراز هویت دو مرحله‌ای");
                return new AuthResult { Succeeded = false, Message = "خطا در فعال‌سازی احراز هویت دو مرحله‌ای" };
            }
        }

        public async Task<AuthResult> DisableTwoFactorAsync(Guid userId, string code)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return new AuthResult { Succeeded = false, Message = "کاربر یافت نشد" };
                }

                if (!_jwtService.ValidateTwoFactorCode(user, code))
                {
                    return new AuthResult { Succeeded = false, Message = "کد تایید اشتباه است" };
                }

                user.DisableTwoFactor();
                await _userRepository.SaveChangesAsync();

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
                await _logger.LogErrorAsync(ex, "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای");
                return new AuthResult { Succeeded = false, Message = "خطا در غیرفعال‌سازی احراز هویت دو مرحله‌ای" };
            }
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
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return user.Role;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت نقش‌های کاربر");
                throw;
            }
        }

        public async Task<bool> AssignRoleToUserAsync(Guid userId, string roleName)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                if (user.Roles.Contains(roleName))
                {
                    return true;
                }

                user.Roles.Add(roleName);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"نقش {roleName} به کاربر {user.Email.Value} اختصاص داده شد");
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
                {
                    return false;
                }

                if (!user.Roles.Contains(roleName))
                {
                    return true;
                }

                user.Roles.Remove(roleName);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"نقش {roleName} از کاربر {user.Email.Value} حذف شد");
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
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return _jwtService.GenerateClaims(user);
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
        public async Task LockAccountAsync(Guid userId, int duration)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.LockAccount(duration);
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"حساب کاربر {user.Email.Value} برای {duration} دقیقه قفل شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در قفل کردن حساب");
                throw;
            }
        }

        public async Task UnlockAccountAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                user.ResetFailedLoginAttempts();
                await _userRepository.SaveChangesAsync();

                await _logger.LogInformationAsync($"قفل حساب کاربر {user.Email.Value} باز شد");
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در باز کردن قفل حساب");
                throw;
            }
        }

        public async Task<AccountLockStatus> GetAccountLockStatusAsync(Guid userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                return user.LockStatus;
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync(ex, "خطا در دریافت وضعیت قفل حساب");
                throw;
            }
        }

        public async Task<LoginHistoryResponse> GetLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new ApplicationException("کاربر یافت نشد");
                }

                var history = await _userRepository.GetLoginHistoryAsync(userId, page, pageSize);
                var totalCount = await _userRepository.GetLoginHistoryCountAsync(userId);

                return new LoginHistoryResponse
                {
                    Items = history,
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
        #endregion
    }
}