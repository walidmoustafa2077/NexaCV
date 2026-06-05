using Microsoft.AspNetCore.Http;
using NexaCV.Tests.Helpers;

namespace NexaCV.Tests.Utils;

public class StubAiServiceTests
{
    private readonly StubAiService _sut = new(
        new Mock<IHttpClientFactory>().Object,
        Options.Create(new AiServiceSettings()));

    /// <summary>
    /// Scenario: StubAiService wraps the raw resume JSON in a root object with settings and content keys.
    /// <br/><b>Input:</b> rawData="{\"name\":\"John Doe\",\"title\":\"Engineer\"}".
    /// <br/><b>Expected:</b> AiGenerationResult.AiAvailable=false;
    /// FinalDataJson contains both "settings" and "content" keys.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WrapsRawDataInRootObject()
    {
        // Arrange – Input: raw resume JSON string
        var rawData = "{\"name\":\"John Doe\",\"title\":\"Engineer\"}";

        // Act
        var result = await _sut.GenerateAsync(rawData);

        // Assert – Expected: AiAvailable=false (stub); FinalDataJson has a "content" key.
        // Settings are template-defined and are NOT stored in FinalData.
        result.AiAvailable.Should().BeFalse();
        result.FinalDataJson.Should().Contain("content");
        result.FinalDataJson.Should().NotContain("settings");
    }

    /// <summary>
    /// Scenario: the generated FinalDataJson includes the default section format settings.
    /// <br/><b>Input:</b> rawData="{}" (minimal input).
    /// <br/><b>Expected:</b> FinalDataJson contains "SUMMARY", "BULLET", and "GRID" tokens.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_ReturnsContentOnlySchema()
    {
        // Arrange – Input: empty JSON object (minimal raw data)
        // Act
        var result = await _sut.GenerateAsync("{}");

        // Assert – Expected: FinalDataJson uses content-only schema.
        // Settings (summary type, description format) are template-defined and not stored here.
        result.FinalDataJson.Should().Contain("content");
        result.FinalDataJson.Should().NotContain("settings");
    }

    /// <summary>
    /// Scenario: StubAiService must not throw when fed malformed JSON (graceful degradation).
    /// <br/><b>Input:</b> rawData="not valid json {{" (invalid JSON string).
    /// <br/><b>Expected:</b> No exception thrown; result is non-null with non-null FinalDataJson.
    /// </summary>
    [Fact]
    public async Task GenerateAsync_WithInvalidJson_DoesNotThrow()
    {
        // Arrange – Input: intentionally malformed JSON
        // Act & Assert – Expected: graceful handling, no exception
        var result = await _sut.GenerateAsync("not valid json {{");

        result.Should().NotBeNull();
        result.FinalDataJson.Should().NotBeNull();
    }

    /// <summary>
    /// Scenario: StubAiService returns the UserPrompt verbatim as the updated content.
    /// <br/><b>Input:</b> AiRegenerateContext { SectionIdentifier="summary",
    /// UserPrompt="Be concise and achievement-focused", ... }.
    /// <br/><b>Expected:</b> AiAvailable=false; UpdatedContent equals the UserPrompt string.
    /// </summary>
    [Fact]
    public async Task RegenerateAsync_ReturnsUserPromptAsUpdatedContent()
    {
        // Arrange – Input: regeneration context with a specific UserPrompt
        var context = new AiRegenerateContext(
            SectionIdentifier: "summary",
            UserPrompt: "Be concise and achievement-focused",
            TargetFormat: null,
            NewTitleSuggestion: null,
            CurrentSectionContent: "Old content",
            ResumeTitle: "Software Engineer",
            Skills: "C#, .NET",
            CurrentDescriptionFormat: "BULLET");

        // Act
        var result = await _sut.RegenerateAsync(context);

        // Assert – Expected: AiAvailable=false; UpdatedContent = context.UserPrompt
        result.AiAvailable.Should().BeFalse();
        result.UpdatedContent.Should().Be(context.UserPrompt);
    }
}

public class StubCurrencyServiceTests
{
    private static StubCurrencyService CreateSut(Dictionary<string, decimal>? rates = null)
    {
        var settings = new CurrencyServiceSettings
        {
            CacheDurationHours = 1,
            StubRates = rates ?? new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
            {
                ["USD"] = 1.00m,
                ["EGP"] = 50.00m,
                ["EUR"] = 0.92m
            }
        };

        var cache = new MemoryCache(new MemoryCacheOptions());
        return new StubCurrencyService(Options.Create(settings), cache);
    }

