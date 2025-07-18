using System;
using MediatR;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands
{
    public class UpdateUserCommand : IRequest<UserDto>
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public bool? IsActive { get; set; }
    }
}
