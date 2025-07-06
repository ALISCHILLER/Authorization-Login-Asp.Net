using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IPermissionRepository : IGenericRepository<Permission>
    {
        Task<Permission?> GetByNameAsync(string name);
        Task<IEnumerable<Permission>> GetByRoleAsync(Guid roleId);
        Task<IEnumerable<Permission>> GetByRoleNameAsync(string roleName);
        Task<IEnumerable<Permission>> GetByUserAsync(Guid userId);
        Task<IEnumerable<Permission>> GetByUsernameAsync(string username);
    }
} 