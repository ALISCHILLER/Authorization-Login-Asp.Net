#nullable enable
using System;

namespace Authorization_Login_Asp.Net.Core.Domain.Common
{
    public interface IAuditable
    {
        string CreatedBy { get; set; }
        string UpdatedBy { get; set; }
    }
}