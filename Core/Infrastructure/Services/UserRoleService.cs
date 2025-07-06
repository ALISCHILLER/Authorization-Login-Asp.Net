using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Core.Infrastructure.Services
{
    public class UserRoleService
    {
        private readonly Authorization_Login_Asp.Net.Core.Domain.Interfaces.IUserRoleRepository _userRoleRepository;

        public UserRoleService(Authorization_Login_Asp.Net.Core.Domain.Interfaces.IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        public async Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _userRoleRepository.GetUserRolesAsync(userId);
        }

        public async Task<bool> HasRoleAsync(Guid userId, string roleName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            return await _userRoleRepository.HasRoleAsync(userId, roleName);
        }

        public async Task AddRoleToUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            await _userRoleRepository.AddRoleToUserAsync(userId, roleId);
        }

        public async Task RemoveRoleFromUserAsync(Guid userId, Guid roleId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            await _userRoleRepository.RemoveRoleFromUserAsync(userId, roleId);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Role name cannot be empty", nameof(roleName));

            return await _userRoleRepository.GetUsersByRoleAsync(roleName);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            return await _userRoleRepository.GetUsersByRoleAsync(roleId);
        }
    }
}