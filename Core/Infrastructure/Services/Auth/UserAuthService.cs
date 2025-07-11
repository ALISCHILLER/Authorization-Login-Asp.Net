using Authorization_Login_Asp.Net.Core.Application.Interfaces.Services;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Threading;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Common;
using System.Security.Claims;
using System;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Auth
{
    public class UserAuthService : IUserAuthenticationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserAuthService> _logger;
        private readonly ILoginHistoryService _loginHistoryService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IDateTimeService _dateTimeService;

        public UserAuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtService jwtService,
            IEmailService emailService,
            IMapper mapper,
            ILogger<UserAuthService> logger,
            ILoginHistoryService loginHistoryService,
            ITwoFactorService twoFactorService,
            IDateTimeService dateTimeService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loginHistoryService = loginHistoryService ?? throw new ArgumentNullException(nameof(loginHistoryService));
            _twoFactorService = twoFactorService ?? throw new ArgumentNullException(nameof(twoFactorService));
            _dateTimeService = dateTimeService ?? throw new ArgumentNullException(nameof(dateTimeService));
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        {
            if (await _userRepository.ExistsByUsernameAsync(request.Username, cancellationToken))
                throw new DomainException("Username already exists."); // Translated
            if (await _userRepository.ExistsByEmailAsync(request.Email, cancellationToken))
                throw new DomainException("Email already exists."); // Translated

            var (hash, salt) = await _passwordHasher.HashPasswordAsync(request.Password);

            var user = new User(
                username: request.Username,
                email: request.Email,
                passwordHash: hash,
                firstName: request.FirstName,
                lastName: request.LastName,
                phoneNumber: request.PhoneNumber
            );
            user.PasswordSalt = salt;

            await _userRepository.AddAsync(user, cancellationToken);
            // Consider Unit of Work for SaveChangesAsync

            // TODO: Implement email verification token generation and sending process.
            // This might involve a separate IVerificationTokenService and updates to IEmailService.
            _logger.LogInformation("Verification email to be sent to {Email} for User ID: {UserId}", user.EmailAddress, user.Id);
            // await _emailService.SendVerificationEmailAsync(user.EmailAddress, verificationToken, cancellationToken);

            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
            // TODO: Review if IP address is strictly necessary for refresh token generation at registration.
            // If so, it needs to be passed into RegisterAsync, possibly via RegisterRequest or a context service.
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, null);

            _logger.LogInformation("User registered successfully: {Username}", user.Username);
            return new AuthResponse
            {
                IsSuccess = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token, // Assuming RefreshToken DTO has a Token string property
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            var user = await ValidateUserAsync(request.UsernameOrEmail, request.Password, cancellationToken);

            if (user == null)
            {
                var tempUser = await _userRepository.GetByUsernameAsync(request.UsernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(request.UsernameOrEmail, cancellationToken);
                if(tempUser != null)
                {
                    // Assuming DeviceInfo is part of LoginRequest or can be constructed
                    await _loginHistoryService.LogFailedLoginAsync(tempUser.Id, request.IpAddress, request.UserAgent, request.DeviceInfo, "Incorrect password");
                }
                throw new DomainException("Invalid username or password."); // Translated
            }

            if (user.IsAccountLocked())
            {
                _logger.LogWarning("Login attempt for locked account: {Username}", user.Username);
                await _loginHistoryService.LogFailedLoginAsync(user.Id, request.IpAddress, request.UserAgent, request.DeviceInfo, "Account locked");
                throw new DomainException($"Your account is locked until {user.AccountLockoutEnd}."); // Translated
            }

            if (user.TwoFactorEnabled)
            {
                _logger.LogInformation("2FA required for user: {Username}", user.Username);
                return new AuthResponse
                {
                    IsSuccess = false,
                    RequiresTwoFactor = true,
                    UserIdFor2FA = user.Id.ToString(),
                    Message = "Two-factor authentication is required." // Translated
                };
            }

            user.LastLoginAt = _dateTimeService.UtcNow;
            user.ResetFailedLoginAttempts();
            await _userRepository.UpdateAsync(user, cancellationToken);
            // Consider Unit of Work for SaveChangesAsync

            await _loginHistoryService.LogSuccessfulLoginAsync(user.Id, request.IpAddress, request.UserAgent, request.DeviceInfo);

            var accessToken = await _jwtService.GenerateAccessTokenAsync(user);
            var refreshToken = await _jwtService.GenerateRefreshTokenAsync(user, request.IpAddress);

            _logger.LogInformation("User logged in successfully: {Username}", user.Username);
            return new AuthResponse
            {
                IsSuccess = true,
                Token = accessToken,
                RefreshToken = refreshToken.Token,
                User = _mapper.Map<UserDto>(user)
            };
        }

        public async Task<User?> ValidateUserAsync(string usernameOrEmail, string password, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(usernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(usernameOrEmail, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Validation attempt for non-existent user: {UsernameOrEmail}", usernameOrEmail);
                return null;
            }

            bool passwordVerified = await _passwordHasher.VerifyPasswordAsync(password, user.PasswordHash, user.PasswordSalt);

            if (!passwordVerified)
            {
                user.IncrementFailedLoginAttempts();
                await _userRepository.UpdateAsync(user, cancellationToken);
                // Consider Unit of Work for SaveChangesAsync
                _logger.LogWarning("Password validation failed for user: {Username}", user.Username);
                return null;
            }
            return user;
        }

        public Task<AuthResponse> ValidateTwoFactorAsync(TwoFactorRequest request, CancellationToken cancellationToken = default)
        {
             _logger.LogWarning("ValidateTwoFactorAsync in UserAuthService is deprecated. Use ITwoFactorService.ValidateTwoFactorLoginAsync instead.");
            return _twoFactorService.ValidateTwoFactorLoginAsync(Guid.Parse(request.UserId), request.Code);
        }

        public Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("RefreshTokenAsync is not implemented yet.");
            throw new NotImplementedException("RefreshTokenAsync logic needs to be implemented, possibly in a dedicated TokenService.");
        }

        public Task<bool> RevokeTokenAsync(string token, string ipAddress)
        {
            _logger.LogWarning("RevokeTokenAsync is not implemented yet.");
            throw new NotImplementedException("RevokeTokenAsync logic needs to be implemented, possibly in a dedicated TokenService.");
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            _logger.LogInformation("Validating token (delegating to IJwtService): {Token}", token);
            return Task.FromResult(_jwtService.ValidateToken(token));
        }

        public async Task<User?> GetUserFromTokenAsync(string token)
        {
            _logger.LogInformation("Getting user from token: {Token}", token);
            var principal = _jwtService.GetPrincipalFromToken(token);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out Guid userId))
            {
                return await _userRepository.GetByIdAsync(userId);
            }
            return null;
        }

        public async Task<bool> IsEmailConfirmedAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user?.IsEmailVerified ?? false;
        }

        public async Task<bool> IsPhoneNumberConfirmedAsync(string phoneNumber)
        {
            // TODO: Implement phone number confirmation logic, requires IUserRepository.GetByPhoneNumberAsync or similar.
            _logger.LogWarning("IsPhoneNumberConfirmedAsync is not fully implemented.");
            return await Task.FromResult(false);
        }

        public async Task<bool> CheckPasswordAsync(User user, string password)
        {
            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || string.IsNullOrEmpty(user.PasswordSalt))
                return false;
            return await _passwordHasher.VerifyPasswordAsync(password, user.PasswordHash, user.PasswordSalt);
        }

        public Task<bool> IsLockedOutAsync(User user)
        {
            return Task.FromResult(user?.IsAccountLocked() ?? false);
        }

        public async Task<int> GetRecentFailedAttemptsAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(usernameOrEmail, cancellationToken)
                       ?? await _userRepository.GetByEmailAsync(usernameOrEmail, cancellationToken);
            return user?.FailedLoginAttempts ?? 0;
        }

        public async Task<DateTime?> GetAccountLockoutEndAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            return user?.AccountLockoutEnd;
        }

        public Task RecordLoginAsync(Guid userId, string ipAddress, string? deviceToken, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("UserAuthService.RecordLoginAsync called, delegating to ILoginHistoryService. UserID: {UserId}", userId);
            var deviceInfo = deviceToken != null ? new DeviceInfo { DeviceName = deviceToken } : null;
            return _loginHistoryService.RecordLoginAsync(userId, ipAddress, deviceToken, cancellationToken); // Corrected to pass cancellationToken
        }
    }
}
