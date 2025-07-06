using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Infrastructure.Services
{
    public class UserPermissionService
    {
        private readonly IUserPermissionRepository _userPermissionRepository;

        public UserPermissionService(IUserPermissionRepository userPermissionRepository)
        {
            _userPermissionRepository = userPermissionRepository;
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            return await _userPermissionRepository.GetUserPermissionsAsync(userId);
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _userPermissionRepository.HasPermissionAsync(userId, permissionName);
        }

        public async Task AddPermissionToUserAsync(Guid userId, Guid permissionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            await _userPermissionRepository.AddPermissionToUserAsync(userId, permissionId);
        }

        public async Task RemovePermissionFromUserAsync(Guid userId, Guid permissionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("Invalid user ID", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            await _userPermissionRepository.RemovePermissionFromUserAsync(userId, permissionId);
        }

        public async Task<IEnumerable<User>> GetUsersByPermissionAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _userPermissionRepository.GetUsersByPermissionAsync(permissionName);
        }

        public async Task<IEnumerable<User>> GetUsersByPermissionAsync(Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Invalid permission ID", nameof(permissionId));

            return await _userPermissionRepository.GetUsersByPermissionAsync(permissionId);
        }
    }
} 