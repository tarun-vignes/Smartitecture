using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Smartitecture.Core.Caching
{
    public class DistributedCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<DistributedCacheService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public DistributedCacheService(IDistributedCache cache, ILogger<DistributedCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                WriteIndented = true
            };
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(cachedValue))
                    return default;

                return JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cache value for key: {Key}", key);
                return default;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromHours(1)
                };

                var json = JsonSerializer.Serialize(value, _jsonOptions);
                await _cache.SetStringAsync(key, json, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting cache value for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing cache value for key: {Key}", key);
            }
        }

        public async Task ClearAsync()
        {
            try
            {
                // Note: Clearing all cache entries is not supported in all distributed cache providers
                // This is a placeholder for specific cache provider implementations
                _logger.LogInformation("Clearing cache is not supported by this provider");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cache");
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var cachedValue = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(cachedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
                return false;
            }
        }

        public async Task AddOrUpdateAsync<T>(string key, T value, Func<T, T, T> updateFunc, TimeSpan? expiry = null)
        {
            try
            {
                var existingValue = await GetAsync<T>(key);
                var newValue = existingValue != null ? updateFunc(existingValue, value) : value;
                await SetAsync(key, newValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding or updating cache value for key: {Key}", key);
            }
        }

        public async Task<Dictionary<string, T>> GetMultipleAsync<T>(IEnumerable<string> keys)
        {
            try
            {
                var result = new Dictionary<string, T>();
                foreach (var key in keys)
                {
                    var value = await GetAsync<T>(key);
                    if (value != null)
                        result[key] = value;
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting multiple cache values");
                return new Dictionary<string, T>();
            }
        }

        public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null)
        {
            try
            {
                foreach (var item in items)
                {
                    await SetAsync(item.Key, item.Value, expiry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting multiple cache values");
            }
        }
    }
}
