using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserPermissionRepository _userPermissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public PermissionService(
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IUserPermissionRepository userPermissionRepository,
            IRolePermissionRepository rolePermissionRepository)
        {
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _userPermissionRepository = userPermissionRepository ?? throw new ArgumentNullException(nameof(userPermissionRepository));
            _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permission, string systemName)
        {
            var permissions = await _userPermissionRepository.GetUserPermissionsAsync(userId);
            return permissions.Any(p => p.Name == permission && p.SystemName == systemName);
        }

        public async Task<object> GetUserPermissionsAsync(Guid userId)
        {
            var permissions = await _userPermissionRepository.GetUserPermissionsAsync(userId);
            return permissions.Select(p => new { p.Id, p.Name, p.Description, p.IsActive, p.SystemName });
        }

        public async Task AddPermissionAsync(Guid userId, string permission, string systemName)
        {
            var perm = (await _permissionRepository.GetAllAsync()).FirstOrDefault(p => p.Name == permission && p.SystemName == systemName);
            if (perm == null) throw new Exception("Permission not found");
            await _userPermissionRepository.AddPermissionAsync(userId, perm.Id);
        }

        public async Task RemovePermissionAsync(Guid userId, string permission, string systemName)
        {
            var perm = (await _permissionRepository.GetAllAsync()).FirstOrDefault(p => p.Name == permission && p.SystemName == systemName);
            if (perm == null) throw new Exception("Permission not found");
            await _userPermissionRepository.RemovePermissionAsync(userId, perm.Id);
        }

        public async Task<bool> HasRolePermissionAsync(string roleName, string permission, string systemName)
        {
            var role = (await _roleRepository.GetByNameAsync(roleName));
            if (role == null) return false;
            var permissions = await _rolePermissionRepository.GetRolePermissionsAsync(role.Id);
            return permissions.Any(p => p.Name == permission && p.SystemName == systemName);
        }

        public async Task<object> GetRolePermissionsAsync(string roleName)
        {
            var role = (await _roleRepository.GetByNameAsync(roleName));
            if (role == null) return Array.Empty<object>();
            var permissions = await _rolePermissionRepository.GetRolePermissionsAsync(role.Id);
            return permissions.Select(p => new { p.Id, p.Name, p.Description, p.IsActive, p.SystemName });
        }
    }
}