namespace NexaCV.Api.Services;

public interface ICurrencyService
{
    /// <summary>
    /// Returns the conversion rate from USD to <paramref name="targetCurrency"/>.
    /// Result is cached according to <c>CurrencyServiceSettings.CacheDurationHours</c>.
    /// </summary>
    /// <param name="targetCurrency">ISO 4217 code (e.g. "EGP", "EUR").</param>
    /// <exception cref="InvalidOperationException">Thrown when the currency is not supported.</exception>
    Task<decimal> GetExchangeRateAsync(string targetCurrency);
}
