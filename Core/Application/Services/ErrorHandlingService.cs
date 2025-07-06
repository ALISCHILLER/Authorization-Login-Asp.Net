using Authorization_Login_Asp.Net.Core.Application.Interfaces;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        public Task LogUserErrorAsync(string userId, string errorMessage)
        {
            // TODO: پیاده‌سازی لاگ خطا
            return Task.CompletedTask;
        }
    }
}
