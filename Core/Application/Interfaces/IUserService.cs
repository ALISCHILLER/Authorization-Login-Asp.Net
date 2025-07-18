using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserDto> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
        // Add more as needed
    }
}
