using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<User> _dbSet;
        private readonly DbSet<LoginHistory> _loginHistorySet;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<User>();
            _loginHistorySet = context.Set<LoginHistory>();
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email.Value == email && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
        }

        public async Task AddAsync(User user)
        {
            await _dbSet.AddAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            _dbSet.Update(user);
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task AddLoginHistoryAsync(LoginHistory loginHistory)
        {
            await _loginHistorySet.AddAsync(loginHistory);
        }

        public async Task UpdateLoginHistoryAsync(LoginHistory loginHistory)
        {
            _loginHistorySet.Update(loginHistory);
            await Task.CompletedTask;
        }

        public async Task<LoginHistory?> GetLastLoginHistoryAsync(Guid userId)
        {
            return await _loginHistorySet.Where(l => l.UserId == userId && !l.IsDeleted)
                .OrderByDescending(l => l.LoginTime).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(Guid userId, int page, int pageSize)
        {
            return await _loginHistorySet.Where(l => l.UserId == userId && !l.IsDeleted)
                .OrderByDescending(l => l.LoginTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetLoginHistoryCountAsync(Guid userId)
        {
            return await _loginHistorySet.CountAsync(l => l.UserId == userId && !l.IsDeleted);
        }

        public async Task<LoginHistory?> GetLastSuccessfulLoginAsync(Guid userId)
        {
            return await _loginHistorySet.Where(l => l.UserId == userId && l.IsSuccessful && !l.IsDeleted)
                .OrderByDescending(l => l.LoginTime).FirstOrDefaultAsync();
        }

        public async Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15)
        {
            var since = DateTime.UtcNow.AddMinutes(-timeWindowMinutes);
            return await _loginHistorySet.CountAsync(l => l.UserId == userId && !l.IsSuccessful && l.LoginTime >= since && !l.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Email.Value == email && !u.IsDeleted, cancellationToken);
        }

        public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(u => u.Username == username && !u.IsDeleted, cancellationToken);
        }

        public async Task<User?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && !u.IsDeleted);
        }

        public async Task<User?> GetByVerificationTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.VerificationToken == token && !u.IsDeleted);
        }

        public async Task<User?> GetByPasswordResetTokenAsync(string token)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.PasswordResetToken == token && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(string roleName)
        {
            return await _dbSet.Where(u => u.Roles.Any(r => r.Name == roleName) && !u.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByRoleAsync(Guid roleId)
        {
            return await _dbSet.Where(u => u.Roles.Any(r => r.Id == roleId) && !u.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByPermissionAsync(string permissionName)
        {
            return await _dbSet.Where(u => u.Roles.Any(r => r.RolePermissions.Any(p => p.Permission.Name == permissionName)) && !u.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<User>> GetByPermissionAsync(Guid permissionId)
        {
            return await _dbSet.Where(u => u.Roles.Any(r => r.RolePermissions.Any(p => p.Permission.Id == permissionId)) && !u.IsDeleted).ToListAsync();
        }

        // IGenericRepository<T> methods
        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet.Where(u => !u.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<User>> FindAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        void IGenericRepository<User>.Update(User entity)
        {
            _dbSet.Update(entity);
        }

        void IGenericRepository<User>.Remove(User entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<User, bool>> predicate)
        {
            return await _dbSet.CountAsync(predicate);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
