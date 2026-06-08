namespace NexaCV.Identity.Settings;

public sealed class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Access token lifetime in seconds. Default 900 = 15 minutes.</summary>
    public int AccessTokenExpiresInSeconds { get; set; } = 900;
}

public sealed class RefreshTokenSettings
{
    /// <summary>Refresh token lifetime in days. Default 7 days.</summary>
    public int ExpiresInDays { get; set; } = 7;
}
