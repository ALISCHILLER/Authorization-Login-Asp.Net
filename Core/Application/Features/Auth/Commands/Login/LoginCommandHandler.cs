using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;
using Authorization_Login_Asp.Net.Core.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Auth.Commands.Login;

/// <summary>
/// پردازشگر دستور ورود کاربر
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserAuthenticationService _userAuthService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserAuthenticationService userAuthService,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userAuthService = userAuthService;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("درخواست ورود کاربر با نام کاربری یا ایمیل: {UsernameOrEmail}", request.UsernameOrEmail);

            // بررسی اعتبار کاربر
            var user = await _userAuthService.ValidateUserAsync(
                request.UsernameOrEmail,
                request.Password,
                cancellationToken);

            if (user == null)
            {
                var failedAttempts = await _userAuthService.GetRecentFailedAttemptsAsync(request.UsernameOrEmail, cancellationToken);
                _logger.LogWarning("ورود ناموفق برای کاربر: {UsernameOrEmail}", request.UsernameOrEmail);
                return new LoginResponse
                {
                    Success = false,
                    Error = "نام کاربری یا رمز عبور اشتباه است",
                    RecentFailedAttempts = failedAttempts
                };
            }

            // بررسی وضعیت حساب کاربری
            if (!user.IsActive)
            {
                _logger.LogWarning("تلاش برای ورود به حساب غیرفعال: {UsernameOrEmail}", request.UsernameOrEmail);
                return new LoginResponse
                {
                    Success = false,
                    Error = "حساب کاربری شما غیرفعال است"
                };
            }

            if (user.IsLocked)
            {
                var lockoutEnd = await _userAuthService.GetAccountLockoutEndAsync(user.Id, cancellationToken);
                _logger.LogWarning("تلاش برای ورود به حساب قفل شده: {UsernameOrEmail}", request.UsernameOrEmail);
                return new LoginResponse
                {
                    Success = false,
                    Error = "حساب کاربری شما قفل شده است",
                    AccountLockoutEnd = lockoutEnd
                };
            }

            // تولید توکن‌ها
            var accessToken = await _tokenService.GenerateAccessTokenAsync(user, cancellationToken);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, request.IpAddress, cancellationToken);

            // ثبت اطلاعات ورود
            await _userAuthService.RecordLoginAsync(
                user.Id,
                request.IpAddress,
                request.DeviceToken,
                cancellationToken);

            _logger.LogInformation("ورود موفق کاربر: {UsernameOrEmail}", request.UsernameOrEmail);

            return new LoginResponse
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = 3600, // 1 hour
                TokenType = "Bearer",
                LastSuccessfulLogin = DateTime.UtcNow,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    IsLocked = user.IsLocked,
                    Roles = user.Roles.Select(r => r.Name).ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "خطا در پردازش درخواست ورود برای کاربر: {UsernameOrEmail}", request.UsernameOrEmail);
            return new LoginResponse
            {
                Success = false,
                Error = "خطا در پردازش درخواست ورود"
            };
        }
    }
} 