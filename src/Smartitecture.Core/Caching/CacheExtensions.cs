using System.Threading.Tasks;

namespace Smartitecture.Core.Caching
{
    public static class CacheExtensions
    {
        public static async Task<T> GetOrSetAsync<T>(this ICacheService cache, string key, Func<Task<T>> valueFactory, TimeSpan? expiry = null)
        {
            var cachedValue = await cache.GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var newValue = await valueFactory();
            await cache.SetAsync(key, newValue, expiry);
            return newValue;
        }

        public static async Task<T> GetOrSetAsync<T>(this ICacheService cache, string key, Func<T> valueFactory, TimeSpan? expiry = null)
        {
            var cachedValue = await cache.GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var newValue = valueFactory();
            await cache.SetAsync(key, newValue, expiry);
            return newValue;
        }

        public static async Task<T> GetOrSetAsync<T>(this ICacheService cache, string key, Func<Task<T>> valueFactory, Func<T, T, T> updateFunc, TimeSpan? expiry = null)
        {
            var cachedValue = await cache.GetAsync<T>(key);
            if (cachedValue != null)
                return cachedValue;

            var newValue = await valueFactory();
            await cache.AddOrUpdateAsync(key, newValue, updateFunc, expiry);
            return newValue;
        }
    }
}
