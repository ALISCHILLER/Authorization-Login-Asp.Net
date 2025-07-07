using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
using Authorization_Login_Asp.Net.Core.Application.DTOs.Auth; // For DeviceInfo

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ILoginHistoryService
    {
        Task LogSuccessfulLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo);
        Task LogFailedLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo, string reason);
        // Methods from AuthenticationService that seem to belong here or IUserRepository
        Task LogLogoutAsync(Guid userId);
        Task<(List<Domain.Entities.LoginHistory> Items, int TotalCount)> GetUserLoginHistoryAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<Domain.Entities.LoginHistory?> GetLastSuccessfulLoginAsync(Guid userId);
        Task<int> GetFailedLoginAttemptsCountAsync(Guid userId, int timeWindowMinutes = 15);

    }
}
