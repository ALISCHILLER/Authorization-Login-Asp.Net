using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن (Repository) رفرش توکن با استفاده از Entity Framework Core
    /// </summary>
    public class RefreshTokenRepository : BaseRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(
            ApplicationDbContext context,
            ILogger<RefreshTokenRepository> logger) : base(context, logger)
        {
        }

        public async Task<RefreshToken> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت توکن با مقدار {Token}", token);
                throw;
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Where(rt => rt.UserId == userId && !rt.IsDeleted)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت توکن‌های کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token, string reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshToken = await _dbSet
                    .FirstOrDefaultAsync(rt => rt.Token == token && !rt.IsDeleted, cancellationToken);

                if (refreshToken == null)
                {
                    return false;
                }

                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.ReasonRevoked = reason;

                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لغو توکن {Token}", token);
                throw;
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId, string reason = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var tokens = await _dbSet
                    .Where(rt => rt.UserId == userId && !rt.IsDeleted && !rt.IsRevoked)
                    .ToListAsync(cancellationToken);

                if (!tokens.Any())
                {
                    return true;
                }

                foreach (var token in tokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.ReasonRevoked = reason;
                }

                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در لغو همه توکن‌های کاربر {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> IsTokenValidAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(rt => 
                    rt.Token == token && 
                    !rt.IsDeleted && 
                    !rt.IsRevoked && 
                    rt.ExpiresAt > DateTime.UtcNow, 
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی اعتبار توکن {Token}", token);
                throw;
            }
        }

        public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var expiredTokens = await _dbSet
                    .Where(rt => 
                        (rt.ExpiresAt <= DateTime.UtcNow || rt.IsRevoked) && 
                        !rt.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!expiredTokens.Any())
                {
                    return 0;
                }

                foreach (var token in expiredTokens)
                {
                    token.IsDeleted = true;
                    token.DeletedAt = DateTime.UtcNow;
                }

                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی توکن‌های منقضی شده");
                throw;
            }
        }

        public async Task<bool> UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Entry(token).State = EntityState.Modified;
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی توکن {TokenId}", token.Id);
                throw;
            }
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(rt => rt.User)
                    .Where(rt => 
                        !rt.IsDeleted && 
                        !rt.IsRevoked && 
                        rt.ExpiresAt > DateTime.UtcNow)
                    .OrderByDescending(rt => rt.CreatedAt)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت توکن‌های فعال");
                throw;
            }
        }
    }
}