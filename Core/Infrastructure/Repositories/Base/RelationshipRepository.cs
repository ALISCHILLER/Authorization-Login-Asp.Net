using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base
{
    /// <summary>
    /// پایه کلاس برای مخازن ارتباطی
    /// این کلاس عملیات مشترک بین مخازن ارتباطی را پیاده‌سازی می‌کند
    /// </summary>
    /// <typeparam name="TEntity">نوع موجودیت ارتباطی</typeparam>
    /// <typeparam name="TKey">نوع کلید اصلی</typeparam>
    /// <typeparam name="TSource">نوع موجودیت منبع</typeparam>
    /// <typeparam name="TTarget">نوع موجودیت هدف</typeparam>
    public abstract class RelationshipRepository<TEntity, TKey, TSource, TTarget> : BaseRepository<TEntity>
        where TEntity : class
        where TKey : struct
        where TSource : class
        where TTarget : class
    {
        protected readonly DbSet<TSource> _sourceDbSet;
        protected readonly DbSet<TTarget> _targetDbSet;

        protected RelationshipRepository(
            ApplicationDbContext context,
            ILogger<RelationshipRepository<TEntity, TKey, TSource, TTarget>> logger) : base(context, logger)
        {
            _sourceDbSet = context.Set<TSource>();
            _targetDbSet = context.Set<TTarget>();
        }

        /// <summary>
        /// دریافت ارتباطات یک موجودیت منبع
        /// </summary>
        protected async Task<IEnumerable<TEntity>> GetBySourceAsync(
            TKey sourceId,
            Expression<Func<TEntity, bool>> sourcePredicate,
            Expression<Func<TEntity, TTarget>> includeTarget,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(includeTarget)
                    .Where(sourcePredicate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت ارتباطات موجودیت منبع {SourceId}", sourceId);
                throw;
            }
        }

        /// <summary>
        /// دریافت ارتباطات یک موجودیت هدف
        /// </summary>
        protected async Task<IEnumerable<TEntity>> GetByTargetAsync(
            TKey targetId,
            Expression<Func<TEntity, bool>> targetPredicate,
            Expression<Func<TEntity, TSource>> includeSource,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet
                    .Include(includeSource)
                    .Where(targetPredicate)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در دریافت ارتباطات موجودیت هدف {TargetId}", targetId);
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود ارتباط
        /// </summary>
        protected async Task<bool> HasRelationshipAsync(
            TKey sourceId,
            TKey targetId,
            Expression<Func<TEntity, bool>> relationshipPredicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _dbSet.AnyAsync(relationshipPredicate, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بررسی ارتباط بین {SourceId} و {TargetId}", sourceId, targetId);
                throw;
            }
        }

        /// <summary>
        /// افزودن ارتباط
        /// </summary>
        protected async Task<bool> AddRelationshipAsync(
            TEntity relationship,
            Expression<Func<TEntity, bool>> relationshipPredicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (await _dbSet.AnyAsync(relationshipPredicate, cancellationToken))
                {
                    return true;
                }

                await _dbSet.AddAsync(relationship, cancellationToken);
                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در افزودن ارتباط");
                throw;
            }
        }

        /// <summary>
        /// حذف ارتباط
        /// </summary>
        protected async Task<bool> RemoveRelationshipAsync(
            Expression<Func<TEntity, bool>> relationshipPredicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var relationship = await _dbSet
                    .FirstOrDefaultAsync(relationshipPredicate, cancellationToken);

                if (relationship == null)
                {
                    return true;
                }

                if (relationship is ISoftDelete softDelete)
                {
                    softDelete.IsDeleted = true;
                    softDelete.DeletedAt = DateTime.UtcNow;
                }
                else
                {
                    _dbSet.Remove(relationship);
                }

                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در حذف ارتباط");
                throw;
            }
        }

        /// <summary>
        /// بروزرسانی ارتباطات
        /// </summary>
        protected async Task<bool> UpdateRelationshipsAsync(
            TKey sourceId,
            IEnumerable<TKey> targetIds,
            Func<TKey, TEntity> createRelationship,
            Expression<Func<TEntity, bool>> sourcePredicate,
            Expression<Func<TEntity, TKey>> targetIdSelector,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var currentRelationships = await _dbSet
                    .Where(sourcePredicate)
                    .ToListAsync(cancellationToken);

                var targetsToAdd = targetIds
                    .Where(tid => !currentRelationships.Any(cr => targetIdSelector.Compile()(cr).Equals(tid)))
                    .ToList();

                var relationshipsToRemove = currentRelationships
                    .Where(cr => !targetIds.Contains(targetIdSelector.Compile()(cr)))
                    .ToList();

                foreach (var targetId in targetsToAdd)
                {
                    await _dbSet.AddAsync(createRelationship(targetId), cancellationToken);
                }

                foreach (var relationship in relationshipsToRemove)
                {
                    if (relationship is ISoftDelete softDelete)
                    {
                        softDelete.IsDeleted = true;
                        softDelete.DeletedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _dbSet.Remove(relationship);
                    }
                }

                return await _context.SaveChangesAsync(cancellationToken) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بروزرسانی ارتباطات برای {SourceId}", sourceId);
                throw;
            }
        }

        /// <summary>
        /// پاکسازی ارتباطات حذف شده
        /// </summary>
        protected async Task<int> CleanupDeletedRelationshipsAsync(
            Expression<Func<TEntity, bool>> deletedPredicate,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var deletedRelationships = await _dbSet
                    .Where(deletedPredicate)
                    .ToListAsync(cancellationToken);

                if (!deletedRelationships.Any())
                {
                    return 0;
                }

                _dbSet.RemoveRange(deletedRelationships);
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در پاکسازی ارتباطات حذف شده");
                throw;
            }
        }
    }
} 