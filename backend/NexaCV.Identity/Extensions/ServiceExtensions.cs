using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NexaCV.Identity.Data;
using NexaCV.Identity.Services;
using NexaCV.Identity.Settings;
using NexaCV.Identity.Swagger;

namespace NexaCV.Identity.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers EF Core with In-Memory database.
    /// To switch to a relational DB, replace UseInMemoryDatabase with
    /// UseNpgsql / UseSqlServer and provide a connection string — no other code changes needed.
    /// </summary>
    public static IServiceCollection AddIdentityDataAccess(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<IdentityDbContext>(opt =>
            opt.UseInMemoryDatabase("NexaCVIdentity"));

        return services;
    }

    /// <summary>Registers the token and auth services.</summary>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }

    /// <summary>Registers token settings from configuration.</summary>
    public static IServiceCollection AddTokenSettings(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.Configure<JwtSettings>(config.GetSection("Jwt"));
        services.Configure<RefreshTokenSettings>(config.GetSection("RefreshToken"));
        return services;
    }

    /// <summary>Configures Swagger/OpenAPI with JWT security definition, tag descriptions, and XML comments.</summary>
    public static IServiceCollection AddIdentitySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "NexaCV Identity Service",
                Version = "v1",
                Description =
                    "## NexaCV — Standalone Auth & Identity Microservice\n\n" +
                    "Issues and manages **JWT Access Tokens** (15 min) and **Refresh Tokens** (7 days) " +
                    "for all NexaCV SaaS applications.\n\n" +
                    "### Token Rotation\n" +
                    "Each `/refresh` call invalidates the supplied Refresh Token and issues a fresh pair. " +
                    "Replaying a revoked token triggers **family revocation** — all active sessions for that " +
                    "user are terminated instantly.\n\n" +
                    "### Integration\n" +
                    "Client apps (e.g. `NexaCV.Api`) validate Access Tokens locally using the shared " +
                    "`Jwt:Secret`, `Jwt:Issuer`, and `Jwt:Audience` settings — no round-trip to this service.\n\n" +
                    "### Default seed user\n" +
                    "Email: `walidmoustafa1215@gmail.com` | Password: `nMxT..iwREcYJ2Y`",
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
        });

        return services;
    }
}