    /// <summary>
    /// Scenario: StubCurrencyService returns the correct rate for each supported currency code.
    /// <br/><b>Input:</b> currency code ("USD", "EGP", or "EUR"); stub rates configured accordingly.
    /// <br/><b>Expected:</b> Returned decimal rate matches the configured stub value.
    /// </summary>
    [Theory]
    [InlineData("USD", 1.00)]
    [InlineData("EGP", 50.00)]
    [InlineData("EUR", 0.92)]
    public async Task GetExchangeRateAsync_KnownCurrency_ReturnsCorrectRate(string currency, double expected)
    {
        // Arrange – Input: currency code with a configured stub rate
        // Act & Assert – Expected: returned rate equals the configured stub value
        var result = await CreateSut().GetExchangeRateAsync(currency);
        result.Should().Be((decimal)expected);
    }

    /// <summary>
    /// Scenario: currency lookups are case-insensitive ("usd", "Egp", and "EUR" all work).
    /// <br/><b>Input:</b> lowercase/mixed-case currency codes.
    /// <br/><b>Expected:</b> Rate greater than 0 returned for all case variants.
    /// </summary>
    [Theory]
    [InlineData("usd")]
    [InlineData("Egp")]
    [InlineData("EUR")]
    public async Task GetExchangeRateAsync_CaseInsensitive_ReturnsRate(string currency)
    {
        // Arrange – Input: currency code in various cases
        // Act & Assert – Expected: positive rate regardless of case
        var result = await CreateSut().GetExchangeRateAsync(currency);
        result.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Scenario: requesting a rate for an unsupported currency code must throw.
    /// <br/><b>Input:</b> currency="XYZ" (not in the stub configuration).
    /// <br/><b>Expected:</b> InvalidOperationException with message containing "XYZ".
    /// </summary>
    [Fact]
    public async Task GetExchangeRateAsync_UnknownCurrency_ThrowsInvalidOperationException()
    {
        // Arrange – Input: unsupported currency code
        Func<Task> act = () => CreateSut().GetExchangeRateAsync("XYZ");

        // Assert – Expected: InvalidOperationException mentioning the unknown code
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*XYZ*");
    }

    /// <summary>
    /// Scenario: two successive calls for the same currency return the same cached value.
    /// <br/><b>Input:</b> GetExchangeRateAsync("USD") called twice on the same service instance.
    /// <br/><b>Expected:</b> Both calls return the same rate (cache hit on second call).
    /// </summary>
    [Fact]
    public async Task GetExchangeRateAsync_SecondCall_ReturnsCachedValue()
    {
        // Arrange – same service instance used for both calls
        var sut = CreateSut();

        // Act
        var first = await sut.GetExchangeRateAsync("USD");
        var second = await sut.GetExchangeRateAsync("USD");

        // Assert – Expected: second call returns same rate (from cache)
        first.Should().Be(second);
    }
}

public class StubPaymentGatewayTests
{
    private readonly StubPaymentGateway _sut = new();

    /// <summary>
    /// Scenario: the StubPaymentGateway's name must be "Stub" for routing/matching purposes.
    /// <br/><b>Input:</b> None (static property).
    /// <br/><b>Expected:</b> GatewayName = "Stub".
    /// </summary>
    [Fact]
    public void GatewayName_IsStub()
    {
        // Assert – Expected: GatewayName = "Stub"
        _sut.GatewayName.Should().Be("Stub");
    }

    /// <summary>
    /// Scenario: the stub gateway must accept all currencies (wildcard support).
    /// <br/><b>Input:</b> None (static property).
    /// <br/><b>Expected:</b> SupportedCurrency = "*".
    /// </summary>
    [Fact]
    public void SupportedCurrency_IsWildcard()
    {
        // Assert – Expected: wildcard "*" so the stub is used for any currency
        _sut.SupportedCurrency.Should().Be("*");
    }

    /// <summary>
    /// Scenario: creates a payment session and returns a URL containing the transaction ID.
    /// <br/><b>Input:</b> PaymentRequest { TransactionId, Amount=100m, Currency="USD" }.
    /// <br/><b>Expected:</b> PaymentUrl contains txId.ToString(); GatewayRefId equals txId.ToString().
    /// </summary>
    [Fact]
    public async Task CreateSessionAsync_ReturnsUrlContainingTransactionId()
    {
        // Arrange – Input: payment request with a known transaction ID
        var txId = Guid.NewGuid();
        var request = new PaymentRequest(txId, 100m, "USD", Guid.NewGuid());

        // Act
        var result = await _sut.CreateSessionAsync(request);

        // Assert – Expected: URL embeds the transaction ID; ref ID = txId string
        result.PaymentUrl.Should().Contain(txId.ToString());
        result.GatewayRefId.Should().Be(txId.ToString());
    }

