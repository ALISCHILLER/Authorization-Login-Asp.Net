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

        /// <summary>
        /// سازنده سرویس JWT
        /// </summary>
        /// <param name="jwtOptions">تنظیمات JWT</param>
        /// <param name="logger">سرویس لاگر</param>
        public JwtService(IOptions<JwtOptions> jwtOptions, ILogger<JwtService> logger)
        {
            _jwtOptions = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public string GenerateToken(Guid userId, string username, string role, IDictionary<string, string> additionalClaims = null)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentNullException(nameof(role));

            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName, username),
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
                };

                // اضافه کردن کلیم‌های اضافی
                if (additionalClaims != null)
                {
                    foreach (var claim in additionalClaims)
                    {
                        claims.Add(new Claim(claim.Key, claim.Value));
                    }
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

                var token = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    expires: expires,
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در تولید توکن JWT برای کاربر {Username}", username);
                throw;
            }
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
                if (user.Role != null)
                {
                    claims.Add(new Claim(ClaimTypes.Role, user.Role.Name));

                    // اضافه کردن دسترسی‌های کاربر از طریق نقش
                    if (user.Role.Permissions != null)
                    {
                        foreach (var permission in user.Role.Permissions)
                        {
                            if (permission.IsActive)
                            {
                                claims.Add(new Claim("Permission", permission.Name));
                            }
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
    }
}