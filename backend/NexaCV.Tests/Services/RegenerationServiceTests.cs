using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class RegenerationServiceTests
{
    private readonly Mock<IResumeRepository> _resumes = new();
    private readonly Mock<IRegenerationRepository> _regenerations = new();
    private readonly Mock<IResumeHistoryRepository> _history = new();
    private readonly Mock<IResumeSectionRegenerationService> _ai = new();

    private RegenerationService CreateSut() =>
        new(_resumes.Object, _regenerations.Object, _history.Object, _ai.Object);

    private static RegenerateRequest MakeRequest(string section = "summary") => new()
    {
        SectionIdentifier = section,
        UserPrompt = "Make it more concise",
        TargetFormat = "PARAGRAPH"
    };

    private void SetupDefaults(Resume resume, int existingCount = 0)
    {
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _regenerations.Setup(r => r.CountBySectionAsync(resume.Id, It.IsAny<string>()))
                      .ReturnsAsync(existingCount);
        _ai.Setup(a => a.RegenerateAsync(It.IsAny<AiRegenerateContext>()))
           .ReturnsAsync(new AiRegenerationResult("Updated summary text", AiAvailable: false));
        _resumes.Setup(r => r.UpdateAsync(resume)).Returns(Task.CompletedTask);
        _history.Setup(h => h.AddAsync(It.IsAny<ResumeHistory>())).Returns(Task.CompletedTask);
        _history.Setup(h => h.PruneAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _regenerations.Setup(r => r.AddAsync(It.IsAny<Regeneration>())).Returns(Task.CompletedTask);
    }

    // ── Regenerate ────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: regenerates a resume section when the user has already used 1 of the 3 allowed slots.
    /// <br/><b>Input:</b> Resume owned by userId; existingCount=1;
    /// RegenerateRequest { SectionIdentifier="summary", UserPrompt="Make it more concise", TargetFormat="PARAGRAPH" }.
    /// <br/><b>Expected:</b> RegenCountUsed=2, RegenCountRemaining=1, AddedCostUsd=0.25,
    /// UpdatedContent="Updated summary text" (from StubAiService).
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_Success_ReturnsResponseWithCorrectCounts()
    {
        // Arrange
        // Input: 1 regen already used for this section → after this call: 2 used, 1 remaining
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        SetupDefaults(resume, existingCount: 1);

        // Act
        var result = await CreateSut().RegenerateAsync(resume.Id, userId, MakeRequest());

        // Assert – Expected: counts reflect 2 used out of 3; cost is $0.25 per regen
        result.RegenCountUsed.Should().Be(2);
        result.RegenCountRemaining.Should().Be(1);
        result.AddedCostUsd.Should().Be(0.25m);
        result.UpdatedContent.GetString().Should().Be("Updated summary text");
    }

    /// <summary>
    /// Scenario: first-ever regeneration for this section; count starts at 0 before the call.
    /// <br/><b>Input:</b> Resume owned by userId; existingCount=0.
    /// <br/><b>Expected:</b> RegenCountUsed=1, RegenCountRemaining=2.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_FirstRegeneration_CountStartsAtOne()
    {
        // Arrange – Input: no prior regenerations for this section
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        SetupDefaults(resume, existingCount: 0);

        // Act
        var result = await CreateSut().RegenerateAsync(resume.Id, userId, MakeRequest());

        // Assert – Expected: first use increments count to 1, leaving 2 remaining
        result.RegenCountUsed.Should().Be(1);
        result.RegenCountRemaining.Should().Be(2);
    }

    /// <summary>
    /// Scenario: all 3 regeneration slots for the section are already consumed; a 4th attempt must be blocked.
    /// <br/><b>Input:</b> Resume owned by userId; existingCount=3 (at maximum).
    /// <br/><b>Expected:</b> TooManyRegenerationsException thrown.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_AtLimit_ThrowsTooManyRegenerationsException()
    {
        // Arrange
        // Input: section already has 3 regenerations (the maximum)
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _regenerations.Setup(r => r.CountBySectionAsync(resume.Id, It.IsAny<string>()))
                      .ReturnsAsync(3); // already at max

        // Act
        Func<Task> act = () => CreateSut().RegenerateAsync(resume.Id, userId, MakeRequest());

        // Assert – Expected: TooManyRegenerationsException (limit exceeded)
        await act.Should().ThrowAsync<TooManyRegenerationsException>();
    }

    /// <summary>
    /// Scenario: regeneration request made by a user who does not own the resume.
    /// <br/><b>Input:</b> Resume owned by userId-A; RegenerateAsync called with userId-B.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_WrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: resume belongs to a different user
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid());
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act – attacker uses a different userId
        Func<Task> act = () => CreateSut().RegenerateAsync(resume.Id, Guid.NewGuid(), MakeRequest());

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    /// <summary>
    /// Scenario: regeneration requested for a resume ID that no longer exists.
    /// <br/><b>Input:</b> GetWithTemplateAsync returns null for any Guid.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_ResumeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for any resume id
        _resumes.Setup(r => r.GetWithTemplateAsync(It.IsAny<Guid>())).ReturnsAsync((Resume?)null);

        // Act
        Func<Task> act = () => CreateSut().RegenerateAsync(Guid.NewGuid(), Guid.NewGuid(), MakeRequest());

        // Assert – Expected: KeyNotFoundException (resume not found)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    /// <summary>
    /// Scenario: verifies the AI service is called with the exact context built from the request.
    /// <br/><b>Input:</b> RegenerateRequest { SectionIdentifier="summary", UserPrompt="Be concise",
    /// NewTitleSuggestion="Senior Engineer" }.
    /// <br/><b>Expected:</b> IAiService.RegenerateAsync called once with AiRegenerateContext
    /// matching all three fields.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_CallsAiWithCorrectContext()
    {
        // Arrange
        // Input: specific prompt, section, and title suggestion
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        SetupDefaults(resume);

        var req = new RegenerateRequest
        {
            SectionIdentifier = "summary",
            UserPrompt = "Be concise",
            NewTitleSuggestion = "Senior Engineer"
        };

        // Act
        await CreateSut().RegenerateAsync(resume.Id, userId, req);

        // Assert – Expected: AI called with context containing the exact request fields
        _ai.Verify(a => a.RegenerateAsync(It.Is<AiRegenerateContext>(ctx =>
            ctx.SectionIdentifier == "summary" &&
            ctx.UserPrompt == "Be concise" &&
            ctx.NewTitleSuggestion == "Senior Engineer"
        )), Times.Once);
    }

    /// <summary>
    /// Scenario: when a TargetFormat is provided, it is forwarded to the AI and the resume
    /// is saved with the updated section content. Settings are template-defined and are NOT
    /// written into FinalData.
    /// <br/><b>Input:</b> RegenerateRequest { SectionIdentifier="summary", UserPrompt="Rewrite",
    /// TargetFormat="PARAGRAPH" }.
    /// <br/><b>Expected:</b> Resume.UpdateAsync called once; FinalData uses content-only schema.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_WithTargetFormat_SavesResume()
    {
        // Arrange
        // Input: request includes TargetFormat="PARAGRAPH"
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        SetupDefaults(resume);

        var req = new RegenerateRequest
        {
            SectionIdentifier = "summary",
            UserPrompt = "Rewrite",
            TargetFormat = "PARAGRAPH"
        };

        // Act
        await CreateSut().RegenerateAsync(resume.Id, userId, req);

        // Assert – UpdateAsync called once; FinalData uses content-only shape (no "settings" key).
        _resumes.Verify(r => r.UpdateAsync(It.Is<Resume>(r =>
            r.FinalData != null && r.FinalData.Contains("content") && !r.FinalData.Contains("\"settings\""))),
            Times.Once);
    }

    /// <summary>
    /// Scenario: sectionIdentifier is an experience entry ID (e.g. "exp_001") rather than a
    /// top-level section name. The service must find the entry inside content["experience"],
    /// update only its "description" field, and leave the rest of the array intact.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_ByEntryId_UpdatesEntryDescriptionInPlace()
    {
        // Arrange – resume has an experience array with one entry whose id is "exp_001"
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        resume.FinalData = """
            {
              "settings": { "summaryType": "Summary", "descriptionFormat": "Paragraph" },
              "content": {
                "personal": { "firstName": "John", "lastName": "Doe" },
                "experience": [
                  { "id": "exp_001", "title": "Dev", "company": "Acme", "startDate": "2020-01", "current": true, "description": "Old description." }
                ]
              }
            }
            """;

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _regenerations.Setup(r => r.CountBySectionAsync(resume.Id, "exp_001")).ReturnsAsync(0);
        _ai.Setup(a => a.RegenerateAsync(It.IsAny<AiRegenerateContext>()))
           .ReturnsAsync(new AiRegenerationResult("Rewrote exp_001 with metrics.", AiAvailable: true));
        _resumes.Setup(r => r.UpdateAsync(resume)).Returns(Task.CompletedTask);
        _history.Setup(h => h.AddAsync(It.IsAny<ResumeHistory>())).Returns(Task.CompletedTask);
        _history.Setup(h => h.PruneAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);
        _regenerations.Setup(r => r.AddAsync(It.IsAny<Regeneration>())).Returns(Task.CompletedTask);

        var req = new RegenerateRequest { SectionIdentifier = "exp_001", UserPrompt = "Add metrics." };

        // Act
        var result = await CreateSut().RegenerateAsync(resume.Id, userId, req);

        // Assert – the response carries the updated content
        result.UpdatedContent.GetString().Should().Be("Rewrote exp_001 with metrics.");

        // The experience entry's description must be updated in FinalData; the array structure is preserved
        resume.FinalData.Should().Contain("Rewrote exp_001 with metrics.");
        resume.FinalData.Should().Contain("\"id\":\"exp_001\"");   // entry still present
        resume.FinalData.Should().Contain("\"title\":\"Dev\"");    // other fields intact
        // No rogue top-level "exp_001" key should have been created
        resume.FinalData.Should().NotMatchRegex("\"exp_001\"\\s*:\\s*\"");
    }

    /// <summary>
    /// Scenario: every regeneration must create a ResumeHistory snapshot with the reason prefixed "REGEN_".
    /// <br/><b>Input:</b> Valid resume and user; section="summary".
    /// <br/><b>Expected:</b> IResumeHistoryRepository.AddAsync called once with
    /// ResumeHistory.Reason starting with "REGEN_".
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_SavesHistorySnapshot()
    {
        // Arrange – Input: valid resume, standard request
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);
        SetupDefaults(resume);

        // Act
        await CreateSut().RegenerateAsync(resume.Id, userId, MakeRequest("summary"));

        // Assert – Expected: history record created with reason starting "REGEN_"
        _history.Verify(h => h.AddAsync(It.Is<ResumeHistory>(rh =>
            rh.Reason.StartsWith("REGEN_")
        )), Times.Once);
    }
}
