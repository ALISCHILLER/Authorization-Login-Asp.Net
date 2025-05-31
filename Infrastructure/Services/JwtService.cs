using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OtpNet;
using QRCoder;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    /// <summary>
    /// پیاده‌سازی سرویس JWT
    /// این کلاس عملیات مربوط به مدیریت توکن‌های JWT و احراز هویت دو مرحله‌ای را پیاده‌سازی می‌کند
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly ILogger<JwtService> _logger;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        /// <summary>
        /// سازنده سرویس JWT
        /// </summary>
        /// <param name="jwtOptions">تنظیمات JWT</param>
        /// <param name="logger">سرویس لاگر</param>
        /// <param name="refreshTokenRepository">ریپوزیتور خدمات رفرش توکن</param>
        public JwtService(
            IOptions<JwtOptions> jwtOptions, 
            ILogger<JwtService> logger,
            IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
        }

        /// <inheritdoc/>
        public string GenerateToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var claims = GenerateClaims(user);
            return GenerateToken(user.Id, user.Username, user.Role.ToString(), claims);
        }

        private string GenerateToken(Guid userId, string username, string role, IEnumerable<Claim> claims)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentNullException(nameof(role));

            var claimsList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            claimsList.AddRange(claims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claimsList,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_jwtOptions.SecretKey);

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var claims = new Dictionary<string, string>();

                foreach (var claim in jwtToken.Claims)
                {
                    claims[claim.Type] = claim.Value;
                }

                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اعتبارسنجی توکن JWT");
                throw new SecurityTokenException("توکن نامعتبر است", ex);
            }
        }

        /// <inheritdoc/>
        public Guid GetUserIdFromToken(string token)
        {
            var claims = ValidateToken(token);
            if (!claims.TryGetValue(JwtRegisteredClaimNames.Sub, out string userIdStr) || 
                !Guid.TryParse(userIdStr, out Guid userId))
            {
                throw new SecurityTokenException("شناسه کاربر در توکن یافت نشد");
            }
            return userId;
        }

        /// <inheritdoc/>
        public string GetUserRoleFromToken(string token)
        {
            var claims = ValidateToken(token);
            if (!claims.TryGetValue(ClaimTypes.Role, out string role))
            {
                throw new SecurityTokenException("نقش کاربر در توکن یافت نشد");
            }
            return role;
        }

        /// <inheritdoc/>
        public (string secret, string qrCode) GenerateTwoFactorSecret()
        {
            try
            {
                // تولید کلید محرمانه تصادفی
                var key = KeyGeneration.GenerateRandomKey(20);
                var secret = Base32Encoding.ToString(key);

                // تولید کد QR
                var issuer = _jwtOptions.Issuer;
                var accountTitle = "User"; // می‌توانید این مقدار را از پارامتر دریافت کنید
                var provisioningUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(accountTitle)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(provisioningUri, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new PngByteQRCode(qrCodeData);
                var qrCodeBytes = qrCode.GetGraphic(20);

                return (secret, Convert.ToBase64String(qrCodeBytes));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تولید کلید محرمانه احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        /// <inheritdoc/>
        public bool ValidateTwoFactorCode(string secret, string code)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code));

            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secret));
                return totp.VerifyTotp(code, out _, new VerificationWindow(2, 2));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در اعتبارسنجی کد احراز هویت دو مرحله‌ای");
                throw;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GenerateRecoveryCodes()
        {
            try
            {
                var codes = new List<string>();
                using var rng = RandomNumberGenerator.Create();
                var buffer = new byte[4];

                for (int i = 0; i < 10; i++)
                {
                    rng.GetBytes(buffer);
                    var code = BitConverter.ToUInt32(buffer, 0) % 100000000;
                    codes.Add(code.ToString("D8"));
                }

                return codes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تولید کدهای بازیابی");
                throw;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<Claim> GenerateClaims(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                    new Claim(ClaimTypes.Email, user.Email.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                };

                // اضافه کردن نقش کاربر
                if (user.PrimaryRole != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.PrimaryRole.Name));

                    // اضافه کردن دسترسی‌های کاربر از طریق نقش
                    foreach (var permission in user.PrimaryRole.Permissions)
                    {
                        if (permission.IsActive)
                        {
                            claims.Add(new Claim("Permission", permission.Name));
                        }
                    }
                }

                // اضافه کردن سایر نقش‌های کاربر
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim("Role", role.Name));
                    foreach (var permission in role.Permissions)
                    {
                        if (permission.IsActive)
                        {
                            claims.Add(new Claim("Permission", permission.Name));
                        }
                    }
                }

                return claims;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تولید کلیم‌های کاربر {Username}", user.Username);
                throw;
            }
        }

        public async Task<string> GenerateTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var token = GenerateToken(user);
            return await Task.FromResult(token);
        }

        public async Task<string> GenerateTokenAsync(Guid userId, string username, string role, IDictionary<string, string> claims = null)
        {
            var claimsList = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            if (claims != null)
            {
                claimsList.AddRange(claims.Select(c => new Claim(c.Key, c.Value)));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claimsList,
                expires: expires,
                signingCredentials: credentials
            );

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public string GenerateRefreshToken(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomToken(),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow
            };

            _refreshTokenRepository.AddAsync(refreshToken).Wait();
            _refreshTokenRepository.SaveChangesAsync().Wait();
            return refreshToken.Token;
        }

        public bool ValidateRefreshToken(User user, string refreshToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var token = _refreshTokenRepository.GetByTokenAsync(refreshToken).Result;
            if (token == null || token.UserId != user.Id)
                return false;

            return !token.IsRevoked && token.ExpiryDate > DateTime.UtcNow;
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                ValidateToken(token);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(User user, string refreshToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token == null || token.UserId != user.Id)
                return false;

            return !token.IsRevoked && token.ExpiryDate > DateTime.UtcNow;
        }

        public async Task<ClaimsPrincipal> GetPrincipalFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtOptions.SecretKey);
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtOptions.Audience,
                    ValidateLifetime = false // Don't validate lifetime here
                };

                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenException("توکن نامعتبر است");
                }

                return await Task.FromResult(principal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت اطلاعات از توکن");
                throw;
            }
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken, string reason = null)
        {
            var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (token != null)
            {
                token.Revoke(reason);
                await _refreshTokenRepository.SaveChangesAsync();
            }
        }

        public async Task RevokeAllRefreshTokensAsync(Guid userId)
        {
            var tokens = await _refreshTokenRepository.GetAllByUserIdAsync(userId);
            foreach (var token in tokens)
            {
                token.Revoke("همه توکن‌های کاربر باطل شدند");
            }
            await _refreshTokenRepository.SaveChangesAsync();
        }

        public async Task<IDictionary<string, string>> GetTokenClaimsAsync(string token)
        {
            var principal = await GetPrincipalFromTokenAsync(token);
            return principal.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        public async Task<DateTime> GetTokenExpirationAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return await Task.FromResult(jwtToken.ValidTo);
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            return await _refreshTokenRepository.IsTokenRevokedAsync(token);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            await _refreshTokenRepository.RevokeAllUserTokensAsync(userId);
        }

        public async Task RevokeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            await _refreshTokenRepository.RevokeTokenAsync(token);
        }

        public (string secret, string qrCode) GenerateTwoFactorSecret(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var (secret, qrCode) = GenerateTwoFactorSecret();
            return (secret, qrCode);
        }

        public bool ValidateTwoFactorToken(User user, string token)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            return ValidateTwoFactorCode(user.TwoFactorSecret, token);
        }

        public async Task<(string accessToken, string refreshToken)> GenerateTokensAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var accessToken = await GenerateTokenAsync(user);
            var refreshToken = await GenerateRefreshTokenAsync(user);
            return (accessToken, refreshToken);
        }

        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var token = GenerateToken(user);
            return await Task.FromResult(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomToken(),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            await _refreshTokenRepository.SaveChangesAsync();
            return refreshToken.Token;
        }
    }

    public class TracingService : Application.Interfaces.ITracingService
    {
        public ActivitySource CreateActivitySource(string name) => new(name);
        
        public void AddTracing(IServiceCollection services)
        {
            // پیاده‌سازی
        }
        
        // سایر متدها...
    }
}