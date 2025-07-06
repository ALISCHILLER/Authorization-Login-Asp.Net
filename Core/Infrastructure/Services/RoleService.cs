using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Enums;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;
using Authorization_Login_Asp.Net.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionRepository _permissionRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly Authorization_Login_Asp.Net.Core.Domain.Interfaces.IUserRoleRepository _userRoleRepository;
        private readonly ILoggingService _logger;
        private readonly IConfiguration _configuration;
        private readonly ITracingService _tracingService;
        private readonly ApplicationDbContext _dbContext;

        public RoleService(
            IRoleRepository roleRepository,
            IPermissionRepository permissionRepository,
            IRolePermissionRepository rolePermissionRepository,
            Authorization_Login_Asp.Net.Core.Domain.Interfaces.IUserRoleRepository userRoleRepository,
            ILoggingService logger,
            IConfiguration configuration,
            ITracingService tracingService,
            ApplicationDbContext dbContext)
        {
            _roleRepository = roleRepository ?? throw new ArgumentNullException(nameof(roleRepository));
            _permissionRepository = permissionRepository ?? throw new ArgumentNullException(nameof(permissionRepository));
            _rolePermissionRepository = rolePermissionRepository ?? throw new ArgumentNullException(nameof(rolePermissionRepository));
            _userRoleRepository = userRoleRepository ?? throw new ArgumentNullException(nameof(userRoleRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _tracingService = tracingService ?? throw new ArgumentNullException(nameof(tracingService));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<IEnumerable<RoleDto>> GetRolesAsync(GetRolesRequest request)
        {
            var query = _dbContext.Roles.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                query = query.Where(r => r.Name.Contains(request.SearchTerm) ||
                                       r.Description.Contains(request.SearchTerm));

            if (request.IsActive.HasValue)
                query = query.Where(r => r.IsActive == request.IsActive.Value);

            // PermissionId filter removed: not present in GetRolesRequest or Role entity

            var roles = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return roles.Select(MapToDto);
        }

        public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
        {
            var role = new Role(
                request.Name,
                request.Description ?? string.Empty,
                RoleType.Custom  // 使用自定义类型作为默认值
            );

            if (!request.IsActive)
            {
                role.Deactivate();
            }

            if (request.PermissionIds?.Any() == true)
            {
                var permissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds);
                foreach (var permission in permissions)
                {
                    await _rolePermissionRepository.AddPermissionToRoleAsync(role.Id, permission.Id);
                }
            }

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();

            return await GetRoleByIdAsync(role.Id);
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request)
        {
            var role = await _roleRepository.GetByIdAsync(request.Id);
            if (role == null)
                throw new ArgumentException("Role not found", nameof(request.Id));

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                role.UpdateDetails(
                    request.Name, 
                    request.Description ?? role.Description
                );
            }

            if (request.IsActive.HasValue)
            {
                if (request.IsActive.Value)
                    role.Activate();
                else
                    role.Deactivate();
            }

            if (request.PermissionIds != null)
            {
                await _rolePermissionRepository.RemoveAllPermissionsFromRoleAsync(role.Id);

                if (request.PermissionIds.Any())
                {
                    var permissions = await _permissionRepository.GetByIdsAsync(request.PermissionIds);
                    foreach (var permission in permissions)
                    {
                        await _rolePermissionRepository.AddPermissionToRoleAsync(role.Id, permission.Id);
                    }
                }
            }

            await _roleRepository.SaveChangesAsync();

            return await GetRoleByIdAsync(role.Id);
        }

        public async Task<RoleDto> GetRoleByIdAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return null;
            }

            var permissions = await _rolePermissionRepository.GetRolePermissionsAsync(id);
            return MapToDto(role, permissions);
        }

        public async Task DeleteRoleAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new ArgumentException("Role not found", nameof(id));
            _roleRepository.Remove(role);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task<bool> IsActiveAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
                throw new ArgumentException("Role not found", nameof(id));

            return role.IsActive;
        }

        public async Task ActivateRoleAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new ArgumentException("Role not found", nameof(id));
            role.Activate();
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task DeactivateRoleAsync(Guid id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role is null)
                throw new ArgumentException("Role not found", nameof(id));
            role.Deactivate();
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync();
        }

        public async Task<bool> IsRoleNameUniqueAsync(string name)
        {
            var exists = await _dbContext.Roles.AnyAsync(r => r.Name == name);
            return !exists;
        }

        private RoleDto MapToDto(Role role, IEnumerable<Permission> permissions = null)
        {
            if (role == null) return null;

            return new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                Type = role.Type,
                IsActive = role.IsActive,
                CreatedAt = role.CreatedAt,
                CreatedBy = role.CreatedBy,
                UpdatedAt = role.LastModifiedAt,
                UpdatedBy = role.LastModifiedBy,
                Permissions = permissions?.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Category = p.Category,
                    CreatedAt = p.CreatedAt
                }).ToList() ?? new List<PermissionDto>()
            };
        }
    }
}