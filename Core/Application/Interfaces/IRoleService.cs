using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetRolesAsync(GetRolesRequest request);
        Task<RoleDto> GetRoleByIdAsync(Guid id);
        Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
        Task<RoleDto> UpdateRoleAsync(UpdateRoleRequest request);
        Task DeleteRoleAsync(Guid id);
        Task<bool> IsRoleNameUniqueAsync(string name);
        Task<bool> IsActiveAsync(Guid id);
        Task ActivateRoleAsync(Guid id);
        Task DeactivateRoleAsync(Guid id);
    }
} 