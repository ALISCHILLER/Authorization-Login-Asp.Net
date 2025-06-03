using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Authorization_Login_Asp.Net.Core.Infrastructure.Repositories.Base;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserPermissionRepository : RelationshipRepository<UserPermission, Guid, User, Permission>, IUserPermissionRepository
    {
        public UserPermissionRepository(
            ApplicationDbContext context,
            ILogger<UserPermissionRepository> logger) : base(context, logger)
        {
        }

        public async Task<IEnumerable<UserPermission>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await GetBySourceAsync(
                userId,
                up => up.UserId == userId && !up.IsDeleted,
                up => up.Permission,
                cancellationToken);
        }

        public async Task<IEnumerable<UserPermission>> GetByPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await GetByTargetAsync(
                permissionId,
                up => up.PermissionId == permissionId && !up.IsDeleted,
                up => up.User,
                cancellationToken);
        }

        public async Task<bool> HasPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await HasRelationshipAsync(
                userId,
                permissionId,
                up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> AddPermissionToUserAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };

            return await AddRelationshipAsync(
                userPermission,
                up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> RemovePermissionFromUserAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            return await RemoveRelationshipAsync(
                up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted,
                cancellationToken);
        }

        public async Task<bool> UpdateUserPermissionsAsync(
            Guid userId,
            IEnumerable<Guid> permissionIds,
            CancellationToken cancellationToken = default)
        {
            return await UpdateRelationshipsAsync(
                userId,
                permissionIds,
                permissionId => new UserPermission
                {
                    UserId = userId,
                    PermissionId = permissionId,
                    CreatedAt = DateTime.UtcNow
                },
                up => up.UserId == userId && !up.IsDeleted,
                up => up.PermissionId,
                cancellationToken);
        }

        public async Task<int> CleanupDeletedUserPermissionsAsync(CancellationToken cancellationToken = default)
        {
            return await CleanupDeletedRelationshipsAsync(
                up => up.IsDeleted && up.DeletedAt < DateTime.UtcNow.AddDays(-30),
                cancellationToken);
        }
    }
} 