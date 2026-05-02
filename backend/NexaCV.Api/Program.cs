using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NexaCV.Api.Data;
using NexaCV.Api.Endpoints;
using NexaCV.Api.Middleware;
using NexaCV.Api.Repositories;
using NexaCV.Api.Services;
using NexaCV.Api.Services.Payment;
using NexaCV.Api.Settings;
using NexaCV.Api.Swagger;

var builder = WebApplication.CreateBuilder(args);

// ── Global JSON options (enum as string for both serialize and deserialize) ───
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
var config = builder.Configuration;

// ── Settings ──────────────────────────────────────────────────
builder.Services.Configure<JwtSettings>(config.GetSection("Jwt"));
builder.Services.Configure<AiServiceSettings>(config.GetSection("AiService"));
builder.Services.Configure<PaymentSettings>(config.GetSection("Payment"));
builder.Services.Configure<CurrencyServiceSettings>(config.GetSection("CurrencyService"));

// ── Database (in-memory) ──────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("NexaCV"));

builder.Services.AddScoped<DataSeeder>();

// ── Authentication / JWT ──────────────────────────────────────
var jwtSettings = config.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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

builder.Services.AddAuthorization();

// ── Swagger ───────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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
            "Swap in real implementations by implementing `IAiService` / `IPaymentGateway`.\n\n" +
            "### Error shape\n" +
            "All errors return `{ status, error }`. Validation errors also include a `details` array.",
        Contact = new OpenApiContact
        {
            Name = "NexaCV Team",
            Email = "dev@nexacv.io"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // Include XML doc comments from the generated file
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Tag descriptions document filter
    c.DocumentFilter<TagDescriptionsDocumentFilter>();

    // Replace generic ProblemDetails schema with actual error shapes on all 4xx/5xx responses
    c.OperationFilter<ErrorSchemaOperationFilter>();

    // Realistic request body example for POST /api/resumes
    c.OperationFilter<CreateResumeExampleOperationFilter>();

    // JWT Bearer security definition
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
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // Use full type names to avoid schema conflicts
    c.CustomSchemaIds(type => type.FullName);

    // Render JsonElement as a plain JSON object in Swagger (not as "string")
    c.MapType<System.Text.Json.JsonElement>(() => new OpenApiSchema { Type = "object" });
    c.MapType<System.Text.Json.JsonElement?>(() => new OpenApiSchema { Type = "object", Nullable = true });
});

// ── Repositories (Scoped) ─────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IResumeRepository, ResumeRepository>();
builder.Services.AddScoped<IRegenerationRepository, RegenerationRepository>();
builder.Services.AddScoped<IResumeHistoryRepository, ResumeHistoryRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IUserMovementRepository, UserMovementRepository>();
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<IDownloadRepository, DownloadRepository>();

// ── Services (Scoped) ─────────────────────────────────────────
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IRegenerationService, RegenerationService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// ── AI (Stub / Mock) ──────────────────────────────────────────
// When AiService:BaseUrl is set (e.g. http://localhost:5001 in Development),
// StubAiService will forward calls to NexaCV.AiMock instead of using local stubs.
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiService, StubAiService>();
// ── Currency (Stub) ──────────────────────────────────────────────────
// Replace StubCurrencyService with a real HTTP client backed by ExchangeRate-API / Fixer.io
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICurrencyService, StubCurrencyService>();
// ── Payment (Stub) ────────────────────────────────────────────
builder.Services.AddScoped<IPaymentGateway, StubPaymentGateway>();
builder.Services.AddScoped<PaymentGatewayFactory>();

// ── Validation ────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── CORS ──────────────────────────────────────────────────────
builder.Services.AddCors(opt =>
    opt.AddPolicy("frontend", p =>
        p.WithOrigins("http://localhost:3000")
         .AllowAnyHeader()
         .AllowAnyMethod()));

// ═════════════════════════════════════════════════════════════
var app = builder.Build();
// ═════════════════════════════════════════════════════════════

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NexaCV API v1");
        c.DocumentTitle = "NexaCV API Documentation";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DisplayRequestDuration();
        c.EnableFilter();
        c.EnableDeepLinking();
    });
}

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

// ── Seed DB on startup ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync(db);
}

// ── Map Endpoints ─────────────────────────────────────────────
AuthEndpoints.Map(app);
UserEndpoints.Map(app);
TemplateEndpoints.Map(app);
ResumeEndpoints.Map(app);
TransactionEndpoints.Map(app);
WebhookEndpoints.Map(app);

app.Run();
