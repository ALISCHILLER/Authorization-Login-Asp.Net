using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces.Services
{
    public interface ILoginHistoryService
    {
        Task LogSuccessfulLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo);
        Task LogFailedLoginAsync(Guid userId, string ipAddress, string? userAgent, DeviceInfo? deviceInfo, string reason);
    }
}
