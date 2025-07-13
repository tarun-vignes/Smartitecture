using System.Threading.Tasks;
using System.Collections.Generic;

namespace Smartitecture.Core.Caching
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task RemoveAsync(string key);
        Task ClearAsync();
        Task<bool> ExistsAsync(string key);
        Task AddOrUpdateAsync<T>(string key, T value, Func<T, T, T> updateFunc, TimeSpan? expiry = null);
        Task<Dictionary<string, T>> GetMultipleAsync<T>(IEnumerable<string> keys);
        Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null);
    }
}
