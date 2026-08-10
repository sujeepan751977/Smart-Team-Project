using Microsoft.Extensions.Caching.Memory;

namespace Recruitment_Project.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public CacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T? Get<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T? value)
                ? value
                : default;
        }

        public void Set<T>(
            string key,
            T value,
            TimeSpan? expiration = null)
        {
            var options = new MemoryCacheEntryOptions();

            if (expiration.HasValue)
            {
                options.AbsoluteExpirationRelativeToNow = expiration.Value;
            }

            _memoryCache.Set(key, value, options);
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        public bool Exists(string key)
        {
            return _memoryCache.TryGetValue(key, out _);
        }
    }
}