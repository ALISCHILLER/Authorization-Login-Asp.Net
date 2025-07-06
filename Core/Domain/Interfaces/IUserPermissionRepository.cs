using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IUserPermissionRepository
    {
        Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> HasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default);
        Task AddPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default);
        Task RemovePermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default);
        Task<IEnumerable<User>> GetUsersWithPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default);
    }
} 