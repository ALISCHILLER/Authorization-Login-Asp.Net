using System;
using System.Threading;
using System.Threading.Tasks;

namespace Authorization_Login_Asp.Net.Core.Application.Interfaces
{
    public interface ICacheService
    {
        Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? absoluteExpiration = null, CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default);
        Task ExtendAsync(string key, TimeSpan time, CancellationToken cancellationToken = default);
    }
}