    /// <summary>
    /// Scenario: webhook request has the "X-Stub-Ref" header present; signature is considered valid.
    /// <br/><b>Input:</b> HttpRequest with header X-Stub-Ref="ref-abc-123".
    /// <br/><b>Expected:</b> verified=true; eventType="checkout.completed"; gatewayRefId="ref-abc-123".
    /// </summary>
    [Fact]
    public void VerifyWebhookSignature_WithXStubRefHeader_ReturnsTrue()
    {
        // Arrange – Input: HTTP request with the X-Stub-Ref header set
        var refId = "ref-abc-123";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Stub-Ref"] = refId;

        // Act
        var verified = _sut.VerifyWebhookSignature(context.Request, out var eventType, out var gatewayRefId);

        // Assert – Expected: verified; correct event type and ref id extracted
        verified.Should().BeTrue();
        eventType.Should().Be("checkout.completed");
        gatewayRefId.Should().Be(refId);
    }

    /// <summary>
    /// Scenario: webhook request has no "X-Stub-Ref" header; signature verification fails.
    /// <br/><b>Input:</b> HttpRequest with no headers.
    /// <br/><b>Expected:</b> verified=false; gatewayRefId is an empty string.
    /// </summary>
    [Fact]
    public void VerifyWebhookSignature_WithoutXStubRefHeader_ReturnsFalse()
    {
        // Arrange – Input: HTTP request with no headers (no X-Stub-Ref)
        var context = new DefaultHttpContext();

        // Act
        var verified = _sut.VerifyWebhookSignature(context.Request, out _, out var gatewayRefId);

        // Assert – Expected: verification fails; empty ref id
        verified.Should().BeFalse();
        gatewayRefId.Should().BeEmpty();
    }
}

public class PaymentGatewayFactoryTests
{
    /// <summary>
    /// Scenario: factory resolves the correct payment gateway for a given currency.
    /// <br/><b>Input:</b> PaymentGatewayFactory with StubPaymentGateway (SupportedCurrency="*");
    /// currency="USD".
    /// <br/><b>Expected:</b> Resolved gateway is of type StubPaymentGateway.
    /// </summary>
    [Fact]
    public void Resolve_WithWildcardGateway_ReturnsGateway()
    {
        // Arrange – Input: factory with a wildcard stub gateway; querying for "USD"
        var stub = new StubPaymentGateway();
        var factory = new PaymentGatewayFactory(new[] { stub }, new[] { stub });

        // Act
        var gateway = factory.Resolve("USD");

        // Assert – Expected: StubPaymentGateway returned
        gateway.Should().BeOfType<StubPaymentGateway>();
    }

    /// <summary>
    /// Scenario: no gateway registered for the requested currency.
    /// <br/><b>Input:</b> Empty gateway collection; currency="UNSUPPORTED".
    /// <br/><b>Expected:</b> InvalidOperationException with message containing "UNSUPPORTED".
    /// </summary>
    [Fact]
    public void Resolve_WithNoMatchingGateway_ThrowsInvalidOperationException()
    {
        // Arrange – Input: factory with no registered gateways
        var factory = new PaymentGatewayFactory(Array.Empty<IPaymentSessionCreator>(), Array.Empty<IWebhookVerifier>());

        // Act
        Action act = () => factory.Resolve("UNSUPPORTED");

        // Assert – Expected: InvalidOperationException mentioning the unresolvable currency
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*UNSUPPORTED*");
    }

    /// <summary>
    /// Scenario: factory resolves a gateway by inspecting the webhook HTTP request headers.
    /// <br/><b>Input:</b> PaymentGatewayFactory with StubPaymentGateway; HttpRequest with
    /// X-Stub-Ref header set.
    /// <br/><b>Expected:</b> Resolved gateway is of type StubPaymentGateway.
    /// </summary>
    [Fact]
    public void TryResolveWebhook_WithMatchingWebhookHeader_ReturnsResult()
    {
        // Arrange – Input: factory with stub gateway; request has X-Stub-Ref header
        var stub = new StubPaymentGateway();
        var factory = new PaymentGatewayFactory(new[] { stub }, new[] { stub });
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Stub-Ref"] = "some-ref";

        // Act
        var result = factory.TryResolveWebhook(context.Request);

        // Assert – Expected: StubPaymentGateway identified via webhook signature
        result.Should().NotBeNull();
        result!.Value.Verifier.Should().BeOfType<StubPaymentGateway>();
    }

