using System;
using System.Collections.Generic;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Roles;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        public bool IsPhoneVerified { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public string PrimaryRole { get; set; } = string.Empty;
        public List<RoleDto> Roles { get; set; } = new();
        public List<PermissionDto> Permissions { get; set; } = new();
    }
}
