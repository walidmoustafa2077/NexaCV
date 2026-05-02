namespace NexaCV.Api.DTOs.Errors;

/// <summary>Standard error response returned for all 4xx and 5xx errors except validation failures.</summary>
public class ApiErrorResponse
{
    /// <summary>HTTP status code.</summary>
    /// <example>401</example>
    public int Status { get; set; }

    /// <summary>Human-readable error message.</summary>
    /// <example>Unauthorized</example>
    public string Error { get; set; } = string.Empty;
}
