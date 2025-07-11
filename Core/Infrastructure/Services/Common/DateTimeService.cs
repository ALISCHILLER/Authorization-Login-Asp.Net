using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using System;

namespace Authorization_Login_Asp.Net.Core.Infrastructure.Services.Common
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime UtcNow => DateTime.UtcNow;
        public DateTime Now => DateTime.Now;
    }
}
