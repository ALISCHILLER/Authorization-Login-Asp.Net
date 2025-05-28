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
    /// پیاده‌سازی مخزن (Repository) ارتباط نقش و پرمیشن با استفاده از Entity Framework Core
    /// </summary>
    public class RolePermissionRepository : IRolePermissionRepository
    {
        private readonly AppDbContext _context;
        private readonly DbSet<RolePermission> _rolePermissions;

        public RolePermissionRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _rolePermissions = _context.Set<RolePermission>();
        }

        public async Task<IEnumerable<RolePermission>> GetAllAsync()
        {
            return await _rolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<RolePermission> GetByIdAsync(Guid id)
        {
            return await _rolePermissions
                .Include(rp => rp.Role)
                .Include(rp => rp.Permission)
                .FirstOrDefaultAsync(rp => rp.Id == id);
        }

        public async Task<IEnumerable<RolePermission>> GetByRoleIdAsync(Guid roleId)
        {
            return await _rolePermissions
                .Include(rp => rp.Permission)
                .Where(rp => rp.RoleId == roleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<RolePermission>> GetByPermissionIdAsync(Guid permissionId)
        {
            return await _rolePermissions
                .Include(rp => rp.Role)
                .Where(rp => rp.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task AddAsync(RolePermission rolePermission)
        {
            if (rolePermission == null)
                throw new ArgumentNullException(nameof(rolePermission));
            await _rolePermissions.AddAsync(rolePermission);
        }

        public void Remove(RolePermission rolePermission)
        {
            if (rolePermission == null)
                throw new ArgumentNullException(nameof(rolePermission));
            _rolePermissions.Remove(rolePermission);
        }

        public void Update(RolePermission rolePermission)
        {
            if (rolePermission == null)
                throw new ArgumentNullException(nameof(rolePermission));
            _context.Entry(rolePermission).State = EntityState.Modified;
        }

        public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId)
        {
            return await _rolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        }
    }
}