using System;
using MediatR;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Users;
using Authorization_Login_Asp.Net.Core.Application.DTOs.Common;

namespace Authorization_Login_Asp.Net.Core.Application.Features.Users.Queries
{
    public class GetUsersQuery : IRequest<PaginatedList<UserDto>>
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
