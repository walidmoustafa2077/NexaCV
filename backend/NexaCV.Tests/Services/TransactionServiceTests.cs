using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Services;

public class TransactionServiceTests
{
    private readonly Mock<IResumeRepository> _resumes = new();
    private readonly Mock<ITransactionRepository> _transactions = new();
    private readonly Mock<IRegenerationRepository> _regenerations = new();
    private readonly Mock<ICurrencyService> _currency = new();
    private readonly PaymentGatewayFactory _gatewayFactory = new(new[] { new StubPaymentGateway() });

    private TransactionService CreateSut() =>
        new(_resumes.Object, _transactions.Object, _regenerations.Object, _currency.Object, _gatewayFactory);

    // ── Checkout ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task CheckoutAsync_WithCompletedResume_ReturnsCheckoutResponse()
    {
        var userId = Guid.NewGuid();
        var template = JwtTestHelper.MakeTemplate();
        var resume = JwtTestHelper.MakeResume(userId, template, ResumeStatus.Completed);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _currency.Setup(c => c.GetExchangeRateAsync("USD")).ReturnsAsync(1.00m);
        _regenerations.Setup(r => r.GetUsdCostSumAsync(resume.Id)).ReturnsAsync(0.50m);
        _transactions.Setup(t => t.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _transactions.Setup(t => t.UpdateAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

        var result = await CreateSut().CheckoutAsync(resume.Id, userId, "USD");

        result.Should().NotBeNull();
        result.BaseAmount.Should().Be(template.BasePriceUsd * 1.00m);
        result.RegenAmount.Should().Be(0.50m);
        result.TotalAmount.Should().Be(template.BasePriceUsd + 0.50m);
        result.Currency.Should().Be("USD");
        result.PaymentUrl.Should().StartWith("https://stub.payment/session/");
    }

    /// <summary>
    /// Scenario: checkout in a non-USD currency; amounts must be multiplied by the exchange rate.
    /// <br/><b>Input:</b> Resume (BasePriceUsd=3.00); EGP exchange rate=50.00; regen cost=$0.
    /// <br/><b>Expected:</b> CheckoutResponse BaseAmount=150.00 (3.00 × 50), Currency="EGP".
    /// </summary>
    [Fact]
    public async Task CheckoutAsync_WithEgpCurrency_AppliesExchangeRate()
    {
        // Arrange
        // Input: BasePriceUsd=3.00, EGP rate=50 → BaseAmount should be 3.00 * 50 = 150.00
        var userId = Guid.NewGuid();
        var template = JwtTestHelper.MakeTemplate();   // BasePriceUsd = 3.00
        var resume = JwtTestHelper.MakeResume(userId, template, ResumeStatus.Completed);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _currency.Setup(c => c.GetExchangeRateAsync("EGP")).ReturnsAsync(50.00m);
        _regenerations.Setup(r => r.GetUsdCostSumAsync(resume.Id)).ReturnsAsync(0m);
        _transactions.Setup(t => t.AddAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);
        _transactions.Setup(t => t.UpdateAsync(It.IsAny<Transaction>())).Returns(Task.CompletedTask);

        // Act
        var result = await CreateSut().CheckoutAsync(resume.Id, userId, "EGP");

        // Assert – Expected: base amount converted at 50x rate
        result.BaseAmount.Should().Be(150.00m); // 3.00 * 50
        result.Currency.Should().Be("EGP");
    }

    /// <summary>
    /// Scenario: checkout fails because the resume is in Draft status (must be Completed first).
    /// <br/><b>Input:</b> Resume (status=Draft); any userId and currency.
    /// <br/><b>Expected:</b> InvalidOperationException with message containing "COMPLETED".
    /// </summary>
    [Fact]
    public async Task CheckoutAsync_ResumeNotCompleted_ThrowsInvalidOperationException()
    {
        // Arrange – Input: resume in Draft state; checkout requires Completed
        var userId = Guid.NewGuid();
        var resume = JwtTestHelper.MakeResume(userId, status: ResumeStatus.Draft);

        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);

        // Act
        Func<Task> act = () => CreateSut().CheckoutAsync(resume.Id, userId, "USD");

        // Assert – Expected: InvalidOperationException noting the required status
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*COMPLETED*");
    }

    /// <summary>
    /// Scenario: checkout attempted by a user who does not own the resume.
    /// <br/><b>Input:</b> Resume owned by userId-A; checkout called with userId-B.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task CheckoutAsync_WrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: resume belongs to a different user
        var resume = JwtTestHelper.MakeResume(Guid.NewGuid(), status: ResumeStatus.Completed);
        _resumes.Setup(r => r.GetWithTemplateAsync(resume.Id)).ReturnsAsync(resume);
        _transactions.Setup(t => t.GetByResumeIdAsync(resume.Id)).ReturnsAsync((Transaction?)null);

        // Act – attacker uses a different userId
        Func<Task> act = () => CreateSut().CheckoutAsync(resume.Id, Guid.NewGuid(), "USD");

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    /// <summary>
    /// Scenario: checkout fails because the resume ID does not exist in the repository.
    /// <br/><b>Input:</b> GetWithTemplateAsync returns null for any Guid.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task CheckoutAsync_ResumeNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for any resume id
        _resumes.Setup(r => r.GetWithTemplateAsync(It.IsAny<Guid>())).ReturnsAsync((Resume?)null);

        // Act
        Func<Task> act = () => CreateSut().CheckoutAsync(Guid.NewGuid(), Guid.NewGuid(), "USD");

        // Assert – Expected: KeyNotFoundException (resume not found)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── GetById ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: retrieves a transaction by ID for the user who owns it.
    /// <br/><b>Input:</b> Transaction with TotalAmount=150.00, Currency="EGP",
    /// PaymentStatus=Pending; userId matches transaction.UserId.
    /// <br/><b>Expected:</b> TransactionDto with TotalAmount=150.00 and PaymentStatus="PENDING".
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WithValidOwner_ReturnsTransactionDto()
    {
        // Arrange
        // Input: transaction owned by userId, status=Pending, amount=150.00 EGP
        var userId = Guid.NewGuid();
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ResumeId = Guid.NewGuid(),
            TotalAmount = 150.00m,
            Currency = "EGP",
            ExchangeRateUsed = 50m,
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Setup(t => t.GetByIdAsync(tx.Id)).ReturnsAsync(tx);

        // Act
        var result = await CreateSut().GetByIdAsync(tx.Id, userId);

        // Assert – Expected: correct DTO with uppercase PaymentStatus string
        result.Id.Should().Be(tx.Id);
        result.TotalAmount.Should().Be(150.00m);
        result.PaymentStatus.Should().Be("PENDING");
    }

    /// <summary>
    /// Scenario: a user requests a transaction that belongs to a different user.
    /// <br/><b>Input:</b> Transaction.UserId = userId-A; GetByIdAsync called with userId-B.
    /// <br/><b>Expected:</b> UnauthorizedAccessException thrown.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WrongUser_ThrowsForbiddenException()
    {
        // Arrange – Input: transaction owned by one user, request made by another
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ResumeId = Guid.NewGuid(),
            Currency = "USD",
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _transactions.Setup(t => t.GetByIdAsync(tx.Id)).ReturnsAsync(tx);

        // Act – different caller Guid
        Func<Task> act = () => CreateSut().GetByIdAsync(tx.Id, Guid.NewGuid());

        // Assert – Expected: ForbiddenException (ownership mismatch → 403)
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    /// <summary>
    /// Scenario: requests a transaction by an ID that does not exist in the repository.
    /// <br/><b>Input:</b> GetByIdAsync returns null for any Guid.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: repository returns null for any transaction id
        _transactions.Setup(t => t.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Transaction?)null);

        // Act
        Func<Task> act = () => CreateSut().GetByIdAsync(Guid.NewGuid(), Guid.NewGuid());

        // Assert – Expected: KeyNotFoundException (transaction not found)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── Fulfill ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Scenario: payment webhook fulfills a transaction, marking it succeeded and the resume as paid.
    /// <br/><b>Input:</b> gatewayRefId="ref-abc123"; Transaction (status=Pending); Resume (status=Completed).
    /// <br/><b>Expected:</b> Transaction.PaymentStatus=Success with CompletedAt set; Resume.Status=Paid.
    /// </summary>
    [Fact]
    public async Task FulfillAsync_WithValidRefId_MarkesTransactionSuccessAndResumePaid()
    {
        // Arrange
        // Input: gateway ref id maps to an existing pending transaction and completed resume
        var refId = "ref-abc123";
        var resumeId = Guid.NewGuid();
        var tx = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ResumeId = resumeId,
            Currency = "USD",
            PaymentStatus = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        var resume = JwtTestHelper.MakeResume(tx.UserId, status: ResumeStatus.Completed);
        resume.Id = resumeId;

        _transactions.Setup(t => t.GetByGatewayRefIdAsync(refId)).ReturnsAsync(tx);
        _transactions.Setup(t => t.UpdateAsync(tx)).Returns(Task.CompletedTask);
        _resumes.Setup(r => r.GetByIdAsync(resumeId)).ReturnsAsync(resume);
        _resumes.Setup(r => r.UpdateAsync(resume)).Returns(Task.CompletedTask);

        // Act
        await CreateSut().FulfillAsync(refId);

        // Assert – Expected: transaction marked Success, resume status changed to Paid
        tx.PaymentStatus.Should().Be(PaymentStatus.Success);
        tx.CompletedAt.Should().NotBeNull();
        resume.Status.Should().Be(ResumeStatus.Paid);
    }

    /// <summary>
    /// Scenario: payment webhook arrives with a gateway ref ID that matches no pending transaction.
    /// <br/><b>Input:</b> GetByGatewayRefIdAsync returns null for any string.
    /// <br/><b>Expected:</b> KeyNotFoundException thrown.
    /// </summary>
    [Fact]
    public async Task FulfillAsync_WithUnknownRefId_ThrowsKeyNotFoundException()
    {
        // Arrange – Input: unknown gateway ref id, no matching transaction
        _transactions.Setup(t => t.GetByGatewayRefIdAsync(It.IsAny<string>())).ReturnsAsync((Transaction?)null);

        // Act
        Func<Task> act = () => CreateSut().FulfillAsync("unknown-ref");

        // Assert – Expected: KeyNotFoundException (transaction not found by ref id)
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
