using System;
using System.Collections.Generic;

namespace Authorization_Login_Asp.Net.Core.Application.Common.Models
{
    /// <summary>
    /// مدل DTO نقش
    /// </summary>
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ICollection<string> Permissions { get; set; } = new List<string>();
    }

    /// <summary>
    /// مدل DTO نقش جدید
    /// </summary>
    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// مدل DTO به‌روزرسانی نقش
    /// </summary>
    public class UpdateRoleDto
    {
        public string Name { get; set; } = string.Empty;
    }
} 