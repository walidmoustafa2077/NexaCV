# NexaCV Backend — Test Results Report

**Date:** April 28, 2026  
**Framework:** xUnit 2.9.3 · .NET 9.0.13  
**Test Project:** `backend/NexaCV.Tests`  
**Total Duration:** 3.40 s  

| Metric | Value |
|---|---|
| Total Tests | 107 |
| Passed | ✅ 107 |
| Failed | ❌ 0 |
| Skipped | ⏭ 0 |

---

## Table of Contents

1. [AuthService Tests](#1-authservice-tests)
2. [UserService Tests](#2-userservice-tests)
3. [ResumeService Tests](#3-resumeservice-tests)
4. [TemplateService Tests](#4-templateservice-tests)
5. [TransactionService Tests](#5-transactionservice-tests)
6. [RegenerationService Tests](#6-regenerationservice-tests)
7. [JwtService Tests](#7-jwtservice-tests)
8. [MappingExtensions Tests](#8-mappingextensions-tests)
9. [Stub Services Tests](#9-stub-services-tests)
10. [Validator Tests](#10-validator-tests)

---

## 1. AuthService Tests

**File:** `Services/AuthServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.AuthServiceTests`  
**Tests:** 6 | **All Passed**

### RegisterAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `RegisterAsync_WithNewUser_ReturnsAuthResponse` | `RegisterRequest { FirstName:"John", LastName:"Doe", Username:"johndoe", Email:"john@example.com", Password:"P@ssw0rd!" }` — repo returns `existsByEmailOrUsername = false` | `AuthResponse` with non-empty `Token`, `ExpiresIn = 86400` | ✅ Passed | 535 ms |
| 2 | `RegisterAsync_WhenEmailOrUsernameExists_ThrowsConflictException` | `RegisterRequest { Email:"jane@example.com", Username:"janedoe" }` — repo returns `existsByEmailOrUsername = true` | Throws `ConflictException` with message containing `"email or username"` | ✅ Passed | 7 ms |

### LoginAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 3 | `LoginAsync_WithValidCredentials_ReturnsAuthResponse` | `LoginRequest { Email: user.Email, Password:"P@ssw0rd!" }` — user exists with BCrypt hash | `AuthResponse { UserId = user.Id, Token: non-empty }` | ✅ Passed | 380 ms |
| 4 | `LoginAsync_WithUnknownEmail_ThrowsUnauthorizedAccessException` | `LoginRequest { Email:"ghost@example.com", Password:"P@ssw0rd!" }` — repo returns `null` | Throws `UnauthorizedAccessException` | ✅ Passed | 70 ms |
| 5 | `LoginAsync_WithWrongPassword_ThrowsUnauthorizedAccessException` | `LoginRequest { Email: user.Email, Password:"WrongPass!" }` — password does not match BCrypt hash | Throws `UnauthorizedAccessException` | ✅ Passed | 403 ms |

### LogoutAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 6 | `LogoutAsync_LogsLogoutMovement` | `userId = Guid.NewGuid()` | `IUserMovementRepository.LogAsync` called once with `ActionType.Logout` | ✅ Passed | 5 ms |

---

## 2. UserService Tests

**File:** `Services/UserServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.UserServiceTests`  
**Tests:** 5 | **All Passed**

### GetProfileAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GetProfileAsync_WithValidUserId_ReturnsProfileDto` | `userId` matching an existing `User` in repo | `UserProfileDto { Id, Email, FirstName }` matching user | ✅ Passed | 402 ms |
| 2 | `GetProfileAsync_WithUnknownUserId_ThrowsKeyNotFoundException` | Random `Guid` — repo returns `null` | Throws `KeyNotFoundException` with message `"User not found"` | ✅ Passed | 2 ms |

### UpdateProfileAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 3 | `UpdateProfileAsync_UpdatesFirstName_ReturnsUpdatedDto` | `UpdateUserRequest { FirstName:"Updated" }` | `UserProfileDto.FirstName = "Updated"` | ✅ Passed | 185 ms |
| 4 | `UpdateProfileAsync_WithPasswordChange_LogsPasswordUpdated` | `UpdateUserRequest { Password:"N3wP@ss!" }` | `IUserMovementRepository.LogAsync` called once with `ActionType.PasswordUpdated` | ✅ Passed | 423 ms |
| 5 | `UpdateProfileAsync_WithNullFields_OnlyAppliesNonNullChanges` | `UpdateUserRequest { FirstName:"NewFirst" }` — `LastName` is `null` | `LastName` unchanged; `FirstName = "NewFirst"` | ✅ Passed | 190 ms |
| 6 | `UpdateProfileAsync_WithUnknownUserId_ThrowsKeyNotFoundException` | Random `Guid` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 4 ms |

---

## 3. ResumeService Tests

**File:** `Services/ResumeServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.ResumeServiceTests`  
**Tests:** 11 | **All Passed**

### CreateAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `CreateAsync_CallsAiAndReturnsDetailDto` | `CreateResumeRequest { TemplateId:1, RawData:"{\"name\":\"John\"}" }` — AI stub returns `{ FinalDataJson:"{\"settings\":{},\"content\":{}}", AiAvailable:false }` | `ResumeDetailDto { Status:"COMPLETED" }`, `IAiService.GenerateAsync` called once | ✅ Passed | 28 ms |

### GetAllByUserAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 2 | `GetAllByUserAsync_ReturnsUserResumes` | `userId` with 2 resumes in repo | List of 2 `ResumeSummaryDto` | ✅ Passed | 2 ms |
| 3 | `GetAllByUserAsync_WithNoResumes_ReturnsEmptyList` | `userId` with 0 resumes | Empty list `[]` | ✅ Passed | 12 ms |

### GetByIdAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 4 | `GetByIdAsync_WithValidOwner_ReturnsDetailDto` | `resumeId` + matching `userId` (owner) | `ResumeDetailDto { Id = resumeId }` | ✅ Passed | 158 ms |
| 5 | `GetByIdAsync_WithWrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by `ownerUserId`, requested by `attackerUserId` | Throws `UnauthorizedAccessException` | ✅ Passed | 3 ms |
| 6 | `GetByIdAsync_NotFound_ThrowsKeyNotFoundException` | Random `resumeId` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 4 ms |

### UpdateFinalDataAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 7 | `UpdateFinalDataAsync_WithValidOwner_UpdatesAndReturnsDto` | `resumeId` + owner `userId` + `finalData = "{\"settings\":{},\"content\":{\"summary\":\"Updated\"}}"` | `ResumeDetailDto.FinalData` = new data; history added with `Reason:"MANUAL_EDIT"` | ✅ Passed | 19 ms |
| 8 | `UpdateFinalDataAsync_WithWrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 5 ms |

### DeleteAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 9 | `DeleteAsync_SoftDeletesResume` | `resumeId` with `Status = Completed`, owner `userId` | `resume.IsDeleted = true`; `UpdateAsync` called once | ✅ Passed | 6 ms |
| 10 | `DeleteAsync_PaidResume_ThrowsInvalidOperationException` | `resumeId` with `Status = Paid` | Throws `InvalidOperationException` with message `"paid resume"` | ✅ Passed | 4 ms |
| 11 | `DeleteAsync_WithWrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 10 ms |

### GetForDownloadAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 12 | `GetForDownloadAsync_PaidResumeWithPdf_ReturnsResume` | `resumeId (Paid)`, `userId (owner)`, `format:"pdf"`, `ip:"127.0.0.1"` | Returns `Resume`; `DownloadRepository.AddAsync` called with `FormatType:"PDF"` | ✅ Passed | 10 ms |
| 13 | `GetForDownloadAsync_UnpaidResume_ThrowsUnauthorizedAccessException` | `resumeId (Completed)`, `format:"pdf"` | Throws `UnauthorizedAccessException` with message containing `"paid"` | ✅ Passed | 4 ms |
| 14 | `GetForDownloadAsync_DocxOnTemplateWithoutWordSupport_ThrowsInvalidOperationException` | `resumeId (Paid)`, template `SupportsWord = false`, `format:"docx"` | Throws `InvalidOperationException` with message containing `"DOCX"` | ✅ Passed | 4 ms |
| 15 | `GetForDownloadAsync_WrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 7 ms |

---

## 4. TemplateService Tests

**File:** `Services/TemplateServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.TemplateServiceTests`  
**Tests:** 5 | **All Passed**

### GetAllAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GetAllAsync_WithNoFilter_ReturnsAllActiveTemplates` | `industryCategory = null` — repo returns 2 templates | List of 2 `TemplateDto` | ✅ Passed | 1 ms |
| 2 | `GetAllAsync_WithIndustryFilter_PassesFilterToRepository` | `industryCategory = "Corporate"` — repo returns 1 template | List of 1 `TemplateDto`; repo called with `"Corporate"` | ✅ Passed | 5 ms |
| 3 | `GetAllAsync_WithNoResults_ReturnsEmptyList` | `industryCategory = "NonExistent"` — repo returns `[]` | Empty list `[]` | ✅ Passed | 3 ms |

### GetByIdAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 4 | `GetByIdAsync_WithValidId_ReturnsMappedDto` | `id = 42` — repo returns template with `Id = 42` | `TemplateDto { Id:42, Name, BasePriceUsd }` matching template | ✅ Passed | 12 ms |
| 5 | `GetByIdAsync_WithInvalidId_ThrowsKeyNotFoundException` | `id = 999` — repo returns `null` | Throws `KeyNotFoundException` with message containing `"999"` | ✅ Passed | 7 ms |

---

## 5. TransactionService Tests

**File:** `Services/TransactionServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.TransactionServiceTests`  
**Tests:** 9 | **All Passed**

### CheckoutAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `CheckoutAsync_WithCompletedResume_ReturnsCheckoutResponse` | `resumeId (Completed)`, `userId (owner)`, `currency:"USD"`, exchange rate `1.00`, regen cost `$0.50` | `CheckoutResponse { BaseAmount:3.00, RegenAmount:0.50, TotalAmount:3.50, Currency:"USD", PaymentUrl starts with "https://stub.payment/session/" }` | ✅ Passed | 5 ms |
| 2 | `CheckoutAsync_WithEgpCurrency_AppliesExchangeRate` | `resumeId (Completed)`, `currency:"EGP"`, exchange rate `50.00`, regen cost `$0.00` | `CheckoutResponse { BaseAmount:150.00, Currency:"EGP" }` | ✅ Passed | 15 ms |
| 3 | `CheckoutAsync_ResumeNotCompleted_ThrowsInvalidOperationException` | `resumeId (Draft)`, `userId (owner)` | Throws `InvalidOperationException` with message containing `"COMPLETED"` | ✅ Passed | 8 ms |
| 4 | `CheckoutAsync_WrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 4 ms |
| 5 | `CheckoutAsync_ResumeNotFound_ThrowsKeyNotFoundException` | Random `resumeId` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 2 ms |

### GetByIdAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 6 | `GetByIdAsync_WithValidOwner_ReturnsTransactionDto` | `txId` + matching owner `userId` — `TotalAmount:150.00, PaymentStatus:Pending` | `TransactionDto { Id, TotalAmount:150.00, PaymentStatus:"PENDING" }` | ✅ Passed | 18 ms |
| 7 | `GetByIdAsync_WrongUser_ThrowsUnauthorizedAccessException` | `txId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 6 ms |
| 8 | `GetByIdAsync_NotFound_ThrowsKeyNotFoundException` | Random `txId` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 2 ms |

### FulfillAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 9 | `FulfillAsync_WithValidRefId_MarkesTransactionSuccessAndResumePaid` | `gatewayRefId = "ref-abc123"` — resolves to pending transaction + completed resume | `tx.PaymentStatus = Success`, `tx.CompletedAt != null`, `resume.Status = Paid` | ✅ Passed | 6 ms |
| 10 | `FulfillAsync_WithUnknownRefId_ThrowsKeyNotFoundException` | `gatewayRefId = "unknown-ref"` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 5 ms |

---

## 6. RegenerationService Tests

**File:** `Services/RegenerationServiceTests.cs`  
**Class:** `NexaCV.Tests.Services.RegenerationServiceTests`  
**Tests:** 7 | **All Passed**

### RegenerateAsync

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `RegenerateAsync_Success_ReturnsResponseWithCorrectCounts` | `SectionIdentifier:"summary"`, `UserPrompt:"Make it more concise"`, existing count = 1 | `RegenerateResponse { RegenCountUsed:2, RegenCountRemaining:1, AddedCostUsd:0.25, UpdatedContent:"Updated summary text" }` | ✅ Passed | 22 ms |
| 2 | `RegenerateAsync_FirstRegeneration_CountStartsAtOne` | `SectionIdentifier:"summary"`, existing count = 0 | `RegenerateResponse { RegenCountUsed:1, RegenCountRemaining:2 }` | ✅ Passed | 28 ms |
| 3 | `RegenerateAsync_AtLimit_ThrowsTooManyRegenerationsException` | `SectionIdentifier:"summary"`, existing count = 3 (at max) | Throws `TooManyRegenerationsException` | ✅ Passed | 9 ms |
| 4 | `RegenerateAsync_WrongUser_ThrowsUnauthorizedAccessException` | `resumeId` owned by another user | Throws `UnauthorizedAccessException` | ✅ Passed | 4 ms |
| 5 | `RegenerateAsync_ResumeNotFound_ThrowsKeyNotFoundException` | Random `resumeId` — repo returns `null` | Throws `KeyNotFoundException` | ✅ Passed | 250 ms |
| 6 | `RegenerateAsync_CallsAiWithCorrectContext` | `SectionIdentifier:"summary"`, `UserPrompt:"Be concise"`, `NewTitleSuggestion:"Senior Engineer"` | `IAiService.RegenerateAsync` called once with `context.SectionIdentifier = "summary"`, `context.NewTitleSuggestion = "Senior Engineer"` | ✅ Passed | 12 ms |
| 7 | `RegenerateAsync_WithTargetFormat_UpdatesSettingsInFinalData` | `SectionIdentifier:"summary"`, `TargetFormat:"PARAGRAPH"` | `IResumeRepository.UpdateAsync` called with `resume.FinalData` containing `"PARAGRAPH"` | ✅ Passed | 8 ms |
| 8 | `RegenerateAsync_SavesHistorySnapshot` | `SectionIdentifier:"summary"`, count = 0 | `IResumeHistoryRepository.AddAsync` called with `Reason` starting with `"REGEN_"` | ✅ Passed | 9 ms |

---

## 7. JwtService Tests

**File:** `Utils/JwtServiceTests.cs`  
**Class:** `NexaCV.Tests.Utils.JwtServiceTests`  
**Tests:** 7 | **All Passed**

**JWT Settings used in all tests:**  
`Secret: "test-super-secret-key-that-is-at-least-32-chars"`, `Issuer: "test-issuer"`, `Audience: "test-audience"`, `ExpiresInSeconds: 86400`

### GenerateToken

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GenerateToken_ReturnsNonEmptyString` | `User { Id, Email }` | Non-empty, non-whitespace JWT string | ✅ Passed | 332 ms |
| 2 | `GenerateToken_ContainsUserIdAsSub` | `User { Id = Guid.NewGuid() }` | Decoded token has `sub` claim = `user.Id.ToString()` | ✅ Passed | 220 ms |
| 3 | `GenerateToken_ContainsEmailClaim` | `User { Email = "test@jwt.com" }` | Decoded token has `email` claim = `"test@jwt.com"` | ✅ Passed | 179 ms |
| 4 | `GenerateToken_ContainsJtiClaim` | Any `User` | Decoded token contains a `jti` claim | ✅ Passed | 233 ms |
| 5 | `GenerateToken_TwoCallsProduceDifferentTokens` | Same `User` called twice | Two tokens are not identical (different `jti` each call) | ✅ Passed | 186 ms |

### GetUserIdFromClaims

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 6 | `GetUserIdFromClaims_WithValidSubClaim` | `ClaimsPrincipal` with `sub = userId.ToString()` | Returns `Guid` equal to `userId` | ✅ Passed | < 1 ms |
| 7 | `GetUserIdFromClaims_WithMissingSubClaim` | `ClaimsPrincipal` with no claims | Throws `UnauthorizedAccessException` | ✅ Passed | 1 ms |
| 8 | `GetUserIdFromClaims_WithNonGuidSubClaim` | `ClaimsPrincipal` with `sub = "not-a-guid"` | Throws `UnauthorizedAccessException` | ✅ Passed | 5 ms |

---

## 8. MappingExtensions Tests

**File:** `Utils/MappingExtensionsTests.cs`  
**Class:** `NexaCV.Tests.Utils.MappingExtensionsTests`  
**Tests:** 13 | **All Passed**

### User → UserProfileDto

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `ToProfileDto_MapsAllFields` | `User { Id, FirstName:"John", LastName:"Doe", Username:"johndoe", Email:"john@example.com", CreatedAt, LastLogin }` | `UserProfileDto` with all fields matching | ✅ Passed | 8 ms |

### Template → TemplateDto

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 2 | `ToDto_Template_MapsAllFields` | `Template { Id:7, Name:"Corporate Pro", IndustryCategory:"Finance", BasePriceUsd:5.00, SupportsWord:true }` | `TemplateDto` with all fields matching | ✅ Passed | 1 ms |
| 3 | `ToDto_Template_WithNullIndustry_MapsAsNull` | `Template { IndustryCategory: null }` | `TemplateDto.IndustryCategory = null` | ✅ Passed | 1 ms |

### Resume → ResumeSummaryDto

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 4 | `ToSummaryDto_MapsStatusAsUpperCase` | `Resume { Status: Completed }` | `ResumeSummaryDto.Status = "COMPLETED"` | ✅ Passed | 1 ms |
| 5 | `ToSummaryDto_PaidStatus_MapsAsPaid` | `Resume { Status: Paid }` | `ResumeSummaryDto.Status = "PAID"` | ✅ Passed | < 1 ms |

### Resume → ResumeDetailDto

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 6 | `ToDetailDto_MapsAllFields` | `Resume { Id, TemplateId, Template, RawData, FinalData, Status:Completed }`, `aiAvailable = true` | `ResumeDetailDto` with all fields; `AiAvailable = true`, `Status = "COMPLETED"` | ✅ Passed | 1 ms |
| 7 | `ToDetailDto_DefaultAiAvailable_IsFalse` | `Resume`, no `aiAvailable` argument | `ResumeDetailDto.AiAvailable = false` | ✅ Passed | 1 ms |

### Regeneration → RegenerateResponse

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 8 | `ToResponseDto_MapsCountsCorrectly` | `Regeneration { SectionIdentifier:"experience", CostUsd:0.25 }`, `totalUsed:2`, `updatedContent:"New content"`, `aiAvailable:false` | `RegenerateResponse { RegenCountUsed:2, RegenCountRemaining:1, AddedCostUsd:0.25, AiAvailable:false }` | ✅ Passed | 2 ms |

### Transaction → CheckoutResponse

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 9 | `ToCheckoutResponse_MapsAllFields` | `Transaction { BaseAmount:150, RegenAmount:12.5, TotalAmount:162.5, Currency:"EGP", ExchangeRateUsed:50 }`, `paymentUrl:"https://pay.example.com/session"` | `CheckoutResponse` with all fields matching | ✅ Passed | 2 ms |

### Transaction → TransactionDto

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 10 | `ToDto_Transaction_MapsStatusAsUpperCase` | `Transaction { PaymentStatus: Success, CompletedAt: DateTime.UtcNow.AddMinutes(5) }` | `TransactionDto.PaymentStatus = "SUCCESS"`, `CompletedAt != null` | ✅ Passed | 3 ms |

### RegisterRequest → User

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 11 | `ToUser_MapsAllFieldsAndSetsNewId` | `RegisterRequest { FirstName:"Jane", LastName:"Smith", Username:"janesmith", Email:"jane@example.com", DateOfBirth:1990-05-15 }`, `passwordHash:"hashed-password"` | `User { Id != Guid.Empty, FirstName:"Jane", PasswordHash:"hashed-password", DateOfBirth: 1990-05-15, CreatedAt ≈ UtcNow }` | ✅ Passed | 3 ms |

### CreateResumeRequest → Resume

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 12 | `ToResume_MapsAllFieldsWithDraftStatus` | `CreateResumeRequest { TemplateId:3, RawData:"{\"name\":\"Test\"}" }`, `userId` | `Resume { Id != Guid.Empty, UserId, TemplateId:3, Status:Draft, RawData, CreatedAt ≈ UtcNow }` | ✅ Passed | 9 ms |

---

## 9. Stub Services Tests

### StubAiService Tests

**File:** `Utils/StubServicesTests.cs`  
**Class:** `NexaCV.Tests.Utils.StubAiServiceTests`  
**Tests:** 4 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GenerateAsync_WrapsRawDataInRootObject` | `rawData = "{\"name\":\"John Doe\",\"title\":\"Engineer\"}"` | `FinalDataJson` contains `"settings"` and `"content"` keys; `AiAvailable = false` | ✅ Passed | 4 ms |
| 2 | `GenerateAsync_IncludesDefaultSettings` | `rawData = "{}"` | `FinalDataJson` contains `"SUMMARY"`, `"BULLET"`, `"GRID"` | ✅ Passed | 62 ms |
| 3 | `GenerateAsync_WithInvalidJson_DoesNotThrow` | `rawData = "not valid json {{"` | No exception thrown; result is non-null | ✅ Passed | 13 ms |
| 4 | `RegenerateAsync_ReturnsUserPromptAsUpdatedContent` | `AiRegenerateContext { SectionIdentifier:"summary", UserPrompt:"Be concise and achievement-focused" }` | `AiRegenerationResult { UpdatedContent = "Be concise and achievement-focused", AiAvailable = false }` | ✅ Passed | 9 ms |

### StubCurrencyService Tests

**File:** `Utils/StubServicesTests.cs`  
**Class:** `NexaCV.Tests.Utils.StubCurrencyServiceTests`  
**Tests:** 7 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GetExchangeRateAsync_KnownCurrency_USD` | `currency = "USD"` | `1.00` | ✅ Passed | 1 ms |
| 2 | `GetExchangeRateAsync_KnownCurrency_EGP` | `currency = "EGP"` | `50.00` | ✅ Passed | < 1 ms |
| 3 | `GetExchangeRateAsync_KnownCurrency_EUR` | `currency = "EUR"` | `0.92` | ✅ Passed | < 1 ms |
| 4 | `GetExchangeRateAsync_CaseInsensitive_usd` | `currency = "usd"` | Rate `> 0` (case-insensitive lookup) | ✅ Passed | < 1 ms |
| 5 | `GetExchangeRateAsync_CaseInsensitive_Egp` | `currency = "Egp"` | Rate `> 0` | ✅ Passed | < 1 ms |
| 6 | `GetExchangeRateAsync_CaseInsensitive_EUR` | `currency = "EUR"` | Rate `> 0` | ✅ Passed | 4 ms |
| 7 | `GetExchangeRateAsync_UnknownCurrency_ThrowsInvalidOperationException` | `currency = "XYZ"` | Throws `InvalidOperationException` with message containing `"XYZ"` | ✅ Passed | 50 ms |
| 8 | `GetExchangeRateAsync_SecondCall_ReturnsCachedValue` | Same `currency = "USD"` called twice | Both return identical value (from cache) | ✅ Passed | 1 ms |

### StubPaymentGateway Tests

**File:** `Utils/StubServicesTests.cs`  
**Class:** `NexaCV.Tests.Utils.StubPaymentGatewayTests`  
**Tests:** 5 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `GatewayName_IsStub` | — | `GatewayName = "Stub"` | ✅ Passed | < 1 ms |
| 2 | `SupportedCurrency_IsWildcard` | — | `SupportedCurrency = "*"` | ✅ Passed | < 1 ms |
| 3 | `CreateSessionAsync_ReturnsUrlContainingTransactionId` | `PaymentRequest { TransactionId = Guid.NewGuid(), Amount:100, Currency:"USD" }` | `PaymentSessionResult { PaymentUrl contains txId, GatewayRefId = txId.ToString() }` | ✅ Passed | 26 ms |
| 4 | `VerifyWebhookSignature_WithXStubRefHeader_ReturnsTrue` | `HttpRequest` with header `X-Stub-Ref: "ref-abc-123"` | Returns `true`; `eventType = "checkout.completed"`, `gatewayRefId = "ref-abc-123"` | ✅ Passed | < 1 ms |
| 5 | `VerifyWebhookSignature_WithoutXStubRefHeader_ReturnsFalse` | `HttpRequest` with no `X-Stub-Ref` header | Returns `false`; `gatewayRefId = ""` | ✅ Passed | < 1 ms |

### PaymentGatewayFactory Tests

**File:** `Utils/StubServicesTests.cs`  
**Class:** `NexaCV.Tests.Utils.PaymentGatewayFactoryTests`  
**Tests:** 4 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `Resolve_WithWildcardGateway_ReturnsGateway` | `currency = "USD"`, factory contains `StubPaymentGateway` (`SupportedCurrency = "*"`) | Returns instance of `StubPaymentGateway` | ✅ Passed | 4 ms |
| 2 | `Resolve_WithNoMatchingGateway_ThrowsInvalidOperationException` | `currency = "UNSUPPORTED"`, empty gateway list | Throws `InvalidOperationException` with message containing `"UNSUPPORTED"` | ✅ Passed | 20 ms |
| 3 | `ResolveByRequest_WithMatchingWebhookHeader_ReturnsGateway` | `HttpRequest` with `X-Stub-Ref` header | Returns `StubPaymentGateway` | ✅ Passed | 5 ms |
| 4 | `ResolveByRequest_WithNoMatchingGateway_ThrowsInvalidOperationException` | `HttpRequest` without `X-Stub-Ref` header (stub signature fails) | Throws `InvalidOperationException` | ✅ Passed | 61 ms |

---

## 10. Validator Tests

### RegisterRequestValidator Tests

**File:** `Validators/ValidatorTests.cs`  
**Class:** `NexaCV.Tests.Validators.RegisterRequestValidatorTests`  
**Tests:** 12 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `ValidRequest_PassesValidation` | `RegisterRequest { FirstName:"John", LastName:"Doe", Username:"johndoe", Email:"john@example.com", Password:"P@ssw0rd!" }` | No validation errors | ✅ Passed | < 1 ms |
| 2 | `FirstName_Empty_FailsValidation` | `FirstName = ""` | Validation error on `FirstName` | ✅ Passed | < 1 ms |
| 3 | `FirstName_TooLong_FailsValidation` | `FirstName = "A" × 51` | Validation error on `FirstName` (max 50) | ✅ Passed | 85 ms |
| 4 | `LastName_Empty_FailsValidation` | `LastName = ""` | Validation error on `LastName` | ✅ Passed | < 1 ms |
| 5 | `Username_Empty_FailsValidation` | `Username = ""` | Validation error on `Username` | ✅ Passed | < 1 ms |
| 6 | `Username_TooLong_FailsValidation` | `Username = "u" × 51` | Validation error on `Username` (max 50) | ✅ Passed | < 1 ms |
| 7 | `Email_Empty_FailsValidation` | `Email = ""` | Validation error on `Email` | ✅ Passed | 1 ms |
| 8 | `Email_InvalidFormat_FailsValidation` | `Email = "not-an-email"` | Validation error on `Email` | ✅ Passed | 1 ms |
| 9 | `Email_TooLong_FailsValidation` | `Email = "a" × 140 + "@example.com"` (> 150 chars) | Validation error on `Email` (max 150) | ✅ Passed | < 1 ms |
| 10 | `Password_TooShort_FailsValidation` | `Password = "Ab@1"` (4 chars) | Validation error on `Password` (min 8) | ✅ Passed | 1 ms |
| 11 | `Password_NoSpecialChar_FailsValidation` | `Password = "Password1"` (no special char) | Validation error: `"Password must contain at least one special character."` | ✅ Passed | 7 ms |
| 12 | `Password_WithSpecialChar_PassesValidation` | `Password = "Password1!"` | No validation error on `Password` | ✅ Passed | 133 ms |

### LoginRequestValidator Tests

**File:** `Validators/ValidatorTests.cs`  
**Class:** `NexaCV.Tests.Validators.LoginRequestValidatorTests`  
**Tests:** 4 | **All Passed**

| # | Test Name | Input | Expected Output | Result | Time |
|---|---|---|---|---|---|
| 1 | `ValidRequest_PassesValidation` | `LoginRequest { Email:"john@example.com", Password:"P@ssw0rd!" }` | No validation errors | ✅ Passed | < 1 ms |
| 2 | `Email_Empty_FailsValidation` | `Email = ""` | Validation error on `Email` | ✅ Passed | < 1 ms |
| 3 | `Email_InvalidFormat_FailsValidation` | `Email = "bad-email"` | Validation error on `Email` | ✅ Passed | < 1 ms |
| 4 | `Password_Empty_FailsValidation` | `Password = ""` | Validation error on `Password` | ✅ Passed | 4 ms |

---

## Summary by Test Class

| Test Class | File | Tests | Passed | Failed | Total Time |
|---|---|---|---|---|---|
| AuthServiceTests | Services/AuthServiceTests.cs | 6 | 6 | 0 | ~1.4 s |
| UserServiceTests | Services/UserServiceTests.cs | 5 | 5 | 0 | ~807 ms |
| ResumeServiceTests | Services/ResumeServiceTests.cs | 11 | 11 | 0 | ~268 ms |
| TemplateServiceTests | Services/TemplateServiceTests.cs | 5 | 5 | 0 | ~22 ms |
| TransactionServiceTests | Services/TransactionServiceTests.cs | 9 | 9 | 0 | ~71 ms |
| RegenerationServiceTests | Services/RegenerationServiceTests.cs | 7 | 7 | 0 | ~342 ms |
| JwtServiceTests | Utils/JwtServiceTests.cs | 7 | 7 | 0 | ~1.2 s |
| MappingExtensionsTests | Utils/MappingExtensionsTests.cs | 13 | 13 | 0 | ~33 ms |
| StubAiServiceTests | Utils/StubServicesTests.cs | 4 | 4 | 0 | ~88 ms |
| StubCurrencyServiceTests | Utils/StubServicesTests.cs | 7 | 7 | 0 | ~57 ms |
| StubPaymentGatewayTests | Utils/StubServicesTests.cs | 5 | 5 | 0 | ~27 ms |
| PaymentGatewayFactoryTests | Utils/StubServicesTests.cs | 4 | 4 | 0 | ~90 ms |
| RegisterRequestValidatorTests | Validators/ValidatorTests.cs | 12 | 12 | 0 | ~228 ms |
| LoginRequestValidatorTests | Validators/ValidatorTests.cs | 4 | 4 | 0 | ~5 ms |
| **TOTAL** | | **107** | **107** | **0** | **~3.40 s** |

> BCrypt password hashing accounts for the longer times in Auth and User tests (~400–535 ms per test), which is expected behaviour — BCrypt is intentionally slow to resist brute-force attacks.
