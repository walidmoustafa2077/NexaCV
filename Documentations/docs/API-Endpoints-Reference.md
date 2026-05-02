# API Endpoints Reference

**System:** NexaCV — .NET 9 Minimal APIs  
**Base URL:** `http://localhost:5000` (development)  
**Content-Type:** `application/json` for all requests and responses  
**Auth:** Protected endpoints require `Authorization: Bearer <token>` header  
**JSON casing:** All field names are **camelCase**

---

## Table of Contents

| # | Method | Path | Auth | Summary |
|---|--------|------|------|---------|
| 1 | POST | `/api/auth/register` | ❌ | Register a new user |
| 2 | POST | `/api/auth/login` | ❌ | Login |
| 3 | POST | `/api/auth/logout` | ✅ | Logout |
| 4 | GET | `/api/users/me` | ✅ | Get current user profile |
| 5 | PUT | `/api/users/me` | ✅ | Update current user profile |
| 6 | GET | `/api/templates` | ❌ | List all templates |
| 7 | GET | `/api/templates/{id}` | ❌ | Get template by ID |
| 8 | POST | `/api/resumes` | ✅ | Create a resume |
| 9 | GET | `/api/resumes` | ✅ | List my resumes |
| 10 | GET | `/api/resumes/{id}` | ✅ | Get resume by ID |
| 11 | PUT | `/api/resumes/{id}` | ✅ | Update resume final data |
| 12 | DELETE | `/api/resumes/{id}` | ✅ | Delete resume |
| 13 | POST | `/api/resumes/{id}/regenerate` | ✅ | Regenerate a section with AI |
| 14 | GET | `/api/resumes/{id}/download` | ✅ | Download resume (not yet implemented) |
| 15 | POST | `/api/transactions/checkout` | ✅ | Initiate checkout |
| 16 | GET | `/api/transactions/{id}` | ✅ | Get transaction by ID |
| 17 | POST | `/api/webhooks/payment` | ❌* | Payment gateway webhook |

> ❌\* — Webhook uses gateway-specific header signature verification instead of JWT.

---

## FinalData JSON Schema

All resumes store their AI-processed content in a structured `FinalData` JSON string with two top-level keys:

```json
{
  "settings": {
    "summaryType": "SUMMARY",
    "descriptionFormat": "BULLET",
    "skillsFormat": "GRID"
  },
  "content": {
    "personal": { "firstName": "John", "lastName": "Doe", "title": "Software Engineer", "email": "john@example.com" },
    "summary": "Results-driven engineer with 5 years of experience...",
    "experience": [
      { "id": "exp_001", "title": "Senior Developer", "company": "Acme Corp", "description": "Built scalable REST APIs..." }
    ],
    "skills": ["C#", ".NET", "React", "SQL"],
    "courses": [
      { "name": "Cloud Computing", "provider": "Coursera" }
    ]
  }
}
```

| `settings` key | Purpose | Accepted values |
|---|---|---|
| `summaryType` | Section title shown on the resume | `SUMMARY` \| `OBJECTIVE` |
| `descriptionFormat` | How experience/summary text is rendered | `BULLET` \| `PARAGRAPH` |
| `skillsFormat` | How the skills section is rendered | `GRID` \| `LIST` |

> **Stub note:** `GenerateAsync` in the stub wraps `rawData` inside `content` and applies default settings. A real AI implementation would parse and enrich the content.

---

## Common Error Shapes

All errors are returned as JSON. The `details` array is only present on **422 Validation** errors.

**Generic error (4xx / 5xx):**
```json
{
  "status": 404,
  "error": "Resume not found."
}
```

**Validation error (422):**
```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "Email", "message": "'Email' is not a valid email address." },
    { "field": "Password", "message": "Password must contain at least one special character." }
  ]
}
```

**Unauthorized (401) — missing or expired token:**
```json
{
  "status": 401,
  "error": "Unauthorized"
}
```

---

## 1. Auth

### 1.1 `POST /api/auth/register`

Register a new user account. Returns a signed 24-hour JWT on success.

| | |
|---|---|
| **Auth** | None |
| **Success** | `201 Created` |
| **Errors** | `409 Conflict`, `422 Unprocessable Entity` |

**Request body:**
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

> `dateOfBirth` is optional. `password` must be ≥ 8 characters and contain at least one special character.

