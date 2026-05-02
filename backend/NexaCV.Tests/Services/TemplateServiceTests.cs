using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class TemplateServiceTests
{
    private readonly Mock<ITemplateRepository> _templates = new();

    private TemplateService CreateSut() => new(_templates.Object);

    // ── GetAll ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: retrieves all active templates when no industry filter is supplied.
    /// <br/><b>Input:</b> filter=null; mock returns 2 active Template stubs (IDs 1 and 2).
    /// <br/><b>Expected:</b> A list of 2 TemplateDtos, all with Id &gt; 0.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithNoFilter_ReturnsAllActiveTemplates()
    {
        // Arrange
        // Input: null industry filter → repository returns every active template
        var list = new List<Template>
        {
            JwtTestHelper.MakeTemplate(1),
            JwtTestHelper.MakeTemplate(2)
        };

        _templates.Setup(r => r.GetActiveAsync(null)).ReturnsAsync(list);

        // Act
        var result = await CreateSut().GetAllAsync(null);

        // Assert – Expected: 2 DTOs returned, each with a positive integer ID
        result.Should().HaveCount(2);
        result.All(t => t.Id > 0).Should().BeTrue();
    }

    /// <summary>
    /// Scenario: retrieves templates scoped to a specific industry category.
    /// <br/><b>Input:</b> filter="Corporate"; mock returns 1 matching template.
    /// <br/><b>Expected:</b> List contains 1 TemplateDto; repository is called exactly once
    /// with the exact filter string "Corporate".
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithIndustryFilter_PassesFilterToRepository()
    {
        // Arrange
        // Input: industry filter = "Corporate"; mock returns one matching template
        var filtered = new List<Template> { JwtTestHelper.MakeTemplate(1) };

        _templates.Setup(r => r.GetActiveAsync("Corporate")).ReturnsAsync(filtered);

        // Act
        var result = await CreateSut().GetAllAsync("Corporate");

        // Assert – Expected: 1 result returned; repository called with the correct filter
        result.Should().HaveCount(1);
        _templates.Verify(r => r.GetActiveAsync("Corporate"), Times.Once);
    }

    /// <summary>
    /// Scenario: requests templates with a category filter that matches nothing.
    /// <br/><b>Input:</b> filter="NonExistent"; mock returns an empty list for any string.
    /// <br/><b>Expected:</b> Empty IEnumerable&lt;TemplateDto&gt; — no exception thrown.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithNoResults_ReturnsEmptyList()
    {
        // Arrange – Input: any filter string, repository has no matches
        _templates.Setup(r => r.GetActiveAsync(It.IsAny<string?>())).ReturnsAsync(new List<Template>());

        // Act
        var result = await CreateSut().GetAllAsync("NonExistent");

        // Assert – Expected: empty collection, no exception
        result.Should().BeEmpty();
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: fetches a single template by its integer ID when that ID exists.
    /// <br/><b>Input:</b> id=42; mock returns a Template stub with Id=42, BasePriceUsd=3.00.
    /// <br/><b>Expected:</b> TemplateDto with Id=42 and all fields correctly mapped from the entity.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsMappedDto()
    {
        // Arrange – Input: template stub at Id=42 returned by the repository mock
        var template = JwtTestHelper.MakeTemplate(42);
        _templates.Setup(r => r.GetByIntIdAsync(42)).ReturnsAsync(template);

        // Act
        var result = await CreateSut().GetByIdAsync(42);

        // Assert – Expected: DTO fields match the template entity
        result.Id.Should().Be(42);
        result.Name.Should().Be(template.Name);
        result.BasePriceUsd.Should().Be(template.BasePriceUsd);
    }

    /// <summary>
    /// Scenario: requests a template with an ID that does not exist in the repository.
    /// <br/><b>Input:</b> id=999; mock returns null for any integer ID.
    /// <br/><b>Expected:</b> KeyNotFoundException with the message containing "999".
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: any id lookup returns null (template not found)
        _templates.Setup(r => r.GetByIntIdAsync(It.IsAny<int>())).ReturnsAsync((Template?)null);

        // Act
        Func<Task> act = () => CreateSut().GetByIdAsync(999);

        // Assert – Expected: KeyNotFoundException referencing the missing id (999)
        await act.Should().ThrowAsync<KeyNotFoundException>()
                 .WithMessage("*999*");
    }
}
