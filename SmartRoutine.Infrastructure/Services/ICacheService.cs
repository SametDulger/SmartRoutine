using StackExchange.Redis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartRoutine.Application.Common.Interfaces;

namespace SmartRoutine.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache memoryCache, ILogger<InMemoryCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = _memoryCache.Get<T>(key);
        _logger.LogDebug("Cache {Operation}: {Key} - {Status}", 
            "GET", key, value != null ? "HIT" : "MISS");
        
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions();
        
        if (expiration.HasValue)
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
        }
        else
        {
            cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30); // Default 30 minutes
        }

        _memoryCache.Set(key, value, cacheEntryOptions);
        _logger.LogDebug("Cache {Operation}: {Key} - Expires in {Expiration}", 
            "SET", key, expiration ?? TimeSpan.FromMinutes(30));

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _logger.LogDebug("Cache {Operation}: {Key}", "REMOVE", key);
        
        return Task.CompletedTask;
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: MemoryCache doesn't support pattern-based removal
        // For production, consider using Redis
        _logger.LogWarning("Pattern-based cache removal not supported in MemoryCache: {Pattern}", pattern);
        
        return Task.CompletedTask;
    }
} 

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redisDb = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await _redisDb.StringGetAsync(key);
        if (value.IsNullOrEmpty)
        {
            _logger.LogDebug("Redis cache miss: {Key}", key);
            return default;
        }
        _logger.LogDebug("Redis cache hit: {Key}", key);
        return System.Text.Json.JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        await _redisDb.StringSetAsync(key, json, expiration);
        _logger.LogDebug("Redis cache set: {Key} (expires: {Expiration})", key, expiration);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _redisDb.KeyDeleteAsync(key);
        _logger.LogDebug("Redis cache remove: {Key}", key);
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Redis pattern silme için SCAN + DEL
        var server = _redisDb.Multiplexer.GetServer(_redisDb.Multiplexer.GetEndPoints().First());
        var keys = server.Keys(pattern: $"*{pattern}*").ToArray();
        if (keys.Length > 0)
        {
            await _redisDb.KeyDeleteAsync(keys);
            _logger.LogDebug("Redis cache remove by pattern: {Pattern} - {Count} keys", pattern, keys.Length);
        }
        else
        {
            _logger.LogDebug("Redis cache remove by pattern: {Pattern} - no keys found", pattern);
        }
    }
} 