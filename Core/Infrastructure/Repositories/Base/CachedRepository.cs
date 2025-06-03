using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Cache;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base
{
    /// <summary>
    /// کلاس پایه برای مخازن با پشتیبانی از کش
    /// </summary>
    /// <typeparam name="T">نوع موجودیت</typeparam>
    public abstract class CachedRepository<T> : BaseRepository<T> where T : class, IEntity
    {
        protected readonly ICacheService _cacheService;
        protected readonly ILogger _logger;
        protected readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(30);

        protected CachedRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger logger) : base(context)
        {
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// دریافت موجودیت از کش یا دیتابیس
        /// </summary>
        protected virtual async Task<T> GetCachedAsync<TKey>(
            TKey key,
            Func<TKey, Task<T>> getFromDb,
            Func<TKey, string> getCacheKey,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = getCacheKey(key);
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () => await getFromDb(key),
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// باطل کردن کش‌های مرتبط
        /// </summary>
        protected virtual async Task InvalidateCacheAsync(
            IEnumerable<string> cacheKeys,
            CancellationToken cancellationToken = default)
        {
            foreach (var key in cacheKeys)
            {
                await _cacheService.RemoveAsync(key, cancellationToken);
            }
        }

        /// <summary>
        /// اعتبارسنجی شناسه
        /// </summary>
        protected static void ValidateId(Guid id, string paramName)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("شناسه نامعتبر است", paramName);
        }

        /// <summary>
        /// اعتبارسنجی نام
        /// </summary>
        protected static void ValidateName(string name, string paramName)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام نمی‌تواند خالی باشد", paramName);
        }

        /// <summary>
        /// اعتبارسنجی موجودیت
        /// </summary>
        protected static void ValidateEntity(T entity, string paramName)
        {
            if (entity == null)
                throw new ArgumentNullException(paramName, "موجودیت نمی‌تواند خالی باشد");
        }

        /// <summary>
        /// به‌روزرسانی موجودیت با مدیریت کش
        /// </summary>
        public override async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
        {
            ValidateEntity(entity, nameof(entity));
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbSet.Update(entity).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(entity, cancellationToken);
            _logger.LogInformation("موجودیت با شناسه {Id} با موفقیت به‌روزرسانی شد", entity.Id);
        }

        /// <summary>
        /// حذف موجودیت با مدیریت کش
        /// </summary>
        public override async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
        {
            ValidateEntity(entity, nameof(entity));
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            await _dbSet.Update(entity).ReloadAsync(cancellationToken);
            await InvalidateEntityCacheAsync(entity, cancellationToken);
            _logger.LogInformation("موجودیت با شناسه {Id} با موفقیت حذف شد", entity.Id);
        }

        /// <summary>
        /// افزودن موجودیت با مدیریت کش
        /// </summary>
        public override async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            ValidateEntity(entity, nameof(entity));
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            await InvalidateEntityCacheAsync(entity, cancellationToken);
            _logger.LogInformation("موجودیت جدید با شناسه {Id} با موفقیت افزوده شد", entity.Id);
        }

        /// <summary>
        /// باطل کردن کش‌های مرتبط با موجودیت
        /// </summary>
        protected abstract Task InvalidateEntityCacheAsync(T entity, CancellationToken cancellationToken = default);
    }
} 