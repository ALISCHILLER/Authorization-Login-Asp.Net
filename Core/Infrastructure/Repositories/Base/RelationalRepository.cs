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
    /// کلاس پایه برای مخازن رابطه‌ای
    /// </summary>
    /// <typeparam name="TEntity">نوع موجودیت اصلی</typeparam>
    /// <typeparam name="TRelated">نوع موجودیت مرتبط</typeparam>
    public abstract class RelationalRepository<TEntity, TRelated> : CachedRepository<TEntity>
        where TEntity : class, IEntity
        where TRelated : class, IEntity
    {
        protected readonly DbSet<TRelated> _relatedDbSet;

        protected RelationalRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger logger) : base(context, cacheService, logger)
        {
            _relatedDbSet = context.Set<TRelated>();
        }

        /// <summary>
        /// افزودن رابطه بین دو موجودیت
        /// </summary>
        protected virtual async Task<bool> AddRelationAsync(
            Guid entityId,
            Guid relatedId,
            Func<TEntity, ICollection<TRelated>> getCollection,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet
                .Include(e => getCollection(e as TEntity))
                .FirstOrDefaultAsync(e => e.Id == entityId && !e.IsDeleted, cancellationToken);

            if (entity == null)
                return false;

            var related = await _relatedDbSet.FindAsync(new object[] { relatedId }, cancellationToken);
            if (related == null)
                return false;

            var collection = getCollection(entity as TEntity);
            if (!collection.Contains(related))
            {
                collection.Add(related);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (result)
                {
                    await InvalidateEntityCacheAsync(entity as TEntity, cancellationToken);
                }
                return result;
            }

            return true;
        }

        /// <summary>
        /// حذف رابطه بین دو موجودیت
        /// </summary>
        protected virtual async Task<bool> RemoveRelationAsync(
            Guid entityId,
            Guid relatedId,
            Func<TEntity, ICollection<TRelated>> getCollection,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet
                .Include(e => getCollection(e as TEntity))
                .FirstOrDefaultAsync(e => e.Id == entityId && !e.IsDeleted, cancellationToken);

            if (entity == null)
                return false;

            var collection = getCollection(entity as TEntity);
            var related = collection.FirstOrDefault(r => r.Id == relatedId);
            if (related != null)
            {
                collection.Remove(related);
                var result = await _context.SaveChangesAsync(cancellationToken) > 0;
                if (result)
                {
                    await InvalidateEntityCacheAsync(entity as TEntity, cancellationToken);
                }
                return result;
            }

            return true;
        }

        /// <summary>
        /// دریافت موجودیت‌های مرتبط
        /// </summary>
        protected virtual async Task<IEnumerable<TRelated>> GetRelatedAsync(
            Guid entityId,
            Func<TEntity, ICollection<TRelated>> getCollection,
            string cacheKey,
            CancellationToken cancellationToken = default)
        {
            return await _cacheService.GetOrCreateAsync(
                cacheKey,
                async () =>
                {
                    var entity = await _dbSet
                        .Include(e => getCollection(e as TEntity))
                        .FirstOrDefaultAsync(e => e.Id == entityId && !e.IsDeleted, cancellationToken);

                    return entity != null ? getCollection(entity as TEntity) : new List<TRelated>();
                },
                _defaultCacheDuration,
                cancellationToken);
        }

        /// <summary>
        /// بررسی وجود رابطه
        /// </summary>
        protected virtual async Task<bool> HasRelationAsync(
            Guid entityId,
            Guid relatedId,
            Func<TEntity, ICollection<TRelated>> getCollection,
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(e => getCollection(e as TEntity))
                .AnyAsync(e => 
                    e.Id == entityId && 
                    !e.IsDeleted && 
                    getCollection(e as TEntity).Any(r => r.Id == relatedId), 
                    cancellationToken);
        }
    }
} 