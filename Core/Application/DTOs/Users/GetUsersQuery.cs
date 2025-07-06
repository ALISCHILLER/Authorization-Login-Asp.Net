using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Users
{
    public class GetUsersQuery
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
} 