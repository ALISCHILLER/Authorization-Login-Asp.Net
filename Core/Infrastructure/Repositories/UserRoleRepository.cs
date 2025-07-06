using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Select(ur => ur.Role)
                .ToListAsync();
        }

        public async Task<bool> HasRoleAsync(Guid userId, string roleName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && 
                               ur.Role.Name == roleName && 
                               !ur.IsDeleted);
        }

        public async Task AddRoleToUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && 
                                         ur.RoleId == roleId && 
                                         !ur.IsDeleted);

            if (userRole != null)
            {
                userRole.IsDeleted = true;
                userRole.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            return await _context.UserRoles
                .Where(ur => ur.Role.Name == roleName && !ur.IsDeleted)
                .Select(ur => ur.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

            return await _context.UserRoles
                .Where(ur => ur.RoleId == roleId && !ur.IsDeleted)
                .Select(ur => ur.User)
                .ToListAsync();
        }
    }
} 