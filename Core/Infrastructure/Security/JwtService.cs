using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Services.Base;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    /// <summary>
    /// سرویس مدیریت توکن‌های JWT
    /// </summary>
    public class JwtService : BaseService, IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;

        public JwtService(
            IUnitOfWork unitOfWork,
            ILogger<JwtService> logger,
            IOptions<JwtSettings> jwtSettings) 
            : base(unitOfWork, logger)
        {
            _jwtSettings = jwtSettings.Value;
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// ایجاد توکن دسترسی
        /// </summary>
        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            var claims = await GetUserClaimsAsync(user);
            return GenerateToken(claims, _jwtSettings.AccessTokenExpirationMinutes);
        }

        /// <summary>
        /// ایجاد توکن رفرش
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            var token = GenerateToken(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("token_type", "refresh")
            }, _jwtSettings.RefreshTokenExpirationDays * 24 * 60);

            await SaveRefreshTokenAsync(user.Id, token);
            return token;
        }

        /// <summary>
        /// اعتبارسنجی توکن
        /// </summary>
        public bool ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                tokenHandler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "توکن نامعتبر است");
                return false;
            }
        }

        /// <summary>
        /// دریافت اطلاعات کاربر از توکن
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "خطا در استخراج اطلاعات از توکن");
                return null;
            }
        }

        /// <summary>
        /// باطل کردن توکن رفرش
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string token)
        {
            await ExecuteInTransaction(async () =>
            {
                var refreshToken = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
                if (refreshToken != null)
                {
                    refreshToken.IsRevoked = true;
                    refreshToken.RevokedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveChangesAsync();
                }
            }, "باطل کردن توکن رفرش");
        }

        private string GenerateToken(Claim[] claims, int expirationMinutes)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                SigningCredentials = _signingCredentials,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task<Claim[]> GetUserClaimsAsync(User user)
        {
            var roles = await _unitOfWork.Users.GetUserRolesAsync(user.Id);
            var permissions = await _unitOfWork.Users.GetUserPermissionsAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("token_type", "access")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            return claims.ToArray();
        }

        private async Task SaveRefreshTokenAsync(Guid userId, string token)
        {
            await ExecuteInTransaction(async () =>
            {
                var refreshToken = new RefreshToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            }, "ذخیره توکن رفرش");
        }
    }
} 