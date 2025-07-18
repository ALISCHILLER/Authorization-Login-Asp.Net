using System;
using MediatR;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands
{
    public class DeleteUserCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
    }
}
