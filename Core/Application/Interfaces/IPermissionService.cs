using System;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, string permission, string systemName);
        Task<object> GetUserPermissionsAsync(Guid userId);
        Task AddPermissionAsync(Guid userId, string permission, string systemName);
        Task RemovePermissionAsync(Guid userId, string permission, string systemName);
        Task<bool> HasRolePermissionAsync(string roleName, string permission, string systemName);
        Task<object> GetRolePermissionsAsync(string roleName);
    }
}
