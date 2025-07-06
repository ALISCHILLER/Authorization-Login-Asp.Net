using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<Role>> GetUserRolesAsync(Guid userId);
        Task<bool> HasRoleAsync(Guid userId, string roleName);
        Task AddRoleToUserAsync(Guid userId, Guid roleId);
        Task RemoveRoleFromUserAsync(Guid userId, Guid roleId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<IEnumerable<User>> GetUsersByRoleAsync(Guid roleId);
    }
} 