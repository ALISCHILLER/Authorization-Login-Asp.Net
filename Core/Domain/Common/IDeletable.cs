#nullable enable
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface IDeletable
    {
        bool IsDeleted { get; /*protected*/ set; } // Setter might be protected if only BaseEntity should set it
        DateTime? DeletedAt { get; /*protected*/ set; }
        string? DeletedBy { get; /*protected*/ set; } // Changed to string?
    }
}