**Response `201 Created`:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwianRpIjoiYWJjZGVmMTIiLCJleHAiOjE3MjAwMDAwMDB9.signature",
  "expiresIn": 86400
}
```

**Response `409 Conflict` — email or username already taken:**
```json
{
  "status": 409,
  "error": "Email already in use."
}
```

**Response `422 Unprocessable Entity`:**
```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "FirstName", "message": "'First Name' must not be empty." },
    { "field": "Password", "message": "Password must contain at least one special character." }
  ]
}
```

---

### 1.2 `POST /api/auth/login`

Authenticate with email and password. Updates `lastLogin` and logs a `LOGIN` movement.

| | |
|---|---|
| **Auth** | None |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `422 Unprocessable Entity` |

**Request body:**
```json
{
  "email": "john.doe@example.com",
  "password": "P@ssw0rd!"
}
```

**Response `200 OK`:**
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIzZmE4NWY2NC01NzE3LTQ1NjItYjNmYy0yYzk2M2Y2NmFmYTYiLCJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwianRpIjoiYWJjZGVmMTIiLCJleHAiOjE3MjAwMDAwMDB9.signature",
  "expiresIn": 86400
}
```

**Response `401 Unauthorized` — wrong email or password:**
```json
{
  "status": 401,
  "error": "Invalid credentials."
}
```

> The error message is intentionally ambiguous to prevent user enumeration.

---

### 1.3 `POST /api/auth/logout`

Logs a `LOGOUT` movement for audit. The JWT is **not** invalidated server-side — clients must discard the token.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `204 No Content` |
| **Errors** | `401 Unauthorized` |

**Request headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request body:** None

**Response `204 No Content`:** *(empty body)*

---

## 2. Users

### 2.1 `GET /api/users/me`

Returns the full profile of the currently authenticated user. Never exposes the password hash.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized` |

**Response `200 OK`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLogin": "2024-06-20T08:45:00Z"
}
```

> `lastLogin` is `null` if the user has never logged in after registration.

---

### 2.2 `PUT /api/users/me`

Partial update: only non-null fields in the request body are applied. Sending `password` re-hashes it with BCrypt and logs a `PASSWORD_UPDATED` movement.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized` |

**Request body:**
```json
{
  "firstName": "Jane",
  "lastName": "Smith",
  "username": "janesmith",
  "password": "N3wP@ss!"
}
```

> All fields are optional. Omit or pass `null` for any field to leave it unchanged. `email` cannot be updated through this endpoint.

**Response `200 OK`:**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Jane",
  "lastName": "Smith",
  "username": "janesmith",
  "email": "john.doe@example.com",
  "createdAt": "2024-01-15T10:30:00Z",
  "lastLogin": "2024-06-20T08:45:00Z"
}
```

---

## 3. Templates

### 3.1 `GET /api/templates`

Returns all active resume templates. No authentication required.

| | |
|---|---|
| **Auth** | None |
| **Success** | `200 OK` |
| **Query params** | `?industryCategory=Corporate` *(optional filter)* |

**Request (no filter):**
```
GET /api/templates
```

**Request (filtered by industry):**
```
GET /api/templates?industryCategory=Corporate
```

**Response `200 OK`:**
```json
[
  {
    "id": 1,
    "name": "Modern Minimalist",
    "industryCategory": "Corporate",
    "thumbnailUrl": null,
    "basePriceUsd": 3.00,
    "supportsWord": true
  },
  {
    "id": 2,
    "name": "Creative",
    "industryCategory": "Creative",
    "thumbnailUrl": null,
    "basePriceUsd": 3.00,
    "supportsWord": false
  },
  {
    "id": 3,
    "name": "Executive",
    "industryCategory": "Corporate",
    "thumbnailUrl": null,
    "basePriceUsd": 3.75,
    "supportsWord": true
  }
]
```

> Seeded templates on startup. `thumbnailUrl` is `null` until image assets are uploaded.

---

### 3.2 `GET /api/templates/{id}`

Returns full detail of a single active template.

| | |
|---|---|
| **Auth** | None |
| **Success** | `200 OK` |
| **Errors** | `404 Not Found` |
| **Path params** | `id` — integer template ID |

**Request:**
```
GET /api/templates/1
```

**Response `200 OK`:**
```json
{
  "id": 1,
  "name": "Modern Minimalist",
  "industryCategory": "Corporate",
  "thumbnailUrl": null,
  "basePriceUsd": 3.00,
  "supportsWord": true
}
```

**Response `404 Not Found`:**
```json
{
  "status": 404,
  "error": "Template not found."
}
```

---

## 4. Resumes

### 4.1 `POST /api/resumes`

