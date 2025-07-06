using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class GetUsersRequest
    {
        public string SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public Guid? RoleId { get; set; }
        public Guid? PermissionId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 