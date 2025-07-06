using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Role>> GetByUsernameAsync(string username);
        Task<IEnumerable<Role>> GetByPermissionAsync(Guid permissionId);
        Task<IEnumerable<Role>> GetByPermissionNameAsync(string permissionName);
        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    }
} 