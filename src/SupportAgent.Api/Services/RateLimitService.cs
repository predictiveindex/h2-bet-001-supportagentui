using StackExchange.Redis;

namespace SupportAgent.Api.Services;

public class RateLimitService
{
    private readonly int _dailyLimit;
    private readonly IConnectionMultiplexer? _redis;
    private readonly ILogger<RateLimitService> _logger;

    // In-memory fallback (used when Redis is unavailable)
    private int _fallbackCount;
    private DateTime _fallbackResetDate;
    private readonly object _fallbackLock = new();

    public RateLimitService(IConfiguration config, ILogger<RateLimitService> logger, IConnectionMultiplexer? redis = null)
    {
        _dailyLimit = config.GetValue<int>("RateLimit:DailyLimit", 1000);
        _logger = logger;
        _redis = redis;
        _fallbackResetDate = DateTime.UtcNow.Date;
    }

    public async Task<bool> TryConsumeAsync()
    {
        if (_redis is not null)
        {
            try
            {
                var db = _redis.GetDatabase();
                var key = $"ratelimit:global:{DateTime.UtcNow:yyyy-MM-dd}";
                var count = await db.StringIncrementAsync(key);

                // On first increment, set expiry to next midnight UTC
                if (count == 1)
                {
                    var nextMidnight = DateTime.UtcNow.Date.AddDays(1);
                    await db.KeyExpireAsync(key, nextMidnight);
                }

                return count <= _dailyLimit;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis unavailable — falling back to in-memory rate limiting");
            }
        }

        return TryConsumeFallback();
    }

    private bool TryConsumeFallback()
    {
        lock (_fallbackLock)
        {
            var today = DateTime.UtcNow.Date;
            if (today > _fallbackResetDate)
            {
                _fallbackCount = 0;
                _fallbackResetDate = today;
            }
            if (_fallbackCount >= _dailyLimit) return false;
            _fallbackCount++;
            return true;
        }
    }
}
