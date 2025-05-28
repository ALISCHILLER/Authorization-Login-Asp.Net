using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Application.Interfaces;
using Authorization_Login_Asp.Net.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Authorization_Login_Asp.Net.Infrastructure.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PermissionService> _logger;
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheOptions;

        public PermissionService(
            IPermissionRepository permissionRepository,
            IRoleRepository roleRepository,
            IUserRepository userRepository,
            ILogger<PermissionService> logger,
            IMemoryCache cache,
            IConfiguration configuration)
        {
            _permissionRepository = permissionRepository;
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _logger = logger;
            _cache = cache;

            var cacheExpirationMinutes = configuration.GetValue<int>("CacheSettings:PermissionCacheExpirationMinutes", 30);
            _cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes))
                .SetSlidingExpiration(TimeSpan.FromMinutes(cacheExpirationMinutes / 2));
        }

        public async Task<IEnumerable<Permission>> GetAllPermissionsAsync()
        {
            try
            {
                const string cacheKey = "all_permissions";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                permissions = await _permissionRepository.GetAllAsync();
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all permissions");
                throw new PermissionServiceException("Failed to get all permissions", ex);
            }
        }

        public async Task<Permission> GetPermissionByIdAsync(int id)
        {
            try
            {
                var cacheKey = $"permission_{id}";
                if (_cache.TryGetValue(cacheKey, out Permission permission))
                {
                    return permission;
                }

                permission = await _permissionRepository.GetByIdAsync(id);
                if (permission != null)
                {
                    _cache.Set(cacheKey, permission, _cacheOptions);
                }
                return permission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permission by id {PermissionId}", id);
                throw new PermissionServiceException("Failed to get permission", ex);
            }
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByRoleIdAsync(int roleId)
        {
            try
            {
                var cacheKey = $"role_permissions_{roleId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permissions for role {RoleId}", roleId);
                throw new PermissionServiceException("Failed to get role permissions", ex);
            }
        }

        public async Task<IEnumerable<Permission>> GetPermissionsByUserIdAsync(int userId)
        {
            try
            {
                var cacheKey = $"user_permissions_{userId}";
                if (_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
                {
                    return permissions;
                }

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new PermissionServiceException($"User not found with id {userId}");
                }

                var role = await _roleRepository.GetByIdAsync(user.RoleId);
                if (role == null)
                {
                    throw new PermissionServiceException($"Role not found for user {userId}");
                }

                permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(role.Id);
                _cache.Set(cacheKey, permissions, _cacheOptions);
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get permissions for user {UserId}", userId);
                throw new PermissionServiceException("Failed to get user permissions", ex);
            }
        }

        public async Task<bool> HasPermissionAsync(int userId, string permissionName)
        {
            try
            {
                var cacheKey = $"user_permission_{userId}_{permissionName}";
                if (_cache.TryGetValue(cacheKey, out bool hasPermission))
                {
                    return hasPermission;
                }

                var permissions = await GetPermissionsByUserIdAsync(userId);
                hasPermission = permissions.Any(p => p.Name.Equals(permissionName, StringComparison.OrdinalIgnoreCase));
                _cache.Set(cacheKey, hasPermission, _cacheOptions);
                return hasPermission;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check permission {PermissionName} for user {UserId}", permissionName, userId);
                throw new PermissionServiceException("Failed to check permission", ex);
            }
        }

        public async Task AssignPermissionToRoleAsync(int roleId, int permissionId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    throw new PermissionServiceException($"Role not found with id {roleId}");
                }

                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    throw new PermissionServiceException($"Permission not found with id {permissionId}");
                }

                await _permissionRepository.AssignPermissionToRoleAsync(roleId, permissionId);
                InvalidateRolePermissionCache(roleId);
                _logger.LogInformation("Assigned permission {PermissionId} to role {RoleId}", permissionId, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign permission {PermissionId} to role {RoleId}", permissionId, roleId);
                throw new PermissionServiceException("Failed to assign permission to role", ex);
            }
        }

        public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    throw new PermissionServiceException($"Role not found with id {roleId}");
                }

                var permission = await _permissionRepository.GetByIdAsync(permissionId);
                if (permission == null)
                {
                    throw new PermissionServiceException($"Permission not found with id {permissionId}");
                }

                await _permissionRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
                InvalidateRolePermissionCache(roleId);
                _logger.LogInformation("Removed permission {PermissionId} from role {RoleId}", permissionId, roleId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to remove permission {PermissionId} from role {RoleId}", permissionId, roleId);
                throw new PermissionServiceException("Failed to remove permission from role", ex);
            }
        }

        private void InvalidateRolePermissionCache(int roleId)
        {
            _cache.Remove($"role_permissions_{roleId}");
            _cache.Remove("all_permissions");
        }

        private void InvalidateUserPermissionCache(int userId)
        {
            _cache.Remove($"user_permissions_{userId}");
            _cache.Remove($"user_permission_{userId}_*");
        }

        public Task<bool> HasPermissionAsync(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserPermissionsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task AddPermissionAsync(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public Task RemovePermissionAsync(Guid userId, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasRolePermissionAsync(string role, string resource, string action)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetRolePermissionsAsync(string role)
        {
            throw new NotImplementedException();
        }
    }

    public class PermissionServiceException : Exception
    {
        public PermissionServiceException(string message) : base(message) { }
        public PermissionServiceException(string message, Exception innerException) 
            : base(message, innerException) { }
    }
}