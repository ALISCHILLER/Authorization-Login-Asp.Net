using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Authorization_Login_Asp.Net.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Infrastructure.Repositories
{
    /// <summary>
    /// پیاده‌سازی مخزن (Repository) پرمیشن‌ها با استفاده از Entity Framework Core
    /// </summary>
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<Permission> _permissions;

        public PermissionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _permissions = _context.Set<Permission>();
        }

        public async Task<IEnumerable<Permission>> GetAllAsync()
        {
            return await _permissions
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .ToListAsync();
        }

        public async Task<Permission> GetByIdAsync(Guid id)
        {
            return await _permissions
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Permission>> GetByTypeAsync(int permissionType)
        {
            return await _permissions
                .Include(p => p.RolePermissions)
                .ThenInclude(rp => rp.Role)
                .Where(p => (int)p.Type == permissionType)
                .ToListAsync();
        }

        public async Task AddAsync(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));
            await _permissions.AddAsync(permission);
        }

        public void Remove(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));
            _permissions.Remove(permission);
        }

        public void Update(Permission permission)
        {
            if (permission == null)
                throw new ArgumentNullException(nameof(permission));
            _context.Entry(permission).State = EntityState.Modified;
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("نام پرمیشن نمی‌تواند خالی باشد.", nameof(name));
            return await _permissions.AnyAsync(p => p.Name == name);
        }

        public Task<IEnumerable<Permission>?> GetPermissionsByRoleIdAsync(int roleId)
        {
            throw new NotImplementedException();
        }

        public Task AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            throw new NotImplementedException();
        }

        public Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            throw new NotImplementedException();
        }
    }
}