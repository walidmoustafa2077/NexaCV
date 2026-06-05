using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexaCV.Api.Data;
using NexaCV.Api.Repositories;
using NexaCV.Api.Services;
using NexaCV.Api.Services.Payment;
using NexaCV.Api.Settings;
using NexaCV.Api.Swagger;

namespace NexaCV.Api.Extensions;

/// <summary>
/// IServiceCollection extension methods that split the flat Program.cs registration
/// block into single-responsibility groups (SRP). Each method owns exactly one
/// cross-cutting concern: authentication, Swagger, persistence, services, etc.
/// </summary>
public static class ServiceExtensions
{
    /// <summary>Registers JWT bearer authentication and settings.</summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("Jwt"));

        var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()!;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>Registers Swagger/OpenAPI documentation with JWT security and custom filters.</summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NexaCV API",
                Version = "v1",
                Description =
                    "## NexaCV — AI-powered Resume Builder API\n\n" +
                    "This API drives the full NexaCV platform. It manages **users**, **resume templates**, " +
                    "**resumes** (with per-section AI regeneration), and a **payment flow** with pluggable gateway support.\n\n" +
                    "### Authentication\n" +
                    "All protected routes require a **Bearer JWT** in the `Authorization` header.\n" +
                    "Obtain a token via `POST /api/auth/register` or `POST /api/auth/login`, " +
                    "then click the **Authorize** button and enter `Bearer <token>`.\n\n" +
                    "### Resume lifecycle\n" +
                    "`DRAFT` → `COMPLETED` (after AI generation) → `PAID` (after successful payment webhook)\n\n" +
                    "### Regeneration limits\n" +
                    "Each section may be regenerated **up to 3 times** (EGP 10 / USD 0.25 per call). " +
                    "A fourth call returns **429 Too Many Requests**.\n\n" +
                    "### Current AI / Payment status\n" +
                    "Both are **stub implementations**. The AI returns raw data unchanged (`ai_available: false`). " +
                    "The payment gateway returns a local stub URL. " +
                    "Swap in real implementations by implementing `IResumeGenerationService` / `IPaymentGateway`.\n\n" +
                    "### Error shape\n" +
                    "All errors return `{ status, error }`. Validation errors also include a `details` array.",
                Contact = new OpenApiContact { Name = "NexaCV Team", Email = "dev@nexacv.io" },
                License = new OpenApiLicense
                {
                    Name = "MIT",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

            c.DocumentFilter<TagDescriptionsDocumentFilter>();
            c.OperationFilter<ErrorSchemaOperationFilter>();
            c.OperationFilter<CreateResumeExampleOperationFilter>();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description =
                    "Paste **only the token** — Swagger will prepend `Bearer ` automatically.\n\n" +
                    "Example: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.CustomSchemaIds(type => type.FullName);
            c.MapType<System.Text.Json.JsonElement>(() => new OpenApiSchema { Type = "object" });
            c.MapType<System.Text.Json.JsonElement?>(() => new OpenApiSchema { Type = "object", Nullable = true });
        });

        return services;
    }

    /// <summary>Registers EF Core database context, data seeder, and settings that belong to persistence.</summary>
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseInMemoryDatabase("NexaCV"));

        services.AddScoped<DataSeeder>();
        return services;
    }

    /// <summary>Registers all repository implementations against their typed interfaces.</summary>
    public static IServiceCollection AddAppRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IResumeRepository, ResumeRepository>();
        services.AddScoped<IRegenerationRepository, RegenerationRepository>();
        services.AddScoped<IResumeHistoryRepository, ResumeHistoryRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IUserMovementRepository, UserMovementRepository>();
        services.AddScoped<ITemplateRepository, TemplateRepository>();
        services.AddScoped<IDownloadRepository, DownloadRepository>();
        return services;
    }

    /// <summary>Registers application services, including the JWT and current-user abstractions.</summary>
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<JwtService>();
        services.AddScoped<ICurrentUserContext, ClaimsPrincipalCurrentUserContext>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ITemplateService, TemplateService>();
        services.AddScoped<IResumeService, ResumeService>();
        services.AddScoped<ITemplateRendererService, TemplateRendererService>();
        services.AddScoped<IRegenerationService, RegenerationService>();
        services.AddScoped<ITransactionService, TransactionService>();
        return services;
    }

    /// <summary>
    /// Registers AI services. <see cref="StubAiService"/> implements both
    /// <see cref="IResumeGenerationService"/> and <see cref="IResumeSectionRegenerationService"/>;
    /// it is registered once and aliased to each interface so the DI container returns the
    /// same scoped instance regardless of which interface is requested.
    /// Swap in real implementations by replacing these registrations.
    /// </summary>
    public static IServiceCollection AddAiServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<AiServiceSettings>(config.GetSection("AiService"));
        services.AddHttpClient();
        services.AddScoped<StubAiService>();
        services.AddScoped<IResumeGenerationService>(sp => sp.GetRequiredService<StubAiService>());
        services.AddScoped<IResumeSectionRegenerationService>(sp => sp.GetRequiredService<StubAiService>());
        return services;
    }

    /// <summary>
    /// Registers payment and currency services. <see cref="StubPaymentGateway"/> implements both
    /// <see cref="IPaymentSessionCreator"/> and <see cref="IWebhookVerifier"/>; it is registered
    /// once and aliased to each segregated interface. Swap in real gateways by adding additional
    /// registrations for each interface without modifying existing code (OCP).
    /// </summary>
    public static IServiceCollection AddPaymentServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<PaymentSettings>(config.GetSection("Payment"));
        services.Configure<CurrencyServiceSettings>(config.GetSection("CurrencyService"));
        services.AddMemoryCache();
        services.AddScoped<ICurrencyService, StubCurrencyService>();
        services.AddScoped<StubPaymentGateway>();
        services.AddScoped<IPaymentSessionCreator>(sp => sp.GetRequiredService<StubPaymentGateway>());
        services.AddScoped<IWebhookVerifier>(sp => sp.GetRequiredService<StubPaymentGateway>());
        services.AddScoped<PaymentGatewayFactory>();
        return services;
    }
}
