using System;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IDateTimeService
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
}
