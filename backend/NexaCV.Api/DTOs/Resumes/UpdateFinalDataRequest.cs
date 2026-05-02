using System.Text.Json;

namespace NexaCV.Api.DTOs.Resumes;

/// <summary>Request body for replacing a resume's <c>finalData</c> JSON.</summary>
public class UpdateFinalDataRequest
{
    /// <summary>
    /// The complete replacement JSON for the resume's <c>finalData</c> field.
    /// Must conform to the <c>{ settings: {...}, content: {...} }</c> schema.
    /// </summary>
    public JsonElement FinalData { get; set; }
}
