using System.Text.Json;
using System.Text.Json.Serialization;
using NexaCV.Api.DTOs.Auth;
using NexaCV.Api.DTOs.Resumes;
using NexaCV.Api.DTOs.Templates;
using NexaCV.Api.DTOs.Transactions;
using NexaCV.Api.DTOs.Users;
using NexaCV.Api.Enums;
using NexaCV.Api.Models;
using NexaCV.Api.Services;

namespace NexaCV.Api.Extensions;

public static class MappingExtensions
{
    private static readonly JsonSerializerOptions CamelCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    // User → UserProfileDto
    public static UserProfileDto ToProfileDto(this User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Username = user.Username,
        Email = user.Email,
        CreatedAt = user.CreatedAt,
        LastLogin = user.LastLogin
    };

    // Template → TemplateDto
    public static TemplateDto ToDto(this Template template) => new()
    {
        Id = template.Id,
        Name = template.Name,
        IndustryCategory = template.IndustryCategory,
        BasePriceUsd = template.BasePriceUsd,
        SupportsWord = template.SupportsWord
    };

    // Resume → ResumeSummaryDto (requires Template nav loaded)
    public static ResumeSummaryDto ToSummaryDto(this Resume resume) => new()
    {
        Id = resume.Id,
        Status = resume.Status.ToString().ToUpperInvariant(),
        TemplateName = resume.Template.Name,
        CreatedAt = resume.CreatedAt
    };

    // Resume → ResumeDetailDto (requires Template nav loaded)
    public static ResumeDetailDto ToDetailDto(
        this Resume resume,
        bool aiAvailable = false,
        IReadOnlyList<AiJobTitleSuggestion>? jobTitleSuggestions = null,
        IReadOnlyList<string>? skillSuggestions = null) => new()
        {
            Id = resume.Id,
            Status = resume.Status.ToString().ToUpperInvariant(),
            TemplateId = resume.TemplateId,
            TemplateName = resume.Template.Name,
            RawData = resume.RawData != null ? JsonSerializer.Deserialize<JsonElement>(resume.RawData) : null,
            FinalData = resume.FinalData != null ? JsonSerializer.Deserialize<JsonElement>(resume.FinalData) : null,
            AiAvailable = aiAvailable,
            CreatedAt = resume.CreatedAt,
            UpdatedAt = resume.UpdatedAt,
            JobTitleSuggestions = jobTitleSuggestions,
            SkillSuggestions = skillSuggestions
        };

    // Regeneration → RegenerateResponse
    public static RegenerateResponse ToResponseDto(this Regeneration regen, int totalUsed, string updatedContent, bool aiAvailable) => new()
    {
        SectionIdentifier = regen.SectionIdentifier,
        UpdatedContent = ParseContentElement(updatedContent),
        RegenCountUsed = totalUsed,
        RegenCountRemaining = 3 - totalUsed,
        AddedCostUsd = regen.CostUsd,
        AiAvailable = aiAvailable
    };

    /// <summary>
    /// Tries to parse <paramref name="value"/> as a JSON document.
    /// Falls back to a JSON string element when the value is not valid JSON.
    /// </summary>
    private static JsonElement ParseContentElement(string value)
    {
        try { return JsonDocument.Parse(value).RootElement.Clone(); }
        catch { return JsonSerializer.SerializeToElement(value); }
    }

    // Transaction → CheckoutResponse
    public static CheckoutResponse ToCheckoutResponse(this Transaction tx, string paymentUrl) => new()
    {
        TransactionId = tx.Id,
        PaymentUrl = paymentUrl,
        BaseAmount = tx.BaseAmount,
        RegenAmount = tx.RegenAmount,
        TotalAmount = tx.TotalAmount,
        Currency = tx.Currency,
        ExchangeRateUsed = tx.ExchangeRateUsed
    };

    // Transaction → TransactionDto
    public static TransactionDto ToDto(this Transaction tx) => new()
    {
        Id = tx.Id,
        ResumeId = tx.ResumeId,
        TotalAmount = tx.TotalAmount,
        Currency = tx.Currency,
        ExchangeRateUsed = tx.ExchangeRateUsed,
        PaymentStatus = tx.PaymentStatus.ToString().ToUpperInvariant(),
        CreatedAt = tx.CreatedAt,
        CompletedAt = tx.CompletedAt
    };

    // RegisterRequest → User
    public static User ToUser(this RegisterRequest req, string passwordHash) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = req.FirstName,
        LastName = req.LastName,
        Username = req.Username,
        Email = req.Email,
        PasswordHash = passwordHash,
        DateOfBirth = req.DateOfBirth,
        CreatedAt = DateTime.UtcNow
    };

    // CreateResumeRequest → Resume
    public static Resume ToResume(this CreateResumeRequest req, Guid userId)
    {
        var now = DateTime.UtcNow;
        return new Resume
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TemplateId = req.TemplateId,
            Status = ResumeStatus.Draft,
            RawData = JsonSerializer.Serialize(req.RawData, CamelCaseOptions),
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
