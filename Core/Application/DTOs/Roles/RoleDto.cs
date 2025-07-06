using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Roles
{
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
