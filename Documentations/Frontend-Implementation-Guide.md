# NexaCV — Frontend Implementation Guide

> **Complete reference for building the frontend against the NexaCV backend API.**
> Covers every endpoint, all request/response schemas, authentication, error handling, resume lifecycle, and practical integration notes.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Architecture Summary](#2-architecture-summary)
3. [Base URLs & Environment](#3-base-urls--environment)
4. [Authentication](#4-authentication)
5. [Error Handling](#5-error-handling)
6. [Enums Reference](#6-enums-reference)
7. [Endpoints](#7-endpoints)
   - [7.1 Auth](#71-auth)
   - [7.2 Users](#72-users)
   - [7.3 Templates](#73-templates)
   - [7.4 Resumes](#74-resumes)
   - [7.5 Transactions](#75-transactions)
   - [7.6 Webhooks](#76-webhooks)
8. [Data Schemas (DTOs & JSON)](#8-data-schemas-dtos--json)
   - [8.1 Auth Schemas](#81-auth-schemas)
   - [8.2 User Schemas](#82-user-schemas)
   - [8.3 Template Schemas](#83-template-schemas)
   - [8.4 Resume Schemas](#84-resume-schemas)
   - [8.5 Transaction Schemas](#85-transaction-schemas)
   - [8.6 Error Schemas](#86-error-schemas)
9. [Resume Lifecycle](#9-resume-lifecycle)
10. [FinalData JSON Structure](#10-finaldata-json-structure)
11. [AI Regeneration Details](#11-ai-regeneration-details)
12. [Payment Flow](#12-payment-flow)
13. [Seeded Data (Templates)](#13-seeded-data-templates)
14. [Frontend Integration Checklist](#14-frontend-integration-checklist)
15. [JWT Token Details](#15-jwt-token-details)
16. [CORS Configuration](#16-cors-configuration)

---

## 1. Project Overview

**NexaCV** is an AI-powered resume builder platform. Users:

1. Register / log in
2. Browse resume templates
3. Fill in a multi-step wizard form (personal info, experience, education, skills, etc.)
4. Submit the wizard → the backend calls an AI service that enhances the content
5. View the AI-polished resume (`finalData`)
6. Optionally regenerate individual sections using additional AI calls (limited to **3 per section**)
7. Pay for the resume via a payment gateway
8. Download the resume as PDF / DOCX (renderer not yet implemented — **501**)

### Key Technical Details

| Item | Value |
|------|-------|
| Backend framework | ASP.NET Core 9 Minimal APIs |
| Auth | JWT Bearer (HS256), 24-hour lifetime |
| Database | In-memory EF Core (data resets on server restart) |
| AI | Stub implementation (echoes input with "AI-Polished: " prefix) |
| Payment | Stub gateway (returns a local URL immediately) |
| Currency | Stub rates (configurable via `appsettings.json`) |
| CORS allowed origin | `http://localhost:3000` |

---

## 2. Architecture Summary

```
Frontend (React/Next.js, port 3000)
        │
        │  REST / JSON over HTTP
        ▼
NexaCV.Api  (ASP.NET Core, port 5166 / 7044)
        │
        ├── Auth, Users, Templates, Resumes, Transactions, Webhooks
        │
        ├── StubAiService  ──(dev)──▶  NexaCV.AiMock (port 5001)
        │                               (real AI mock server)
        │
        ├── StubPaymentGateway  (local stub — no real gateway)
        │
        └── In-Memory EF Core DB
```

---

## 3. Base URLs & Environment

| Environment | HTTP | HTTPS |
|-------------|------|-------|
| Development | `http://localhost:5166` | `https://localhost:7044` |
| Swagger UI | `http://localhost:5166/swagger` | — |

All API routes are prefixed with `/api`.

### Environment Variables (`.env.example`)

The backend uses `appsettings.json`. The frontend should configure its own `.env`:

```env
NEXT_PUBLIC_API_URL=http://localhost:5166
```

---

## 4. Authentication

### Overview

All protected endpoints require a **Bearer JWT** in the `Authorization` header.

```
Authorization: Bearer <token>
```

### Obtaining a Token

Call `POST /api/auth/register` or `POST /api/auth/login`. Both return an `AuthResponse` containing the token.

### Token Properties

| Property | Value |
|----------|-------|
| Algorithm | HS256 |
| Issuer | `NexaCV` |
| Audience | `NexaCV` |
| Lifetime | 86 400 seconds (24 hours) |
| Claims | `sub` (userId as GUID), `email`, `jti` |

### Protected vs Public Endpoints

| Group | Auth Required |
|-------|--------------|
| `POST /api/auth/register` | No |
| `POST /api/auth/login` | No |
| `POST /api/auth/logout` | **Yes** |
| `GET /api/templates` | No |
| `GET /api/templates/{id}` | No |
| `POST /api/webhooks/payment` | No (gateway signature auth) |
| All `/api/users/*` | **Yes** |
| All `/api/resumes/*` | **Yes** |
| All `/api/transactions/*` | **Yes** |

### Frontend Storage Recommendation

Store the JWT in memory or `httpOnly` cookie. Avoid `localStorage` for security. Refresh by re-logging in (no refresh token endpoint exists).

---

## 5. Error Handling

All errors follow a consistent JSON shape.

### Standard Error (4xx / 5xx)

```json
{
  "status": 404,
  "error": "Resume not found."
}
```

### Validation Error (422)

```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "RawData.Content.Personal.FirstName", "message": "First name is required." },
    { "field": "RawData.Content.Personal.Email", "message": "A valid email address is required." }
  ]
}
```

### HTTP Status Code Reference

| Code | Meaning | When |
|------|---------|------|
| 200 | OK | Successful GET / PUT / POST (checkout, login) |
| 201 | Created | Register, create resume |
| 204 | No Content | Logout, delete resume |
| 400 | Bad Request | Invalid operation (e.g. deleting PAID resume, DOCX unsupported) |
| 401 | Unauthorized | Missing / invalid / expired token or bad credentials |
| 403 | Forbidden | Resource belongs to another user, or resume not PAID for download |
| 404 | Not Found | Resource does not exist or is soft-deleted |
| 409 | Conflict | Email/username already taken, pending transaction already exists |
| 422 | Unprocessable Entity | Validation failure (FluentValidation) |
| 429 | Too Many Requests | Regeneration limit (3 per section) exceeded |
| 500 | Internal Server Error | Unexpected server-side error |
| 501 | Not Implemented | Download endpoint (PDF renderer not wired) |

---

## 6. Enums Reference

All enums are serialized as **strings** (not integers) in JSON.

### `ResumeStatus`

| Value | Meaning |
|-------|---------|
| `DRAFT` | Resume created but AI generation running (transient — immediately becomes COMPLETED) |
| `COMPLETED` | AI generation done. Ready for editing / regeneration / checkout |
| `PAID` | Payment confirmed. Ready for download |

> **Note:** The API returns these as uppercase strings: `"COMPLETED"`, `"PAID"`, `"DRAFT"`.

### `PaymentStatus`

| Value | Meaning |
|-------|---------|
| `PENDING` | Transaction created, waiting for gateway confirmation |
| `SUCCESS` | Payment confirmed via webhook |
| `FAILED` | Payment failed |

### `SummaryType`

| Value | Meaning |
|-------|---------|
| `Summary` | Professional summary section |
| `Objective` | Career objective section |

### `DescriptionFormat`

| Value | Meaning |
|-------|---------|
| `Paragraph` | Description text as prose |
| `Bulleted` | Description as bullet points |

### `ActionType` (audit log, not exposed to frontend)

| Value |
|-------|
| `Login` |
| `Logout` |
| `PasswordUpdated` |

---

## 7. Endpoints

---

### 7.1 Auth

#### `POST /api/auth/register`

Register a new user account.

**Auth required:** No

**Request Body:**

```json
{
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "P@ssw0rd!",
  "dateOfBirth": "1995-06-15"
}
```

| Field | Type | Required | Rules |
|-------|------|----------|-------|
| `firstName` | string | Yes | Max 50 chars |
| `lastName` | string | Yes | Max 50 chars |
| `username` | string | Yes | Max 50 chars, unique |
| `email` | string | Yes | Valid email, max 150 chars, unique |
| `password` | string | Yes | Min 8, max 128 chars; must contain uppercase, digit, special char |
| `dateOfBirth` | string (`YYYY-MM-DD`) | No | Optional |

**Success Response — `201 Created`:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400
}
```

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 409 | Email or username already taken |
| 422 | Validation failure |

---

#### `POST /api/auth/login`

Authenticate an existing user.

**Auth required:** No

**Request Body:**

```json
{
  "email": "john.doe@example.com",
  "password": "P@ssw0rd!"
}
```

| Field | Type | Required |
|-------|------|----------|
| `email` | string | Yes |
| `password` | string | Yes |

**Success Response — `200 OK`:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 86400
}
```

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Invalid email or password (deliberately ambiguous) |
| 422 | Validation failure |

---

#### `POST /api/auth/logout`

Log out the current user. Records an audit movement.

**Auth required:** Yes

**Request Body:** None

**Success Response — `204 No Content`**

> The JWT is **not** server-side invalidated. The client must discard the token.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |

---

### 7.2 Users

#### `GET /api/users/me`

Get the current authenticated user's profile.

**Auth required:** Yes

**Success Response — `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLogin": "2024-05-01T08:00:00Z"
}
```

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |

---

#### `PUT /api/users/me`

Partially update the current user's profile.

**Auth required:** Yes

**Request Body** (all fields optional — only non-null fields are applied):

```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "username": "janesmith",
  "password": "N3wP@ss!"
}
```

| Field | Type | Notes |
|-------|------|-------|
| `firstName` | string? | Leave `null` to keep unchanged |
| `lastName` | string? | Leave `null` to keep unchanged |
| `username` | string? | Must be unique |
| `password` | string? | Re-hashed with BCrypt; logs `PASSWORD_UPDATED` movement |

**Success Response — `200 OK`:** Returns the updated `UserProfileDto` (same shape as GET /me).

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |

---

### 7.3 Templates

#### `GET /api/templates`

List all active resume templates.

**Auth required:** No

**Query Parameters:**

| Parameter | Type | Notes |
|-----------|------|-------|
| `industryCategory` | string? | Filter by category (e.g. `Corporate`, `Creative`) |

**Success Response — `200 OK`:**

```json
[
  {
    "id": 1,
    "name": "Modern Minimalist",
    "industryCategory": "Corporate",
    "basePriceUsd": 3.00,
    "supportsWord": true
  },
  {
    "id": 2,
    "name": "Creative",
    "industryCategory": "Creative",
    "basePriceUsd": 3.00,
    "supportsWord": false
  },
  {
    "id": 3,
    "name": "Executive",
    "industryCategory": "Corporate",
    "basePriceUsd": 3.75,
    "supportsWord": true
  }
]
```

---

#### `GET /api/templates/{id}`

Get a single template by its integer ID.

**Auth required:** No

**Path Parameter:** `id` — integer template ID

**Success Response — `200 OK`:** Single `TemplateDto` object (same shape as above).

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 404 | Template not found or inactive |

---

### 7.4 Resumes

All resume endpoints require authentication. Users can only access their own resumes.

---

#### `POST /api/resumes`

Create a new resume from wizard form data.

**Auth required:** Yes

**Request Body:**

```json
{
  "templateId": 1,
  "rawData": {
    "settings": {
      "summaryType": "Summary",
      "descriptionFormat": "Bulleted"
    },
    "content": {
      "personal": {
        "firstName": "John",
        "middleName": "Alexander",
        "lastName": "Doe",
        "email": "john.doe@example.com",
        "phone": "+201012345678",
        "location": "Cairo, Egypt",
        "zipCode": "11511",
        "dateOfBirth": "1995-05-15",
        "linkedinUrl": "linkedin.com/in/johndoe",
        "siteUrl": null
      },
      "summary": "Results-driven Senior Software Engineer with 5+ years of experience...",
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "Led migration of monolithic API to microservices architecture."
        }
      ],
      "education": [
        {
          "id": "edu_001",
          "institution": "Cairo University",
          "degree": "B.Sc. in Computer Science",
          "fieldOfStudy": "Computer Science",
          "grade": "3.7 / 4.0",
          "startDate": "2015-09",
          "endDate": "2019-06"
        }
      ],
      "courses": [
        {
          "id": "crs_001",
          "name": "Cloud Computing Architecture",
          "provider": "Coursera",
          "date": "2023-01"
        }
      ],
      "skills": ["C#", ".NET 9", "ASP.NET Core", "React", "SQL Server", "Azure", "Docker"]
    }
  }
}
```

**Validation Rules:**

| Field | Required | Rule |
|-------|----------|------|
| `templateId` | Yes | Must be a valid active template ID |
| `rawData.content.personal.firstName` | Yes | Not empty |
| `rawData.content.personal.lastName` | Yes | Not empty |
| `rawData.content.personal.email` | Yes | Valid email |
| `rawData.content.personal.phone` | Yes | Not empty |
| `rawData.content.personal.location` | Yes | Not empty |
| `rawData.content.experience` | Yes | At least one entry |
| `experience[].title` | Yes | Not empty |
| `experience[].company` | Yes | Not empty |
| `experience[].description` | Yes | Not empty |
| `education[].institution` | Yes (if present) | Not empty |
| `education[].degree` | Yes (if present) | Not empty |
| `education[].endDate` | Yes (if present) | Not empty |

**Success Response — `201 Created`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": { ... },
  "finalData": {
    "settings": {
      "summaryType": "Summary",
      "descriptionFormat": "Bulleted"
    },
    "content": {
      "personal": { ... },
      "summary": "AI-Polished: Results-driven Senior Software Engineer...",
      "experience": [ ... ],
      "education": [ ... ],
      "courses": [ ... ],
      "skills": ["C#", ".NET 9", "ASP.NET Core", ...]
    }
  },
  "aiAvailable": false,
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-05-01T10:00:00Z",
  "jobTitleSuggestions": [
    { "title": "Senior Backend Developer", "score": 10 },
    { "title": "Software Architect", "score": 9 },
    { "title": "Senior Software Engineer", "score": 9 },
    { "title": "Solutions Architect", "score": 8 }
  ],
  "skillSuggestions": [
    "MediatR", "CQRS", "Domain-Driven Design", "xUnit",
    "FluentValidation", "Entity Framework Core", "SignalR",
    "Azure Functions", "Azure Service Bus", "Kubernetes"
  ]
}
```

> **Important:** `jobTitleSuggestions` and `skillSuggestions` are **ephemeral** — they are only returned in this response and are never stored in the database. Save them client-side if you want to show them later.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |
| 404 | Template not found |
| 422 | Validation failure |

---

#### `GET /api/resumes`

List all resumes for the authenticated user.

**Auth required:** Yes

**Success Response — `200 OK`:**

```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "status": "COMPLETED",
    "templateName": "Modern Minimalist",
    "createdAt": "2024-05-01T10:00:00Z"
  }
]
```

> Soft-deleted resumes are excluded automatically.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |

---

#### `GET /api/resumes/{id}`

Get full resume detail including `rawData` and `finalData`.

**Auth required:** Yes

**Path Parameter:** `id` — GUID

**Success Response — `200 OK`:** `ResumeDetailDto` (same shape as POST response, without `jobTitleSuggestions` / `skillSuggestions`).

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |
| 403 | Resume belongs to a different user |
| 404 | Resume not found or soft-deleted |

---

#### `PUT /api/resumes/{id}`

Replace the resume's `finalData` JSON (manual edit by the user).

**Auth required:** Yes

**Path Parameter:** `id` — GUID

**Request Body:**

```json
{
  "finalData": {
    "settings": {
      "summaryType": "Summary",
      "descriptionFormat": "Bulleted"
    },
    "content": {
      "personal": { ... },
      "summary": "Updated summary text...",
      "experience": [ ... ],
      "education": [ ... ],
      "skills": [ ... ]
    }
  }
}
```

> The `finalData` object must conform to the `{ settings: {...}, content: {...} }` structure (see [Section 10](#10-finaldata-json-structure)).

**Success Response — `200 OK`:** Updated `ResumeDetailDto`.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |
| 403 | Resume belongs to a different user |

---

#### `DELETE /api/resumes/{id}`

Soft-delete a resume.

**Auth required:** Yes

**Path Parameter:** `id` — GUID

**Success Response — `204 No Content`**

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 400 | Resume status is `PAID` (cannot delete) |
| 401 | Missing or invalid token |
| 403 | Resume belongs to a different user |

---

#### `POST /api/resumes/{id}/regenerate`

Regenerate a single section of a resume using AI.

**Auth required:** Yes

**Path Parameter:** `id` — GUID (resume ID)

**Request Body:**

```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Make it more concise and achievement-focused",
  "targetFormat": "Paragraph",
  "newTitleSuggestion": "Senior Cloud Architect"
}
```

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| `sectionIdentifier` | string | Yes | Key in `finalData.content` (e.g. `summary`, `experience`, `skills`) **or** an entry ID (e.g. `exp_001`) to regenerate a single experience entry |
| `userPrompt` | string | Yes | Instruction for the AI |
| `targetFormat` | string? | No | `"Paragraph"`, `"Bulleted"` (for summary/experience), `"GRID"`, `"LIST"` (for skills). Updates `finalData.settings` |
| `newTitleSuggestion` | string? | No | Hint for the AI — aligns output to this job title |

**Section Identifier Values:**

| Value | Regenerates |
|-------|-------------|
| `summary` | The summary/objective text |
| `experience` | All experience descriptions |
| `exp_001`, `exp_002`, etc. | A single experience entry's description |
| `skills` | The skills list |

**Success Response — `200 OK`:**

```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "AI-Polished: Results-driven engineer...",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": false
}
```

| Field | Type | Notes |
|-------|------|-------|
| `sectionIdentifier` | string | Echo of the request |
| `updatedContent` | any | New content (string, array, or object depending on section) |
| `regenCountUsed` | int | Total times this section has been regenerated (max 3) |
| `regenCountRemaining` | int | `3 - regenCountUsed` |
| `addedCostUsd` | decimal | Always `0.25` USD per call |
| `aiAvailable` | bool | `false` while stub AI is active |

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |
| 403 | Resume belongs to a different user |
| 422 | Validation failure (`sectionIdentifier` or `userPrompt` empty) |
| 429 | This section has already been regenerated 3 times |

---

#### `GET /api/resumes/{id}/download`

Download a paid resume as PDF or DOCX.

**Auth required:** Yes

**Path Parameter:** `id` — GUID

**Query Parameter:** `format` — `pdf` (default) or `docx`

**Current Status:** Always returns `501 Not Implemented`. The endpoint validates ownership and payment status, records the download attempt, but the PDF renderer is not yet wired in.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 400 | `docx` requested for a template that does not support Word |
| 401 | Missing or invalid token |
| 403 | Resume is not `PAID` or belongs to another user |
| 501 | Not implemented (expected in current build) |

---

### 7.5 Transactions

#### `POST /api/transactions/checkout`

Initiate payment for a `COMPLETED` resume.

**Auth required:** Yes

**Request Body:**

```json
{
  "resumeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currency": "EGP"
}
```

| Field | Type | Notes |
|-------|------|-------|
| `resumeId` | GUID | Must belong to the authenticated user and be `COMPLETED` |
| `currency` | string | `"EGP"` or `"USD"` (ISO 4217) |

**Price Calculation:**

```
totalAmount = (template.basePriceUsd × exchangeRate) + (regenCount × 0.25 × exchangeRate)
```

**Stub exchange rates:**

| Currency | Rate (vs USD) |
|----------|--------------|
| USD | 1.00 |
| EGP | 50.00 |
| EUR | 0.92 |
| GBP | 0.79 |
| SAR | 3.75 |
| AED | 3.67 |

**Success Response — `200 OK`:**

```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentUrl": "https://stub.payment/session/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "baseAmount": 150.00,
  "regenAmount": 25.00,
  "totalAmount": 175.00,
  "currency": "EGP",
  "exchangeRateUsed": 50.00
}
```

> **Frontend action:** Redirect the user to `paymentUrl` to complete payment. After payment, the gateway posts to the webhook which marks the resume as `PAID`.

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 400 | Resume is not `COMPLETED` (e.g. is `DRAFT` or `PAID`) |
| 401 | Missing or invalid token |
| 403 | Resume belongs to a different user |
| 409 | A pending transaction already exists for this resume |

---

#### `GET /api/transactions/{id}`

Get a single transaction by ID. Use this to poll payment status.

**Auth required:** Yes

**Path Parameter:** `id` — GUID

**Success Response — `200 OK`:**

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resumeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 175.00,
  "currency": "EGP",
  "exchangeRateUsed": 50.00,
  "paymentStatus": "SUCCESS",
  "createdAt": "2024-05-01T10:00:00Z",
  "completedAt": "2024-05-01T10:05:00Z"
}
```

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 401 | Missing or invalid token |
| 403 | Transaction belongs to a different user |
| 404 | Transaction not found |

---

### 7.6 Webhooks

#### `POST /api/webhooks/payment`

Receives payment confirmation from the payment gateway. **Do not call this from the frontend.**

**Auth required:** No (uses gateway-specific signature verification)

**Stub Testing:** To simulate a successful payment in development, POST to this endpoint with the header:

```
X-Stub-Ref: <transactionId>
```

Where `<transactionId>` is the value from `CheckoutResponse.transactionId`.

**Effect:** Transitions the transaction to `SUCCESS` and the resume to `PAID`.

**Success Response — `200 OK`**

**Error Responses:**

| Status | Condition |
|--------|-----------|
| 400 | Invalid / unrecognized webhook signature |

---

## 8. Data Schemas (DTOs & JSON)

### 8.1 Auth Schemas

#### `RegisterRequest`

```json
{
  "firstName": "John",          // string, required, max 50
  "lastName": "Doe",            // string, required, max 50
  "username": "johndoe",        // string, required, max 50, unique
  "email": "john@example.com",  // string, required, valid email, max 150, unique
  "password": "P@ssw0rd!",      // string, required, min 8, max 128, uppercase + digit + special
  "dateOfBirth": "1995-06-15"   // string (YYYY-MM-DD), optional
}
```

#### `LoginRequest`

```json
{
  "email": "john@example.com",  // string, required
  "password": "P@ssw0rd!"       // string, required
}
```

#### `AuthResponse`

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",  // GUID
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...", // JWT string
  "expiresIn": 86400                                   // seconds (24h)
}
```

---

### 8.2 User Schemas

#### `UserProfileDto`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLogin": "2024-05-01T08:00:00Z"  // null if never logged in after register
}
```

#### `UpdateUserRequest`

```json
{
  "firstName": "Jane",      // string?, null = keep unchanged
  "lastName": "Smith",      // string?, null = keep unchanged
  "username": "janesmith",  // string?, null = keep unchanged
  "password": "N3wP@ss!"    // string?, null = keep unchanged; triggers PASSWORD_UPDATED audit
}
```

---

### 8.3 Template Schemas

#### `TemplateDto`

```json
{
  "id": 1,                          // int
  "name": "Modern Minimalist",      // string
  "industryCategory": "Corporate",  // string?, null = generic
  "basePriceUsd": 3.00,             // decimal (base price in USD)
  "supportsWord": true              // bool (whether DOCX export is supported)
}
```

---

### 8.4 Resume Schemas

#### `ResumeSummaryDto` (list view)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "COMPLETED",
  "templateName": "Modern Minimalist",
  "createdAt": "2024-05-01T10:00:00Z"
}
```

#### `ResumeDetailDto` (full view)

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": { /* RawResumeData object */ },
  "finalData": { /* FinalData object — see Section 10 */ },
  "aiAvailable": false,
  "createdAt": "2024-05-01T10:00:00Z",
  "updatedAt": "2024-05-01T10:00:00Z",
  "jobTitleSuggestions": [           // only on POST /api/resumes
    { "title": "Senior Backend Developer", "score": 10 },
    { "title": "Software Architect", "score": 9 }
  ],
  "skillSuggestions": [              // only on POST /api/resumes
    "MediatR", "CQRS", "Domain-Driven Design"
  ]
}
```

#### `CreateResumeRequest` — Full Schema

```json
{
  "templateId": 1,
  "rawData": {
    "settings": {
      "summaryType": "Summary",       // "Summary" | "Objective"
      "descriptionFormat": "Bulleted" // "Paragraph" | "Bulleted"
    },
    "content": {
      "personal": {
        "firstName": "John",          // required
        "middleName": "Alexander",    // optional
        "lastName": "Doe",            // required
        "email": "john@example.com",  // required
        "phone": "+201012345678",     // required
        "location": "Cairo, Egypt",   // required
        "zipCode": "11511",           // optional
        "dateOfBirth": "1995-05-15",  // optional
        "linkedinUrl": "linkedin.com/in/johndoe",  // optional
        "siteUrl": "johndoe.dev"      // optional
      },
      "summary": "Results-driven engineer...",  // optional
      "experience": [                           // required, min 1 entry
        {
          "id": "exp_001",                      // optional (client-generated)
          "title": "Senior Software Engineer",  // required
          "company": "TechFlow Systems",        // required
          "startDate": "2021-03",               // optional (YYYY-MM format)
          "endDate": "2023-10",                 // optional; null = current
          "description": "Led migration..."     // required
        }
      ],
      "education": [                            // optional
        {
          "id": "edu_001",
          "institution": "Cairo University",    // required if entry present
          "degree": "B.Sc. in Computer Science",// required if entry present
          "fieldOfStudy": "Computer Science",   // optional
          "grade": "3.7 / 4.0",                 // optional
          "startDate": "2015-09",               // optional
          "endDate": "2019-06"                  // required if entry present
        }
      ],
      "courses": [                              // optional
        {
          "id": "crs_001",
          "name": "Cloud Computing Architecture",  // optional
          "provider": "Coursera",                  // optional
          "date": "2023-01"                        // optional (YYYY-MM)
        }
      ],
      "skills": ["C#", ".NET 9", "React"]      // optional; array of strings
    }
  }
}
```

#### `UpdateFinalDataRequest`

```json
{
  "finalData": {
    "settings": { ... },
    "content": { ... }
  }
}
```

#### `RegenerateRequest`

```json
{
  "sectionIdentifier": "summary",                  // required
  "userPrompt": "Make it more concise",            // required
  "targetFormat": "Paragraph",                     // optional
  "newTitleSuggestion": "Senior Cloud Architect"   // optional
}
```

#### `RegenerateResponse`

```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Polished summary text...",    // string | array | object
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": false
}
```

---

### 8.5 Transaction Schemas

#### `CheckoutRequest`

```json
{
  "resumeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "currency": "EGP"
}
```

#### `CheckoutResponse`

```json
{
  "transactionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "paymentUrl": "https://stub.payment/session/3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "baseAmount": 150.00,
  "regenAmount": 25.00,
  "totalAmount": 175.00,
  "currency": "EGP",
  "exchangeRateUsed": 50.00
}
```

#### `TransactionDto`

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "resumeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "totalAmount": 175.00,
  "currency": "EGP",
  "exchangeRateUsed": 50.00,
  "paymentStatus": "SUCCESS",           // "PENDING" | "SUCCESS" | "FAILED"
  "createdAt": "2024-05-01T10:00:00Z",
  "completedAt": "2024-05-01T10:05:00Z" // null while PENDING
}
```

---

### 8.6 Error Schemas

#### Standard Error (`ApiErrorResponse`)

```json
{
  "status": 404,
  "error": "Resume not found."
}
```

#### Validation Error (`ValidationErrorResponse`)

```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "RawData.Content.Personal.FirstName", "message": "First name is required." },
    { "field": "RawData.Content.Personal.Email", "message": "A valid email address is required." }
  ]
}
```

---

## 9. Resume Lifecycle

```
POST /api/resumes
        │
        ▼
     [DRAFT] ──▶ AI generation (stub) ──▶ [COMPLETED]
                                                │
                        ┌───────────────────────┤
                        │                       │
                   Regenerate              Checkout
             (up to 3× per section)   POST /api/transactions/checkout
                        │                       │
                        └──────────┬────────────┘
                                   │
                              [COMPLETED]
                                   │
                         Webhook confirms payment
                         POST /api/webhooks/payment
                                   │
                                   ▼
                              [PAID] ──▶ Download (501 pending)
```

### Status Transition Rules

| From | To | Trigger |
|------|----|---------|
| — | `COMPLETED` | `POST /api/resumes` (DRAFT is transient) |
| `COMPLETED` | `PAID` | `POST /api/webhooks/payment` (after successful payment) |
| `PAID` | — | No further transitions |
| `COMPLETED` | deleted | `DELETE /api/resumes/{id}` |
| `DRAFT` | deleted | `DELETE /api/resumes/{id}` |
| `PAID` | — | **Cannot be deleted** → returns 400 |

---

## 10. FinalData JSON Structure

`finalData` is a JSON object with the following top-level structure. This is what gets rendered into the final PDF/DOCX.

```json
{
  "settings": {
    "summaryType": "Summary",         // "Summary" | "Objective"
    "descriptionFormat": "Bulleted",  // "Paragraph" | "Bulleted"
    "skillsFormat": "LIST"            // "LIST" | "GRID" (set by regeneration)
  },
  "content": {
    "personal": {
      "firstName": "John",
      "middleName": "Alexander",
      "lastName": "Doe",
      "email": "john.doe@example.com",
      "phone": "+201012345678",
      "location": "Cairo, Egypt",
      "zipCode": "11511",
      "dateOfBirth": "1995-05-15",
      "linkedinUrl": "linkedin.com/in/johndoe",
      "siteUrl": null
    },
    "summary": "AI-Polished: Results-driven Senior Software Engineer with 5+ years...",
    "experience": [
      {
        "id": "exp_001",
        "title": "Senior Software Engineer",
        "company": "TechFlow Systems",
        "startDate": "2021-03",
        "endDate": "2023-10",
        "description": "AI-Polished: Spearheaded the migration of a monolithic REST API..."
      }
    ],
    "education": [
      {
        "id": "edu_001",
        "institution": "Cairo University",
        "degree": "B.Sc. in Computer Science",
        "fieldOfStudy": "Computer Science",
        "grade": "3.7 / 4.0",
        "startDate": "2015-09",
        "endDate": "2019-06"
      }
    ],
    "courses": [
      {
        "id": "crs_001",
        "name": "Cloud Computing Architecture",
        "provider": "Coursera",
        "date": "2023-01"
      }
    ],
    "skills": ["C#", ".NET 9", "ASP.NET Core", "React", "SQL Server", "Azure", "Docker"]
  }
}
```

### Fields Modified by AI

The stub AI prefixes free-text fields with `"AI-Polished: "`. When a real AI is connected, these will be genuinely rewritten:

| Section | Field(s) Modified |
|---------|------------------|
| `content.summary` | The entire string |
| `content.experience[].description` | Each entry's description |
| Other fields | Left unchanged (IDs, dates, company names, etc.) |

---

## 11. AI Regeneration Details

### Regeneration Limits

- **Maximum 3 regenerations per section** per resume
- Tracked by `sectionIdentifier` + `resumeId`
- The 4th call returns **429 Too Many Requests**
- Each call adds **$0.25 USD** to the resume's total cost

### Regenerable Sections

| `sectionIdentifier` | What is rewritten |
|--------------------|------------------|
| `summary` | The `content.summary` string |
| `experience` | All entries' `description` fields |
| `exp_001`, `exp_002`, … | Only that specific entry's `description` |
| `skills` | The `content.skills` array |

### Regeneration Cost Impact on Checkout

```
regenCostUsd = number_of_regenerations × 0.25
totalUsd = template.basePriceUsd + regenCostUsd
totalInCurrency = totalUsd × exchangeRate
```

### `aiAvailable` Flag

| Value | Meaning |
|-------|---------|
| `false` | Stub AI active — content is echoed with "AI-Polished: " prefix |
| `true` | Real AI model produced the content |

---

## 12. Payment Flow

### Complete Payment Sequence (Frontend Perspective)

```
1. User clicks "Pay"
   → POST /api/transactions/checkout { resumeId, currency }
   ← { transactionId, paymentUrl, totalAmount, currency, ... }

2. Redirect user to paymentUrl
   → (User completes payment on gateway page)

3. Gateway posts to backend (backend only)
   → POST /api/webhooks/payment (with X-Stub-Ref header in dev)

4. Poll for confirmation (optional)
   → GET /api/transactions/{transactionId}
   ← { paymentStatus: "SUCCESS", completedAt: "..." }
   OR paymentStatus: "PENDING" (keep polling)

5. On SUCCESS
   → GET /api/resumes/{resumeId}
   ← { status: "PAID", ... }
   → Show download button (currently returns 501)
```

### Stub Testing (Development)

To simulate a payment callback without a real gateway:

```bash
curl -X POST http://localhost:5166/api/webhooks/payment \
  -H "X-Stub-Ref: <transactionId>"
```

This will:
1. Mark the transaction as `SUCCESS`
2. Set `completedAt` to UTC now
3. Transition the resume to `PAID`

---

## 13. Seeded Data (Templates)

The following templates are automatically seeded into the database on startup:

| ID | Name | Category | Price (USD) | Supports Word |
|----|------|----------|-------------|---------------|
| 1 | Modern Minimalist | Corporate | $3.00 | Yes |
| 2 | Creative | Creative | $3.00 | No |
| 3 | Executive | Corporate | $3.75 | Yes |

> The database is **in-memory** and resets on server restart. Templates are re-seeded automatically.

---

## 14. Frontend Integration Checklist

### Authentication Flow
- [ ] Store JWT token after login/register
- [ ] Attach `Authorization: Bearer <token>` to all protected requests
- [ ] Handle `401` → redirect to login, clear stored token
- [ ] Handle `403` → show "access denied" message
- [ ] On logout: call `POST /api/auth/logout`, then clear token client-side

### Resume Wizard
- [ ] Fetch templates on wizard load: `GET /api/templates`
- [ ] Collect all wizard data per `CreateResumeRequest` schema
- [ ] Validate `experience` has at least 1 entry before submit
- [ ] On submit: `POST /api/resumes`
- [ ] Show `jobTitleSuggestions` and `skillSuggestions` from the response (ephemeral!)
- [ ] Store suggestions in component state — they won't be in `GET /api/resumes/{id}`

### Resume Editor
- [ ] Display `finalData.content` for editing (not `rawData`)
- [ ] For each section, show regeneration button
- [ ] Track regeneration count per section (show remaining out of 3)
- [ ] On regenerate: `POST /api/resumes/{id}/regenerate`
- [ ] On manual save: `PUT /api/resumes/{id}` with full `finalData`
- [ ] Show `aiAvailable: false` disclaimer while stub AI is active

### Payment
- [ ] Show resume price breakdown (base + regen costs)
- [ ] On "Pay": `POST /api/transactions/checkout`
- [ ] Redirect to `paymentUrl`
- [ ] After redirect back: poll `GET /api/transactions/{transactionId}`
- [ ] When `paymentStatus === "SUCCESS"`: refresh resume status
- [ ] Show download button only when `status === "PAID"` (button disabled until PDF renderer is ready)

### Error Handling
- [ ] Parse `422` responses and show per-field errors
- [ ] Show `429` message: "Regeneration limit reached for this section"
- [ ] Show `409` message on duplicate register or pending transaction

---

## 15. JWT Token Details

### Token Claims

| Claim | Value |
|-------|-------|
| `sub` | User ID (GUID as string) |
| `email` | User's email |
| `jti` | Unique token ID (GUID) |
| `iss` | `NexaCV` |
| `aud` | `NexaCV` |
| `exp` | Unix timestamp (24h from issuance) |

### Extracting `userId` Client-Side

Decode the JWT payload (base64) to get the `sub` claim:

```js
const payload = JSON.parse(atob(token.split('.')[1]));
const userId = payload.sub; // GUID string
```

### Token Expiry

Default lifetime is **24 hours** (`expiresIn: 86400`). There is no refresh token mechanism — the user must log in again after expiry.

---

## 16. CORS Configuration

The backend allows requests from `http://localhost:3000` with any header and method.

If your frontend runs on a different port or domain, request a CORS change in `Program.cs`:

```csharp
.WithOrigins("http://localhost:3000")  // change this
```

**Allowed:** All HTTP methods, all headers.

---

## Quick Reference — All Endpoints

| Method | Path | Auth | Summary |
|--------|------|------|---------|
| `POST` | `/api/auth/register` | No | Register a new user |
| `POST` | `/api/auth/login` | No | Login |
| `POST` | `/api/auth/logout` | Yes | Logout (audit only) |
| `GET` | `/api/users/me` | Yes | Get current user profile |
| `PUT` | `/api/users/me` | Yes | Update current user profile |
| `GET` | `/api/templates` | No | List all active templates |
| `GET` | `/api/templates/{id}` | No | Get template by ID |
| `POST` | `/api/resumes` | Yes | Create resume (AI generation) |
| `GET` | `/api/resumes` | Yes | List my resumes |
| `GET` | `/api/resumes/{id}` | Yes | Get resume detail |
| `PUT` | `/api/resumes/{id}` | Yes | Update resume final data |
| `DELETE` | `/api/resumes/{id}` | Yes | Soft-delete resume |
| `POST` | `/api/resumes/{id}/regenerate` | Yes | AI regenerate a section |
| `GET` | `/api/resumes/{id}/download` | Yes | Download resume (501) |
| `POST` | `/api/transactions/checkout` | Yes | Initiate payment |
| `GET` | `/api/transactions/{id}` | Yes | Get transaction / poll status |
| `POST` | `/api/webhooks/payment` | No* | Payment gateway callback |

*Authenticated via gateway signature, not JWT.
