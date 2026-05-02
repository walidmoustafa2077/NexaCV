namespace NexaCV.Api.DTOs.Errors;

/// <summary>Error response returned for validation failures (422 Unprocessable Entity).</summary>
public class ValidationErrorResponse
{
    /// <summary>HTTP status code (422).</summary>
    /// <example>422</example>
    public int Status { get; set; }

    /// <summary>Summary error message.</summary>
    /// <example>Validation failed</example>
    public string Error { get; set; } = string.Empty;

    /// <summary>Per-field validation errors.</summary>
    public IEnumerable<ValidationFieldError> Details { get; set; } = [];
}

/// <summary>A single field-level validation error.</summary>
public class ValidationFieldError
{
    /// <summary>Name of the invalid field.</summary>
    /// <example>email</example>
    public string Field { get; set; } = string.Empty;

    /// <summary>Validation failure message.</summary>
    /// <example>'Email' must not be empty.</example>
    public string Message { get; set; } = string.Empty;
}