Create a new resume. Passes `rawData` through the AI pipeline, sets `finalData`, and transitions status from `DRAFT` to `COMPLETED` atomically. An initial history snapshot is saved with reason `INITIAL_AI_GEN`.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `404 Not Found` (invalid templateId) |

**Request body:**
```json
{
  "templateId": 1,
  "rawData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer with 5 years building scalable web applications.\",\"experience\":[{\"company\":\"Acme Corp\",\"role\":\"Senior Developer\",\"years\":\"2021-2024\"}],\"education\":[{\"institution\":\"Cairo University\",\"degree\":\"BSc Computer Science\",\"year\":2019}],\"skills\":[\"C#\",\".NET\",\"React\",\"SQL\"]}"
}
```

> `rawData` is a JSON string (double-encoded). It is passed directly to the AI generation pipeline.

**Response `200 OK`:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer...\"}",
  "finalData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer...\"}",
  "aiAvailable": false,
  "createdAt": "2024-06-20T09:00:00Z",
  "updatedAt": "2024-06-20T09:00:00Z"
}
```

> `aiAvailable: false` while the stub AI is active — `finalData` mirrors `rawData` in the stub build.

**Response `404 Not Found` — invalid template ID:**
```json
{
  "status": 404,
  "error": "Template not found."
}
```

---

### 4.2 `GET /api/resumes`

List all non-deleted resumes belonging to the authenticated user. Soft-deleted resumes are excluded.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized` |

**Response `200 OK`:**
```json
[
  {
    "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "status": "COMPLETED",
    "templateName": "Modern Minimalist",
    "createdAt": "2024-06-20T09:00:00Z"
  },
  {
    "id": "b2c3d4e5-f6a7-8901-bcde-f12345678901",
    "status": "PAID",
    "templateName": "Executive",
    "createdAt": "2024-06-18T14:22:00Z"
  }
]
```

---

### 4.3 `GET /api/resumes/{id}`

Get full resume detail including `rawData`, `finalData`, and `aiAvailable`.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |
| **Path params** | `id` — UUID |

**Response `200 OK`:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer...\"}",
  "finalData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer with proven track record in...\"}",
  "aiAvailable": false,
  "createdAt": "2024-06-20T09:00:00Z",
  "updatedAt": "2024-06-20T09:15:00Z"
}
```

**Response `403 Forbidden`:**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

**Response `404 Not Found`:**
```json
{
  "status": 404,
  "error": "Resume not found."
}
```

---

### 4.4 `PUT /api/resumes/{id}`

Replace the resume's `finalData` JSON with a manually edited version. Saves a history snapshot with reason `MANUAL_EDIT`.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `403 Forbidden` |
| **Path params** | `id` — UUID |

**Request body:**
```json
{
  "finalData": "{\"name\":\"John Doe\",\"title\":\"Lead Software Engineer\",\"summary\":\"Seasoned full-stack engineer with expertise in cloud architecture and team leadership.\",\"experience\":[{\"company\":\"Acme Corp\",\"role\":\"Lead Developer\",\"years\":\"2021-2024\"}],\"skills\":[\"C#\",\".NET\",\"Azure\",\"React\",\"SQL\"]}"
}
```

**Response `200 OK`:**
```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",\"summary\":\"Experienced full-stack developer...\"}",
  "finalData": "{\"name\":\"John Doe\",\"title\":\"Lead Software Engineer\",\"summary\":\"Seasoned full-stack engineer...\"}",
  "aiAvailable": false,
  "createdAt": "2024-06-20T09:00:00Z",
  "updatedAt": "2024-06-20T10:30:00Z"
}
```

---

### 4.5 `DELETE /api/resumes/{id}`

Soft-delete a resume. The record remains in the database but is excluded from all queries via the global EF filter.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `204 No Content` |
| **Errors** | `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |
| **Path params** | `id` — UUID |

**Response `204 No Content`:** *(empty body)*

---

### 4.6 `POST /api/resumes/{id}/regenerate`

Context-aware regeneration of a single section inside `FinalData.content`. Each call costs **$0.25 USD** and is recorded as a `Regeneration` row. A history snapshot is saved with reason `REGEN_{sectionIdentifier}`. Maximum **3 regenerations per section**.

The service extracts the resume's current `title`, `skills`, and `descriptionFormat` from `FinalData` and passes them to the AI alongside the user's prompt, so the output stays coherent with the rest of the document.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `403 Forbidden`, `429 Too Many Requests` |
| **Path params** | `id` — UUID |

**Request body — content change only:**
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Make it more concise and achievement-focused"
}
```

**Request body — structural format change (Bullet → Paragraph):**
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Rewrite as a flowing paragraph",
  "targetFormat": "PARAGRAPH"
}
```

