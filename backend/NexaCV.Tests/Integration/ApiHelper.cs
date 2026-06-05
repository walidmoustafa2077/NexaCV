using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using NexaCV.Api.DTOs.Auth;
using NexaCV.Api.DTOs.Resumes;

namespace NexaCV.Tests.Integration;

/// <summary>Shared factory methods for building test payloads and authenticating clients.</summary>
public static class ApiHelper
{
    private static readonly JsonSerializerOptions JsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    // ── Auth helpers ──────────────────────────────────────────────────────────

    public static RegisterRequest BuildRegisterRequest(string? email = null, string? username = null) => new()
    {
        FirstName = "Test",
        LastName = "User",
        Username = username ?? $"user{Guid.NewGuid():N}",
        Email = email ?? $"{Guid.NewGuid():N}@test.com",
        Password = "P@ssw0rd!"
    };

    /// <summary>Registers a new user and returns (token, email, password).</summary>
    public static async Task<(string Token, string Email, string Password)> RegisterAsync(
        HttpClient client, RegisterRequest? req = null)
    {
        req ??= BuildRegisterRequest();
        var res = await client.PostAsJsonAsync("/api/auth/register", req);
        res.EnsureSuccessStatusCode();
        var body = await res.Content.ReadFromJsonAsync<AuthResponse>(JsonOpts);
        return (body!.Token, req.Email, req.Password);
    }

    /// <summary>Returns an HttpClient with the Bearer token pre-set.</summary>
    public static HttpClient CreateAuthorizedClient(NexaCVWebFactory factory, string token)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    // ── Resume helpers ────────────────────────────────────────────────────────

    public static CreateResumeRequest BuildCreateResumeRequest(int templateId = 1) => new()
    {
        TemplateId = templateId,
        RawData = new RawResumeData
        {
            Content = new RawResumeContent
            {
                Personal = new PersonalInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Phone = "+1234567890",
                    Location = "New York, USA"
                },
                Experience =
                [
                    new ExperienceEntry
                    {
                        Title       = "Software Engineer",
                        Company     = "Tech Corp",
                        Description = "Developed amazing software"
                    }
                ],
                Education =
                [
                    new EducationEntry
                    {
                        Institution = "University of Example",
                        Degree      = "B.Sc. Computer Science"
                    }
                ]
            }
        }
    };

    /// <summary>Creates a resume and returns its ID from the Location header.</summary>
    public static async Task<Guid> CreateResumeAsync(HttpClient client, int templateId = 1)
    {
        var res = await client.PostAsJsonAsync("/api/resumes", BuildCreateResumeRequest(templateId));
        res.EnsureSuccessStatusCode();
        var doc = await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
        return doc.GetProperty("id").GetGuid();
    }

    // ── JSON helpers ──────────────────────────────────────────────────────────

    public static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage res)
        => await res.Content.ReadFromJsonAsync<JsonElement>(JsonOpts);
}
