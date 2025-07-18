using System;
using System.Threading;
using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Application.DTOs;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using Authorization_Login_Asp.Net.Core.Domain.Entities;
using Authorization_Login_Asp.Net.Core.Domain.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);
            return user == null ? null! : MapToDto(user);
        }

        public async Task<UserDto> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            return user == null ? null! : MapToDto(user);
        }

        public async Task<UserDto> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);
            return user == null ? null! : MapToDto(user);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task<bool> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.ActivateAsync(id, cancellationToken);
        }

        public async Task<bool> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _userRepository.DeactivateAsync(id, cancellationToken);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.EmailAddress,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                ProfileImageUrl = user.ProfileImageUrl,
                IsEmailVerified = user.IsEmailVerified,
                IsPhoneVerified = user.IsPhoneVerified,
                LastLoginAt = user.LastLoginAt,
                TwoFactorEnabled = user.TwoFactorEnabled,
                PrimaryRole = user.Roles.FirstOrDefault()?.Name ?? string.Empty,
                Roles = user.Roles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name ?? string.Empty,
                    Description = r.Description ?? string.Empty,
                    IsActive = r.IsActive,
                    Permissions = r.RolePermissions.Select(p => p.Permission.Name).ToList()
                }).ToList(),
                Permissions = user.UserPermissions.Select(up => new PermissionDto
                {
                    Id = up.Permission.Id,
                    Name = up.Permission.Name,
                    Description = up.Permission.Description,
                    IsActive = up.Permission.IsActive
                }).ToList()
            };
        }
    }
}