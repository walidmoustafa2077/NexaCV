# NexaCV — Backend Implementation Plan & Map

**Stack:** .NET 9 Minimal API · EF Core 8 In-Memory DB · Custom JWT + BCrypt · Repository + Service pattern · SOLID
**Location:** `c:\Users\dream\Desktop\Dev\NexaCV\backend`
**AI:** `IAiService` interface + `StubAiService` scaffolded; OpenAI integration deferred
**Payment:** Generic `IPaymentGateway` abstraction; no Stripe.net — add any gateway by implementing the interface
**Mapping:** Static `MappingExtensions.cs` — no AutoMapper
**Frontend:** Deferred to next session

---

## Table of Contents

1. [Project Scaffold](#1-project-scaffold)
2. [Project Structure](#2-project-structure)
3. [Database Models](#3-database-models)
4. [AppDbContext & DataSeeder](#4-appdbcontext--dataseeder)
5. [Enums](#5-enums)
6. [DTOs](#6-dtos)
7. [Mapping Extensions](#7-mapping-extensions)
8. [Repository Layer](#8-repository-layer)
9. [Services](#9-services)
10. [AI & Payment Abstractions](#10-ai--payment-abstractions)
11. [Endpoints](#11-endpoints)
12. [Middleware](#12-middleware)
13. [Program.cs — Wiring](#13-programcs--wiring)
14. [File Map](#14-file-map)
15. [Verification Checklist](#15-verification-checklist)
16. [Open Decisions](#16-open-decisions)

---

## 1. Project Scaffold

### Solution & Project
```
dotnet new sln -n NexaCV
dotnet new webapi -n NexaCV.Api --use-minimal-apis
dotnet sln add NexaCV.Api/NexaCV.Api.csproj
```

### NuGet Packages
| Package | Purpose |
| :--- | :--- |
| `Microsoft.EntityFrameworkCore` | ORM core |
| `Microsoft.EntityFrameworkCore.InMemory` | In-memory provider (swap to Npgsql later) |
| `BCrypt.Net-Next` | Password hashing |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT middleware |
| `System.IdentityModel.Tokens.Jwt` | JWT generation |
| `FluentValidation.AspNetCore` | Request validation |
| `Swashbuckle.AspNetCore` | Swagger UI + OpenAPI spec |

> **Excluded intentionally:**
> - `Npgsql.EntityFrameworkCore.PostgreSQL` — swap in when moving to a real DB (one line in `Program.cs`)
> - `Stripe.net` — `IPaymentGateway` abstraction used instead
> - `AutoMapper` — static extension methods used instead

### Environment Variables (`.env.example`)
```
JWT_SECRET=your_super_secret_key_here
OPENAI_API_KEY=sk-...          # unused until real AI impl
```
> No `DB_PASSWORD` needed for in-memory. Add `.env` to `.gitignore` **before the first commit**.

### `.gitignore` — mandatory entries
```
.env
*.user
bin/
obj/
```

---

## 2. Project Structure

```
NexaCV.Api/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── .env.example
├── Data/
│   ├── AppDbContext.cs
│   └── DataSeeder.cs
├── Enums/
│   ├── ResumeStatus.cs
│   ├── PaymentStatus.cs
│   └── ActionType.cs
├── Models/
│   ├── User.cs
│   ├── UserMovement.cs
│   ├── Template.cs
│   ├── Resume.cs
│   ├── Regeneration.cs
│   ├── Transaction.cs
│   └── Download.cs
├── DTOs/
│   ├── Auth/
│   │   ├── RegisterRequest.cs
│   │   ├── LoginRequest.cs
│   │   └── AuthResponse.cs
│   ├── Users/
│   │   ├── UserProfileDto.cs
│   │   └── UpdateUserRequest.cs
│   ├── Templates/
│   │   └── TemplateDto.cs
│   ├── Resumes/
│   │   ├── CreateResumeRequest.cs
│   │   ├── ResumeSummaryDto.cs
│   │   ├── ResumeDetailDto.cs
│   │   ├── RegenerateRequest.cs
│   │   └── RegenerateResponse.cs
│   └── Transactions/
│       ├── CheckoutRequest.cs
│       ├── CheckoutResponse.cs
│       └── TransactionDto.cs
├── Extensions/
│   └── MappingExtensions.cs
├── Repositories/
│   ├── IRepository.cs
│   ├── EfRepository.cs
│   ├── IUserRepository.cs          + UserRepository.cs
│   ├── IResumeRepository.cs        + ResumeRepository.cs
│   ├── IRegenerationRepository.cs  + RegenerationRepository.cs
│   ├── ITransactionRepository.cs   + TransactionRepository.cs
│   ├── IUserMovementRepository.cs  + UserMovementRepository.cs
│   ├── ITemplateRepository.cs      + TemplateRepository.cs
│   └── IDownloadRepository.cs      + DownloadRepository.cs
├── Services/
│   ├── JwtService.cs
│   ├── IAuthService.cs             + AuthService.cs
│   ├── IUserService.cs             + UserService.cs
│   ├── ITemplateService.cs         + TemplateService.cs
│   ├── IResumeService.cs           + ResumeService.cs
│   ├── IRegenerationService.cs     + RegenerationService.cs
│   ├── ITransactionService.cs      + TransactionService.cs
│   ├── IAiService.cs               + StubAiService.cs
│   └── Payment/
│       ├── IPaymentGateway.cs
│       ├── PaymentGatewayFactory.cs
│       └── StubPaymentGateway.cs
├── Settings/
│   ├── JwtSettings.cs
│   ├── AiServiceSettings.cs
│   └── PaymentSettings.cs
├── Endpoints/
│   ├── AuthEndpoints.cs
│   ├── UserEndpoints.cs
│   ├── TemplateEndpoints.cs
│   ├── ResumeEndpoints.cs
│   ├── TransactionEndpoints.cs
│   └── WebhookEndpoints.cs
└── Middleware/
    └── ExceptionMiddleware.cs
```

**SOLID alignment:**
| Principle | How it's applied |
| :--- | :--- |
| **S** | Repositories = data access only. Services = business rules only. Endpoints = HTTP translation only. |
| **O** | Add a payment gateway by implementing `IPaymentGateway` — zero changes to existing code. |
| **L** | All implementations are substitutable via their interfaces. |
| **I** | `IUserRepository` extends `IRepository<T>` with only user-specific queries. |
| **D** | Endpoints depend on service interfaces; services depend on repository interfaces — never concretions. |

---

## 3. Database Models

> All models are persistence-only — no business logic. Status/enum columns use the C# enums defined in §5.

### `Models/User.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | `Guid.NewGuid()` on create |
| `FirstName` | `string` | NOT NULL, MaxLength(50) | |
| `LastName` | `string` | NOT NULL, MaxLength(50) | |
| `Username` | `string` | UNIQUE, NOT NULL, MaxLength(50) | |
| `Email` | `string` | UNIQUE, NOT NULL, MaxLength(150) | |
| `PasswordHash` | `string` | NOT NULL, MaxLength(255) | BCrypt output |
| `DateOfBirth` | `DateOnly?` | NULL | |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | Set on insert |
| `LastLogin` | `DateTime?` | NULL | **Set in `AuthService.LoginAsync`** |
| Nav | `ICollection<UserMovement>` | | |
| Nav | `ICollection<Resume>` | | |
| Nav | `ICollection<Transaction>` | | |

### `Models/UserMovement.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `UserId` | `Guid` | FK → `users(id)` | |
| `ActionType` | `ActionType` (enum) | NOT NULL, MaxLength(50) | `LOGIN / LOGOUT / PASSWORD_UPDATED` |
| `IpAddress` | `string?` | NULL, MaxLength(45) | IPv4 or IPv6 |
| `UserAgent` | `string?` | NULL | TEXT |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | |
| Nav | `User` | | |

### `Models/Template.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `int` | PK, `ValueGeneratedOnAdd()` | Auto-increment (SERIAL). Do **not** set manually in seeder. |
| `Name` | `string` | NOT NULL, MaxLength(100) | |
| `IndustryCategory` | `string?` | NULL, MaxLength(50) | |
| `BasePriceEgp` | `decimal` | NOT NULL, DECIMAL(10,2) | |
| `BasePriceUsd` | `decimal` | NOT NULL, DECIMAL(10,2) | |
| `SupportsWord` | `bool` | DEFAULT false | |
| `IsActive` | `bool` | DEFAULT true | |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | |
| Nav | `ICollection<Resume>` | | |

### `Models/Resume.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `UserId` | `Guid` | FK → `users(id)` | |
| `TemplateId` | `int` | FK → `templates(id)` | |
| `Status` | `ResumeStatus` (enum) | NOT NULL, MaxLength(20) | `DRAFT → COMPLETED → PAID` |
| `RawData` | `string?` | NULL | JSON string (JSONB on Postgres) |
| `FinalData` | `string?` | NULL | JSON string (JSONB on Postgres) |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | |
| `UpdatedAt` | `DateTime` | DEFAULT NOW() | **Must be set explicitly on every mutation** |
| `IsDeleted` | `bool` | DEFAULT false | **Soft delete flag — never hard-delete a resume** |
| Nav | `User` | | |
| Nav | `Template` | | |
| Nav | `ICollection<Regeneration>` | | |
| Nav | `Transaction?` | | |
| Nav | `ICollection<Download>` | | |

### `Models/Regeneration.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `ResumeId` | `Guid` | FK → `resumes(id)` | |
| `SectionIdentifier` | `string` | NOT NULL, MaxLength(100) | e.g. `WORK_EXP_ID_1`, `SUMMARY` |
| `UserPrompt` | `string?` | NULL | TEXT |
| `CostEgp` | `decimal` | NOT NULL, DECIMAL(10,2) | 10.00 |
| `CostUsd` | `decimal` | NOT NULL, DECIMAL(10,2) | 0.25 |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | |
| Nav | `Resume` | | |

### `Models/Transaction.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `UserId` | `Guid` | FK → `users(id)` | |
| `ResumeId` | `Guid` | FK → `resumes(id)` | |
| `BaseAmount` | `decimal` | NOT NULL, DECIMAL(10,2) | Template price at checkout time |
| `RegenAmount` | `decimal` | NOT NULL, DECIMAL(10,2) | SUM of regen costs |
| `TotalAmount` | `decimal` | NOT NULL, DECIMAL(10,2) | base + regen |
| `Currency` | `string` | NOT NULL, MaxLength(3) | `EGP` or `USD` |
| `PaymentStatus` | `PaymentStatus` (enum) | NOT NULL, MaxLength(20) | `PENDING → SUCCESS / FAILED` |
| `GatewayRefId` | `string?` | NULL, MaxLength(255) | Stripe Session ID / Paymob Order ID |
| `CreatedAt` | `DateTime` | DEFAULT NOW() | |
| `CompletedAt` | `DateTime?` | NULL | Set by webhook fulfillment |
| Nav | `User` | | |
| Nav | `Resume` | | |

### `Models/Download.cs`
| Property | Type | Constraints | Notes |
| :--- | :--- | :--- | :--- |
| `Id` | `Guid` | PK | |
| `ResumeId` | `Guid` | FK → `resumes(id)` | |
| `FormatType` | `string` | NOT NULL, MaxLength(10) | `PDF` or `DOCX` |
| `DownloadedAt` | `DateTime` | DEFAULT NOW() | |
| `IpAddress` | `string?` | NULL, MaxLength(45) | Abuse prevention |
| Nav | `Resume` | | |

---

## 4. AppDbContext & DataSeeder

### `Data/AppDbContext.cs`
```
DbSet<User>           Users
DbSet<UserMovement>   UserMovements
DbSet<Template>       Templates
DbSet<Resume>         Resumes
DbSet<Regeneration>   Regenerations
DbSet<Transaction>    Transactions
DbSet<Download>       Downloads
```

**`OnModelCreating` — active config:**
- All enum columns: `.HasConversion<string>()` (stores "DRAFT", "LOGIN", etc.)
- `Template.Id`: `ValueGeneratedOnAdd()` (SERIAL)
- `Resume`: `.HasQueryFilter(r => !r.IsDeleted)` — global soft-delete filter; applies to all queries automatically

**`OnModelCreating` — commented-out for PostgreSQL swap:**
```csharp
// HasIndex(u => u.Email).IsUnique()                   on Users
// HasIndex(u => u.Username).IsUnique()                on Users
// HasIndex(r => r.UserId)                             on Resumes
// HasIndex(r => new { r.ResumeId, r.SectionIdentifier }) on Regenerations
// Property(r => r.RawData).HasColumnType("jsonb")     on Resume
// Property(r => r.FinalData).HasColumnType("jsonb")   on Resume
// All HasMaxLength() constraints (see model tables above)
```

### `Data/DataSeeder.cs`
```
SeedAsync(AppDbContext db):
  if db.Templates.Any() → return early
  db.Templates.AddRange(
    new Template { Name="Modern Minimalist", IndustryCategory="Corporate",
                   BasePriceEgp=120.00m, BasePriceUsd=3.00m,
                   SupportsWord=true,  IsActive=true, CreatedAt=utcNow },
    new Template { Name="Creative",         IndustryCategory="Creative",
                   BasePriceEgp=120.00m, BasePriceUsd=3.00m,
                   SupportsWord=false, IsActive=true, CreatedAt=utcNow },
    new Template { Name="Executive",        IndustryCategory="Corporate",
                   BasePriceEgp=150.00m, BasePriceUsd=3.75m,
                   SupportsWord=true,  IsActive=true, CreatedAt=utcNow }
  )
  // Do NOT set Id — let EF auto-generate (ValueGeneratedOnAdd)
  db.SaveChanges()
```

> **PostgreSQL swap path:** Replace `UseInMemoryDatabase("NexaCV")` with `UseNpgsql(connectionString)` in `Program.cs`, uncomment all fluent config, run `dotnet ef migrations add Initial_Schema && dotnet ef database update`.

---

## 5. Enums

### `Enums/ResumeStatus.cs`
```csharp
public enum ResumeStatus { Draft, Completed, Paid }
// Stored as: "DRAFT", "COMPLETED", "PAID"
```

### `Enums/PaymentStatus.cs`
```csharp
public enum PaymentStatus { Pending, Success, Failed }
// Stored as: "PENDING", "SUCCESS", "FAILED"
```

### `Enums/ActionType.cs`
```csharp
public enum ActionType { Login, Logout, PasswordUpdated }
// Stored as: "LOGIN", "LOGOUT", "PASSWORD_UPDATED"
```

All three use `.HasConversion<string>()` in `AppDbContext`.

---

## 6. DTOs

### Auth
```
RegisterRequest:
  string FirstName, LastName, Username, Email, Password
  DateOnly? DateOfBirth
  Validator: all required except DOB; Email format; Password ≥8 chars + special char

LoginRequest:
  string Email, Password

AuthResponse:
  Guid   UserId
  string Token
  int    ExpiresIn   // 86400
```

### Users
```
UserProfileDto:
  Guid      Id
  string    FirstName, LastName, Username, Email
  DateTime  CreatedAt
  DateTime? LastLogin

UpdateUserRequest:
  string? FirstName, LastName, Username
  string? Password    // if present → re-hash + log PASSWORD_UPDATED
```

### Templates
```
TemplateDto:
  int      Id
  string   Name
  string?  IndustryCategory
  decimal  BasePriceEgp, BasePriceUsd
  bool     SupportsWord
```

### Resumes
```
CreateResumeRequest:
  int    TemplateId
  string RawData     // JSON string from wizard

ResumeSummaryDto:   (list view)
  Guid     Id
  string   Status
  string   TemplateName
  DateTime CreatedAt

ResumeDetailDto:    (full view)
  Guid     Id
  string   Status
  int      TemplateId
  string   TemplateName
  string?  RawData
  string?  FinalData
  bool     AiAvailable
  DateTime CreatedAt, UpdatedAt

RegenerateRequest:
  string SectionIdentifier
  string UserPrompt

RegenerateResponse:
  string  SectionIdentifier
  string  UpdatedContent
  int     RegenCountUsed
  int     RegenCountRemaining   // 3 - used
  decimal AddedCostEgp
  decimal AddedCostUsd
  bool    AiAvailable           // false until real AI impl
```

### Transactions
```
CheckoutRequest:
  Guid   ResumeId
  string Currency    // "EGP" | "USD"

CheckoutResponse:
  Guid    TransactionId
  string  PaymentUrl
  decimal BaseAmount, RegenAmount, TotalAmount
  string  Currency

TransactionDto:
  Guid      Id
  Guid      ResumeId
  decimal   TotalAmount
  string    Currency
  string    PaymentStatus
  DateTime  CreatedAt
  DateTime? CompletedAt
```

---

## 7. Mapping Extensions

**`Extensions/MappingExtensions.cs`** — static methods only, no third-party mapper.

| Method | Direction | Notes |
| :--- | :--- | :--- |
| `User.ToProfileDto()` | Model → DTO | Excludes `PasswordHash` |
| `Template.ToDto()` | Model → DTO | |
| `Resume.ToSummaryDto()` | Model → DTO | Requires `Template` nav loaded |
| `Resume.ToDetailDto(bool aiAvailable)` | Model → DTO | Requires `Template` nav loaded |
| `Regeneration.ToResponseDto(int totalUsed)` | Model → DTO | Computes `RegenCountRemaining = 3 - totalUsed` |
| `Transaction.ToCheckoutResponse(string paymentUrl)` | Model → DTO | |
| `Transaction.ToDto()` | Model → DTO | |
| `RegisterRequest.ToUser(string passwordHash)` | DTO → Model | Sets `CreatedAt = UtcNow`, `Id = NewGuid` |
| `CreateResumeRequest.ToResume(Guid userId)` | DTO → Model | Sets `Status = Draft`, `CreatedAt = UpdatedAt = UtcNow` |

---

## 8. Repository Layer

### `Repositories/IRepository.cs` (generic)
```csharp
Task<T?>      GetByIdAsync(Guid id);
Task<List<T>> GetAllAsync();
Task          AddAsync(T entity);
Task          UpdateAsync(T entity);
Task          DeleteAsync(T entity);
```

### `Repositories/EfRepository.cs` (generic base)
```
protected AppDbContext _db
Implements IRepository<T> via _db.Set<T>()
SaveChangesAsync() called after every mutation
```

### Specific Repositories

| Interface | Extra Methods |
| :--- | :--- |
| `IUserRepository` | `GetByEmailAsync(email)` → `User?`  `GetByUsernameAsync(username)` → `User?`  `ExistsByEmailOrUsernameAsync(email, username)` → `bool` |
| `IResumeRepository` | `GetByUserIdAsync(userId)` → `List<Resume>`  `GetWithTemplateAsync(resumeId)` → `Resume?` (includes Template) |
| `IRegenerationRepository` | `CountBySectionAsync(resumeId, sectionId)` → `int`  `GetCostSumByResumeAsync(resumeId)` → `(decimal Egp, decimal Usd)` |
| `ITransactionRepository` | `GetByResumeIdAsync(resumeId)` → `Transaction?`  `GetByGatewayRefIdAsync(refId)` → `Transaction?` |
| `IUserMovementRepository` | `LogAsync(userId, actionType, ip, ua)` → `Task` |
| `ITemplateRepository` | `GetActiveAsync(industryCategory?)` → `List<Template>` |
| `IDownloadRepository` | *(no extra methods — inherits `AddAsync` from base)* |

---

## 9. Services

### `Services/JwtService.cs`
```
GenerateToken(User user) → string
  HS256 signed, claims: sub (user.Id), email
  TTL from JwtSettings.ExpiresInSeconds (86400)

GetUserIdFromClaims(ClaimsPrincipal cp) → Guid
  throws UnauthorizedAccessException if claim missing
```

### `Services/IAuthService.cs` + `AuthService.cs`
```
RegisterAsync(RegisterRequest req, string? ip, string? ua) → AuthResponse
  IUserRepository.ExistsByEmailOrUsernameAsync() → throw 409 if true
  BCrypt.HashPassword(req.Password) → passwordHash
  req.ToUser(passwordHash) → user
  IUserRepository.AddAsync(user)
  JwtService.GenerateToken(user) → token
  return AuthResponse(user.Id, token, 86400)

LoginAsync(LoginRequest req, string? ip, string? ua) → AuthResponse
  IUserRepository.GetByEmailAsync(req.Email) → user ?? throw 401
  BCrypt.Verify(req.Password, user.PasswordHash) || throw 401
  user.LastLogin = DateTime.UtcNow               ← IMPORTANT
  IUserRepository.UpdateAsync(user)
  IUserMovementRepository.LogAsync(user.Id, ActionType.Login, ip, ua)
  return AuthResponse(user.Id, token, 86400)

LogoutAsync(Guid userId) → Task
  IUserMovementRepository.LogAsync(userId, ActionType.Logout, null, null)
```

### `Services/IUserService.cs` + `UserService.cs`
```
GetProfileAsync(Guid userId) → UserProfileDto
  IUserRepository.GetByIdAsync(userId) ?? throw KeyNotFoundException

UpdateProfileAsync(Guid userId, UpdateUserRequest req) → UserProfileDto
  fetch user ?? throw KeyNotFoundException
  patch non-null fields
  if req.Password != null:
    user.PasswordHash = BCrypt.HashPassword(req.Password)
    IUserMovementRepository.LogAsync(userId, ActionType.PasswordUpdated, ...)
  user.UpdatedAt not applicable (User has no UpdatedAt — only Resume does)
  IUserRepository.UpdateAsync(user)
  return user.ToProfileDto()
```

### `Services/ITemplateService.cs` + `TemplateService.cs`
```
GetAllAsync(string? industryCategory) → List<TemplateDto>
  ITemplateRepository.GetActiveAsync(industryCategory)
  .Select(t => t.ToDto())

GetByIdAsync(int id) → TemplateDto
  ITemplateRepository.GetByIdAsync(id) ?? throw KeyNotFoundException
  .ToDto()
```

### `Services/IResumeService.cs` + `ResumeService.cs`
```
CreateAsync(Guid userId, CreateResumeRequest req) → ResumeDetailDto
  req.ToResume(userId) → resume (Status=Draft)
  IResumeRepository.AddAsync(resume)
  IAiService.GenerateAsync(resume.RawData) → result
  resume.FinalData = result.FinalDataJson
  resume.Status = ResumeStatus.Completed
  resume.UpdatedAt = DateTime.UtcNow          ← explicit set
  IResumeRepository.UpdateAsync(resume)
  return resume.ToDetailDto(result.AiAvailable)

GetAllByUserAsync(Guid userId) → List<ResumeSummaryDto>
  IResumeRepository.GetByUserIdAsync(userId)
  .Select(r => r.ToSummaryDto())

GetByIdAsync(Guid resumeId, Guid userId) → ResumeDetailDto
  IResumeRepository.GetWithTemplateAsync(resumeId) ?? throw KeyNotFoundException
  resume.UserId != userId → throw UnauthorizedAccessException
  return resume.ToDetailDto()

UpdateFinalDataAsync(Guid resumeId, Guid userId, string finalData) → ResumeDetailDto
  fetch + ownership check
  resume.FinalData = finalData
  resume.UpdatedAt = DateTime.UtcNow          ← explicit set
  IResumeRepository.UpdateAsync(resume)
  return resume.ToDetailDto()

DeleteAsync(Guid resumeId, Guid userId) → Task
  fetch + ownership check
  resume.Status == Paid → throw InvalidOperationException("Cannot delete a paid resume")
  // Soft delete — preserves transaction history for accounting/audit
  resume.IsDeleted = true
  resume.UpdatedAt = DateTime.UtcNow
  IResumeRepository.UpdateAsync(resume)
  // EF global query filter (HasQueryFilter) hides deleted records from all subsequent queries

GetForDownloadAsync(Guid resumeId, Guid userId, string format) → Resume
  fetch + ownership check
  resume.Status != Paid → throw UnauthorizedAccessException
  format == "docx" && !resume.Template.SupportsWord → throw InvalidOperationException
  IDownloadRepository.AddAsync(new Download { ResumeId, FormatType=format, IpAddress })
  return resume
```

### `Services/IRegenerationService.cs` + `RegenerationService.cs`
```
RegenerateAsync(Guid resumeId, Guid userId, RegenerateRequest req) → RegenerateResponse
  resume = IResumeRepository.GetWithTemplateAsync(resumeId) ?? throw 404
  resume.UserId != userId → throw UnauthorizedAccessException
  count = IRegenerationRepository.CountBySectionAsync(resumeId, req.SectionIdentifier)
  count >= 3 → throw TooManyRegenerationsException
  result = IAiService.RegenerateAsync(req.SectionIdentifier, req.UserPrompt, resume.FinalData)
  // JSON patching — NEVER use string.Replace on raw JSON
  // Use JsonNode.Parse(resume.FinalData) → node[req.SectionIdentifier] = result.UpdatedContent → node.ToJsonString()
  var root = JsonNode.Parse(resume.FinalData)!.AsObject()
  root[req.SectionIdentifier] = JsonValue.Create(result.UpdatedContent)
  resume.FinalData = root.ToJsonString()
  resume.UpdatedAt = DateTime.UtcNow           ← explicit set
  IResumeRepository.UpdateAsync(resume)
  regen = new Regeneration { ResumeId, SectionIdentifier, UserPrompt, CostEgp=10m, CostUsd=0.25m }
  IRegenerationRepository.AddAsync(regen)
  return regen.ToResponseDto(count + 1)
```

### `Services/ITransactionService.cs` + `TransactionService.cs`
```
CheckoutAsync(Guid resumeId, Guid userId, string currency) → CheckoutResponse
  resume = IResumeRepository.GetWithTemplateAsync(resumeId) ?? throw 404
  resume.UserId != userId → throw UnauthorizedAccessException
  resume.Status != Completed → throw InvalidOperationException("Resume must be COMPLETED")
  baseAmount = currency == "EGP" ? resume.Template.BasePriceEgp : resume.Template.BasePriceUsd
  (regenEgp, regenUsd) = IRegenerationRepository.GetCostSumByResumeAsync(resumeId)
  regenAmount = currency == "EGP" ? regenEgp : regenUsd
  tx = new Transaction { UserId, ResumeId, BaseAmount, RegenAmount,
                         TotalAmount = base+regen, Currency, Status=Pending }
  ITransactionRepository.AddAsync(tx)
  gateway = PaymentGatewayFactory.Resolve(currency)
  result = gateway.CreateSessionAsync(new PaymentRequest(tx.Id, tx.TotalAmount, currency, resumeId))
  tx.GatewayRefId = result.GatewayRefId
  ITransactionRepository.UpdateAsync(tx)
  return tx.ToCheckoutResponse(result.PaymentUrl)

GetByIdAsync(Guid txId, Guid userId) → TransactionDto
  tx = ITransactionRepository.GetByIdAsync(txId) ?? throw KeyNotFoundException
  tx.UserId != userId → throw UnauthorizedAccessException
  return tx.ToDto()

FulfillAsync(string gatewayRefId) → Task
  tx = ITransactionRepository.GetByGatewayRefIdAsync(gatewayRefId) ?? throw KeyNotFoundException
  tx.PaymentStatus = PaymentStatus.Success
  tx.CompletedAt = DateTime.UtcNow
  ITransactionRepository.UpdateAsync(tx)
  resume = IResumeRepository.GetByIdAsync(tx.ResumeId) ?? throw KeyNotFoundException
  resume.Status = ResumeStatus.Paid
  resume.UpdatedAt = DateTime.UtcNow           ← explicit set
  IResumeRepository.UpdateAsync(resume)
```

---

## 10. AI & Payment Abstractions

### AI — `Services/IAiService.cs` + `StubAiService.cs`
```csharp
public interface IAiService
{
    Task<AiGenerationResult>    GenerateAsync(string rawDataJson);
    Task<AiRegenerationResult>  RegenerateAsync(string sectionId, string userPrompt, string currentFinalDataJson);
}

public record AiGenerationResult(string FinalDataJson, bool AiAvailable);
public record AiRegenerationResult(string UpdatedContent, bool AiAvailable);
```

`StubAiService`:
- `GenerateAsync` → returns `AiGenerationResult(rawDataJson, AiAvailable: false)`
- `RegenerateAsync` → returns `AiRegenerationResult(userPrompt, AiAvailable: false)`

`AiServiceSettings`: binds `ApiKey` (OPENAI_API_KEY), `Model` ("gpt-4o-mini"), `TimeoutSeconds` (5). Values present but unused until real implementation.

---

### Payment — `Services/Payment/`

#### `IPaymentGateway.cs`
```csharp
public interface IPaymentGateway
{
    string GatewayName       { get; }   // "Stub", "Stripe", "Paymob"
    string SupportedCurrency { get; }   // "USD", "EGP", "*" (any)

    Task<PaymentSessionResult> CreateSessionAsync(PaymentRequest request);

    bool VerifyWebhookSignature(
        HttpRequest request,
        out string eventType,
        out string gatewayRefId);
}

public record PaymentRequest(Guid TransactionId, decimal Amount, string Currency, Guid ResumeId);
public record PaymentSessionResult(string PaymentUrl, string GatewayRefId);
```

#### `PaymentGatewayFactory.cs`
```
Resolve(string currency) → IPaymentGateway
  pick gateway where SupportedCurrency == currency || SupportedCurrency == "*"
  throw InvalidOperationException if none found

ResolveByRequest(HttpRequest req) → IPaymentGateway
  each gateway tries VerifyWebhookSignature; return first that matches
  throw InvalidOperationException if none match
```

#### `StubPaymentGateway.cs`
```
GatewayName       = "Stub"
SupportedCurrency = "*"

CreateSessionAsync  → PaymentSessionResult($"https://stub.payment/session/{tx.Id}", tx.Id.ToString())
VerifyWebhookSignature → always true; eventType="checkout.completed"; gatewayRefId = header["X-Stub-Ref"]
```

**Adding a real gateway (e.g. Stripe):**
1. Add `Stripe.net` NuGet package
2. Create `StripePaymentGateway : IPaymentGateway` — implement 2 methods
3. Register in DI: `services.AddScoped<IPaymentGateway, StripePaymentGateway>()`
4. Zero changes to any service, endpoint, or factory

---

## 11. Endpoints

All endpoints follow the pattern: **validate → extract userId from JWT → call service → return DTO**.
Business logic and DB access are never done inside endpoint handlers.

> **Minimal API response typing:** Use `.Produces<T>(200)`, `.Produces(204)`, `.ProducesProblem(400)`, `.ProducesProblem(401)`, `.ProducesProblem(404)` etc. on each `MapGet`/`MapPost` call. This is the Minimal API equivalent of `[ProducesResponseType]` and is required for Swagger UI to display accurate status code documentation.

### `Endpoints/AuthEndpoints.cs`
| Method | Route | Auth | Service call | Success | Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| POST | `/api/auth/register` | None | `IAuthService.RegisterAsync` | `201 AuthResponse` | `409` |
| POST | `/api/auth/login` | None | `IAuthService.LoginAsync` | `200 AuthResponse` | `401` |
| POST | `/api/auth/logout` | Required | `IAuthService.LogoutAsync` | `204` | `401` |

### `Endpoints/UserEndpoints.cs`
| Method | Route | Auth | Service call | Success | Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/users/me` | Required | `IUserService.GetProfileAsync` | `200 UserProfileDto` | `401` |
| PUT | `/api/users/me` | Required | `IUserService.UpdateProfileAsync` | `200 UserProfileDto` | `401` |

### `Endpoints/TemplateEndpoints.cs`
| Method | Route | Auth | Service call | Success | Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| GET | `/api/templates` | None | `ITemplateService.GetAllAsync` | `200 List<TemplateDto>` | — |
| GET | `/api/templates/{id}` | None | `ITemplateService.GetByIdAsync` | `200 TemplateDto` | `404` |

### `Endpoints/ResumeEndpoints.cs`
| Method | Route | Auth | Service call | Success | Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| POST | `/api/resumes` | Required | `IResumeService.CreateAsync` | `200 ResumeDetailDto` | `401`,`404` |
| GET | `/api/resumes` | Required | `IResumeService.GetAllByUserAsync` | `200 List<ResumeSummaryDto>` | `401` |
| GET | `/api/resumes/{id}` | Required | `IResumeService.GetByIdAsync` | `200 ResumeDetailDto` | `401`,`403`,`404` |
| PUT | `/api/resumes/{id}` | Required | `IResumeService.UpdateFinalDataAsync` | `200 ResumeDetailDto` | `401`,`403` |
| DELETE | `/api/resumes/{id}` | Required | `IResumeService.DeleteAsync` | `204` | `400`,`401`,`403` |
| POST | `/api/resumes/{id}/regenerate` | Required | `IRegenerationService.RegenerateAsync` | `200 RegenerateResponse` | `401`,`403`,`429` |
| GET | `/api/resumes/{id}/download` | Required | `IResumeService.GetForDownloadAsync` | `501` (deferred) | `400`,`401`,`403` |

Query param for download: `?format=pdf` or `?format=docx`

### `Endpoints/TransactionEndpoints.cs`
| Method | Route | Auth | Service call | Success | Errors |
| :--- | :--- | :--- | :--- | :--- | :--- |
| POST | `/api/transactions/checkout` | Required | `ITransactionService.CheckoutAsync` | `200 CheckoutResponse` | `400`,`401`,`403` |
| GET | `/api/transactions/{id}` | Required | `ITransactionService.GetByIdAsync` | `200 TransactionDto` | `401`,`403`,`404` |

### `Endpoints/WebhookEndpoints.cs`
| Method | Route | Auth | Notes |
| :--- | :--- | :--- | :--- |
| POST | `/api/webhooks/payment` | **None (no JWT)** | Gateway verifies own signature via `VerifyWebhookSignature()`. On success calls `ITransactionService.FulfillAsync()`. Returns `200` immediately. `400` if no gateway matches. |

---

## 12. Middleware

### `Middleware/ExceptionMiddleware.cs`

| Exception | HTTP Status | Response shape |
| :--- | :--- | :--- |
| `ValidationException` (FluentValidation) | `422` | `{ status, error: "Validation failed", details: [{field, message}] }` |
| `UnauthorizedAccessException` | `401` | `{ status, error }` |
| `KeyNotFoundException` | `404` | `{ status, error }` |
| `InvalidOperationException` | `400` | `{ status, error }` |
| `TooManyRegenerationsException` (custom) | `429` | `{ status, error: "Regeneration limit reached for this section" }` |
| Any other `Exception` | `500` | `{ status, error: "Internal server error" }` |

> Define `TooManyRegenerationsException : Exception` alongside `RegenerationService`.

---

## 13. Program.cs — Wiring

```
// Settings
builder.Services.Configure<JwtSettings>(config.GetSection("Jwt"))
builder.Services.Configure<AiServiceSettings>(config.GetSection("AiService"))
builder.Services.Configure<PaymentSettings>(config.GetSection("Payment"))

// Database
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("NexaCV"))

// Seeder
builder.Services.AddScoped<DataSeeder>()

// Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => { /* validate issuer, audience, lifetime, signing key from JwtSettings */ })
builder.Services.AddAuthorization()

// Swagger
builder.Services.AddSwaggerGen(c => {
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { ... })
    c.AddSecurityRequirement(...)
})

// Repositories (Scoped)
builder.Services.AddScoped<IUserRepository, UserRepository>()
builder.Services.AddScoped<IResumeRepository, ResumeRepository>()
builder.Services.AddScoped<IRegenerationRepository, RegenerationRepository>()
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>()
builder.Services.AddScoped<IUserMovementRepository, UserMovementRepository>()
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>()
builder.Services.AddScoped<IDownloadRepository, DownloadRepository>()

// Services (Scoped)
builder.Services.AddSingleton<JwtService>()
builder.Services.AddScoped<IAuthService, AuthService>()
builder.Services.AddScoped<IUserService, UserService>()
builder.Services.AddScoped<ITemplateService, TemplateService>()
builder.Services.AddScoped<IResumeService, ResumeService>()
builder.Services.AddScoped<IRegenerationService, RegenerationService>()
builder.Services.AddScoped<ITransactionService, TransactionService>()

// AI (Stub)
builder.Services.AddScoped<IAiService, StubAiService>()

// Payment (Stub)
builder.Services.AddScoped<IPaymentGateway, StubPaymentGateway>()
builder.Services.AddScoped<PaymentGatewayFactory>()

// Validation
builder.Services.AddFluentValidationAutoValidation()

// CORS
builder.Services.AddCors(opt =>
    opt.AddPolicy("frontend", p => p.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()))

// --- Pipeline ---
app.UseMiddleware<ExceptionMiddleware>()

if (app.Environment.IsDevelopment()) {
    app.UseSwagger()
    app.UseSwaggerUI()    // served at /swagger
}

app.UseCors("frontend")
app.UseAuthentication()
app.UseAuthorization()

// Startup — seed DB
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>()
    db.Database.EnsureCreated()
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>()
    await seeder.SeedAsync(db)
}

// Map endpoints
AuthEndpoints.Map(app)
UserEndpoints.Map(app)
TemplateEndpoints.Map(app)
ResumeEndpoints.Map(app)
TransactionEndpoints.Map(app)
WebhookEndpoints.Map(app)
```

---

## 14. File Map

```
backend/
├── NexaCV.sln
└── NexaCV.Api/
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    ├── .env.example
    ├── Data/
    │   ├── AppDbContext.cs
    │   └── DataSeeder.cs
    ├── Enums/
    │   ├── ResumeStatus.cs
    │   ├── PaymentStatus.cs
    │   └── ActionType.cs
    ├── Models/
    │   ├── User.cs
    │   ├── UserMovement.cs
    │   ├── Template.cs
    │   ├── Resume.cs
    │   ├── Regeneration.cs
    │   ├── Transaction.cs
    │   └── Download.cs
    ├── DTOs/
    │   ├── Auth/
    │   │   ├── RegisterRequest.cs
    │   │   ├── LoginRequest.cs
    │   │   └── AuthResponse.cs
    │   ├── Users/
    │   │   ├── UserProfileDto.cs
    │   │   └── UpdateUserRequest.cs
    │   ├── Templates/
    │   │   └── TemplateDto.cs
    │   ├── Resumes/
    │   │   ├── CreateResumeRequest.cs
    │   │   ├── ResumeSummaryDto.cs
    │   │   ├── ResumeDetailDto.cs
    │   │   ├── RegenerateRequest.cs
    │   │   └── RegenerateResponse.cs
    │   └── Transactions/
    │       ├── CheckoutRequest.cs
    │       ├── CheckoutResponse.cs
    │       └── TransactionDto.cs
    ├── Extensions/
    │   └── MappingExtensions.cs
    ├── Repositories/
    │   ├── IRepository.cs
    │   ├── EfRepository.cs
    │   ├── IUserRepository.cs
    │   ├── UserRepository.cs
    │   ├── IResumeRepository.cs
    │   ├── ResumeRepository.cs
    │   ├── IRegenerationRepository.cs
    │   ├── RegenerationRepository.cs
    │   ├── ITransactionRepository.cs
    │   ├── TransactionRepository.cs
    │   ├── IUserMovementRepository.cs
    │   ├── UserMovementRepository.cs
    │   ├── ITemplateRepository.cs
    │   ├── TemplateRepository.cs
    │   ├── IDownloadRepository.cs
    │   └── DownloadRepository.cs
    ├── Services/
    │   ├── JwtService.cs
    │   ├── IAuthService.cs
    │   ├── AuthService.cs
    │   ├── IUserService.cs
    │   ├── UserService.cs
    │   ├── ITemplateService.cs
    │   ├── TemplateService.cs
    │   ├── IResumeService.cs
    │   ├── ResumeService.cs
    │   ├── IRegenerationService.cs
    │   ├── RegenerationService.cs
    │   ├── ITransactionService.cs
    │   ├── TransactionService.cs
    │   ├── IAiService.cs
    │   ├── StubAiService.cs
    │   └── Payment/
    │       ├── IPaymentGateway.cs
    │       ├── PaymentGatewayFactory.cs
    │       └── StubPaymentGateway.cs
    ├── Settings/
    │   ├── JwtSettings.cs
    │   ├── AiServiceSettings.cs
    │   └── PaymentSettings.cs
    ├── Endpoints/
    │   ├── AuthEndpoints.cs
    │   ├── UserEndpoints.cs
    │   ├── TemplateEndpoints.cs
    │   ├── ResumeEndpoints.cs
    │   ├── TransactionEndpoints.cs
    │   └── WebhookEndpoints.cs
    └── Middleware/
        └── ExceptionMiddleware.cs
```

**Total files:** 62 (excl. `.csproj`, `appsettings`, generated)

---

## 15. Verification Checklist

- [ ] `dotnet run` — starts without errors, in-memory DB created
- [ ] `GET /api/templates` — returns 3 seeded templates with EF-generated IDs
- [ ] Swagger UI loads at `https://localhost:{port}/swagger` in Development
- [ ] "Authorize" button in Swagger UI accepts `Bearer <token>` and passes it to protected endpoints
- [ ] `POST /api/auth/register` → `201 { user_id, token }`; duplicate email → `409`
- [ ] `POST /api/auth/login` → JWT; `user.last_login` updated in DB; `user_movements` row inserted
- [ ] `POST /api/auth/logout` → `204`; `user_movements` LOGOUT row inserted
- [ ] `GET /api/users/me` → profile (no password_hash in response)
- [ ] `POST /api/resumes` (valid JWT) → `final_data = raw_data`, `ai_available: false`, `status = COMPLETED`
- [ ] `PUT /api/resumes/{id}` → `updated_at` changes in response
- [ ] `DELETE /api/resumes/{id}` on PAID resume → `400`
- [ ] `POST /api/resumes/{id}/regenerate` × 4 — first 3 succeed (+10 EGP each, `ai_available: false`), 4th → `429`
- [ ] `GET /api/resumes/{id}` with a different user's JWT → `403`
- [ ] `GET /api/resumes/{id}/download` on unpaid resume → `403`
- [ ] `GET /api/resumes/{id}/download` on paid resume → `501`
- [ ] `POST /api/transactions/checkout` → total = base + regen sum; stub `payment_url`
- [ ] `POST /api/webhooks/payment` missing signature → `400`
- [ ] `POST /api/webhooks/payment` stub call → `transaction.payment_status = SUCCESS`, `resume.status = PAID`
- [ ] Adding a second `IPaymentGateway` class + DI registration → routes correctly; zero changes to `ITransactionService` or endpoints

---

## 16. Open Decisions

| # | Topic | Decision |
| :--- | :--- | :--- |
| 1 | EF Core version | EF Core 8 on .NET 9 — stable, no EF9 features needed |
| 2 | DB provider | **In-memory for now.** Swap to `Npgsql` — one line in `Program.cs` + uncomment fluent config |
| 3 | JSONB mapping | `RawData`/`FinalData` stored as `string` (in-memory). On Postgres: `HasColumnType("jsonb")` |
| 4 | DTO mapping | Static `MappingExtensions.cs` — no AutoMapper |
| 11 | Soft deletes | `Resume.IsDeleted` bool + `HasQueryFilter(r => !r.IsDeleted)`. Hard deletes are forbidden on resumes — preserves transaction history for accounting and chargebacks. |
| 5 | Payment extensibility | `IPaymentGateway` + `PaymentGatewayFactory` — add any gateway by implementing the interface |
| 6 | Swagger | `Swashbuckle.AspNetCore`; JWT Bearer security definition; UI at `/swagger`; dev-only |
| 7 | PDF renderer | **QuestPDF** — modern fluent C# library; no external process, no OpenXML complexity. Add `QuestPDF` NuGet, implement `IResumeDocumentRenderer`, inject into `ResumeService.GetForDownloadAsync`. Remove the `501` stub when ready. |
| 8 | IP geolocation | **Deferred** — MaxMind GeoLite2 for EGP/USD auto-detection |
| 9 | Refresh tokens | **Out of scope** for v1 — single 24h JWT |
| 10 | JSON patching | `FinalData` is a raw JSON string; section patching done via `System.Text.Json` `JsonNode` in `RegenerationService` |
