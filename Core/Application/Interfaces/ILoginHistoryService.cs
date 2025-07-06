using System;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.ValueObjects;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ILoginHistoryService
    {
        Task LogSuccessfulLoginAsync(Guid userId, string ip, string location, DeviceInfo deviceInfo);
        Task LogFailedLoginAsync(Guid userId, string ip, string location, DeviceInfo deviceInfo, string reason);
    }
}
