using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<UserDto> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        // Add more as needed
    }
}
