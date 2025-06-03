using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Data
{
    /// <summary>
    /// پیاده‌سازی مدیریت واحد کار برای مدیریت تراکنش‌ها و مخزن‌ها
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _currentTransaction;
        private bool _disposed;

        // مخزن‌ها
        private IUserRepository _users;
        private IRoleRepository _roles;
        private IPermissionRepository _permissions;
        private IRolePermissionRepository _rolePermissions;
        private IRefreshTokenRepository _refreshTokens;
        private ILoginHistoryRepository _loginHistory;
        private INotificationRepository _notifications;
        private ISystemErrorRepository _systemErrors;
        private ISecurityErrorRepository _securityErrors;
        private IValidationErrorRepository _validationErrors;
        private IPerformanceErrorRepository _performanceErrors;
        private IAuditLogRepository _auditLogs;

        public UnitOfWork(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Repository Properties
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
        public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
        public IRolePermissionRepository RolePermissions => _rolePermissions ??= new RolePermissionRepository(_context);
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ??= new RefreshTokenRepository(_context);
        public ILoginHistoryRepository LoginHistory => _loginHistory ??= new LoginHistoryRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        public ISystemErrorRepository SystemErrors => _systemErrors ??= new SystemErrorRepository(_context);
        public ISecurityErrorRepository SecurityErrors => _securityErrors ??= new SecurityErrorRepository(_context);
        public IValidationErrorRepository ValidationErrors => _validationErrors ??= new ValidationErrorRepository(_context);
        public IPerformanceErrorRepository PerformanceErrors => _performanceErrors ??= new PerformanceErrorRepository(_context);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
        #endregion

        #region Transaction Management
        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                return;
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("تراکنش جدید شروع شد");
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                if (_currentTransaction == null)
                {
                    return;
                }

                await SaveChangesAsync();
                await _currentTransaction.CommitAsync();
                _logger.LogInformation("تراکنش با موفقیت ثبت شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت تراکنش");
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            try
            {
                if (_currentTransaction == null)
                {
                    return;
                }

                await _currentTransaction.RollbackAsync();
                _logger.LogInformation("تراکنش با موفقیت برگشت داده شد");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در برگشت تراکنش");
                throw;
            }
            finally
            {
                if (_currentTransaction != null)
                {
                    _currentTransaction.Dispose();
                    _currentTransaction = null;
                }
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "خطای همزمانی در ذخیره تغییرات");
                throw;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی پایگاه داده");
                throw;
            }
        }
        #endregion

        #region Cache Management
        public async Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
        {
            if (_cache.TryGetValue(key, out T cachedValue))
            {
                return cachedValue;
            }

            var value = await factory();
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiration ?? TimeSpan.FromMinutes(30))
                .SetSlidingExpiration(TimeSpan.FromMinutes(15));

            _cache.Set(key, value, cacheOptions);
            return value;
        }

        public Task RemoveFromCacheAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveFromCacheByPatternAsync(string pattern)
        {
            var cacheEntries = _cache.GetType().GetProperty("EntriesCollection")?.GetValue(_cache);
            if (cacheEntries == null) return Task.CompletedTask;

            foreach (var entry in cacheEntries as dynamic)
            {
                var key = entry.GetType().GetProperty("Key")?.GetValue(entry)?.ToString();
                if (key != null && key.Contains(pattern))
                {
                    _cache.Remove(key);
                }
            }

            return Task.CompletedTask;
        }
        #endregion

        #region Error Logging
        public async Task LogErrorAsync(Exception ex, string source, Dictionary<string, object> additionalData = null)
        {
            try
            {
                var error = new SystemError
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    Source = source,
                    AdditionalData = additionalData != null ? System.Text.Json.JsonSerializer.Serialize(additionalData) : null,
                    Timestamp = DateTime.UtcNow
                };

                await SystemErrors.AddAsync(error);
                await SaveChangesAsync();
                _logger.LogError(ex, "خطا در {Source}: {Message}", source, ex.Message);
            }
            catch (Exception logEx)
            {
                _logger.LogError(logEx, "خطا در ثبت خطا");
            }
        }

        public async Task LogSystemErrorAsync(Exception ex, string context, string userId = null)
        {
            await LogErrorAsync(ex, $"System Error in {context}", new Dictionary<string, object>
            {
                { "UserId", userId },
                { "Context", context }
            });
        }

        public async Task LogSecurityErrorAsync(string message, string context, string userId = null)
        {
            try
            {
                var error = new SecurityError
                {
                    Message = message,
                    Context = context,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                await SecurityErrors.AddAsync(error);
                await SaveChangesAsync();
                _logger.LogWarning("خطای امنیتی در {Context}: {Message}", context, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت خطای امنیتی");
            }
        }

        public async Task LogValidationErrorAsync(string message, string context, string userId = null)
        {
            try
            {
                var error = new ValidationError
                {
                    Message = message,
                    Context = context,
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                };

                await ValidationErrors.AddAsync(error);
                await SaveChangesAsync();
                _logger.LogWarning("خطای اعتبارسنجی در {Context}: {Message}", context, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت خطای اعتبارسنجی");
            }
        }

        public async Task LogPerformanceErrorAsync(string message, string context, long duration)
        {
            try
            {
                var error = new PerformanceError
                {
                    Message = message,
                    Context = context,
                    Duration = duration,
                    Timestamp = DateTime.UtcNow
                };

                await PerformanceErrors.AddAsync(error);
                await SaveChangesAsync();
                _logger.LogWarning("خطای عملکردی در {Context}: {Message} (Duration: {Duration}ms)", context, message, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در ثبت خطای عملکردی");
            }
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _currentTransaction?.Dispose();
                    _context.Dispose();
                }

                _disposed = true;
            }
        }
        #endregion
    }
} 