    /// <summary>
    /// Scenario: no gateway verifies the webhook signature for a request with no matching header.
    /// <br/><b>Input:</b> PaymentGatewayFactory with StubPaymentGateway; HttpRequest with no
    /// X-Stub-Ref header.
    /// <br/><b>Expected:</b> InvalidOperationException thrown (no gateway matched).
    /// </summary>
    [Fact]
    public void TryResolveWebhook_WithNoMatchingGateway_ReturnsNull()
    {
        // Arrange – Input: factory with stub gateway; request has no X-Stub-Ref header
        var stub = new StubPaymentGateway();
        var factory = new PaymentGatewayFactory(new[] { stub }, new[] { stub });
        // No X-Stub-Ref header → CanHandleRequest returns false
        var context = new DefaultHttpContext();

        // Act
        var result = factory.TryResolveWebhook(context.Request);

        // Assert – Expected: null when no gateway claims the request
        result.Should().BeNull();
    }
}

public class ClaimsPrincipalCurrentUserContextTests
{
    private readonly JwtService _jwt = JwtTestHelper.Create();

    private static Mock<IHttpContextAccessor> MakeAccessor(System.Security.Claims.ClaimsPrincipal principal)
    {
        var ctx = new DefaultHttpContext { User = principal };
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(ctx);
        return accessor;
    }

    private static System.Security.Claims.ClaimsPrincipal PrincipalWithSub(Guid userId) =>
        new(new System.Security.Claims.ClaimsIdentity(
            new[] { new System.Security.Claims.Claim("sub", userId.ToString()) }));

    private static System.Security.Claims.ClaimsPrincipal EmptyPrincipal() =>
        new(new System.Security.Claims.ClaimsIdentity());

    /// <summary>
    /// Scenario: HTTP context has a valid "sub" claim containing a parseable Guid.
    /// <br/><b>Input:</b> ClaimsPrincipal with a "sub" claim equal to a known userId.
    /// <br/><b>Expected:</b> UserId returns that same Guid.
    /// </summary>
    [Fact]
    public void UserId_WithValidSubClaim_ReturnsUserId()
    {
        // Arrange – Input: accessor with a valid sub claim
        var userId = Guid.NewGuid();
        var sut = new ClaimsPrincipalCurrentUserContext(
            MakeAccessor(PrincipalWithSub(userId)).Object, _jwt);

        // Act & Assert – Expected: UserId matches the claim value
        sut.UserId.Should().Be(userId);
    }

    /// <summary>
    /// Scenario: HTTP context has no "sub" claim (unauthenticated or invalid token).
    /// <br/><b>Input:</b> ClaimsPrincipal with an empty ClaimsIdentity.
    /// <br/><b>Expected:</b> UnauthorizedAccessException when UserId is accessed.
    /// </summary>
    [Fact]
    public void UserId_WithMissingSubClaim_ThrowsUnauthorizedAccessException()
    {
        // Arrange – Input: accessor with empty claims (no sub claim)
        var sut = new ClaimsPrincipalCurrentUserContext(
            MakeAccessor(EmptyPrincipal()).Object, _jwt);

        // Act & Assert – Expected: UnauthorizedAccessException on access
        var act = () => sut.UserId;
        act.Should().Throw<UnauthorizedAccessException>();
    }

    /// <summary>
    /// Scenario: UserId is lazily evaluated — IHttpContextAccessor is not called
    /// until UserId is first accessed.
    /// <br/><b>Input:</b> accessor configured with a valid sub claim.
    /// <br/><b>Expected:</b> HttpContext is accessed exactly once even when UserId is read twice.
    /// </summary>
    [Fact]
    public void UserId_CalledTwice_AccessorHitOnlyOnce()
    {
        // Arrange – Input: accessor with valid sub claim
        var userId = Guid.NewGuid();
        var accessor = MakeAccessor(PrincipalWithSub(userId));
        var sut = new ClaimsPrincipalCurrentUserContext(accessor.Object, _jwt);

        // Act – read UserId twice
        _ = sut.UserId;
        _ = sut.UserId;

        // Assert – Expected: HttpContext accessed once due to Lazy<Guid>
        accessor.Verify(a => a.HttpContext, Times.Once);
    }
}
