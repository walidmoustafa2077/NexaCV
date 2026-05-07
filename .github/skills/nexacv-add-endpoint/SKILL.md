---
name: nexacv-add-endpoint
description: 'Add a new backend API endpoint to NexaCV. Use when adding a route, new feature endpoint, or extending an existing feature group. Covers DTO → FluentValidation → Service method → Minimal API endpoint → DI registration → Swagger docs. Triggers: "add endpoint", "new route", "new API", "add backend feature", "add controller".'
argument-hint: 'Describe the endpoint (e.g. "GET /api/users/{id}/activity")'
---

# NexaCV: Add Backend API Endpoint

Covers the full lifecycle for adding a new Minimal API endpoint to NexaCV.Api, following existing project conventions.

## Stack Reference
- **Framework**: ASP.NET Core 9 Minimal APIs
- **Validation**: FluentValidation (called via `validator.ValidateAndThrowAsync`)
- **Auth**: JWT via `JwtService.GetUserIdFromClaims(ctx.User)`
- **Pattern**: Repository → Service → Endpoint
- **Error shape**: `{ status, error }` or `{ status, error, details[] }` — handled by `ExceptionMiddleware`

---

## Step 1 — DTO(s)

Create request/response records in `backend/NexaCV.Api/DTOs/<Feature>/`.

```csharp
// backend/NexaCV.Api/DTOs/Resumes/MyNewRequest.cs
namespace NexaCV.Api.DTOs.Resumes;

public record MyNewRequest
{
    public required string SomeField { get; init; }
}
```

- Use `record` types with `{ get; init; }` properties.
- Response DTOs go in the same folder (e.g., `MyNewDto.cs`).

---

## Step 2 — FluentValidation Validator

Add a validator in `backend/NexaCV.Api/DTOs/<Feature>/` (colocated with the DTO).

```csharp
// backend/NexaCV.Api/DTOs/Resumes/MyNewRequestValidator.cs
using FluentValidation;

namespace NexaCV.Api.DTOs.Resumes;

public class MyNewRequestValidator : AbstractValidator<MyNewRequest>
{
    public MyNewRequestValidator()
    {
        RuleFor(x => x.SomeField).NotEmpty().MaximumLength(200);
    }
}
```

Validators are auto-discovered via `builder.Services.AddValidatorsFromAssembly(...)` — **no manual registration needed**.

---

## Step 3 — Repository (if needed)

If the endpoint needs a new data access method, add it to the appropriate interface + implementation in `backend/NexaCV.Api/Repositories/`.

```csharp
// In IResumeRepository.cs
Task<Resume?> GetByCustomCriteriaAsync(Guid userId, string criteria);

// In ResumeRepository.cs
public async Task<Resume?> GetByCustomCriteriaAsync(Guid userId, string criteria)
    => await _db.Resumes.FirstOrDefaultAsync(r => r.UserId == userId && r.SomeField == criteria);
```

---

## Step 4 — Service Method

Add the business logic method to the existing service in `backend/NexaCV.Api/Services/`.

```csharp
// In IResumeService.cs
Task<MyNewDto> DoSomethingAsync(Guid userId, MyNewRequest req);

// In ResumeService.cs
public async Task<MyNewDto> DoSomethingAsync(Guid userId, MyNewRequest req)
{
    // 1. Fetch / validate ownership
    var entity = await _resumes.GetByIdAsync(req.EntityId, userId)
        ?? throw new KeyNotFoundException("Not found.");
    
    // 2. Apply business logic
    // ...
    
    // 3. Persist
    await _resumes.UpdateAsync(entity);
    
    // 4. Return DTO
    return entity.ToMyNewDto();
}
```

---

## Step 5 — Endpoint

Add the route handler to the correct file in `backend/NexaCV.Api/Endpoints/<Feature>Endpoints.cs`.

```csharp
// Inside the existing static Map(WebApplication app) method:

group.MapPost("/something", async (
    MyNewRequest req,
    IValidator<MyNewRequest> validator,
    JwtService jwt,
    IResumeService resumeService,
    HttpContext ctx) =>
{
    await validator.ValidateAndThrowAsync(req);           // 422 on failure
    var userId = jwt.GetUserIdFromClaims(ctx.User);
    var result = await resumeService.DoSomethingAsync(userId, req);
    return Results.Ok(result);
})
.WithName("DoSomething")
.WithSummary("One-line summary")
.WithDescription("Detailed description of what the endpoint does, preconditions, and behavior.")
.Produces<MyNewDto>(200)
.ProducesProblem(401)
.ProducesProblem(422);
```

**Auth decisions**:
- `.RequireAuthorization()` is set on the group — all routes in the group require JWT by default.
- For public endpoints, use a **new group** without `.RequireAuthorization()`.

---

## Step 6 — Register in Program.cs

In `backend/NexaCV.Api/Program.cs`, call `Map()` on the new or existing endpoint class:

```csharp
// Only needed if adding a brand-new feature file:
FeatureEndpoints.Map(app);
```

Existing feature groups (Auth, Resumes, Templates, Transactions, Users, Webhooks) are already registered.

---

## Step 7 — Mapping Extension (if new DTO)

Add a `ToMyNewDto()` extension method to `backend/NexaCV.Api/Extensions/` or colocate it with the model.

```csharp
// backend/NexaCV.Api/Extensions/ResumeExtensions.cs (add to existing file)
public static MyNewDto ToMyNewDto(this Resume resume) => new()
{
    Id = resume.Id,
    SomeField = resume.SomeField
};
```

---

## Checklist

- [ ] DTO record created in correct `DTOs/<Feature>/` folder
- [ ] FluentValidation validator added (colocated with DTO)
- [ ] Repository interface + implementation updated (if new data access needed)
- [ ] Service interface + implementation updated
- [ ] Endpoint handler added to `<Feature>Endpoints.cs` with Swagger metadata
- [ ] `.WithName()`, `.WithSummary()`, `.WithDescription()` present
- [ ] `.Produces<T>()` and `.ProducesProblem()` declarations complete
- [ ] `Map()` called in `Program.cs` (if new feature file)
- [ ] Mapping extension added (if new response DTO)
- [ ] Tests written (use the `nexacv-write-tests` skill)
