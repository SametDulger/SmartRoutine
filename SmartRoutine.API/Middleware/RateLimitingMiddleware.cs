using System.Net;
using System.Text.Json;
using StackExchange.Redis;

namespace SmartRoutine.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly Dictionary<string, (int count, DateTime resetTime)> _rateLimitStore = new();
    private readonly int _maxRequests;
    private readonly TimeSpan _timeWindow;
    private readonly IDatabase? _redisDb;
    private readonly bool _useRedis;

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        var envMaxRequests = Environment.GetEnvironmentVariable("SMARTROUTINE_RATELIMIT_REQUESTS");
        var envWindow = Environment.GetEnvironmentVariable("SMARTROUTINE_RATELIMIT_WINDOW");
        _maxRequests = !string.IsNullOrEmpty(envMaxRequests)
            ? int.Parse(envMaxRequests)
            : configuration.GetValue<int>("ApiSettings:RateLimitRequests", 100);
        _timeWindow = !string.IsNullOrEmpty(envWindow)
            ? TimeSpan.FromSeconds(int.Parse(envWindow))
            : TimeSpan.FromSeconds(configuration.GetValue<int>("ApiSettings:RateLimitWindow", 60));

        var redisConnectionString = configuration["Redis:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            var redis = ConnectionMultiplexer.Connect(redisConnectionString);
            _redisDb = redis.GetDatabase();
            _useRedis = true;
        }
        else
        {
            _useRedis = false;
        }
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var isLimited = _useRedis
            ? await IsRateLimitedRedis(clientId)
            : IsRateLimited(clientId);
        if (isLimited)
        {
            _logger.LogWarning("Rate limit exceeded for client: {ClientId}", clientId);
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            var response = new
            {
                message = "Rate limit exceeded. Too many requests.",
                retryAfter = _useRedis ? await GetRetryAfterRedis(clientId) : GetRetryAfter(clientId)
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
            return;
        }
        if (_useRedis)
            await IncrementRequestCountRedis(clientId);
        else
            IncrementRequestCount(clientId);
        await _next(context);
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Use IP address as client identifier
        // In production, you might want to use user ID for authenticated requests
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // For authenticated requests, you could use user ID
        var userId = context.User?.FindFirst("sub")?.Value;
        
        return userId ?? clientIp;
    }

    private bool IsRateLimited(string clientId)
    {
        if (!_rateLimitStore.ContainsKey(clientId))
        {
            return false;
        }

        var (count, resetTime) = _rateLimitStore[clientId];
        
        if (DateTime.UtcNow >= resetTime)
        {
            // Time window has passed, reset the counter
            _rateLimitStore.Remove(clientId);
            return false;
        }

        return count >= _maxRequests;
    }

    private void IncrementRequestCount(string clientId)
    {
        var resetTime = DateTime.UtcNow.Add(_timeWindow);

        if (_rateLimitStore.ContainsKey(clientId))
        {
            var (count, existingResetTime) = _rateLimitStore[clientId];
            
            if (DateTime.UtcNow >= existingResetTime)
            {
                // Reset the counter for a new time window
                _rateLimitStore[clientId] = (1, resetTime);
            }
            else
            {
                // Increment the counter
                _rateLimitStore[clientId] = (count + 1, existingResetTime);
            }
        }
        else
        {
            _rateLimitStore[clientId] = (1, resetTime);
        }
    }

    private int GetRetryAfter(string clientId)
    {
        if (!_rateLimitStore.ContainsKey(clientId))
        {
            return 0;
        }

        var (_, resetTime) = _rateLimitStore[clientId];
        return Math.Max(0, (int)(resetTime - DateTime.UtcNow).TotalSeconds);
    }

    // Redis destekli rate limit fonksiyonları:
    private async Task<bool> IsRateLimitedRedis(string clientId)
    {
        if (_redisDb == null) return false;
        var count = await _redisDb.StringGetAsync($"ratelimit:{clientId}:count");
        var reset = await _redisDb.StringGetAsync($"ratelimit:{clientId}:reset");
        if (count.IsNullOrEmpty || reset.IsNullOrEmpty) return false;
        var resetTime = DateTime.Parse(reset!);
        if (DateTime.UtcNow >= resetTime)
        {
            await _redisDb.KeyDeleteAsync($"ratelimit:{clientId}:count");
            await _redisDb.KeyDeleteAsync($"ratelimit:{clientId}:reset");
            return false;
        }
        return int.Parse(count!) >= _maxRequests;
    }
    private async Task IncrementRequestCountRedis(string clientId)
    {
        if (_redisDb == null) return;
        var countKey = $"ratelimit:{clientId}:count";
        var resetKey = $"ratelimit:{clientId}:reset";
        var count = await _redisDb.StringGetAsync(countKey);
        var reset = await _redisDb.StringGetAsync(resetKey);
        if (count.IsNullOrEmpty || reset.IsNullOrEmpty)
        {
            await _redisDb.StringSetAsync(countKey, 1);
            await _redisDb.StringSetAsync(resetKey, DateTime.UtcNow.Add(_timeWindow).ToString("o"));
        }
        else
        {
            await _redisDb.StringIncrementAsync(countKey);
        }
    }
    private async Task<int> GetRetryAfterRedis(string clientId)
    {
        if (_redisDb == null) return 0;
        var reset = await _redisDb.StringGetAsync($"ratelimit:{clientId}:reset");
        if (reset.IsNullOrEmpty) return 0;
        var resetTime = DateTime.Parse(reset!);
        return Math.Max(0, (int)(resetTime - DateTime.UtcNow).TotalSeconds);
    }
} 