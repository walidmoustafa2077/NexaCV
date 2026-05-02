namespace NexaCV.Api.Settings;

public class CurrencyServiceSettings
{
    /// <summary>How long (hours) to cache exchange rates. Reduces external API calls.</summary>
    public int CacheDurationHours { get; set; } = 1;

    /// <summary>
    /// Hardcoded USD-based rates used by <c>StubCurrencyService</c>.
    /// Key = ISO 4217 currency code; Value = units of that currency per 1 USD.
    /// </summary>
    public Dictionary<string, decimal> StubRates { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = 1.00m,
        ["EGP"] = 50.00m,
        ["EUR"] = 0.92m,
        ["GBP"] = 0.79m,
        ["SAR"] = 3.75m,
        ["AED"] = 3.67m
    };
}
