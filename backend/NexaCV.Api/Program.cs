using System.Text.Json.Serialization;
using FluentValidation;
using NexaCV.Api.Data;
using NexaCV.Api.Endpoints;
using NexaCV.Api.Extensions;
using NexaCV.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// ── Global JSON options ───────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var config = builder.Configuration;

// ── Registration delegated to focused extension methods (SRP) ─
builder.Services.AddJwtAuthentication(config);
builder.Services.AddSwaggerDocumentation();
builder.Services.AddDataAccess(config);
builder.Services.AddAppRepositories();
builder.Services.AddAppServices();
builder.Services.AddAiServices(config);
builder.Services.AddPaymentServices(config);
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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

// Exposed for WebApplicationFactory<Program> in integration tests
public partial class Program { }
