using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace SmartRoutine.Application.Behaviors;

public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? SlidingExpiration { get; }
    TimeSpan? AbsoluteExpiration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(IMemoryCache cache, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only cache requests that implement ICacheable
        if (request is not ICacheable cacheableRequest)
        {
            return await next();
        }

        var cacheKey = cacheableRequest.CacheKey;

        // Try to get from cache
        if (_cache.TryGetValue(cacheKey, out TResponse? cachedResponse) && cachedResponse != null)
        {
            _logger.LogInformation("Cache hit for key: {CacheKey}", cacheKey);
            return cachedResponse;
        }

        // Not in cache, execute request
        _logger.LogInformation("Cache miss for key: {CacheKey}", cacheKey);
        var response = await next();

        // Cache the response
        var cacheEntryOptions = new MemoryCacheEntryOptions();

        if (cacheableRequest.SlidingExpiration.HasValue)
        {
            cacheEntryOptions.SetSlidingExpiration(cacheableRequest.SlidingExpiration.Value);
        }

        if (cacheableRequest.AbsoluteExpiration.HasValue)
        {
            cacheEntryOptions.SetAbsoluteExpiration(cacheableRequest.AbsoluteExpiration.Value);
        }

        // Set default expiration if none specified
        if (!cacheableRequest.SlidingExpiration.HasValue && !cacheableRequest.AbsoluteExpiration.HasValue)
        {
            cacheEntryOptions.SetSlidingExpiration(TimeSpan.FromMinutes(30));
        }

        _cache.Set(cacheKey, response, cacheEntryOptions);
        _logger.LogInformation("Cached response for key: {CacheKey}", cacheKey);

        return response;
    }
} 