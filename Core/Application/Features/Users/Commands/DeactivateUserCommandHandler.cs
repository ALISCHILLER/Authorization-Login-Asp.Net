using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Authorization_Login_Asp.Net.Core.Application.Interfaces;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Commands
{
    public class DeactivateUserCommandHandler : IRequestHandler<DeactivateUserCommand, bool>
    {
        private readonly IUserService _userService;

        public DeactivateUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<bool> Handle(DeactivateUserCommand request, CancellationToken cancellationToken)
        {
            return await _userService.DeactivateAsync(request.UserId);
        }
    }
}
