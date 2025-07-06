// استفاده از رابط‌های تعریف شده در لایه کاربرد
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
// استفاده از کلاس‌های پایگاه داده در لایه زیرساخت
// استفاده از Entity Framework Core برای عملیات پایگاه داده
using Microsoft.EntityFrameworkCore;
// استفاده از کلاس‌های پایه سیستم
using System;
// استفاده از کلاس‌های مجموعه‌ها
using System.Collections.Generic;
// استفاده از کلاس‌های LINQ
using System.Linq;
// استفاده از کلاس‌های مدیریت نخ‌ها
using System.Threading;
// استفاده از کلاس‌های عملیات ناهمگام
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;
using System.Linq.Expressions;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن نقش برای انجام عملیات روی مدل Role
    /// </summary>
    public class RoleRepository : BaseRepository<Role>, IRoleRepository
    {
        private readonly ILogger<RoleRepository> _logger;

        public RoleRepository(
            ApplicationDbContext context,
            ILogger<RoleRepository> logger) : base(context)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty", nameof(name));

            return await _dbSet
                .Include(r => r.Permissions)
                .Include(r => r.Users)
                .FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            return await _dbSet
                .Include(r => r.Users)
                .Where(r => r.Users.Any(u => u.Id == userId) && !r.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Role>> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be empty", nameof(username));

            return await _dbSet
                .Include(r => r.Users)
                .Where(r => r.Users.Any(u => u.Username == username) && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetByPermissionAsync(Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r => r.RolePermissions.Any(rp => rp.PermissionId == permissionId) && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<Role>> GetByPermissionNameAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _dbSet
                .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
                .Where(r => r.RolePermissions.Any(rp => rp.Permission != null && rp.Permission.Name == permissionName) && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Role name cannot be empty", nameof(name));

            return await _dbSet.AnyAsync(r => r.Name == name && !r.IsDeleted, cancellationToken);
        }

        // IGenericRepository<Role> implementation
        public async Task<Role?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(id));
            return await _dbSet.FindAsync(id);
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _dbSet.Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task<IEnumerable<Role>> FindAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Where(r => !r.IsDeleted).ToListAsync();
        }

        public async Task AddAsync(Role entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            await _dbSet.AddAsync(entity);
        }

        public void Update(Role entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Remove(Role entity)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            // حذف منطقی فقط اگر پراپرتی‌ها قابل نوشتن باشند
            var isDeletedProp = entity.GetType().GetProperty("IsDeleted");
            var deletedAtProp = entity.GetType().GetProperty("DeletedAt");
            if (isDeletedProp != null && isDeletedProp.CanWrite)
                isDeletedProp.SetValue(entity, true);
            if (deletedAtProp != null && deletedAtProp.CanWrite)
                deletedAtProp.SetValue(entity, DateTime.UtcNow);
            _dbSet.Update(entity);
        }

        public async Task<bool> ExistsAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<Role, bool>> predicate)
        {
            return await _dbSet.Where(predicate).Where(r => !r.IsDeleted).CountAsync();
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}