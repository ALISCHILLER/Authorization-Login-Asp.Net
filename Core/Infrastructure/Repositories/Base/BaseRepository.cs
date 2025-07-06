using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base
{
    public abstract class BaseRepository<T> : IGenericRepository<T> where T : class, Authorization_Login_Asp.Net.Core.Domain.Common.IEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        protected BaseRepository(ApplicationDbContext context, Microsoft.Extensions.Logging.ILogger logger)
            : this(context) { }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
        }

        public virtual void Remove(T entity)
        {
            _dbSet.Remove(entity);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}