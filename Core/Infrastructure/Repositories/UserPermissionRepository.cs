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
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Repositories
{
    public class UserPermissionRepository : BaseRepository<UserPermission>, IUserPermissionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserPermissionRepository> _logger;

        public UserPermissionRepository(
            ApplicationDbContext context,
            ICacheService cacheService,
            ILogger<UserPermissionRepository> logger) : base(context, cacheService, logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<UserPermission>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _dbSet
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserPermission>> GetByPermissionIdAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _dbSet
                .Include(up => up.User)
                .Where(up => up.PermissionId == permissionId && !up.IsDeleted)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _dbSet
                .AnyAsync(up => 
                    up.UserId == userId && 
                    up.PermissionId == permissionId && 
                    !up.IsDeleted, 
                    cancellationToken);
        }

        public async Task AddPermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };

            await _dbSet.AddAsync(userPermission, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RemovePermissionAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            var userPermission = await _dbSet
                .FirstOrDefaultAsync(up => 
                    up.UserId == userId && 
                    up.PermissionId == permissionId && 
                    !up.IsDeleted, 
                    cancellationToken);

            if (userPermission != null)
            {
                userPermission.IsDeleted = true;
                userPermission.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _dbSet
                .Include(up => up.Permission)
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .Select(up => up.Permission)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<User>> GetUsersWithPermissionAsync(Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _dbSet
                .Include(up => up.User)
                .Where(up => up.PermissionId == permissionId && !up.IsDeleted)
                .Select(up => up.User)
                .ToListAsync(cancellationToken);
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

        /// <summary>
        /// دریافت دسترسی‌های مستقیم یک کاربر
        /// </summary>
        public async Task<IEnumerable<UserPermission>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            return await _dbSet
                .Where(up => up.UserId == userGuid && !up.IsDeleted)
                .Include(up => up.Permission)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی مستقیم کاربر به یک عملیات خاص
        /// </summary>
        public async Task<bool> HasPermissionAsync(string userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            return await _dbSet
                .AnyAsync(up => 
                    up.UserId == userGuid && 
                    !up.IsDeleted && 
                    up.Permission.Name == permissionName && 
                    up.Permission.IsActive, 
                    cancellationToken);
        }

        /// <summary>
        /// بررسی دسترسی مستقیم کاربر به چند عملیات
        /// </summary>
        public async Task<bool> HasAllPermissionsAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentException("لیست دسترسی‌ها نمی‌تواند خالی باشد", nameof(permissionNames));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            var userPermissions = await _dbSet
                .Where(up => 
                    up.UserId == userGuid && 
                    !up.IsDeleted && 
                    up.Permission.IsActive)
                .Select(up => up.Permission.Name)
                .ToListAsync(cancellationToken);

            return permissionNames.All(permissionName => userPermissions.Contains(permissionName));
        }

        /// <summary>
        /// بررسی دسترسی مستقیم کاربر به حداقل یکی از عملیات
        /// </summary>
        public async Task<bool> HasAnyPermissionAsync(string userId, IEnumerable<string> permissionNames, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (permissionNames == null || !permissionNames.Any())
                throw new ArgumentException("لیست دسترسی‌ها نمی‌تواند خالی باشد", nameof(permissionNames));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            return await _dbSet
                .AnyAsync(up => 
                    up.UserId == userGuid && 
                    !up.IsDeleted && 
                    up.Permission.IsActive && 
                    permissionNames.Contains(up.Permission.Name), 
                    cancellationToken);
        }

        /// <summary>
        /// اضافه کردن دسترسی مستقیم به کاربر
        /// </summary>
        public async Task<UserPermission> AddPermissionToUserAsync(string userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            var permission = await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == permissionName && p.IsActive, cancellationToken);

            if (permission == null)
                throw new InvalidOperationException($"دسترسی '{permissionName}' یافت نشد");

            var userPermission = new UserPermission
            {
                UserId = userGuid,
                PermissionId = permission.Id
            };

            await _dbSet.AddAsync(userPermission, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return userPermission;
        }

        /// <summary>
        /// حذف دسترسی مستقیم از کاربر
        /// </summary>
        public async Task RemovePermissionFromUserAsync(string userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("شناسه کاربر نمی‌تواند خالی باشد", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("نام دسترسی نمی‌تواند خالی باشد", nameof(permissionName));

            if (!Guid.TryParse(userId, out Guid userGuid))
                throw new FormatException($"فرمت شناسه کاربر '{userId}' نامعتبر است");

            var userPermission = await _dbSet
                .FirstOrDefaultAsync(up => 
                    up.UserId == userGuid && 
                    !up.IsDeleted && 
                    up.Permission.Name == permissionName, 
                    cancellationToken);

            if (userPermission != null)
            {
                userPermission.IsDeleted = true;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _context.UserPermissions
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .Select(up => up.Permission)
                .ToListAsync();
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string permissionName, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));
            return await _dbSet.AnyAsync(up => up.UserId == userId && up.Permission.Name == permissionName && !up.IsDeleted, cancellationToken);
        }

        public async Task AddPermissionToUserAsync(Guid userId, Guid permissionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            var userPermission = new UserPermission
            {
                UserId = userId,
                PermissionId = permissionId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.UserPermissions.AddAsync(userPermission);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePermissionFromUserAsync(Guid userId, Guid permissionId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            var userPermission = await _context.UserPermissions
                .FirstOrDefaultAsync(up => up.UserId == userId && 
                                         up.PermissionId == permissionId && 
                                         !up.IsDeleted);

            if (userPermission != null)
            {
                userPermission.IsDeleted = true;
                userPermission.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<User>> GetUsersByPermissionAsync(string permissionName)
        {
            if (string.IsNullOrWhiteSpace(permissionName))
                throw new ArgumentException("Permission name cannot be empty", nameof(permissionName));

            return await _context.UserPermissions
                .Where(up => up.Permission.Name == permissionName && !up.IsDeleted)
                .Select(up => up.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByPermissionAsync(Guid permissionId)
        {
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _context.UserPermissions
                .Where(up => up.PermissionId == permissionId && !up.IsDeleted)
                .Select(up => up.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsByRoleAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (roleId == Guid.Empty)
                throw new ArgumentException("Role ID cannot be empty", nameof(roleId));

            return await _context.UserPermissions
                .Where(up => up.UserId == userId && !up.IsDeleted)
                .Include(up => up.Permission)
                .Select(up => up.Permission)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> IsPermissionAssignedToUserAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));

            return await _context.UserPermissions
                .AnyAsync(up => up.UserId == userId && 
                               up.PermissionId == permissionId && 
                               !up.IsDeleted, 
                          cancellationToken);
        }

        public async Task<int> GetUserPermissionCountAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            return await _context.UserPermissions
                .CountAsync(up => up.UserId == userId && !up.IsDeleted, cancellationToken);
        }

        public async Task<IEnumerable<Permission>> GetUserPermissionsByTypeAsync(Guid userId, string permissionType, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));

            if (string.IsNullOrWhiteSpace(permissionType))
                throw new ArgumentException("Permission type cannot be empty", nameof(permissionType));

            return await _context.UserPermissions
                .Where(up => up.UserId == userId && 
                            !up.IsDeleted && 
                            up.Permission.Type == permissionType)
                .Include(up => up.Permission)
                .Select(up => up.Permission)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid userId, Guid permissionId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (permissionId == Guid.Empty)
                throw new ArgumentException("Permission ID cannot be empty", nameof(permissionId));
            return await _dbSet.AnyAsync(up => up.UserId == userId && up.PermissionId == permissionId && !up.IsDeleted, cancellationToken);
        }
    }
} 