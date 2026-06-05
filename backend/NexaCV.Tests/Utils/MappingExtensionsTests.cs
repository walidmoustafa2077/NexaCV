using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Utils;

public class MappingExtensionsTests
{
    // ── User → UserProfileDto ─────────────────────────────────────────────────

    /// <summary>
    /// Scenario: maps every field of a User entity to a UserProfileDto.
    /// <br/><b>Input:</b> User { Id, FirstName="John", LastName="Doe", Username="johndoe",
    /// Email="john@example.com", CreatedAt=2024-01-01, LastLogin=2024-06-01 }.
    /// <br/><b>Expected:</b> All DTO fields equal their corresponding User properties.
    /// </summary>
    [Fact]
    public void ToProfileDto_MapsAllFields()
    {
        // Arrange – Input: fully populated User entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            PasswordHash = "hash",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            LastLogin = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var dto = user.ToProfileDto();

        // Assert – Expected: every field mapped 1-to-1
        dto.Id.Should().Be(user.Id);
        dto.FirstName.Should().Be("John");
        dto.LastName.Should().Be("Doe");
        dto.Username.Should().Be("johndoe");
        dto.Email.Should().Be("john@example.com");
        dto.CreatedAt.Should().Be(user.CreatedAt);
        dto.LastLogin.Should().Be(user.LastLogin);
    }

    // ── Template → TemplateDto ────────────────────────────────────────────────

    /// <summary>
    /// Scenario: maps every field of a Template entity to a TemplateDto.
    /// <br/><b>Input:</b> Template { Id=7, Name="Corporate Pro", IndustryCategory="Finance",
    /// BasePriceUsd=5.00, SupportsWord=true, IsActive=true }.
    /// <br/><b>Expected:</b> All DTO fields match the Template values.
    /// </summary>
    [Fact]
    public void ToDto_Template_MapsAllFields()
    {
        // Arrange – Input: fully populated Template entity
        var template = new Template
        {
            Id = 7,
            Name = "Corporate Pro",
            IndustryCategory = "Finance",
            BasePriceUsd = 5.00m,
            SupportsWord = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = template.ToDto();

        // Assert – Expected: every field mapped correctly
        dto.Id.Should().Be(7);
        dto.Name.Should().Be("Corporate Pro");
        dto.IndustryCategory.Should().Be("Finance");
        dto.BasePriceUsd.Should().Be(5.00m);
        dto.SupportsWord.Should().BeTrue();
    }

    /// <summary>
    /// Scenario: Template with a null IndustryCategory must not crash and must map the null as-is.
    /// <br/><b>Input:</b> Template stub with IndustryCategory=null.
    /// <br/><b>Expected:</b> TemplateDto.IndustryCategory is null.
    /// </summary>
    [Fact]
    public void ToDto_Template_WithNullIndustry_MapsAsNull()
    {
        // Arrange – Input: template stub with null IndustryCategory
        var template = JwtTestHelper.MakeTemplate();
        template.IndustryCategory = null;

        // Act
        var dto = template.ToDto();

        // Assert – Expected: IndustryCategory is null in the DTO
        dto.IndustryCategory.Should().BeNull();
    }

    // ── Resume → ResumeSummaryDto ─────────────────────────────────────────────

    /// <summary>
    /// Scenario: maps a Resume with status=Completed to a ResumeSummaryDto.
    /// <br/><b>Input:</b> Resume (status=Completed) with a linked Template.
    /// <br/><b>Expected:</b> DTO Status="COMPLETED" (uppercase), Id and TemplateName correctly set.
    /// </summary>
    [Fact]
    public void ToSummaryDto_MapsStatusAsUpperCase()
    {
        // Arrange – Input: completed resume stub with linked template
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid(), status: ResumeStatus.Completed);

        // Act
        var dto = resume.ToSummaryDto();

        // Assert – Expected: status uppercased, id and template name mapped
        dto.Status.Should().Be("COMPLETED");
        dto.Id.Should().Be(resume.Id);
        dto.TemplateName.Should().Be(resume.Template.Name);
    }

