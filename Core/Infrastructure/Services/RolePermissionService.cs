using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Infrastructure.Services
{
    public class RolePermissionService
    {
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public RolePermissionService(IRolePermissionRepository rolePermissionRepository)
        {
            _rolePermissionRepository = rolePermissionRepository;
        }

        public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            return await _rolePermissionRepository.GetRolePermissionsAsync(roleId);
        }

        public async Task<bool> HasPermissionAsync(Guid roleId, string permissionName)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _rolePermissionRepository.HasPermissionAsync(roleId, permissionName);
        }

        public async Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            await _rolePermissionRepository.AddPermissionToRoleAsync(roleId, permissionId);
        }

        public async Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId)
        {
            if (roleId == Guid.Empty)
                throw new ArgumentException("Invalid role ID", nameof(roleId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            await _rolePermissionRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _rolePermissionRepository.GetRolesByPermissionAsync(permissionName);
        }

        public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            return await _rolePermissionRepository.GetRolesByPermissionAsync(permissionId);
        }
    }
} 