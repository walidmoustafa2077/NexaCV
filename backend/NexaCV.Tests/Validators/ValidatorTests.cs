using FluentValidation.TestHelper;

namespace NexaCV.Tests.Validators;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    /// <summary>
    /// Scenario: a fully valid registration payload should pass all validation rules.
    /// <br/><b>Input:</b> RegisterRequest { FirstName="John", LastName="Doe", Username="johndoe",
    /// Email="john@example.com", Password="P@ssw0rd!" }.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        // Arrange – Input: fully valid registration payload
        var req = new RegisterRequest
        {
            FirstName = "John",
            LastName = "Doe",
            Username = "johndoe",
            Email = "john@example.com",
            Password = "P@ssw0rd!"
        };

        // Act
        var result = _validator.TestValidate(req);

        // Assert – Expected: no validation errors
        result.ShouldNotHaveAnyValidationErrors();
    }

    // ── FirstName ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: FirstName cannot be empty.
    /// <br/><b>Input:</b> ValidRequest() with FirstName set to string.Empty.
    /// <br/><b>Expected:</b> Validation error for FirstName.
    /// </summary>
    [Fact]
    public void FirstName_Empty_FailsValidation()
    {
        // Arrange – Input: empty FirstName
        var req = ValidRequest();
        req.FirstName = string.Empty;

        // Act & Assert – Expected: error on FirstName field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    /// <summary>
    /// Scenario: FirstName cannot exceed 50 characters.
    /// <br/><b>Input:</b> ValidRequest() with FirstName = 51 'A' characters.
    /// <br/><b>Expected:</b> Validation error for FirstName.
    /// </summary>
    [Fact]
    public void FirstName_TooLong_FailsValidation()
    {
        // Arrange – Input: FirstName with 51 characters (1 over the max)
        var req = ValidRequest();
        req.FirstName = new string('A', 51);

        // Act & Assert – Expected: error on FirstName (max length exceeded)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    // ── LastName ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: LastName cannot be empty.
    /// <br/><b>Input:</b> ValidRequest() with LastName set to string.Empty.
    /// <br/><b>Expected:</b> Validation error for LastName.
    /// </summary>
    [Fact]
    public void LastName_Empty_FailsValidation()
    {
        // Arrange – Input: empty LastName
        var req = ValidRequest();
        req.LastName = string.Empty;

        // Act & Assert – Expected: error on LastName field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.LastName);
    }

    // ── Username ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Username cannot be empty.
    /// <br/><b>Input:</b> ValidRequest() with Username set to string.Empty.
    /// <br/><b>Expected:</b> Validation error for Username.
    /// </summary>
    [Fact]
    public void Username_Empty_FailsValidation()
    {
        // Arrange – Input: empty Username
        var req = ValidRequest();
        req.Username = string.Empty;

        // Act & Assert – Expected: error on Username field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Username);
    }

    /// <summary>
    /// Scenario: Username cannot exceed 50 characters.
    /// <br/><b>Input:</b> ValidRequest() with Username = 51 'u' characters.
    /// <br/><b>Expected:</b> Validation error for Username.
    /// </summary>
    [Fact]
    public void Username_TooLong_FailsValidation()
    {
        // Arrange – Input: Username with 51 characters (1 over the max)
        var req = ValidRequest();
        req.Username = new string('u', 51);

        // Act & Assert – Expected: error on Username (max length exceeded)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Username);
    }

    // ── Email ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Email must be in valid format ("@" present etc.).
    /// <br/><b>Input:</b> ValidRequest() with Email="not-an-email" (no @ symbol).
    /// <br/><b>Expected:</b> Validation error for Email.
    /// </summary>
    [Fact]
    public void Email_InvalidFormat_FailsValidation()
    {
        // Arrange – Input: Email that is not a valid email address
        var req = ValidRequest();
        req.Email = "not-an-email";

        // Act & Assert – Expected: error on Email (invalid format)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Scenario: Email cannot be empty.
    /// <br/><b>Input:</b> ValidRequest() with Email set to string.Empty.
    /// <br/><b>Expected:</b> Validation error for Email.
    /// </summary>
    [Fact]
    public void Email_Empty_FailsValidation()
    {
        // Arrange – Input: empty Email string
        var req = ValidRequest();
        req.Email = string.Empty;

        // Act & Assert – Expected: error on Email field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Scenario: Email cannot exceed 150 characters.
    /// <br/><b>Input:</b> ValidRequest() with Email = 140 'a' chars + "@example.com" (&gt;150 total).
    /// <br/><b>Expected:</b> Validation error for Email.
    /// </summary>
    [Fact]
    public void Email_TooLong_FailsValidation()
    {
        // Arrange – Input: Email string that exceeds 150 characters
        var req = ValidRequest();
        req.Email = new string('a', 140) + "@example.com"; // > 150 chars

        // Act & Assert – Expected: error on Email (max length exceeded)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    // ── Password ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: Password must be at least 8 characters long.
    /// <br/><b>Input:</b> ValidRequest() with Password="Ab@1" (only 4 characters).
    /// <br/><b>Expected:</b> Validation error for Password.
    /// </summary>
    [Fact]
    public void Password_TooShort_FailsValidation()
    {
        // Arrange – Input: password shorter than 8 characters
        var req = ValidRequest();
        req.Password = "Ab@1";

        // Act & Assert – Expected: error on Password (minimum length not met)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }

    /// <summary>
    /// Scenario: Password must contain at least one special character.
    /// <br/><b>Input:</b> ValidRequest() with Password="Password1" (no special char).
    /// <br/><b>Expected:</b> Validation error for Password with message
    /// "Password must contain at least one special character.".
    /// </summary>
    [Fact]
    public void Password_NoSpecialChar_FailsValidation()
    {
        // Arrange – Input: password with letters and digits but no special character
        var req = ValidRequest();
        req.Password = "Password1";

        // Act & Assert – Expected: error with specific special-character message
        _validator.TestValidate(req)
            .ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one special character.");
    }

    /// <summary>
    /// Scenario: a password meeting all rules (length ≥ 8, has special char) must pass.
    /// <br/><b>Input:</b> ValidRequest() with Password="Password1!".
    /// <br/><b>Expected:</b> No validation error for Password.
    /// </summary>
    [Fact]
    public void Password_WithSpecialChar_PassesValidation()
    {
        // Arrange – Input: valid password with uppercase, digit, and special character
        var req = ValidRequest();
        req.Password = "Password1!";

        // Act & Assert – Expected: no error on Password
        _validator.TestValidate(req).ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    private static RegisterRequest ValidRequest() => new()
    {
        FirstName = "John",
        LastName = "Doe",
        Username = "johndoe",
        Email = "john@example.com",
        Password = "P@ssw0rd!"
    };
}

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    /// <summary>
    /// Scenario: a fully valid login payload should pass all validation rules.
    /// <br/><b>Input:</b> LoginRequest { Email="john@example.com", Password="P@ssw0rd!" }.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        // Arrange – Input: valid email and password
        var req = new LoginRequest { Email = "john@example.com", Password = "P@ssw0rd!" };

        // Act & Assert – Expected: no validation errors
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Scenario: login Email cannot be empty.
    /// <br/><b>Input:</b> LoginRequest { Email=string.Empty, Password="P@ssw0rd!" }.
    /// <br/><b>Expected:</b> Validation error for Email.
    /// </summary>
    [Fact]
    public void Email_Empty_FailsValidation()
    {
        // Arrange – Input: empty Email in login request
        var req = new LoginRequest { Email = string.Empty, Password = "P@ssw0rd!" };

        // Act & Assert – Expected: error on Email field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Scenario: login Email must be a valid email address.
    /// <br/><b>Input:</b> LoginRequest { Email="bad-email", Password="P@ssw0rd!" }.
    /// <br/><b>Expected:</b> Validation error for Email.
    /// </summary>
    [Fact]
    public void Email_InvalidFormat_FailsValidation()
    {
        // Arrange – Input: malformed email in login request
        var req = new LoginRequest { Email = "bad-email", Password = "P@ssw0rd!" };

        // Act & Assert – Expected: error on Email (invalid format)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Email);
    }

    /// <summary>
    /// Scenario: login Password cannot be empty.
    /// <br/><b>Input:</b> LoginRequest { Email="john@example.com", Password=string.Empty }.
    /// <br/><b>Expected:</b> Validation error for Password.
    /// </summary>
    [Fact]
    public void Password_Empty_FailsValidation()
    {
        // Arrange – Input: empty Password in login request
        var req = new LoginRequest { Email = "john@example.com", Password = string.Empty };

        // Act & Assert – Expected: error on Password field
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Password);
    }
}

public class RegenerateRequestValidatorTests
{
    private readonly RegenerateRequestValidator _validator = new();

    /// <summary>
    /// Scenario: a fully valid regenerate request must pass all validation rules.
    /// <br/><b>Input:</b> RegenerateRequest { SectionIdentifier="summary", UserPrompt="Be concise" }.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        // Arrange – Input: all required fields provided
        var req = new RegenerateRequest { SectionIdentifier = "summary", UserPrompt = "Be concise" };

        // Act & Assert – Expected: no validation errors
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Scenario: SectionIdentifier cannot be empty.
    /// <br/><b>Input:</b> RegenerateRequest { SectionIdentifier="", UserPrompt="Be concise" }.
    /// <br/><b>Expected:</b> Validation error for SectionIdentifier.
    /// </summary>
    [Fact]
    public void SectionIdentifier_Empty_FailsValidation()
    {
        // Arrange – Input: empty SectionIdentifier
        var req = new RegenerateRequest { SectionIdentifier = string.Empty, UserPrompt = "Be concise" };

        // Act & Assert – Expected: error on SectionIdentifier
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.SectionIdentifier);
    }

    /// <summary>
    /// Scenario: UserPrompt cannot be empty.
    /// <br/><b>Input:</b> RegenerateRequest { SectionIdentifier="summary", UserPrompt="" }.
    /// <br/><b>Expected:</b> Validation error for UserPrompt.
    /// </summary>
    [Fact]
    public void UserPrompt_Empty_FailsValidation()
    {
        // Arrange – Input: empty UserPrompt
        var req = new RegenerateRequest { SectionIdentifier = "summary", UserPrompt = string.Empty };

        // Act & Assert – Expected: error on UserPrompt
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.UserPrompt);
    }

    /// <summary>
    /// Scenario: optional fields (TargetFormat, NewTitleSuggestion) may be null without error.
    /// <br/><b>Input:</b> RegenerateRequest with both optional fields null.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void OptionalFields_Null_PassesValidation()
    {
        // Arrange – Input: required fields set, optional fields omitted
        var req = new RegenerateRequest
        {
            SectionIdentifier = "experience",
            UserPrompt = "Make it results-oriented",
            TargetFormat = null,
            NewTitleSuggestion = null
        };

        // Act & Assert – Expected: no errors (optional fields are truly optional)
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }
}

public class RenameResumeRequestValidatorTests
{
    private readonly RenameResumeRequestValidator _validator = new();

    /// <summary>
    /// Scenario: a valid name within the limit must pass all rules.
    /// <br/><b>Input:</b> RenameResumeRequest { Name="My Software Engineer Resume" }.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void ValidRequest_PassesValidation()
    {
        // Arrange – Input: valid name
        var req = new RenameResumeRequest { Name = "My Software Engineer Resume" };

        // Act & Assert – Expected: no validation errors
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Scenario: Name cannot be empty.
    /// <br/><b>Input:</b> RenameResumeRequest { Name="" }.
    /// <br/><b>Expected:</b> Validation error for Name.
    /// </summary>
    [Fact]
    public void Name_Empty_FailsValidation()
    {
        // Arrange – Input: empty Name
        var req = new RenameResumeRequest { Name = string.Empty };

        // Act & Assert – Expected: error on Name
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Name);
    }

    /// <summary>
    /// Scenario: Name at exactly the maximum length (100 chars) must pass.
    /// <br/><b>Input:</b> RenameResumeRequest { Name = 100 'A' characters }.
    /// <br/><b>Expected:</b> No validation errors.
    /// </summary>
    [Fact]
    public void Name_ExactlyMaxLength_PassesValidation()
    {
        // Arrange – Input: Name at the boundary (100 chars)
        var req = new RenameResumeRequest { Name = new string('A', 100) };

        // Act & Assert – Expected: no errors at boundary length
        _validator.TestValidate(req).ShouldNotHaveAnyValidationErrors();
    }

    /// <summary>
    /// Scenario: Name exceeding 100 characters must fail.
    /// <br/><b>Input:</b> RenameResumeRequest { Name = 101 'A' characters }.
    /// <br/><b>Expected:</b> Validation error for Name.
    /// </summary>
    [Fact]
    public void Name_TooLong_FailsValidation()
    {
        // Arrange – Input: Name one character over the limit
        var req = new RenameResumeRequest { Name = new string('A', 101) };

        // Act & Assert – Expected: error on Name (max length exceeded)
        _validator.TestValidate(req).ShouldHaveValidationErrorFor(x => x.Name);
    }
}
