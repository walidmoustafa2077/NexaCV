using NexaCV.Identity.Data;
using NexaCV.Identity.Endpoints;
using NexaCV.Identity.Extensions;
using NexaCV.Identity.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// ── Global JSON options ───────────────────────────────────────
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ── Service registrations ─────────────────────────────────────
builder.Services.AddTokenSettings(config);
builder.Services.AddIdentityDataAccess(config);
builder.Services.AddIdentityServices();
builder.Services.AddIdentitySwagger();

builder.Services.AddCors(opt =>
    opt.AddPolicy("allow-all-dev", p =>
        p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ═════════════════════════════════════════════════════════════
var app = builder.Build();
// ═════════════════════════════════════════════════════════════

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NexaCV Identity Service v1");
        c.DocumentTitle = "NexaCV Identity Service";
        c.DefaultModelsExpandDepth(2);
        c.DefaultModelExpandDepth(2);
        c.DisplayRequestDuration();
        c.EnableFilter();
        c.EnableDeepLinking();
    });

    app.UseCors("allow-all-dev");
}

app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        context.Response.ContentType = "application/json";

        var exceptionFeature = context.Features
            .Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionFeature?.Error is not { } error)
            return;

        var (status, message) = error switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, error.Message),
            InvalidOperationException => (StatusCodes.Status409Conflict, error.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, error.Message),
            SecurityException => (StatusCodes.Status401Unauthorized, error.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(new { status, error = message });
    });
});

// ── Seed DB on startup ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    await IdentitySeeder.SeedAsync(db);
}

// ── Map Endpoints ─────────────────────────────────────────────
AuthEndpoints.Map(app);

app.Run();
