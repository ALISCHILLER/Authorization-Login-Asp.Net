using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IRolePermissionRepository
    {
        Task<IEnumerable<Permission>> GetRolePermissionsAsync(Guid roleId);
        Task<bool> HasPermissionAsync(Guid roleId, string permissionName);
        Task AddPermissionToRoleAsync(Guid roleId, Guid permissionId);
        Task RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId);
        Task<IEnumerable<Role>> GetRolesByPermissionAsync(string permissionName);
        Task<IEnumerable<Role>> GetRolesByPermissionAsync(Guid permissionId);
    }
} 