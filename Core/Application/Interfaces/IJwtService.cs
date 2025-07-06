using System.Threading.Tasks;
using Authorization_Login_Asp.Net.Core.Domain.Entities;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface IJwtService
    {
        Task<string> GenerateTokenAsync(User user);
    }
}
