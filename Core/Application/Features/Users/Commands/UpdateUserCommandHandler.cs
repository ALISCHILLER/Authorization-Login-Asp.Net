using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, UserDto>
    {
        private readonly IUserService _userService;
        public UpdateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }
        public async Task<UserDto> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.UpdateAsync(request, cancellationToken);
        }
    }
}
