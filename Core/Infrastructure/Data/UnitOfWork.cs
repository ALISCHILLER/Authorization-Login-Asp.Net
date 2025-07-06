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
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction? _currentTransaction; // قابل null
        private bool _disposed;

        // مخزن‌ها
        private IUserRepository? _users;
        private IRoleRepository? _roles;
        private IPermissionRepository? _permissions;
        private IRolePermissionRepository? _rolePermissions;
        private IRefreshTokenRepository? _refreshTokens;
        // private ILoginHistoryRepository? _loginHistory;
        // private INotificationRepository? _notifications;
        // private IAuditLogRepository? _auditLogs;

        public UnitOfWork(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILoggerFactory loggerFactory)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = _loggerFactory.CreateLogger<UnitOfWork>();
        }

        #region Repository Properties
        public IUserRepository Users => _users ??= new UserRepository(_context);
        public IRoleRepository Roles => _roles ??= new RoleRepository(_context, _loggerFactory.CreateLogger<RoleRepository>());
        public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
        public IRolePermissionRepository RolePermissions => _rolePermissions ??= new RolePermissionRepository(_context, _loggerFactory.CreateLogger<RolePermissionRepository>());
        public IRefreshTokenRepository RefreshTokens => _refreshTokens ?? (_refreshTokens = new RefreshTokenRepository(_context, _loggerFactory.CreateLogger<RefreshTokenRepository>()));
        // public ILoginHistoryRepository LoginHistory => _loginHistory ?? (_loginHistory = new LoginHistoryRepository(_context, _loggerFactory.CreateLogger<LoginHistoryRepository>()));
        // public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
        // public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
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
        public async Task<T> GetOrSetCacheAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            if (_cache.TryGetValue(key, out T? cachedValue) && cachedValue != null)
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
                if (!string.IsNullOrEmpty(key) && key!.Contains(pattern))
                {
                    _cache.Remove(key);
                }
            }

            return Task.CompletedTask;
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