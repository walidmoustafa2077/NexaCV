# NexaCV Backend — Implementation Reference

> **Build status:** ✅ 0 errors, 0 warnings  
> **Runtime:** .NET 9  
> **Persistence:** EF Core 9 InMemory  
> **Auth:** JWT HS256 (24 h TTL)  
> **Swagger UI:** `/swagger` (Development only)

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [NuGet Packages](#2-nuget-packages)
3. [Configuration & Settings](#3-configuration--settings)
4. [Domain Models](#4-domain-models)
5. [Enumerations](#5-enumerations)
6. [Data Layer](#6-data-layer)
7. [Repositories](#7-repositories)
8. [DTOs](#8-dtos)
9. [Mapping Extensions](#9-mapping-extensions)
10. [Services](#10-services)
11. [Endpoints](#11-endpoints)
12. [Middleware](#12-middleware)
13. [Swagger / OpenAPI](#13-swagger--openapi)
14. [API Endpoint Summary](#14-api-endpoint-summary)
15. [Authentication Flow](#15-authentication-flow)
16. [Resume Lifecycle](#16-resume-lifecycle)
17. [Payment Flow](#17-payment-flow)
18. [Stub Implementations](#18-stub-implementations)
19. [Error Handling](#19-error-handling)
20. [Known Limitations & Next Steps](#20-known-limitations--next-steps)

---

## High-Level Architecture

![NexaCV High-Level Architecture Component Diagram](../images/backend/High-Level%20Architecture%20Component%20Diagram.png)

---

## 1. Project Structure

```
NexaCV.sln
└── backend/
    └── NexaCV.Api/
        ├── NexaCV.Api.csproj
        ├── Program.cs
        ├── appsettings.json
        ├── appsettings.Development.json
        ├── .env.example
        ├── Data/
        │   ├── AppDbContext.cs
        │   └── DataSeeder.cs
        ├── DTOs/
        │   ├── Auth/
        │   │   ├── AuthResponse.cs
        │   │   ├── LoginRequest.cs
        │   │   └── RegisterRequest.cs
        │   ├── Resumes/
        │   │   ├── CreateResumeRequest.cs
        │   │   ├── RegenerateRequest.cs
        │   │   ├── RegenerateResponse.cs
        │   │   ├── ResumeDetailDto.cs
        │   │   └── ResumeSummaryDto.cs
        │   ├── Templates/
        │   │   └── TemplateDto.cs
        │   ├── Transactions/
        │   │   ├── CheckoutRequest.cs
        │   │   ├── CheckoutResponse.cs
        │   │   └── TransactionDto.cs
        │   └── Users/
        │       ├── UpdateUserRequest.cs
        │       └── UserProfileDto.cs
        ├── Endpoints/
        │   ├── AuthEndpoints.cs
        │   ├── ResumeEndpoints.cs
        │   ├── TemplateEndpoints.cs
        │   ├── TransactionEndpoints.cs
        │   ├── UserEndpoints.cs
        │   └── WebhookEndpoints.cs
        ├── Enums/
        │   ├── ActionType.cs
        │   ├── PaymentStatus.cs
        │   └── ResumeStatus.cs
        ├── Extensions/
        │   └── MappingExtensions.cs
        ├── Middleware/
        │   └── ExceptionMiddleware.cs
        ├── Models/
        │   ├── Download.cs
        │   ├── Regeneration.cs
        │   ├── Resume.cs
        │   ├── ResumeHistory.cs
        │   ├── Template.cs
        │   ├── Transaction.cs
        │   ├── User.cs
        │   └── UserMovement.cs
        ├── Repositories/
        │   ├── IRepository.cs
        │   ├── EfRepository.cs
        │   ├── IUserRepository.cs              + UserRepository.cs
        │   ├── IResumeRepository.cs            + ResumeRepository.cs
        │   ├── IResumeHistoryRepository.cs     + ResumeHistoryRepository.cs
        │   ├── ITemplateRepository.cs          + TemplateRepository.cs
        │   ├── ITransactionRepository.cs       + TransactionRepository.cs
        │   ├── IRegenerationRepository.cs      + RegenerationRepository.cs
        │   ├── IDownloadRepository.cs          + DownloadRepository.cs
        │   └── IUserMovementRepository.cs      + UserMovementRepository.cs
        ├── Services/
        │   ├── IAuthService.cs    + AuthService.cs
        │   ├── IUserService.cs    + UserService.cs
        │   ├── ITemplateService.cs + TemplateService.cs
        │   ├── IResumeService.cs  + ResumeService.cs
        │   ├── IRegenerationService.cs + RegenerationService.cs
        │   ├── ITransactionService.cs + TransactionService.cs
        │   ├── IAiService.cs      + StubAiService.cs
        │   ├── JwtService.cs
        │   ├── CustomExceptions.cs
        │   └── Payment/
        │       ├── IPaymentGateway.cs
        │       ├── StubPaymentGateway.cs
        │       └── PaymentGatewayFactory.cs
        ├── Settings/
        │   ├── AiServiceSettings.cs
        │   ├── JwtSettings.cs
        │   └── PaymentSettings.cs
        └── Swagger/
            └── TagDescriptionsDocumentFilter.cs
```

**Total source files:** 79

---

## 2. NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `BCrypt.Net-Next` | 4.1.0 | Password hashing (work factor 11) |
| `FluentValidation.AspNetCore` | 11.3.1 | Request body validation |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.0.4 | JWT middleware |
| `Microsoft.AspNetCore.OpenApi` | 9.0.13 | OpenAPI scaffolding |
| `Microsoft.EntityFrameworkCore.InMemory` | 9.0.4 | In-memory persistence |
| `Swashbuckle.AspNetCore` | 6.9.0 | Swagger UI + JSON spec generation |
| `System.IdentityModel.Tokens.Jwt` | 8.17.0 | JWT signing / validation |

> **Note:** Swashbuckle 6.x was chosen over 10.x due to a breaking `AddSwaggerGen` API change introduced in 10.x that is incompatible with the `IServiceCollection` extension pattern used in .NET 9 Minimal APIs.

---

## 3. Configuration & Settings

### `appsettings.json` sections

```json
{
  "Jwt": {
    "Secret": "<min-32-char-secret>",
    "Issuer": "nexacv-api",
    "Audience": "nexacv-client",
    "ExpiresInSeconds": 86400
  },
  "AiService": {
    "ApiKey": "",
    "Model": "gpt-4o",
    "TimeoutSeconds": 30
  },
  "Payment": {
    "DefaultGateway": "stub"
  }
}
```

### Settings classes

| Class | Bound section | Properties |
|---|---|---|
| `JwtSettings` | `Jwt` | `Secret`, `Issuer`, `Audience`, `ExpiresInSeconds` |
| `AiServiceSettings` | `AiService` | `ApiKey`, `Model`, `TimeoutSeconds` |
| `PaymentSettings` | `Payment` | `DefaultGateway` |
| `CurrencyServiceSettings` | `CurrencyService` | `CacheDurationHours`, `StubRates` (dictionary) |

All three are registered via `builder.Services.Configure<T>` and injected as `IOptions<T>`.

---

## 4. Domain Models

### `User`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `FullName` | `string` | |
| `Username` | `string` | Unique |
| `Email` | `string` | Unique |
| `PasswordHash` | `string` | BCrypt hash |
| `PhoneNumber` | `string?` | |
| `DateOfBirth` | `DateOnly?` | |
| `ProfilePictureUrl` | `string?` | |
| `LastLogin` | `DateTime?` | Updated on login |
| `CreatedAt` | `DateTime` | UTC |
| `Resumes` | `ICollection<Resume>` | Nav |
| `Movements` | `ICollection<UserMovement>` | Nav |

### `UserMovement`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `UserId` | `Guid` | FK → User |
| `ActionType` | `ActionType` (enum) | Stored as string |
| `IpAddress` | `string?` | Captured from connection |
| `UserAgent` | `string?` | Captured from headers |
| `CreatedAt` | `DateTime` | UTC |

### `Template`

| Property | Type | Notes |
|---|---|---|
| `Id` | `int` | Auto-increment PK (`ValueGeneratedOnAdd`) |
| `Name` | `string` | |
| `IndustryCategory` | `string` | e.g. `Corporate`, `Creative` |
| `ThumbnailUrl` | `string?` | |
| `BasePriceUsd` | `decimal` | **Single source of truth for pricing (USD).** Local prices are calculated at checkout via `ICurrencyService`. |
| `SupportsWord` | `bool` | Controls DOCX download |
| `IsActive` | `bool` | Soft visibility flag |

### `Resume`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `UserId` | `Guid` | FK → User |
| `TemplateId` | `int` | FK → Template |
| `RawData` | `string` | Original wizard JSON |
| `FinalData` | `string?` | AI-produced (or manually edited) JSON |
| `AiAvailable` | `bool` | Whether AI was reachable; persisted from generation result |
| `Status` | `ResumeStatus` (enum) | `Draft` → `Completed` → `Paid` |
| `CreatedAt` | `DateTime` | UTC |
| `UpdatedAt` | `DateTime?` | Set on `PUT /resumes/{id}` |
| `IsDeleted` | `bool` | Soft-delete flag |
| `Template` | `Template` | Nav |
| `Regenerations` | `ICollection<Regeneration>` | Nav |
| `Transactions` | `ICollection<Transaction>` | Nav |
| `Downloads` | `ICollection<Download>` | Nav |
| `History` | `ICollection<ResumeHistory>` | Nav — audit trail |

### `ResumeHistory`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `ResumeId` | `Guid` | FK → Resume |
| `SnapshotData` | `string` | Full `FinalData` JSON captured at event time |
| `Reason` | `string` | `INITIAL_AI_GEN`, `REGEN_{SectionIdentifier}`, `MANUAL_EDIT` |
| `CreatedAt` | `DateTime` | UTC |

**Global EF query filter:** `modelBuilder.Entity<Resume>().HasQueryFilter(r => !r.IsDeleted)`

### `Regeneration`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `ResumeId` | `Guid` | FK → Resume |
| `SectionIdentifier` | `string` | Key within `finalData` (e.g. `SUMMARY`) |
| `UserPrompt` | `string` | User's edit prompt |
| `CostUsd` | `decimal` | Fixed: **$0.25 USD** — local currency cost is derived at checkout via `ICurrencyService` |
| `CreatedAt` | `DateTime` | UTC |

### `Transaction`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `ResumeId` | `Guid` | FK → Resume |
| `UserId` | `Guid` | FK → User |
| `BaseAmount` | `decimal` | Template price converted to target currency at checkout |
| `RegenAmount` | `decimal` | Total regen cost converted to target currency at checkout |
| `TotalAmount` | `decimal` | `BaseAmount + RegenAmount` |
| `Currency` | `string` | ISO 4217 code (`EGP`, `USD`, `EUR`, …) |
| `ExchangeRateUsed` | `decimal` | **USD → target rate captured at checkout time. Stored for financial auditing / dispute resolution.** |
| `PaymentStatus` | `PaymentStatus` (enum) | Stored as string |
| `GatewayRefId` | `string?` | External gateway reference |
| `CreatedAt` | `DateTime` | UTC |
| `CompletedAt` | `DateTime?` | Set by webhook fulfillment |

### `Download`

| Property | Type | Notes |
|---|---|---|
| `Id` | `Guid` | PK |
| `ResumeId` | `Guid` | FK → Resume |
| `UserId` | `Guid` | FK → User |
| `Format` | `string` | `pdf` or `docx` |
| `IpAddress` | `string?` | |
| `CreatedAt` | `DateTime` | UTC |

---

## 5. Enumerations

All enums are stored as their **string name** in the database (`.HasConversion<string>()`).

### `ResumeStatus`
```csharp
Draft, Completed, Paid
```

### `PaymentStatus`
```csharp
Pending, Success, Failed
```

### `ActionType`
```csharp
Login, Logout, PasswordUpdated
```

---

## 6. Data Layer

### `AppDbContext`

- Inherits `DbContext`
- DbSets: `Users`, `UserMovements`, `Templates`, `Resumes`, `Regenerations`, `Transactions`, `Downloads`, `ResumeHistories`
- `OnModelCreating`:
  - All enums → `.HasConversion<string>()`
  - `Template.Id` → `ValueGeneratedOnAdd()` (auto-increment)
  - `Resume` → `HasQueryFilter(r => !r.IsDeleted)`

### Entity-Relationship Diagram

![NexaCV Entity-Relationship (ER) Diagram](../images/backend/Entity-Relationship%20%28ER%29%20Diagram.png)

### `DataSeeder`

Seeds three templates on startup if `Templates` table is empty:

| # | Name | Category | EGP | USD | SupportsWord |
|---|---|---|---|---|---|
| 1 | Modern Minimalist | Corporate | 120 | 3.00 | ✅ |
| 2 | Creative | Creative | 120 | 3.00 | ❌ |
| 3 | Executive | Corporate | 150 | 3.75 | ✅ |

Called from `Program.cs` after the pipeline is built:
```csharp
using var scope = app.Services.CreateScope();
await DataSeeder.SeedAsync(scope.ServiceProvider);
```

### `ResumeHistory` — Retention Policy

After every `AddAsync`, `IResumeHistoryRepository.PruneAsync(resumeId, maxSnapshots: 10)` is called. This deletes the oldest snapshots beyond the newest 10, keeping the table bounded. The limit is configurable at the call site — pass a different value when calling `PruneAsync` if a per-resume or per-tier limit is needed.

---

## 7. Repositories

### Generic base

```csharp
IRepository<T>
  GetByIdAsync(id)
  GetAllAsync()
  AddAsync(entity)
  Update(entity)
  Delete(entity)
  SaveChangesAsync()
```

Implemented by `EfRepository<T> : IRepository<T>`.

### Specialised repositories

| Interface | Concrete | Key methods beyond base |
|---|---|---|
| `IUserRepository` | `UserRepository` | `GetByEmailAsync`, `GetByUsernameAsync`, `ExistsAsync(email, username)` |
| `IResumeRepository` | `ResumeRepository` | `GetAllByUserAsync`, `GetByIdWithDetailsAsync` (eager-loads Template, Regenerations) |
| `ITemplateRepository` | `TemplateRepository` | `GetAllActiveAsync(industryCategory?)` |
| `ITransactionRepository` | `TransactionRepository` | `GetByGatewayRefIdAsync`, `GetByIdWithResumeAsync` |
| `IRegenerationRepository` | `RegenerationRepository` | `CountBySectionAsync(resumeId, sectionIdentifier)`, `GetUsdCostSumAsync(resumeId)` |
| `IResumeHistoryRepository` | `ResumeHistoryRepository` | `GetByResumeIdAsync(resumeId)`, `PruneAsync(resumeId, maxSnapshots = 10)` |
| `IDownloadRepository` | `DownloadRepository` | `AddDownloadAsync` |
| `IUserMovementRepository` | `UserMovementRepository` | `AddMovementAsync` |

---

## 8. DTOs

### Auth

#### `RegisterRequest`
```csharp
string FullName       // required
string Username       // required, 3–30 chars, alphanumeric+underscore
string Email          // required, valid email
string Password       // required, ≥8 chars, ≥1 special char
string? PhoneNumber
DateOnly? DateOfBirth
string? ProfilePictureUrl
```
Validated by `RegisterRequestValidator` (FluentValidation).

#### `LoginRequest`
```csharp
string Email     // required
string Password  // required
```
Validated by `LoginRequestValidator`.

#### `AuthResponse`
```csharp
string Token       // signed JWT
DateTime ExpiresAt // token expiry (UTC)
UserProfileDto User
```

---

### Users

#### `UserProfileDto`
```csharp
Guid Id
string FullName
string Username
string Email
string? PhoneNumber
DateOnly? DateOfBirth
string? ProfilePictureUrl
DateTime? LastLogin
DateTime CreatedAt
```

#### `UpdateUserRequest`
```csharp
string? FullName
string? PhoneNumber
DateOnly? DateOfBirth
string? ProfilePictureUrl
string? Password   // if set: re-hashed + movement logged
```

---

### Templates

#### `TemplateDto`
```csharp
int Id
string Name
string IndustryCategory
string? ThumbnailUrl
decimal BasePriceUsd   // single base price in USD; local price shown via ICurrencyService at checkout
bool SupportsWord
```

---

### Resumes

#### `CreateResumeRequest`
```csharp
int TemplateId
string RawData   // JSON string from wizard form
```

#### `ResumeSummaryDto`
```csharp
Guid Id
int TemplateId
string TemplateName
string Status      // enum as string
DateTime CreatedAt
DateTime? UpdatedAt
```

#### `ResumeDetailDto`
```csharp
Guid Id
int TemplateId
string TemplateName
string RawData
string? FinalData
bool AiAvailable
string Status
DateTime CreatedAt
DateTime? UpdatedAt
int RegenerationCount
decimal TotalRegenCostEgp
decimal TotalRegenCostUsd
```

#### `RegenerateRequest`
```csharp
string SectionIdentifier   // key within finalData (e.g. "SUMMARY")
string Prompt              // edit instruction
```

#### `RegenerateResponse`
```csharp
string SectionIdentifier
string? UpdatedContent    // AI output
bool AiAvailable
int RegenCountUsed        // total regens used for this section
int RegenCountRemaining   // 3 - RegenCountUsed
decimal AddedCostUsd      // always $0.25; local currency cost derived at checkout
```

---

### Transactions

#### `CheckoutRequest`
```csharp
Guid ResumeId
string Currency    // "EGP" or "USD"
```

#### `CheckoutResponse`
```csharp
Guid TransactionId
string PaymentUrl      // redirect URL from gateway
decimal BaseAmount     // in target currency
decimal RegenAmount    // in target currency
decimal TotalAmount    // in target currency
string Currency        // ISO 4217 code
decimal ExchangeRateUsed  // USD → target rate applied at checkout
```

#### `TransactionDto`
```csharp
Guid Id
Guid ResumeId
decimal TotalAmount
string Currency
decimal ExchangeRateUsed  // audit field
string PaymentStatus
DateTime CreatedAt
DateTime? CompletedAt
```

---

## 9. Mapping Extensions

`Extensions/MappingExtensions.cs` — all static extension methods, no AutoMapper dependency.

| Method | From | To |
|---|---|---|
| `ToProfileDto()` | `User` | `UserProfileDto` |
| `ToTemplateDto()` | `Template` | `TemplateDto` |
| `ToSummaryDto()` | `Resume` | `ResumeSummaryDto` |
| `ToDetailDto(regenCount, costEgp, costUsd)` | `Resume` | `ResumeDetailDto` |
| `ToTransactionDto()` | `Transaction` | `TransactionDto` |

---

## 10. Services

### `JwtService` (Singleton)

Responsibilities:
- `GenerateToken(user)` — creates HS256 JWT with claims `sub` (userId), `email`, `jti` (new Guid)
- `GetUserIdFromClaims(ClaimsPrincipal)` — extracts and parses `sub` claim

### `IAuthService` / `AuthService`

| Method | Description |
|---|---|
| `RegisterAsync(req, ip, ua)` | Validates uniqueness (throws `ConflictException`), hashes password, saves user, logs `Login` movement, returns JWT |
| `LoginAsync(req, ip, ua)` | Checks credentials (throws `UnauthorizedAccessException` on failure), updates `lastLogin`, logs `Login` movement, returns JWT |
| `LogoutAsync(userId)` | Logs `Logout` movement only (JWT stateless — client discards) |

### `IUserService` / `UserService`

| Method | Description |
|---|---|
| `GetProfileAsync(userId)` | Returns `UserProfileDto` |
| `UpdateProfileAsync(userId, req)` | Applies non-null fields; if `req.Password` set: re-hashes + logs `PasswordUpdated` movement |

### `ITemplateService` / `TemplateService`

| Method | Description |
|---|---|
| `GetAllAsync(industryCategory?)` | Returns all active templates, optionally filtered |
| `GetByIdAsync(id)` | Returns single active template; throws `KeyNotFoundException` if not found |

### `IResumeService` / `ResumeService`

| Method | Description |
|---|---|
| `CreateAsync(userId, req)` | Validates template exists, calls `IAiService.GenerateAsync`, stores resume as `Completed`, persists `AiAvailable`, inserts `ResumeHistory` (`INITIAL_AI_GEN`), prunes history, returns detail DTO |
| `GetAllByUserAsync(userId)` | Returns summary list (soft-deleted excluded by EF filter) |
| `GetByIdAsync(id, userId)` | Loads with details, checks ownership (403) |
| `UpdateFinalDataAsync(id, userId, finalData)` | Sets `FinalData`, sets `UpdatedAt`, checks ownership, inserts `ResumeHistory` (`MANUAL_EDIT`), prunes history — **every write to `FinalData` is captured, including manual edits** |
| `DeleteAsync(id, userId)` | Checks ownership + not `Paid`; sets `IsDeleted = true` |
| `GetForDownloadAsync(id, userId, format, ip)` | Validates ownership + `Paid` status; validates `SupportsWord` for `docx`; records `Download` row |

### `IRegenerationService` / `RegenerationService`

| Method | Description |
|---|---|
| `RegenerateAsync(id, userId, req)` | Checks ownership (403); counts existing regens for section; throws `TooManyRegenerationsException` (429) if ≥ 3; calls `IAiService.RegenerateAsync` passing **current `FinalData`** (including any manual edits) as context; patches section in `FinalData` JSON; inserts `ResumeHistory` (`REGEN_{SectionIdentifier}`); prunes history; persists `Regeneration`; returns response |

**Context passed to AI for regeneration:** The full `FinalData` JSON (including any prior manual edits) is forwarded to `IAiService.RegenerateAsync`. A real implementation should extract `Summary`, `Skills`, and `Title` from it to build a contextual system prompt:
> *"You are an expert resume editor. User wants to improve: `{SectionIdentifier}`. Context: Use this Professional Summary: `{Summary}`. Keep these skills in mind: `{Skills}`. Constraint: Ensure output is consistent with the current resume title: `{Title}`."*

> **Note (Courses/Certifications):** The AI system prompt for `GenerateAsync` should also explicitly address the `courses` / `certifications` array in `RawData`. Recommended wording: *"Include a Certifications & Courses section; extract it from the `courses` array in the input. Treat it as evidence of continuous learning."* Make `courses` a first-class step (Step 4) in the frontend wizard — recruiters weigh it heavily.

**Regeneration pricing:**
- EGP: 10.00 per call
- USD: 0.25 per call

**Section cap:** 3 regenerations per `(resumeId, sectionIdentifier)` pair.

### `ITransactionService` / `TransactionService`

| Method | Description |
|---|---|
| `CheckoutAsync(resumeId, userId, currency)` | Validates resume is `Completed`; calls `ICurrencyService.GetExchangeRateAsync(currency)` to get live USD→target rate; multiplies `Template.BasePriceUsd` and total regen cost (USD) by the rate; stores `ExchangeRateUsed` on the transaction; creates `Pending` transaction; calls gateway `CreatePaymentSession`; returns `CheckoutResponse` with all amounts in the target currency |
| `GetByIdAsync(id, userId)` | Loads transaction, checks ownership (403) |
| `FulfillAsync(gatewayRefId)` | Called by webhook; finds transaction by gateway ref; sets `Success`, `CompletedAt`; transitions resume to `Paid` |

### `IAiService` / `StubAiService`

```csharp
Task<AiGenerationResult> GenerateAsync(string rawDataJson)
Task<AiRegenerationResult> RegenerateAsync(string sectionId, string userPrompt, string currentFinalDataJson)
```

Stub behaviour:
- `GenerateAsync` returns `(rawDataJson, AiAvailable: false)` — mirrors input unchanged
- `RegenerateAsync` returns `(userPrompt, AiAvailable: false)`

### `ICurrencyService` / `StubCurrencyService`

```csharp
Task<decimal> GetExchangeRateAsync(string targetCurrency)  // returns USD → targetCurrency rate
```

Stub behaviour:
- Reads hardcoded rates from `CurrencyServiceSettings.StubRates` (default: USD 1.0, EGP 50.0, EUR 0.92, GBP 0.79, SAR 3.75, AED 3.67)
- Results are cached in `IMemoryCache` for `CacheDurationHours` (default 1 h) to avoid repeated dictionary lookups and to mirror production cache TTL behaviour
- Throws `InvalidOperationException` for unsupported currency codes

**To wire a real provider:** implement `ICurrencyService` against ExchangeRate-API or Fixer.io and swap the DI registration:
```csharp
// Replace stub
builder.Services.AddScoped<ICurrencyService, ExchangeRateApiService>(); // implement ICurrencyService
```

### `IPaymentGateway` / `StubPaymentGateway`

```csharp
string GatewayName { get; }
bool MatchesCurrency(string currency)
bool MatchesWebhookRequest(HttpRequest request)
Task<string> CreatePaymentSession(Guid transactionId, decimal amount, string currency)
bool VerifyWebhookSignature(HttpRequest, out string? eventType, out string? gatewayRefId)
```

Stub behaviour:
- `GatewayName` = `"stub"`
- `MatchesCurrency` = always `true`
- `CreatePaymentSession` returns `http://localhost:5000/stub-pay/{transactionId}`
- `VerifyWebhookSignature` reads `X-Stub-Ref` header; sets `gatewayRefId` to that value; returns `true`

### `PaymentGatewayFactory` (Scoped)

- `ResolveByCurrency(currency)` — returns first gateway where `MatchesCurrency(currency)` is true
- `ResolveByRequest(HttpRequest)` — returns first gateway where `MatchesWebhookRequest(request)` is true
- Throws `InvalidOperationException` if no match found

---

## 11. Endpoints

All endpoints live in `NexaCV.Api.Endpoints` and are registered in `Program.cs` via static `Map(WebApplication)` methods. Every endpoint is decorated with `.WithName()`, `.WithSummary()`, `.WithDescription()`, `.Produces<T>()`, and `.ProducesProblem()` for full Swagger documentation.

### Auth — `/api/auth`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `POST` | `/api/auth/register` | ❌ | Register a new user |
| `POST` | `/api/auth/login` | ❌ | Login with email and password |
| `POST` | `/api/auth/logout` | ✅ | Logout the current user |

### Users — `/api/users`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `GET` | `/api/users/me` | ✅ | Get the current user's profile |
| `PUT` | `/api/users/me` | ✅ | Update the current user's profile |

### Templates — `/api/templates`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `GET` | `/api/templates` | ❌ | List all active templates |
| `GET` | `/api/templates/{id}` | ❌ | Get a single template by ID |

### Resumes — `/api/resumes`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `POST` | `/api/resumes` | ✅ | Create a new resume |
| `GET` | `/api/resumes` | ✅ | List all resumes for the current user |
| `GET` | `/api/resumes/{id}` | ✅ | Get a single resume by ID |
| `PUT` | `/api/resumes/{id}` | ✅ | Replace the resume's `finalData` JSON |
| `DELETE` | `/api/resumes/{id}` | ✅ | Soft-delete a resume |
| `POST` | `/api/resumes/{id}/regenerate` | ✅ | Regenerate a single resume section with AI |
| `GET` | `/api/resumes/{id}/download` | ✅ | Download a paid resume as PDF or DOCX *(501)* |

### Transactions — `/api/transactions`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `POST` | `/api/transactions/checkout` | ✅ | Initiate payment checkout for a resume |
| `GET` | `/api/transactions/{id}` | ✅ | Get a transaction by ID |

### Webhooks — `/api/webhooks`

| Method | Path | Auth | Summary |
|---|---|---|---|
| `POST` | `/api/webhooks/payment` | ❌ (gateway sig) | Inbound payment gateway webhook |

---

## 12. Middleware

### `ExceptionMiddleware`

Global exception handler registered first in the pipeline. Maps exceptions to structured JSON problem responses:

| Exception | HTTP Status |
|---|---|
| `KeyNotFoundException` | 404 Not Found |
| `UnauthorizedAccessException` | 401 Unauthorized |
| `ConflictException` | 409 Conflict |
| `TooManyRegenerationsException` | 429 Too Many Requests |
| `ValidationException` (FluentValidation) | 422 Unprocessable Entity (with `details` array) |
| Any other `Exception` | 500 Internal Server Error |

Error response shape:
```json
{
  "error": "Human-readable message",
  "details": ["field: message"]   // present for 422 only
}
```

---

## 13. Swagger / OpenAPI

### Configuration (in `Program.cs`)

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexaCV API",
        Version = "v1",
        Description = "AI-powered resume builder API ...",
        Contact = new OpenApiContact { Name = "NexaCV Team", Email = "api@nexacv.io" },
        License  = new OpenApiLicense { Name = "Proprietary" }
    });

    // XML comments from all public members
    c.IncludeXmlComments(xmlPath);

    // Descriptive tag groups
    c.DocumentFilter<TagDescriptionsDocumentFilter>();

    // Avoid schema collision on same-named nested types
    c.CustomSchemaIds(type => type.FullName);

    // JWT Bearer security definition
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { ... });
});
```

Swagger UI options:
```csharp
app.UseSwaggerUI(options =>
{
    options.DisplayRequestDuration();
    options.EnableFilter();
    options.EnableDeepLinking();
});
```

### `TagDescriptionsDocumentFilter`

Adds human-readable descriptions to each tag group visible in the Swagger UI sidebar:

| Tag | Description |
|---|---|
| Auth | Registration, login, and logout endpoints |
| Users | Authenticated user profile management |
| Templates | Resume template catalog |
| Resumes | Resume creation, editing, regeneration, and download |
| Transactions | Payment checkout and status polling |
| Webhooks | Inbound callbacks from payment gateways |

### XML Documentation

- `<GenerateDocumentationFile>true</GenerateDocumentationFile>` is set in the `.csproj`
- `<NoWarn>$(NoWarn);1591</NoWarn>` suppresses missing-doc warnings for internal members
- All public DTOs, endpoint records, and request/response models have `/// <summary>` + `/// <example>` doc comments

---

## 14. API Endpoint Summary

### POST `/api/auth/register`

**Request:**
```json
{
  "fullName": "Ahmed Samy",
  "username": "ahmed_samy",
  "email": "ahmed@example.com",
  "password": "P@ssword1",
  "phoneNumber": "+201001234567",
  "dateOfBirth": "1995-06-15"
}
```

**Response 201:**
```json
{
  "token": "<jwt>",
  "expiresAt": "2025-01-20T12:00:00Z",
  "user": { "id": "...", "fullName": "Ahmed Samy", ... }
}
```

**Errors:** `409` email/username taken · `422` validation failure

---

### POST `/api/auth/login`

**Request:**
```json
{ "email": "ahmed@example.com", "password": "P@ssword1" }
```

**Response 200:** Same shape as `AuthResponse` above.

**Errors:** `401` wrong credentials · `422` validation failure

---

### POST `/api/auth/logout`  *(Bearer required)*

**Response 204 No Content**

---

### GET `/api/users/me`  *(Bearer required)*

**Response 200:** `UserProfileDto`

---

### PUT `/api/users/me`  *(Bearer required)*

**Request:**
```json
{ "fullName": "New Name", "password": "N3wP@ss!" }
```

**Response 200:** Updated `UserProfileDto`

---

### GET `/api/templates?industryCategory=Corporate`

**Response 200:** `TemplateDto[]`

---

### GET `/api/templates/{id}`

**Response 200:** `TemplateDto`  
**Errors:** `404` not found

---

### POST `/api/resumes`  *(Bearer required)*

**Request:**
```json
{
  "templateId": 1,
  "rawData": "{\"name\":\"Ahmed\",\"summary\":\"Senior Developer\", ...}"
}
```

**Response 200:** `ResumeDetailDto` (with `aiAvailable: false` in stub mode)

---

### GET `/api/resumes`  *(Bearer required)*

**Response 200:** `ResumeSummaryDto[]`

---

### GET `/api/resumes/{id}`  *(Bearer required)*

**Response 200:** `ResumeDetailDto`  
**Errors:** `403` wrong user · `404` not found

---

### PUT `/api/resumes/{id}`  *(Bearer required)*

**Request:**
```json
{ "finalData": "{...updated json...}" }
```

**Response 200:** `ResumeDetailDto`

---

### DELETE `/api/resumes/{id}`  *(Bearer required)*

**Response 204 No Content**  
**Errors:** `400` resume is `PAID` · `403` wrong user

---

### POST `/api/resumes/{id}/regenerate`  *(Bearer required)*

**Request:**
```json
{ "sectionIdentifier": "SUMMARY", "prompt": "Make it more concise" }
```

**Response 200:**
```json
{
  "sectionIdentifier": "SUMMARY",
  "resultContent": "[Stub] SUMMARY: Make it more concise",
  "aiAvailable": false,
  "regenerationCount": 1,
  "addedCostEgp": 10.00,
  "addedCostUsd": 0.25
}
```

**Errors:** `403` wrong user · `429` section regeneration limit (3) exceeded

---

### POST `/api/transactions/checkout`  *(Bearer required)*

**Request:**
```json
{ "resumeId": "...", "currency": "EGP" }
```

**Response 200:**
```json
{
  "transactionId": "...",
  "paymentUrl": "http://localhost:5000/stub-pay/...",
  "baseAmount": 120.00,
  "regenAmount": 20.00,
  "totalAmount": 140.00,
  "currency": "EGP"
}
```

**Errors:** `400` resume not `COMPLETED` · `403` wrong user

---

### GET `/api/transactions/{id}`  *(Bearer required)*

**Response 200:** `TransactionDto`  
**Errors:** `403` wrong user · `404` not found

---

### POST `/api/webhooks/payment`

**Headers (stub):** `X-Stub-Ref: <transactionId>`

**Response 200:** `{}`  
**Errors:** `400` unrecognised gateway or invalid signature

---

## 15. Authentication Flow

![Authentication Flow Sequence Diagram](../images/backend/Authentication%20Flow%20Sequence%20Diagram.png)

**JWT Claims:**
- `sub` — user GUID
- `email` — user email
- `jti` — unique token identifier (new GUID per issuance)
- `iss` / `aud` — from `JwtSettings`
- `exp` — `now + ExpiresInSeconds` (default 86400 s = 24 h)

---

## 16. Resume Lifecycle

![Resume Lifecycle State Diagram](../images/backend/Resume%20Lifecycle%20State%20Diagram.png)

**Constraints:**
- Cannot delete a `PAID` resume
- Cannot checkout a `DRAFT` or `PAID` resume
- Maximum 3 AI regenerations per `(resumeId, sectionIdentifier)` pair

---

## 17. Payment Flow

![Payment Flow Sequence Diagram](../images/backend/Payment%20Flow%20Sequence%20Diagram.png)

---

## 18. Stub Implementations

Both stub services return `aiAvailable: false` / `false` and are registered as the concrete types in `Program.cs`. They are designed to be drop-in replaced by real implementations by swapping the DI registrations:

```csharp
// Current (stub)
builder.Services.AddScoped<IAiService, StubAiService>();
builder.Services.AddScoped<ICurrencyService, StubCurrencyService>();
builder.Services.AddScoped<IPaymentGateway, StubPaymentGateway>();

// Replace with real implementations
builder.Services.AddScoped<IAiService, OpenAiService>();              // implement IAiService
builder.Services.AddScoped<ICurrencyService, ExchangeRateApiService>(); // implement ICurrencyService
builder.Services.AddSingleton<IPaymentGateway, PaymobGateway>();     // implement IPaymentGateway
```

---

## 19. Error Handling

All errors from the API follow a consistent JSON shape (produced by `ExceptionMiddleware`):

```json
{
  "error": "Human-readable description of the problem"
}
```

Validation failures (422) include a `details` array:

```json
{
  "error": "Validation failed",
  "details": [
    "Email: 'Email' is not a valid email address.",
    "Password: Password must be at least 8 characters."
  ]
}
```

### HTTP Status Code Reference

| Status | When |
|---|---|
| 200 | Successful GET / POST (non-create) |
| 201 | Successful resource creation |
| 204 | Successful delete or logout |
| 400 | Bad request (invalid business state) |
| 401 | Missing/invalid JWT or wrong credentials |
| 403 | Authenticated but not the owner of the resource |
| 404 | Resource not found (or soft-deleted) |
| 409 | Email or username already registered |
| 422 | FluentValidation failure |
| 429 | AI regeneration limit exceeded |
| 501 | Download endpoint not yet fully implemented |
| 500 | Unhandled server error |

---

## 20. Known Limitations & Next Steps

### Current Limitations

| Area | Limitation |
|---|---|
| Persistence | EF Core InMemory — data is lost on restart |
| AI | Stub only — `finalData` mirrors `rawData`; no real LLM calls |
| Payment | Stub only — `paymentUrl` is a localhost link |
| File rendering | `GET /resumes/{id}/download` returns **501** — PDF/DOCX rendering not implemented |
| JWT revocation | Stateless JWT — logout only logs a movement; token remains valid until expiry |
| Rate limiting | No global rate limiting middleware (only per-section regeneration cap) |
| Authorization | No admin role; all endpoints are user-scoped only |

### Suggested Next Steps

1. **Replace InMemory with PostgreSQL**
   - Add `Npgsql.EntityFrameworkCore.PostgreSQL` package
   - Change `UseInMemoryDatabase` → `UseNpgsql(connectionString)`
   - Add and run EF migrations: `dotnet ef migrations add InitialCreate`

2. **Wire real AI service**
   - Implement `IAiService` against OpenAI / Azure OpenAI API
   - Use `AiServiceSettings.ApiKey`, `.Model`, `.TimeoutSeconds`
   - Register in DI replacing `StubAiService`

3. **Wire real payment gateway**
   - Implement `IPaymentGateway` for Paymob / Stripe
   - Register alongside (or replacing) `StubPaymentGateway` in `PaymentGatewayFactory`

4. **Implement PDF/DOCX rendering**
   - Add `QuestPDF` (PDF) and `DocumentFormat.OpenXml` (DOCX)
   - Return `File(bytes, contentType, filename)` from the download endpoint

5. **Add JWT refresh tokens**
   - `RefreshToken` model + `POST /auth/refresh` endpoint
   - Rotate tokens on each refresh; revoke on logout

6. **Add global rate limiting**
   - Use `Microsoft.AspNetCore.RateLimiting` (built into .NET 7+)
   - Apply fixed/sliding window per IP on auth endpoints

7. **Add integration tests**
   - Use `WebApplicationFactory<Program>` with `UseInMemoryDatabase`
   - Cover all happy paths and the key error branches

8. **CI/CD & Docker**
   - Add `Dockerfile` (multi-stage: `sdk` → `aspnet`)
   - GitHub Actions workflow: build → test → push image
