using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NexaCV.Api.Settings;

namespace NexaCV.Api.Services;

/// <summary>
/// Stub implementation of <see cref="ICurrencyService"/> that reads rates from
/// <see cref="CurrencyServiceSettings.StubRates"/> and caches them in <see cref="IMemoryCache"/>.
/// Replace with a real implementation backed by ExchangeRate-API or Fixer.io for production.
/// </summary>
public class StubCurrencyService : ICurrencyService
{
    private readonly CurrencyServiceSettings _settings;
    private readonly IMemoryCache _cache;

    public StubCurrencyService(IOptions<CurrencyServiceSettings> settings, IMemoryCache cache)
    {
        _settings = settings.Value;
        _cache = cache;
    }

    public Task<decimal> GetExchangeRateAsync(string targetCurrency)
    {
        var code = targetCurrency.ToUpperInvariant();
        var cacheKey = $"fx:usd_to_{code}";

        var rate = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow =
                TimeSpan.FromHours(_settings.CacheDurationHours);

            if (!_settings.StubRates.TryGetValue(code, out var r))
                throw new InvalidOperationException(
                    $"Unsupported currency '{code}'. Add it to CurrencyService:StubRates in appsettings.");

            return r;
        });

        return Task.FromResult(rate);
    }
}