    /// <summary>
    /// Scenario: verifies the Paid enum value is also uppercased in the summary DTO.
    /// <br/><b>Input:</b> Resume (status=Paid).
    /// <br/><b>Expected:</b> DTO Status="PAID".
    /// </summary>
    [Fact]
    public void ToSummaryDto_PaidStatus_MapsAsPaid()
    {
        // Arrange – Input: paid resume stub
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid(), status: ResumeStatus.Paid);

        // Act
        var dto = resume.ToSummaryDto();

        // Assert – Expected: Status = "PAID"
        dto.Status.Should().Be("PAID");
    }

    // ── Resume → ResumeDetailDto ──────────────────────────────────────────────

    /// <summary>
    /// Scenario: maps a Resume to a ResumeDetailDto including all nested fields and AiAvailable flag.
    /// <br/><b>Input:</b> Resume stub (status=Completed); aiAvailable=true.
    /// <br/><b>Expected:</b> All DTO fields (Id, TemplateId, TemplateName, RawData, FinalData,
    /// AiAvailable=true, Status="COMPLETED") correctly mapped.
    /// </summary>
    [Fact]
    public void ToDetailDto_MapsAllFields()
    {
        // Arrange – Input: completed resume stub with template; aiAvailable=true
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid());

        // Act
        var dto = resume.ToDetailDto(aiAvailable: true);

        // Assert – Expected: all DTO properties match the resume entity
        dto.Id.Should().Be(resume.Id);
        dto.TemplateId.Should().Be(resume.TemplateId);
        dto.TemplateName.Should().Be(resume.Template.Name);
        dto.RawData?.GetRawText().Should().Be(resume.RawData);
        dto.FinalData?.GetRawText().Should().Be(resume.FinalData);
        dto.AiAvailable.Should().BeTrue();
        dto.Status.Should().Be("COMPLETED");
    }

    /// <summary>
    /// Scenario: ToDetailDto default parameter means AiAvailable defaults to false.
    /// <br/><b>Input:</b> Resume stub; ToDetailDto called without the aiAvailable argument.
    /// <br/><b>Expected:</b> DTO.AiAvailable = false.
    /// </summary>
    [Fact]
    public void ToDetailDto_DefaultAiAvailable_IsFalse()
    {
        // Arrange – Input: resume stub; no aiAvailable argument passed
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid());

        // Act
        var dto = resume.ToDetailDto();

        // Assert – Expected: AiAvailable defaults to false
        dto.AiAvailable.Should().BeFalse();
    }

    // ── Regeneration → RegenerateResponse ────────────────────────────────────

    /// <summary>
    /// Scenario: maps a Regeneration entity to a RegenerateResponse DTO with computed counts.
    /// <br/><b>Input:</b> Regeneration { SectionIdentifier="experience", CostUsd=0.25 };
    /// totalUsed=2, updatedContent="New content", aiAvailable=false.
    /// <br/><b>Expected:</b> DTO with RegenCountUsed=2, RegenCountRemaining=1 (3-2),
    /// AddedCostUsd=0.25, AiAvailable=false.
    /// </summary>
    [Fact]
    public void ToResponseDto_MapsCountsCorrectly()
    {
        // Arrange – Input: regeneration entity; 2 used out of max 3
        var regen = new Regeneration
        {
            Id = Guid.NewGuid(),
            ResumeId = Guid.NewGuid(),
            SectionIdentifier = "experience",
            CostUsd = 0.25m,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = regen.ToResponseDto(totalUsed: 2, updatedContent: "New content", aiAvailable: false);

        // Assert – Expected: counts and cost correctly mapped
        dto.SectionIdentifier.Should().Be("experience");
        dto.UpdatedContent.GetString().Should().Be("New content");
        dto.RegenCountUsed.Should().Be(2);
        dto.RegenCountRemaining.Should().Be(1); // 3 - 2
        dto.AddedCostUsd.Should().Be(0.25m);
        dto.AiAvailable.Should().BeFalse();
    }

    // ── Transaction → CheckoutResponse ───────────────────────────────────────

    /// <summary>
    /// Scenario: maps a Transaction entity to a CheckoutResponse DTO including the payment URL.
    /// <br/><b>Input:</b> Transaction { BaseAmount=150, RegenAmount=12.5, TotalAmount=162.5,
    /// Currency="EGP", ExchangeRateUsed=50 }; paymentUrl="https://pay.example.com/session".
    /// <br/><b>Expected:</b> All DTO fields match including PaymentUrl.
    /// </summary>
    [Fact]
    public void ToCheckoutResponse_MapsAllFields()
    {
        // Arrange – Input: transaction entity with EGP amounts and a payment URL
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ResumeId = Guid.NewGuid(),
            BaseAmount = 150m,
            RegenAmount = 12.5m,
            TotalAmount = 162.5m,
            Currency = "EGP",
            ExchangeRateUsed = 50m,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var dto = tx.ToCheckoutResponse("https://pay.example.com/session");

        // Assert – Expected: all amounts, currency, rate, and PaymentUrl mapped correctly
        dto.TransactionId.Should().Be(tx.Id);
        dto.PaymentUrl.Should().Be("https://pay.example.com/session");
        dto.BaseAmount.Should().Be(150m);
        dto.RegenAmount.Should().Be(12.5m);
        dto.TotalAmount.Should().Be(162.5m);
        dto.Currency.Should().Be("EGP");
        dto.ExchangeRateUsed.Should().Be(50m);
    }

    // ── Transaction → TransactionDto ─────────────────────────────────────────

    /// <summary>
    /// Scenario: maps a completed Transaction to a TransactionDto with uppercase PaymentStatus.
    /// <br/><b>Input:</b> Transaction { TotalAmount=100, Currency="USD", PaymentStatus=Success,
    /// CompletedAt=UtcNow+5min }.
    /// <br/><b>Expected:</b> DTO PaymentStatus="SUCCESS", CompletedAt not null, Currency="USD".
    /// </summary>
    [Fact]
    public void ToDto_Transaction_MapsStatusAsUpperCase()
    {
        // Arrange – Input: succeeded transaction entity
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ResumeId = Guid.NewGuid(),
            TotalAmount = 100m,
            Currency = "USD",
            ExchangeRateUsed = 1m,
            PaymentStatus = PaymentStatus.Success,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow.AddMinutes(5)
        };

        // Act
        var dto = tx.ToDto();

        // Assert – Expected: PaymentStatus uppercased; CompletedAt present
        dto.PaymentStatus.Should().Be("SUCCESS");
        dto.CompletedAt.Should().NotBeNull();
        dto.Currency.Should().Be("USD");
    }

    // ── RegisterRequest → User ────────────────────────────────────────────────

    /// <summary>
    /// Scenario: maps a RegisterRequest to a User entity, hashing the password and generating a new Id.
    /// <br/><b>Input:</b> RegisterRequest { FirstName="Jane", LastName="Smith", Username="janesmith",
    /// Email="jane@example.com", Password="P@ssw0rd!", DateOfBirth=1990-05-15 };
    /// hashedPassword="hashed-password".
    /// <br/><b>Expected:</b> User with a new non-empty Id, all fields mapped, PasswordHash="hashed-password",
    /// CreatedAt within 5 seconds of UtcNow.
    /// </summary>
    [Fact]
    public void ToUser_MapsAllFieldsAndSetsNewId()
    {
        // Arrange – Input: registration payload with pre-hashed password string
        var req = new RegisterRequest
        {
            FirstName = "Jane",
            LastName = "Smith",
            Username = "janesmith",
            Email = "jane@example.com",
            Password = "P@ssw0rd!",
            DateOfBirth = new DateOnly(1990, 5, 15)
        };

        // Act
        var user = req.ToUser("hashed-password");

        // Assert – Expected: all fields mapped; new Guid Id generated; timestamps set
        user.Id.Should().NotBe(Guid.Empty);
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.Username.Should().Be("janesmith");
        user.Email.Should().Be("jane@example.com");
        user.PasswordHash.Should().Be("hashed-password");
        user.DateOfBirth.Should().Be(new DateOnly(1990, 5, 15));
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── CreateResumeRequest → Resume ──────────────────────────────────────────

    /// <summary>
    /// Scenario: maps a CreateResumeRequest to a Resume entity with Draft status and timestamps.
    /// <br/><b>Input:</b> userId (Guid); CreateResumeRequest { TemplateId=3, RawData with firstName="Test" }.
    /// <br/><b>Expected:</b> Resume with a new non-empty Id, UserId matching userId,
    /// Status=Draft, RawData serialised as JSON, TemplateId mapped, timestamps within 5 seconds of UtcNow.
    /// </summary>
    [Fact]
    public void ToResume_MapsAllFieldsWithDraftStatus()
    {
        // Arrange – Input: userId and create request
        var userId = Guid.NewGuid();
        var req = new CreateResumeRequest
        {
            TemplateId = 3,
            RawData = new RawResumeData
            {
                Content = new RawResumeContent
                {
                    Personal = new PersonalInfo { FirstName = "Test" }
                }
            }
        };

        // Act
        var resume = req.ToResume(userId);

        // Assert – Expected: new Guid, correct fields, Draft status, recent timestamps
        resume.Id.Should().NotBe(Guid.Empty);
        resume.UserId.Should().Be(userId);
        resume.TemplateId.Should().Be(3);
        resume.RawData.Should().Contain("\"firstName\":\"Test\"");
        resume.Status.Should().Be(ResumeStatus.Draft);
        resume.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        resume.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── Template.CapabilitiesJson → TemplateDto.Capabilities ─────────────────

    /// <summary>
    /// Scenario: Template with serialized CapabilitiesJson is mapped to a TemplateDto
    /// with a populated Capabilities object.
    /// <br/><b>Input:</b> Template { CapabilitiesJson = JSON with MaxExperienceItems=5, HasHobbySection=true }.
    /// <br/><b>Expected:</b> TemplateDto.Capabilities is not null; MaxExperienceItems=5; HasHobbySection=true.
    /// </summary>
    [Fact]
    public void ToDto_Template_WithCapabilitiesJson_DeserializesCapabilities()
    {
        // Arrange – Input: template with a serialized TemplateCapabilities JSON
        var template = JwtTestHelper.MakeTemplate();
        template.CapabilitiesJson = """
            {
              "maxExperienceItems": 5,
              "hasHobbySection": true,
              "hasProjectSection": false,
              "hasLanguageSection": true,
              "supportedSummaryTypes": ["SUMMARY"],
              "supportedDescriptionFormats": ["BULLETED"],
              "supportedSkillsLayouts": ["FLAT"]
            }
            """;

        // Act
        var dto = template.ToDto();

        // Assert – Expected: Capabilities deserialized correctly
        dto.Capabilities.Should().NotBeNull();
        dto.Capabilities!.MaxExperienceItems.Should().Be(5);
        dto.Capabilities.HasHobbySection.Should().BeTrue();
        dto.Capabilities.HasProjectSection.Should().BeFalse();
        dto.Capabilities.SupportedSummaryTypes.Should().ContainSingle().Which.Should().Be("SUMMARY");
    }

    /// <summary>
    /// Scenario: Template without CapabilitiesJson maps to a TemplateDto with null Capabilities.
    /// <br/><b>Input:</b> Template { CapabilitiesJson = null }.
    /// <br/><b>Expected:</b> TemplateDto.Capabilities is null.
    /// </summary>
    [Fact]
    public void ToDto_Template_WithNullCapabilitiesJson_CapabilitiesIsNull()
    {
        // Arrange – Input: template stub with no capabilities JSON
        var template = JwtTestHelper.MakeTemplate();
        template.CapabilitiesJson = null;

        // Act
        var dto = template.ToDto();

        // Assert – Expected: Capabilities is null (no JSON to deserialize)
        dto.Capabilities.Should().BeNull();
    }
}
