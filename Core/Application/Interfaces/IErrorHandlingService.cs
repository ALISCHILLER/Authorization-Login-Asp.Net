using System.Threading.Tasks;

using System; // For Exception
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IErrorHandlingService
    {
        Task LogUserErrorAsync(string? userId, string errorMessage, Exception ex);
        Task LogSystemErrorAsync(string systemComponent, string errorMessage, Exception ex);
    }
}
