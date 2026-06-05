using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class ResumeServiceTests
{
    private readonly Mock<IResumeRepository> _resumes = new();
    private readonly Mock<IDownloadRepository> _downloads = new();
    private readonly Mock<IResumeHistoryRepository> _history = new();
    private readonly Mock<IResumeGenerationService> _ai = new();
    private readonly Mock<ITemplateRendererService> _renderer = new();
    private readonly Mock<ITemplateRepository> _templates = new();

    private ResumeService CreateSut() => new(_resumes.Object, _downloads.Object, _history.Object, _ai.Object, _renderer.Object, _templates.Object);

    private static void SetupHistory(Mock<IResumeHistoryRepository> history)
    {
        history.Setup(h => h.AddAsync(It.IsAny<ResumeHistory>())).Returns(Task.CompletedTask);
        history.Setup(h => h.PruneAsync(It.IsAny<Guid>(), It.IsAny<int>())).Returns(Task.CompletedTask);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: creates a new resume, invokes the AI generator, and returns the completed detail DTO.
    /// <br/><b>Input:</b> userId (Guid); CreateResumeRequest { TemplateId, RawData with firstName="John" };
    /// AI returns FinalData JSON with settings and content.
    /// <br/><b>Expected:</b> Non-null ResumeDetailDto with Status="COMPLETED";
    /// IAiService.GenerateAsync called exactly once.
    /// </summary>
    [Fact]
    public async Task CreateAsync_CallsAiAndReturnsDetailDto()
    {
        // Arrange – Input: userId, template stub, create request with typed raw data
        var userId = Guid.NewGuid();
        var template = JwtTestHelper.MakeTemplate();
        var req = new CreateResumeRequest
        {
            TemplateId = template.Id,
            RawData = new RawResumeData
            {
                Content = new RawResumeContent
                {
                    Personal = new PersonalInfo { FirstName = "John" }
                }
            }
        };

        _resumes.Setup(r => r.AddAsync(It.IsAny<Resume>())).Returns(Task.CompletedTask);
        _templates.Setup(t => t.GetByIntIdAsync(template.Id)).ReturnsAsync(template);
        _ai.Setup(a => a.GenerateAsync(It.IsAny<string>()))
           .ReturnsAsync(new AiGenerationResult("{\"settings\":{},\"content\":{}}", AiAvailable: false));
        _resumes.Setup(r => r.UpdateAsync(It.IsAny<Resume>())).Returns(Task.CompletedTask);

        // GetWithTemplateAsync returns the resume with a template
        _resumes.Setup(r => r.GetWithTemplateAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Guid id) =>
                {
                    var resume = req.ToResume(userId);
                    resume.Template = template;
                    resume.FinalData = "{\"settings\":{},\"content\":{}}";
                    resume.Status = ResumeStatus.Completed;
                    return resume;
                });
        SetupHistory(_history);

        var result = await CreateSut().CreateAsync(userId, req);

        result.Should().NotBeNull();
        result.Status.Should().Be("COMPLETED");
        _ai.Verify(a => a.GenerateAsync(It.IsAny<string>()), Times.Once);
    }

    // ── GetAllByUser ──────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: retrieves all resumes belonging to a specific user.
    /// <br/><b>Input:</b> userId with 2 resumes in the repository.
    /// <br/><b>Expected:</b> List of 2 ResumeSummaryDtos.
    /// </summary>
    [Fact]
    public async Task GetAllByUserAsync_ReturnsUserResumes()
    {
        // Arrange – Input: userId, mock returns 2 resume stubs for that user
        var userId = Guid.NewGuid();
        var template = JwtTestHelper.MakeTemplate();
        var resumes = new List<Resume>
        {
            JwtTestHelper.MakeResume(userId, template),
            JwtTestHelper.MakeResume(userId, template)
        };

        _resumes.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(resumes);

        // Act
        var result = await CreateSut().GetAllByUserAsync(userId);

        // Assert – Expected: exactly 2 DTOs returned
        result.Should().HaveCount(2);
    }

    /// <summary>
    /// Scenario: user has no resumes; the service must return an empty collection without error.
    /// <br/><b>Input:</b> userId with no resumes in the repository.
    /// <br/><b>Expected:</b> Empty IEnumerable&lt;ResumeSummaryDto&gt;.
    /// </summary>
    [Fact]
    public async Task GetAllByUserAsync_WithNoResumes_ReturnsEmptyList()
    {
        // Arrange – Input: repository returns empty list for this user
        var userId = Guid.NewGuid();
        _resumes.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(new List<Resume>());

        // Act
        var result = await CreateSut().GetAllByUserAsync(userId);

        // Assert – Expected: empty list, no exception
        result.Should().BeEmpty();
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: fetches a single resume by ID when the requesting user is the owner.
    /// <br/><b>Input:</b> Resume owned by userId; GetWithTemplateAsync returns it.
    /// <br/><b>Expected:</b> ResumeDetailDto with Id matching the resume.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithValidOwner_ReturnsDetailDto()
    {
        // Arrange – Input: resume stub owned by userId
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act
        var result = await CreateSut().GetByIdAsync(resume.Id, userId);

        // Assert – Expected: DTO id matches resume id
        result.Id.Should().Be(resume.Id);
    }

    /// <summary>
    /// Scenario: user tries to read a resume they do not own (authorization check).
    /// <br/><b>Input:</b> Resume owned by ownerUserId; GetByIdAsync called with attackerUserId.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithWrongUser_ThrowsUnauthorizedAccessException()
    {
        // Arrange – Input: resume belongs to one user, request from another
        var ownerUserId = Guid.NewGuid();
        var attackerUserId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(ownerUserId);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act – attacker supplies their own userId
        Func<Task> act = () => CreateSut().GetByIdAsync(resume.Id, attackerUserId);

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    /// <summary>
    /// Scenario: requested resume ID does not exist in the repository.
    /// <br/><b>Input:</b> GetWithTemplateAsync returns null for any Guid.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for any resume id
        _resumes.Setup(r => r.GetWithTemplateAsync(It.IsAny<Guid>())).ReturnsAsync((Resume?)null);

        // Act
        Func<Task> act = () => CreateSut().GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert – Expected: KeyNotFoundException (resume not found)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── UpdateFinalData ───────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: the resume owner manually edits the FinalData JSON.
    /// <br/><b>Input:</b> Resume owned by userId; newData JSON string with updated summary.
    /// <br/><b>Expected:</b> Returned DTO has FinalData equal to newData;
    /// history record created with Reason="MANUAL_EDIT".
    /// </summary>
    [Fact]
    public async Task UpdateFinalDataAsync_WithValidOwner_UpdatesAndReturnsDto()
    {
        // Arrange
        // Input: resume owned by userId; new FinalData JSON with modified summary
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _resumes.Setup(r => r.UpdateAsync(resume)).Returns(Task.CompletedTask);
        SetupHistory(_history);

        var newData = "{\"settings\":{},\"content\":{\"summary\":\"Updated\"}}";

        // Act
        var result = await CreateSut().UpdateFinalDataAsync(resume.Id, userId, newData);

        // Assert – Expected: FinalData updated; MANUAL_EDIT history entry created
        result.FinalData?.ToString().Should().Be(newData);
        _history.Verify(h => h.AddAsync(It.Is<ResumeHistory>(rh => rh.Reason == "MANUAL_EDIT")), Times.Once);
    }

    /// <summary>
    /// Scenario: a user tries to edit the FinalData of a resume they do not own.
    /// <br/><b>Input:</b> Resume owned by one userId; UpdateFinalDataAsync called with a different userId.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task UpdateFinalDataAsync_WithWrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: resume belongs to a different user
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid());
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act – different userId
        Func<Task> act = () => CreateSut().UpdateFinalDataAsync(resume.Id, Guid.NewGuid(), "{}");

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: soft-deletes a Completed resume owned by the requesting user.
    /// <br/><b>Input:</b> Resume (status=Completed) owned by userId.
    /// <br/><b>Expected:</b> Resume.IsDeleted=true; UpdateAsync called once.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_SoftDeletesResume()
    {
        // Arrange – Input: completed resume owned by userId
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId, status: ResumeStatus.Completed);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _resumes.Setup(r => r.UpdateAsync(resume)).Returns(Task.CompletedTask);

        // Act
        await CreateSut().DeleteAsync(resume.Id, userId);

        // Assert – Expected: IsDeleted flag set to true; UpdateAsync called once
        resume.IsDeleted.Should().BeTrue();
        _resumes.Verify(r => r.UpdateAsync(resume), Times.Once);
    }

    /// <summary>
    /// Scenario: attempts to delete a resume that has already been paid — must be blocked.
    /// <br/><b>Input:</b> Resume (status=Paid) owned by userId.
    /// <br/><b>Expected:</b> InvalidOperationException with message containing "paid resume".
    /// </summary>
    [Fact]
    public async Task DeleteAsync_PaidResume_ThrowsInvalidOperationException()
    {
        // Arrange – Input: paid resume (cannot be deleted after payment)
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId, status: ResumeStatus.Paid);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act
        Func<Task> act = () => CreateSut().DeleteAsync(resume.Id, userId);

        // Assert – Expected: InvalidOperationException protecting paid resumes
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*paid resume*");
    }

    /// <summary>
    /// Scenario: a user tries to delete a resume that belongs to someone else.
    /// <br/><b>Input:</b> Resume owned by userId-A; DeleteAsync called with userId-B.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WithWrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: resume belongs to a different user
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid());
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act – different userId
        Func<Task> act = () => CreateSut().DeleteAsync(resume.Id, Guid.NewGuid());

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    // ── GetForDownload ────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: a paid resume owner downloads it in PDF format.
    /// <br/><b>Input:</b> Resume (status=Paid) owned by userId; format="pdf"; ipAddress="127.0.0.1".
    /// <br/><b>Expected:</b> Returns the same Resume entity; Download record with FormatType="PDF" is created.
    /// </summary>
    [Fact]
    public async Task GetForDownloadAsync_PaidResumeWithPdf_ReturnsResume()
    {
        // Arrange – Input: paid resume, requesting PDF format
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId, status: ResumeStatus.Paid);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _downloads.Setup(d => d.AddAsync(It.IsAny<Download>())).Returns(Task.CompletedTask);

        // Act
        var result = await CreateSut().GetForDownloadAsync(resume.Id, userId, "pdf", "127.0.0.1");

        // Assert – Expected: same resume returned; Download record with FormatType="PDF" persisted
        result.Should().BeSameAs(resume);
        _downloads.Verify(d => d.AddAsync(It.Is<Download>(dl => dl.FormatType == "PDF")), Times.Once);
    }

    /// <summary>
    /// Scenario: download attempted for a resume that has not been paid for yet.
    /// <br/><b>Input:</b> Resume (status=Completed, not Paid) owned by userId; format="pdf".
    /// <br/><b>Expected:</b> UnauthorizedAccessException with message containing "paid".
    /// </summary>
    [Fact]
    public async Task GetForDownloadAsync_UnpaidResume_ThrowsUnauthorizedAccessException()
    {
        // Arrange – Input: resume in Completed (not Paid) status
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId, status: ResumeStatus.Completed);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act
        Func<Task> act = () => CreateSut().GetForDownloadAsync(resume.Id, userId, "pdf", null);

        // Assert – Expected: download blocked with UnauthorizedAccessException (resume not paid)
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
                 .WithMessage("*paid*");
    }

    /// <summary>
    /// Scenario: DOCX format requested for a template that does not support Word output.
    /// <br/><b>Input:</b> Resume (status=Paid) with template (supportsWord=false); format="docx".
    /// <br/><b>Expected:</b> InvalidOperationException with message containing "DOCX".
    /// </summary>
    [Fact]
    public async Task GetForDownloadAsync_DocxOnTemplateWithoutWordSupport_ThrowsInvalidOperationException()
    {
        // Arrange – Input: template with supportsWord=false; download request for docx
        var userId = Guid.NewGuid();
        var template = JwtTestHelper.MakeTemplate(supportsWord: false);
        var resume = JwtTestHelper.MakeResume(userId, template, ResumeStatus.Paid);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act
        Func<Task> act = () => CreateSut().GetForDownloadAsync(resume.Id, userId, "docx", null);

        // Assert – Expected: InvalidOperationException (template doesn't support DOCX)
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*DOCX*");
    }

    /// <summary>
    /// Scenario: download attempted by a user who does not own the resume.
    /// <br/><b>Input:</b> Resume (status=Paid) owned by userId-A; GetForDownloadAsync called with userId-B.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task GetForDownloadAsync_WrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: paid resume owned by someone else
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid(), status: ResumeStatus.Paid);
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act – attacker uses a different userId
        Func<Task> act = () => CreateSut().GetForDownloadAsync(resume.Id, Guid.NewGuid(), "pdf", null);

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
