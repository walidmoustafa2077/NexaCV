namespace NexaCV.Api.Settings;

public class AiServiceSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4o-mini";
    public int TimeoutSeconds { get; set; } = 5;
    /// <summary>
    /// When non-empty, the StubAiService forwards requests to this base URL
    /// instead of generating a local stub response. Point to NexaCV.AiMock
    /// (http://localhost:5001) during local development.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;
}
