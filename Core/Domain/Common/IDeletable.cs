#nullable enable
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface IDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        string DeletedBy { get; set; }
    }
}