**Request body — new title alignment:**
```json
{
  "sectionIdentifier": "experience",
  "userPrompt": "Highlight architecture decisions",
  "newTitleSuggestion": "Senior Cloud Architect"
}
```

> `sectionIdentifier` must exactly match a key under `FinalData.content` (e.g. `summary`, `experience`, `skills`).  
> `targetFormat` updates `FinalData.settings.descriptionFormat` (or `skillsFormat` when targeting the `skills` section).  
> `targetFormat` and `newTitleSuggestion` are both optional.

**Response `200 OK`:**
```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Make it more concise and achievement-focused",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": false
}
```

> `updatedContent` echoes `userPrompt` while the stub AI is active. When a real AI is wired in it will contain the rewritten section content. `aiAvailable: false` in stub builds.

**Response `429 Too Many Requests` — section limit reached:**
```json
{
  "status": 429,
  "error": "Regeneration limit reached for section summary."
}
```

---

### 4.7 `GET /api/resumes/{id}/download`

Download a paid resume as PDF or DOCX. **Status: 501 Not Implemented.** The endpoint validates ownership and payment, records the download attempt, and will stream the file once a PDF renderer (QuestPDF) is integrated.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `501 Not Implemented` *(pending renderer integration)* |
| **Errors** | `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| **Path params** | `id` — UUID |
| **Query params** | `?format=pdf` *(default)* or `?format=docx` |

**Request:**
```
GET /api/resumes/a1b2c3d4-e5f6-7890-abcd-ef1234567890/download?format=pdf
```

**Response `501 Not Implemented`:** *(returns 501 until renderer is wired in)*

**Response `400 Bad Request` — DOCX requested for a template that does not support Word:**
```json
{
  "status": 400,
  "error": "This template does not support Word export."
}
```

**Response `403 Forbidden` — resume not PAID or belongs to another user:**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

---

## 5. Transactions

### 5.1 `POST /api/transactions/checkout`

Create a PENDING transaction and delegate to the payment gateway for a payment URL. The total is calculated dynamically:

> `total = (basePriceUsd + sumRegenCostsUsd) × exchangeRate`

The exchange rate at checkout time is stored in `exchangeRateUsed` for audit and dispute resolution.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |

**Request body:**
```json
{
  "resumeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "currency": "EGP"
}
```

> Supported currencies: `USD`, `EGP`, `EUR`, `GBP`, `SAR`, `AED`. The resume must be in `COMPLETED` status.

**Response `200 OK`**

Example: template $3.00 USD + 1 regen $0.25 USD, converted to EGP at rate 50.00.

```json
{
  "transactionId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "paymentUrl": "http://localhost:5000/stub-pay/c3d4e5f6-a7b8-9012-cdef-123456789012",
  "baseAmount": 150.00,
  "regenAmount": 12.50,
  "totalAmount": 162.50,
  "currency": "EGP",
  "exchangeRateUsed": 50.00
}
```

**Response `200 OK`**

Example: same resume, `currency: "USD"` (no conversion needed).

```json
{
  "transactionId": "d4e5f6a7-b8c9-0123-defa-234567890123",
  "paymentUrl": "http://localhost:5000/stub-pay/d4e5f6a7-b8c9-0123-defa-234567890123",
  "baseAmount": 3.00,
  "regenAmount": 0.25,
  "totalAmount": 3.25,
  "currency": "USD",
  "exchangeRateUsed": 1.00
}
```

**Response `400 Bad Request` — resume not in COMPLETED status:**
```json
{
  "status": 400,
  "error": "Resume must be COMPLETED before checkout."
}
```

**Response `400 Bad Request` — resume already PAID:**
```json
{
  "status": 400,
  "error": "Resume must be COMPLETED before checkout."
}
```

---

### 5.2 `GET /api/transactions/{id}`

Get full detail of a payment transaction. Poll this endpoint to check if `paymentStatus` has transitioned to `SUCCESS`.

| | |
|---|---|
| **Auth** | Bearer JWT ✅ |
| **Success** | `200 OK` |
| **Errors** | `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |
| **Path params** | `id` — UUID |

