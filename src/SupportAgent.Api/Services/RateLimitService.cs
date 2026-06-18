namespace SupportAgent.Api.Services;

public class RateLimitService
{
    private readonly int _dailyLimit;
    private int _count;
    private DateTime _resetDate;
    private readonly object _lock = new();

    public RateLimitService(IConfiguration config)
    {
        _dailyLimit = config.GetValue<int>("RateLimit:DailyLimit", 1000);
        _resetDate = DateTime.UtcNow.Date;
    }

    public bool TryConsume()
    {
        lock (_lock)
        {
            var today = DateTime.UtcNow.Date;
            if (today > _resetDate)
            {
                _count = 0;
                _resetDate = today;
            }
            if (_count >= _dailyLimit) return false;
            _count++;
            return true;
        }
    }
}
