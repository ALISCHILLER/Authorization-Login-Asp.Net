using Authorization_Login_Asp.Net.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByRefreshTokenAsync(string refreshToken);
        Task<User?> GetByVerificationTokenAsync(string token);
        Task<User?> GetByPasswordResetTokenAsync(string token);
        Task<IEnumerable<User>> GetByRoleAsync(string roleName);
        Task<IEnumerable<User>> GetByRoleAsync(Guid roleId);
        Task<IEnumerable<User>> GetByPermissionAsync(string permissionName);
        Task<IEnumerable<User>> GetByPermissionAsync(Guid permissionId);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        // متدهای مورد نیاز برای تاریخچه ورود
        Task AddLoginHistoryAsync(LoginHistory loginHistory);
        Task UpdateLoginHistoryAsync(LoginHistory loginHistory);
        Task<LoginHistory?> GetLastLoginHistoryAsync(Guid userId);
        Task<IEnumerable<LoginHistory>> GetLoginHistoryAsync(Guid userId, int page, int pageSize);
        Task<int> GetLoginHistoryCountAsync(Guid userId);
        Task<LoginHistory?> GetLastSuccessfulLoginAsync(Guid userId);
        Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    }
}