**Response `200 OK` — pending payment:**
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "resumeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "totalAmount": 162.50,
  "currency": "EGP",
  "exchangeRateUsed": 50.00,
  "paymentStatus": "PENDING",
  "createdAt": "2024-06-20T11:00:00Z",
  "completedAt": null
}
```

**Response `200 OK` — after successful payment:**
```json
{
  "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "resumeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "totalAmount": 162.50,
  "currency": "EGP",
  "exchangeRateUsed": 50.00,
  "paymentStatus": "SUCCESS",
  "createdAt": "2024-06-20T11:00:00Z",
  "completedAt": "2024-06-20T11:02:15Z"
}
```

> `paymentStatus` values: `PENDING`, `SUCCESS`, `FAILED`.

**Response `404 Not Found`:**
```json
{
  "status": 404,
  "error": "Transaction not found."
}
```

---

## 6. Webhooks

### 6.1 `POST /api/webhooks/payment`

Receives callbacks from payment gateways. No JWT required — authentication is performed by each gateway's `VerifyWebhookSignature()` implementation. On a verified success event: transaction transitions to `SUCCESS`, `completedAt` is set, and the associated resume transitions to `PAID`.

| | |
|---|---|
| **Auth** | Gateway-specific header signature (no JWT) |
| **Success** | `200 OK` |
| **Errors** | `400 Bad Request` |

**Stub testing** — use the `X-Stub-Ref` header with the `transactionId` from the checkout response:

```
POST /api/webhooks/payment
X-Stub-Ref: c3d4e5f6-a7b8-9012-cdef-123456789012
```

**Request body:** *(empty for stub; real gateways POST their own event payload)*

**Response `200 OK`:** *(empty body)*

**Response `400 Bad Request` — unrecognized gateway:**
```json
{
  "error": "No matching payment gateway for this webhook."
}
```

**Response `400 Bad Request` — invalid signature:**
```json
{
  "error": "Invalid webhook signature."
}
```

---

## End-to-End Flow Example

A complete sequence from registration to a confirmed paid resume.

### Step 1 — Register

```
POST /api/auth/register

{
  "firstName": "John",
  "lastName": "Doe",
  "username": "johndoe",
  "email": "john@example.com",
  "password": "P@ssw0rd!"
}

→ 201
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "token": "eyJ...",
  "expiresIn": 86400
}
```

### Step 2 — Browse Templates

```
GET /api/templates

→ 200
[
  { "id": 1, "name": "Modern Minimalist", "basePriceUsd": 3.00, "supportsWord": true },
  { "id": 2, "name": "Creative",          "basePriceUsd": 3.00, "supportsWord": false },
  { "id": 3, "name": "Executive",         "basePriceUsd": 3.75, "supportsWord": true }
]
```

### Step 3 — Create Resume

```
POST /api/resumes
Authorization: Bearer eyJ...

{
  "templateId": 1,
  "rawData": "{\"name\":\"John Doe\",\"title\":\"Software Engineer\",...}"
}

→ 200
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "status": "COMPLETED",
  "templateName": "Modern Minimalist",
  "finalData": "{...}",
  "aiAvailable": false
}
```

### Step 4 — Regenerate a Section (optional)

```
POST /api/resumes/a1b2c3d4-e5f6-7890-abcd-ef1234567890/regenerate
Authorization: Bearer eyJ...

{
  "sectionIdentifier": "summary",
  "userPrompt": "More achievement-focused",
  "targetFormat": "PARAGRAPH"
}

→ 200
{
  "sectionIdentifier": "summary",
  "updatedContent": "More achievement-focused",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": false
}
```

### Step 5 — Checkout

```
POST /api/transactions/checkout
Authorization: Bearer eyJ...

{
  "resumeId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "currency": "EGP"
}

→ 200
{
  "transactionId": "c3d4e5f6-a7b8-9012-cdef-123456789012",
  "paymentUrl": "http://localhost:5000/stub-pay/c3d4e5f6-a7b8-9012-cdef-123456789012",
  "baseAmount": 150.00,
  "regenAmount": 12.50,
  "totalAmount": 162.50,
  "currency": "EGP",
  "exchangeRateUsed": 50.00
}
```

### Step 6 — Simulate Payment (stub only)

```
POST /api/webhooks/payment
X-Stub-Ref: c3d4e5f6-a7b8-9012-cdef-123456789012

→ 200
```

### Step 7 — Confirm Payment Status

```
GET /api/transactions/c3d4e5f6-a7b8-9012-cdef-123456789012
Authorization: Bearer eyJ...

→ 200
{
  "paymentStatus": "SUCCESS",
  "completedAt": "2024-06-20T11:02:15Z",
  ...
}
```

### Step 8 — Download Resume *(pending renderer)*

```
GET /api/resumes/a1b2c3d4-e5f6-7890-abcd-ef1234567890/download?format=pdf
Authorization: Bearer eyJ...

→ 501  (renderer not yet wired in)
```
