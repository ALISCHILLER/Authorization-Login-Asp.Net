using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands
{
    public class ActivateUserCommandHandler : IRequestHandler<ActivateUserCommand, bool>
    {
        private readonly IUserService _userService;

        public ActivateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.ActivateAsync(request.UserId);
        }
    }
}
