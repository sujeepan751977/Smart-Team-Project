using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Recruitment_Project.Options;

namespace Recruitment_Project.Caching
{
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _cacheOptions;

        public CacheService(
            IMemoryCache memoryCache,
            IOptions<CacheOptions> cacheOptions)
        {
            _memoryCache = memoryCache;
            _cacheOptions = cacheOptions.Value;
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
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiration ??
                    TimeSpan.FromMinutes(
                        _cacheOptions.DefaultExpirationMinutes > 0
                            ? _cacheOptions.DefaultExpirationMinutes
                            : 10)
            };

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
