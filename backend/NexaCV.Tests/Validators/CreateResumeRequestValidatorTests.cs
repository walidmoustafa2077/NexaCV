using FluentValidation.TestHelper;

namespace NexaCV.Tests.Validators;

public class CreateResumeRequestValidatorTests
{
    private readonly CreateResumeRequestValidator _validator = new();

    // ── Happy path ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: a fully populated, valid payload should pass all validation rules.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        _validator.TestValidate(ValidRequest()).ShouldNotHaveAnyValidationErrors();
    }

    // ── Personal — First Name ─────────────────────────────────────────────────

    /// <summary>
    /// Scenario: FirstName cannot be empty.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.FirstName.
    /// </summary>
    [Fact]
    public void FirstName_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.FirstName = string.Empty;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.FirstName);
    }

    // ── Personal — Last Name ──────────────────────────────────────────────────

    /// <summary>
    /// Scenario: LastName cannot be empty.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.LastName.
    /// </summary>
    [Fact]
    public void LastName_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.LastName = string.Empty;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.LastName);
    }

    // ── Personal — Email ──────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Email cannot be empty.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.Email.
    /// </summary>
    [Fact]
    public void Email_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.Email = string.Empty;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.Email);
    }

    /// <summary>
    /// Scenario: Email must be a valid email address format.
    /// <br/><b>Input:</b> Email="not-an-email".
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.Email.
    /// </summary>
    [Fact]
    public void Email_InvalidFormat_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.Email = "not-an-email";

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.Email);
    }

    // ── Personal — Phone ──────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Phone cannot be empty.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.Phone.
    /// </summary>
    [Fact]
    public void Phone_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.Phone = string.Empty;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.Phone);
    }

    // ── Personal — Location ───────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Location cannot be empty.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Personal.Location.
    /// </summary>
    [Fact]
    public void Location_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Personal.Location = string.Empty;

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Personal.Location);
    }

    // ── Education ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Institution name cannot be empty in an education entry.
    /// <br/><b>Expected:</b> Validation error on the Institution field.
    /// </summary>
    [Fact]
    public void Education_Institution_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Education[0].Institution = string.Empty;

        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Institution"));
    }

    /// <summary>
    /// Scenario: Degree cannot be empty in an education entry.
    /// <br/><b>Expected:</b> Validation error on the Degree field.
    /// </summary>
    [Fact]
    public void Education_Degree_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Education[0].Degree = string.Empty;

        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Degree"));
    }

    /// <summary>
    /// Scenario: Graduation date (EndDate) is optional — a null EndDate means the
    /// student is currently enrolled and must not produce a validation error.
    /// <br/><b>Expected:</b> No validation error for the EndDate field.
    /// </summary>
    [Fact]
    public void Education_GraduationDate_Empty_PassesValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Education[0].EndDate = null;

        var result = _validator.TestValidate(req);
        result.Errors.Should().NotContain(e => e.PropertyName.Contains("EndDate"));
    }

    // ── Experience ────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: At least one experience entry is required.
    /// <br/><b>Input:</b> Empty Experience list.
    /// <br/><b>Expected:</b> Validation error for RawData.Content.Experience.
    /// </summary>
    [Fact]
    public void Experience_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Experience = [];

        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.RawData.Content.Experience);
    }

    /// <summary>
    /// Scenario: Job title cannot be empty in an experience entry.
    /// <br/><b>Expected:</b> Validation error on the Title field.
    /// </summary>
    [Fact]
    public void Experience_Title_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Experience[0].Title = string.Empty;

        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Title"));
    }

    /// <summary>
    /// Scenario: Company cannot be empty in an experience entry.
    /// <br/><b>Expected:</b> Validation error on the Company field.
    /// </summary>
    [Fact]
    public void Experience_Company_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Experience[0].Company = string.Empty;

        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Company"));
    }

    /// <summary>
    /// Scenario: Description cannot be empty in an experience entry.
    /// <br/><b>Expected:</b> Validation error on the Description field.
    /// </summary>
    [Fact]
    public void Experience_Description_Empty_FailsValidation()
    {
        var req = ValidRequest();
        req.RawData.Content.Experience[0].Description = string.Empty;

        var result = _validator.TestValidate(req);
        result.ShouldHaveAnyValidationError();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Description"));
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static CreateResumeRequest ValidRequest() => new()
    {
        TemplateId = 1,
        RawData = new RawResumeData
        {
            Content = new RawResumeContent
            {
                Personal = new PersonalInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@example.com",
                    Phone = "+201012345678",
                    Location = "Cairo, Egypt"
                },
                Experience =
                [
                    new ExperienceEntry
                    {
                        Title = "Senior Product Designer",
                        Company = "TechFlow Systems",
                        Description = "Led the design system team."
                    }
                ],
                Education =
                [
                    new EducationEntry
                    {
                        Institution = "Cairo University",
                        Degree = "B.Sc. in Computer Science",
                        EndDate = "2019-06"
                    }
                ]
            }
        }
    };
}
