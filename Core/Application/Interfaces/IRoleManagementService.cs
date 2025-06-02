using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IRoleManagementService
    {
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> GetRoleByIdAsync(Guid id);
        Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
        Task<RoleDto> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
        Task DeleteRoleAsync(Guid id);
        Task AssignPermissionsToRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds);
        Task RemovePermissionsFromRoleAsync(Guid roleId, IEnumerable<Guid> permissionIds);
    }
} 