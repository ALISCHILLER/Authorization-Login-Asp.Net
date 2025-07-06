using System;

namespace Authorization_Login_Asp.Net.Core.Application.DTOs.Common
{
    public abstract class BaseDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
} 