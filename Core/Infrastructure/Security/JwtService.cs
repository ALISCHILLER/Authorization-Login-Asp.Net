using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
// using Authorization_Login_Asp.Net.Core.Domain.Interfaces; // IJwtService is in Application layer
using Authorization_Login_Asp.Net.Core.Application.Interfaces; // For IJwtService and IUnitOfWork
using Authorization_Login_Asp.Net.Core.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using System.Collections.Generic; // For List<Claim>

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Security
{
    /// <summary>
    /// سرویس مدیریت توکن‌های JWT
    /// </summary>
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly SecurityKey _securityKey;
        private readonly SigningCredentials _signingCredentials;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<JwtService> _logger;

        public JwtService(
            IOptions<JwtSettings> jwtSettings,
            IUnitOfWork unitOfWork,
            ILogger<JwtService> logger)
        {
            _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrEmpty(_jwtSettings.SecretKey))
            {
                _logger.LogError("JWT SecretKey is not configured."); // Log error instead of just throwing
                throw new ArgumentNullException(nameof(_jwtSettings.SecretKey), "JWT SecretKey cannot be null or empty.");
            }
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            _signingCredentials = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256);
        }

        /// <summary>
        /// ایجاد توکن دسترسی
        /// </summary>
        public async Task<string> GenerateAccessTokenAsync(User user)
        {
            if (user == null)
            {
                _logger.LogError("User object cannot be null for GenerateAccessTokenAsync.");
                throw new ArgumentNullException(nameof(user));
            }
            var claims = await GetUserClaimsAsync(user);
            return GenerateToken(claims, _jwtSettings.AccessTokenExpirationMinutes);
        }

        /// <summary>
        /// ایجاد توکن رفرش
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            if (user == null)
            {
                _logger.LogError("User object cannot be null for GenerateRefreshTokenAsync.");
                throw new ArgumentNullException(nameof(user));
            }
            var token = GenerateToken(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("token_type", "refresh")
            }, _jwtSettings.RefreshTokenExpirationDays * 24 * 60);

            await SaveRefreshTokenAsync(user.Id, token, user.SecurityStamp); // Pass user's SecurityStamp
            return token;
        }

        /// <summary>
        /// اعتبارسنجی توکن
        /// </summary>
        public bool ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return false;
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
            catch (SecurityTokenException ex) // Catch specific exceptions
            {
                _logger.LogWarning(ex, "Token validation failed: {TokenValidationFailure}", ex.Message);
                return false;
            }
            catch (Exception ex) // Catch any other unexpected error
            {
                _logger.LogError(ex, "An unexpected error occurred during token validation.");
                return false;
            }
        }

        /// <summary>
        /// دریافت اطلاعات کاربر از توکن
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            if (string.IsNullOrEmpty(token)) return null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token); // This doesn't validate the signature or expiry

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = _securityKey,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtSettings.Audience,
                    ValidateLifetime = false // Lifetime is not validated here, usually done by middleware
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning(ex, "Failed to get principal from token: {TokenValidationFailure}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting principal from token.");
                return null;
            }
        }

        /// <summary>
        /// باطل کردن توکن رفرش
        /// </summary>
        public async Task RevokeRefreshTokenAsync(string token)
        {
            if (string.IsNullOrEmpty(token)) return;

            var refreshTokenEntity = await _unitOfWork.RefreshTokens.GetByTokenAsync(token);
            if (refreshTokenEntity != null)
            {
                refreshTokenEntity.Revoke("User initiated revocation or token replaced."); // Provide a reason
                _unitOfWork.RefreshTokens.Update(refreshTokenEntity); // Explicitly mark as updated
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Refresh token {Token} revoked.", token);
            }
            else
            {
                _logger.LogWarning("Attempted to revoke a non-existent or already revoked refresh token: {Token}", token);
            }
        }

        private string GenerateToken(IEnumerable<Claim> claims, int expirationMinutes)
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

        private async Task SaveRefreshTokenAsync(Guid userId, string token, string securityStamp)
        {
            // It's good practice to invalidate older refresh tokens for the user if a max limit is set.
            // This logic would typically be here or in a dedicated RefreshTokenService.

            var refreshToken = new RefreshToken
            {
                // Id will be set by BaseEntity constructor
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                // CreatedAt will be set by BaseEntity constructor
                // SecurityStamp might be useful to invalidate refresh tokens if user's security context changes (e.g. password change)
                // For now, not adding SecurityStamp to RefreshToken entity directly, but it's a consideration.
            };
            // refreshToken.MarkAsCreated(null); // Or pass system/user ID if available

            await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(); // Save changes for the new refresh token
            _logger.LogInformation("Refresh token saved for user {UserId}", userId);
        }
    }
} 