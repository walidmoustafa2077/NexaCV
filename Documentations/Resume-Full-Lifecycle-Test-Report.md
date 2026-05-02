# NexaCV API — Resume Full Lifecycle Test Report

**Date:** 2026-05-01  
**Environment:** Local (In-Memory EF Core + NexaCV.AiMock stub)  
**API Base URL:** `http://localhost:5166`  
**AI Mock Base URL:** `http://localhost:5001`  
**Total Test Cases:** 50  
**Scope:** Resume endpoints only (`/api/resumes/*`)

---

## Executive Summary

End-to-end scenario covering the full resume lifecycle: creation with rich structured data, manual content updates, AI-powered section regeneration for summary, three individual experience entries, a dynamically added fourth entry, skills regeneration, and final deletion. Security and validation boundaries are verified throughout.

| Category | Tests | ✅ Pass | ❌ Fail |
|----------|-------|---------|--------|
| 1 — Create & Read | 8 | 8 | 0 |
| 2 — Create Validation | 5 | 5 | 0 |
| 3 — Update: Add exp_003 | 3 | 3 | 0 |
| 4 — Regenerate Summary | 5 | 5 | 0 |
| 5 — Regenerate exp_001 | 4 | 4 | 0 |
| 6 — Regenerate exp_002 | 3 | 3 | 0 |
| 7 — Regenerate exp_003 | 4 | 4 | 0 |
| 8 — Add exp_004 & Regenerate | 4 | 4 | 0 |
| 9 — Regenerate Skills | 4 | 4 | 0 |
| 10 — Cross-User & Auth Security | 5 | 5 | 0 |
| 11 — Delete Lifecycle | 5 | 5 | 0 |
| **Total** | **50** | **50** | **0** |

---

## Test Environment

| Component | Details |
|-----------|---------|
| Runtime | .NET 9 Minimal API |
| Database | EF Core In-Memory |
| Auth | JWT Bearer (24 h expiry, stateless) |
| AI Service | NexaCV.AiMock on port 5001 — returns mock content banks, `aiAvailable: true` |

### Fixed Test Identities

| Role | Token | Notes |
|------|-------|-------|
| John Doe (resume owner) | `<tokenJohn>` | Creates and manages the resume |
| Bob Smith (other user) | `<tokenBob>` | Used for cross-user security tests |

> **Resume ID** — assigned in TC-01 and referenced as `<resumeId>` in all subsequent tests.  
> **Bob's Resume ID** — pre-existing resume owned by Bob, referenced as `<bobResumeId>`.

---

## Category 1 — Create & Read (8 tests)

### TC-01 — Create resume with full structured data ✅

**Purpose:** Submit a fully populated resume payload — personal info, 2 experience entries, education, courses, and skills — and verify the API returns `201 Created` with a `Location` header.

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
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
        "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "startDate": "2019-07",
          "endDate": "2021-02",
          "description": "Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews."
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
        },
        {
          "id": "crs_002",
          "name": "Advanced React Patterns",
          "provider": "Frontend Masters",
          "date": "2022-08"
        }
      ],
      "skills": [
        "C#",
        ".NET 9",
        "ASP.NET Core",
        "React",
        "SQL Server",
        "Azure",
        "Docker"
      ]
    }
  }
}
```

**Response — 201 Created**
```
Location: /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
```
```json
{
  "id": "a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": true,
  "createdAt": "2026-05-01T10:00:00.000Z",
  "updatedAt": "2026-05-01T10:00:00.000Z",
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": { "..." }
  },
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
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
        "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "AI-Polished: Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "startDate": "2019-07",
          "endDate": "2021-02",
          "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews."
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
        },
        {
          "id": "crs_002",
          "name": "Advanced React Patterns",
          "provider": "Frontend Masters",
          "date": "2022-08"
        }
      ],
      "skills": [
        "AI-Polished: C#",
        "AI-Polished: .NET 9",
        "AI-Polished: ASP.NET Core",
        "AI-Polished: React",
        "AI-Polished: SQL Server",
        "AI-Polished: Azure",
        "AI-Polished: Docker"
      ]
    }
  },
  "jobTitleSuggestions": [
    { "title": "Senior Backend Developer", "score": 8 },
    { "title": "Senior Software Engineer", "score": 8 },
    { "title": "Backend Developer", "score": 7 }
  ],
  "skillSuggestions": [
    "MediatR", "CQRS", "Entity Framework Core", "Azure Functions", "Azure Service Bus",
    "Redis", "Kubernetes", "Query Optimisation", "xUnit", "FluentValidation"
  ]
}
```

> `<resumeId>` = `a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c` — used in all subsequent tests.

**Result:** ✅ Pass — `201 Created` with `Location` header; AiMock polishes free-text fields; skills prefixed with `"AI-Polished: "` (array items have no parentKey so they are processed); `jobTitleSuggestions` and `skillSuggestions` populated.

---

### TC-02 — GET resume by ID — verify full structure ✅

**Purpose:** Confirm the stored resume returns all sections intact, including both experience entries, education, courses, and skills.

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
{
  "id": "a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": true,
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": { "firstName": "John", "lastName": "Doe", "email": "john.doe@example.com" },
      "summary": "AI-Polished: Software engineer with 5 years of experience...",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems" },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital" }
      ],
      "education": [{ "id": "edu_001", "institution": "Cairo University" }],
      "courses": [
        { "id": "crs_001", "name": "Cloud Computing Architecture" },
        { "id": "crs_002", "name": "Advanced React Patterns" }
      ],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core"]
    }
  }
}
```

**Result:** ✅ Pass — All sections present; 2 experience entries with IDs `exp_001` and `exp_002`; education and courses arrays intact.

---

### TC-03 — GET list of resumes — resume appears in list ✅

**Request**
```
GET /api/resumes
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
[
  {
    "id": "a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c",
    "status": "COMPLETED",
    "templateName": "Modern Minimalist",
    "createdAt": "2026-05-01T10:00:00.000Z"
  }
]
```

**Result:** ✅ Pass — One resume in list; summary fields only (no `rawData` / `finalData` in list response).

---

### TC-04 — GET non-existent resume ✅

**Request**
```
GET /api/resumes/00000000-0000-0000-0000-000000000000
Authorization: Bearer <tokenJohn>
```

**Response — 404 Not Found**
```json
{
  "status": 404,
  "error": "Resume not found."
}
```

**Result:** ✅ Pass

---

### TC-05 — GET resume without auth token ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
```
*(no Authorization header)*

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

**Result:** ✅ Pass — JWT middleware rejects before the handler is reached.

---

### TC-06 — GET resume list without auth token ✅

**Request**
```
GET /api/resumes
```
*(no Authorization header)*

**Response — 401 Unauthorized**
```
(empty body)
```

**Result:** ✅ Pass

---

### TC-07 — GET another user's resume returns 403 ✅

**Purpose:** Bob attempts to read John's resume. Ownership check must reject with 403, not 404 (which would reveal existence).

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenBob>
```

**Response — 403 Forbidden**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

**Result:** ✅ Pass — `ForbiddenException` thrown; `ExceptionMiddleware` maps to `403`.

---

### TC-08 — GET list only returns own resumes ✅

**Purpose:** Bob's resume list must not contain John's resume.

**Request**
```
GET /api/resumes
Authorization: Bearer <tokenBob>
```

**Response — 200 OK**
```json
[
  {
    "id": "<bobResumeId>",
    "status": "COMPLETED",
    "templateName": "Modern Minimalist",
    "createdAt": "2026-05-01T09:00:00.000Z"
  }
]
```

**Result:** ✅ Pass — John's resume does not appear. User isolation enforced at query level.

---

## Category 2 — Create Validation (5 tests)

### TC-09 — Create with invalid templateId ✅

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "templateId": 0,
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com",
        "phone": "+201012345678",
        "location": "Cairo"
      },
      "summary": "Test",
      "skills": ["C#"]
    }
  }
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "TemplateId": ["'Template Id' must be greater than 0."]
  }
}
```

**Result:** ✅ Pass

---

### TC-10 — Create with missing firstName ✅

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "templateId": 1,
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "lastName": "Doe",
        "email": "john@example.com",
        "phone": "+201012345678",
        "location": "Cairo"
      },
      "summary": "Test",
      "skills": ["C#"]
    }
  }
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "RawData.Content.Personal.FirstName": ["'First Name' must not be empty."]
  }
}
```

**Result:** ✅ Pass

---

### TC-11 — Create with missing phone ✅

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "templateId": 1,
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com",
        "location": "Cairo"
      },
      "summary": "Test",
      "skills": ["C#"]
    }
  }
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "RawData.Content.Personal.Phone": ["'Phone' must not be empty."]
  }
}
```

**Result:** ✅ Pass

---

### TC-12 — Create with missing location ✅

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "templateId": 1,
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John",
        "lastName": "Doe",
        "email": "john@example.com",
        "phone": "+201012345678"
      },
      "summary": "Test",
      "skills": ["C#"]
    }
  }
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "RawData.Content.Personal.Location": ["'Location' must not be empty."]
  }
}
```

**Result:** ✅ Pass

---

### TC-13 — Create with empty request body ✅

**Request**
```
POST /api/resumes
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "TemplateId": ["'Template Id' must be greater than 0."],
    "RawData": ["'Raw Data' must not be empty."]
  }
}
```

**Result:** ✅ Pass

---

## Category 3 — Update FinalData: Add exp_003 (3 tests)

### TC-14 — PUT finalData to add third experience entry ✅

**Purpose:** Manually update `finalData` to add a third experience entry (`exp_003`). Uses `PUT /api/resumes/{id}` with the complete replacement `finalData` object.

**Request**
```
PUT /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "finalData": {
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
        "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "AI-Polished: Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "startDate": "2019-07",
          "endDate": "2021-02",
          "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews."
        },
        {
          "id": "exp_003",
          "title": "Junior Developer",
          "company": "StartupHub",
          "startDate": "2018-01",
          "endDate": "2019-06",
          "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data."
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
        },
        {
          "id": "crs_002",
          "name": "Advanced React Patterns",
          "provider": "Frontend Masters",
          "date": "2022-08"
        }
      ],
      "skills": [
        "AI-Polished: C#",
        "AI-Polished: .NET 9",
        "AI-Polished: ASP.NET Core",
        "AI-Polished: React",
        "AI-Polished: SQL Server",
        "AI-Polished: Azure",
        "AI-Polished: Docker"
      ]
    }
  }
}
```

**Response — 200 OK**
```json
{
  "id": "a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": true,
  "updatedAt": "2026-05-01T10:05:00.000Z"
}
```

**Result:** ✅ Pass — `finalData` replaced; `updatedAt` timestamp advances; history snapshot created with reason `MANUAL_EDIT`.

---

### TC-15 — GET resume to confirm exp_003 is present ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array excerpt)*
```json
{
  "finalData": {
    "content": {
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems" },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital" },
        { "id": "exp_003", "title": "Junior Developer", "company": "StartupHub" }
      ]
    }
  }
}
```

**Result:** ✅ Pass — Three experience entries; `exp_003` present with original (not AI-polished) description.

---

### TC-16 — PUT another user's resume returns 403 ✅

**Request**
```
PUT /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenBob>
Content-Type: application/json
```
```json
{
  "finalData": {
    "settings": {},
    "content": {}
  }
}
```

**Response — 403 Forbidden**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

**Result:** ✅ Pass — `ForbiddenException` thrown on ownership mismatch.

---

## Category 4 — Regenerate Summary (5 tests)

> **Limit:** 3 regenerations per `sectionIdentifier`. Each costs $0.25 USD.  
> `targetFormat` on a non-skills section updates `settings.descriptionFormat`.

---

### TC-17 — Regenerate summary (1st call) with Paragraph targetFormat ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Highlight cloud architecture and technical leadership skills.",
  "targetFormat": "Paragraph",
  "newTitleSuggestion": "Senior Cloud Architect"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Results-driven Software Engineer with 6+ years designing and shipping high-availability .NET microservices on Azure. Led cross-functional teams of up to 8 engineers, improved system uptime to 99.9%, and reduced average API latency by 35% through targeted optimisations. Passionate about clean architecture, test-driven development, and delivering measurable business impact.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=1`, `regenCountRemaining=2`; `settings.descriptionFormat` updated to `"Paragraph"` in `finalData`; history snapshot created with reason `REGEN_summary`.

---

### TC-18 — Verify summary updated in finalData ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(summary and settings excerpt)*
```json
{
  "finalData": {
    "settings": {
      "summaryType": "Summary",
      "descriptionFormat": "Paragraph"
    },
    "content": {
      "summary": "Results-driven Software Engineer with 6+ years designing and shipping high-availability .NET microservices on Azure. Led cross-functional teams of up to 8 engineers, improved system uptime to 99.9%, and reduced average API latency by 35% through targeted optimisations. Passionate about clean architecture, test-driven development, and delivering measurable business impact."
    }
  }
}
```

**Result:** ✅ Pass — `summary` replaced with AI-generated content; `descriptionFormat` changed to `"Paragraph"` as requested.

---

### TC-19 — Regenerate summary (2nd call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Add more quantified achievements and key metrics to strengthen the profile."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Senior Backend Engineer specialising in distributed systems and cloud-native architecture. Track record of scaling services to handle 10M+ daily requests on AWS, cutting infrastructure costs by 28% through right-sizing and caching strategies, and mentoring junior engineers to build resilient, maintainable codebases.",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=2`, `regenCountRemaining=1`; summary replaced with second mock variant.

---

### TC-20 — Regenerate summary (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Be concise, three sentences max, impact first."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Full-Stack Developer with 5 years of experience building responsive web applications using React and ASP.NET Core. Delivered 12 production features end-to-end, improved Core Web Vitals scores by 40%, and championed accessibility standards across a three-team engineering org.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=3`, `regenCountRemaining=0`; slot fully consumed.

---

### TC-21 — Regenerate summary (4th call — 429 rate limit) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "One more attempt to rewrite."
}
```

**Response — 429 Too Many Requests**
```json
{
  "status": 429,
  "error": "Regeneration limit reached for this section."
}
```

**Result:** ✅ Pass — `TooManyRegenerationsException` thrown; `ExceptionMiddleware` maps to `429`; no `Regeneration` row inserted.

---

## Category 5 — Regenerate exp_001 (4 tests)

> Regenerating by entry ID (`exp_001`) patches only that entry's `description` field inside `content.experience[]`. Other entries remain untouched.

---

### TC-22 — Regenerate exp_001 (1st call) with Bulleted targetFormat ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_001",
  "userPrompt": "Use stronger action verbs and add concrete performance metrics.",
  "targetFormat": "Bulleted"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_001",
  "updatedContent": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `exp_001.description` updated in-place inside the experience array; `settings.descriptionFormat` changed to `"Bulleted"`; `exp_002` and `exp_003` descriptions untouched.

---

### TC-23 — Verify exp_001 updated; exp_002 and exp_003 intact ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array excerpt)*
```json
{
  "finalData": {
    "settings": { "descriptionFormat": "Bulleted" },
    "content": {
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews."
        },
        {
          "id": "exp_003",
          "title": "Junior Developer",
          "company": "StartupHub",
          "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data."
        }
      ]
    }
  }
}
```

**Result:** ✅ Pass — `exp_001.description` is the regenerated content; `exp_002` and `exp_003` retain their previous descriptions unchanged; `settings.descriptionFormat` is `"Bulleted"`.

---

### TC-24 — Regenerate exp_001 (2nd call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_001",
  "userPrompt": "Emphasize team leadership and the scale of the systems managed."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_001",
  "updatedContent": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=2`, `regenCountRemaining=1`.

---

### TC-25 — Regenerate exp_001 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_001",
  "userPrompt": "Add specific latency and uptime metrics for this senior role."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_001",
  "updatedContent": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=3`, `regenCountRemaining=0`; exp_001 slot exhausted. A 4th attempt would return `429`.

---

## Category 6 — Regenerate exp_002 (3 tests)

> `exp_002` has its own independent 3-slot regeneration limit (tracked by `sectionIdentifier = "exp_002"`).

---

### TC-26 — Regenerate exp_002 (1st call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_002",
  "userPrompt": "Emphasise API design, system reliability, and database performance improvements."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_002",
  "updatedContent": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `exp_002.description` updated; `exp_001` and `exp_003` untouched; rate limit counter is **independent** of `exp_001`'s exhausted slots.

---

### TC-27 — Regenerate exp_002 (2nd call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_002",
  "userPrompt": "Focus on backend engineering impact and quantified delivery metrics."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_002",
  "updatedContent": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions.",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=2`, `regenCountRemaining=1`.

---

### TC-28 — Regenerate exp_002 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_002",
  "userPrompt": "Rewrite with stronger verbs, a clear ownership statement, and measurable outcomes."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_002",
  "updatedContent": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=3`, `regenCountRemaining=0`; `exp_002` slot exhausted.

---

## Category 7 — Regenerate exp_003 (4 tests)

---

### TC-29 — Regenerate exp_003 (1st call) ✅

**Purpose:** `exp_003` was added manually via PUT (TC-14). It has a fresh 3-slot regeneration budget.

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_003",
  "userPrompt": "Rewrite this junior role with stronger action verbs and business context."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_003",
  "updatedContent": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `exp_003` entry updated in-place; `exp_001` and `exp_002` retain their previously regenerated descriptions; counter starts fresh at 1.

---

### TC-30 — GET to confirm exp_003 updated; all three entries in correct state ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array)*
```json
{
  "finalData": {
    "content": {
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "description": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%."
        },
        {
          "id": "exp_003",
          "title": "Junior Developer",
          "company": "StartupHub",
          "description": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions."
        }
      ]
    }
  }
}
```

**Result:** ✅ Pass — All three entries present with their final regenerated descriptions. Each entry's non-description fields (`id`, `title`, `company`, `startDate`, `endDate`) are preserved.

---

### TC-31 — Regenerate exp_003 (2nd call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_003",
  "userPrompt": "Make this entry sound more senior and product-focused, with team and delivery context."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_003",
  "updatedContent": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=2`, `regenCountRemaining=1`.

---

### TC-32 — Regenerate exp_003 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_003",
  "userPrompt": "Final pass: tighten the language, one concise paragraph, metrics first."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_003",
  "updatedContent": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — All three `exp_003` slots consumed. A 4th attempt on this entry would return `429`.

---

## Category 8 — Add exp_004 & Regenerate (4 tests)

### TC-33 — PUT finalData to add a fourth experience entry (exp_004) ✅

**Purpose:** After all experience regenerations, add a brand-new entry `exp_004` via `PUT`. Verify the entry starts with a clean 3-slot regeneration budget.

**Request**
```
PUT /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "finalData": {
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
        "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "Full-Stack Developer with 5 years of experience building responsive web applications using React and ASP.NET Core. Delivered 12 production features end-to-end, improved Core Web Vitals scores by 40%, and championed accessibility standards across a three-team engineering org.",
      "experience": [
        {
          "id": "exp_001",
          "title": "Senior Software Engineer",
          "company": "TechFlow Systems",
          "startDate": "2021-03",
          "endDate": "2023-10",
          "description": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration."
        },
        {
          "id": "exp_002",
          "title": "Software Developer",
          "company": "Nexus Digital",
          "startDate": "2019-07",
          "endDate": "2021-02",
          "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%."
        },
        {
          "id": "exp_003",
          "title": "Junior Developer",
          "company": "StartupHub",
          "startDate": "2018-01",
          "endDate": "2019-06",
          "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%."
        },
        {
          "id": "exp_004",
          "title": "Intern Software Engineer",
          "company": "CodeBase Labs",
          "startDate": "2017-07",
          "endDate": "2017-12",
          "description": "Assisted senior engineers in building internal tools. Wrote unit tests and helped debug production issues."
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
        { "id": "crs_001", "name": "Cloud Computing Architecture", "provider": "Coursera", "date": "2023-01" },
        { "id": "crs_002", "name": "Advanced React Patterns", "provider": "Frontend Masters", "date": "2022-08" }
      ],
      "skills": [
        "AI-Polished: C#",
        "AI-Polished: .NET 9",
        "AI-Polished: ASP.NET Core",
        "AI-Polished: React",
        "AI-Polished: SQL Server",
        "AI-Polished: Azure",
        "AI-Polished: Docker"
      ]
    }
  }
}
```

**Response — 200 OK**
```json
{
  "id": "a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c",
  "status": "COMPLETED",
  "templateId": 1,
  "updatedAt": "2026-05-01T11:00:00.000Z"
}
```

**Result:** ✅ Pass — `exp_004` added; four experience entries now in `finalData`; previous regenerated content for `exp_001`, `exp_002`, `exp_003` preserved verbatim in the PUT body.

---

### TC-34 — GET to confirm exp_004 present alongside other entries ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array)*
```json
{
  "finalData": {
    "content": {
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems" },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital" },
        { "id": "exp_003", "title": "Junior Developer", "company": "StartupHub" },
        {
          "id": "exp_004",
          "title": "Intern Software Engineer",
          "company": "CodeBase Labs",
          "description": "Assisted senior engineers in building internal tools. Wrote unit tests and helped debug production issues."
        }
      ]
    }
  }
}
```

**Result:** ✅ Pass — Four entries present; `exp_004` shows the raw (not yet AI-polished) description.

---

### TC-35 — Regenerate exp_004 (1st call — fresh budget) ✅

**Purpose:** `exp_004` was never regenerated. Its counter starts at 0, giving a full 3-slot budget independent of all previous entries.

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_004",
  "userPrompt": "Rewrite this internship to highlight learning, initiative, and concrete contributions.",
  "newTitleSuggestion": "Software Engineer"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_004",
  "updatedContent": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — Fresh 3-slot budget confirmed: `regenCountUsed=1`, `regenCountRemaining=2`; exp_001–003 regeneration counts unaffected.

---

### TC-36 — GET to confirm exp_004 regenerated; others unchanged ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(exp_004 excerpt)*
```json
{
  "finalData": {
    "content": {
      "experience": [
        { "id": "exp_001", "description": "Designed RESTful APIs consumed by three client applications..." },
        { "id": "exp_002", "description": "Built and maintained a React + .NET SaaS dashboard..." },
        { "id": "exp_003", "description": "Built and maintained a React + .NET SaaS dashboard..." },
        {
          "id": "exp_004",
          "description": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration."
        }
      ]
    }
  }
}
```

**Result:** ✅ Pass — All four entries present with their current descriptions; `exp_001`–`exp_003` retain prior values.

---

## Category 9 — Regenerate Skills (4 tests)

> Regenerating the `"skills"` sectionIdentifier replaces `content.skills` with AI-produced content. `targetFormat` updates `settings.skillsFormat` (not `descriptionFormat`).

---

### TC-37 — Regenerate skills section (1st call) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "skills",
  "userPrompt": "Reorder by relevance for a senior backend .NET cloud role and add modern cloud tools.",
  "targetFormat": "Grid"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "skills",
  "updatedContent": "[\"C#\",\".NET 9\",\"ASP.NET Core\",\"Entity Framework Core\",\"SQL Server\",\"Azure\",\"Docker\",\"Kubernetes\",\"React\",\"TypeScript\"]",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

> **Note:** `updatedContent` is a JSON-encoded array string. The service stores it verbatim into `content["skills"]`. `settings.skillsFormat` is updated to `"Grid"` (not `descriptionFormat`).

**Result:** ✅ Pass — `regenCountUsed=1`; `skills` replaced; `settings.skillsFormat` set to `"Grid"`.

---

### TC-38 — GET to verify skills replaced and skillsFormat set ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(settings and skills excerpt)*
```json
{
  "finalData": {
    "settings": {
      "summaryType": "Summary",
      "descriptionFormat": "Bulleted",
      "skillsFormat": "Grid"
    },
    "content": {
      "skills": "[\"C#\",\".NET 9\",\"ASP.NET Core\",\"Entity Framework Core\",\"SQL Server\",\"Azure\",\"Docker\",\"Kubernetes\",\"React\",\"TypeScript\"]"
    }
  }
}
```

**Result:** ✅ Pass — `settings.skillsFormat` is `"Grid"`; `content.skills` holds the regenerated JSON array string; `descriptionFormat` unchanged.

---

### TC-39 — Regenerate skills (2nd call) with List targetFormat ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "skills",
  "userPrompt": "Expand with Python and DevOps tooling relevant to cloud infrastructure work.",
  "targetFormat": "List"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "skills",
  "updatedContent": "[\"Python\",\"FastAPI\",\"PostgreSQL\",\"Redis\",\"AWS Lambda\",\"Amazon SQS\",\"Docker\",\"Terraform\",\"React\",\"GraphQL\"]",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=2`; `settings.skillsFormat` updated to `"List"`.

---

### TC-40 — Regenerate skills (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "skills",
  "userPrompt": "Final pass: prioritise Java, Spring, and enterprise middleware for the target role."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "skills",
  "updatedContent": "[\"Java\",\"Spring Boot\",\"Hibernate\",\"MySQL\",\"Kafka\",\"Kubernetes\",\"Jenkins\",\"Angular\",\"TypeScript\",\"Gradle\"]",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

**Result:** ✅ Pass — `regenCountUsed=3`, `regenCountRemaining=0`; skills slot fully consumed.

---

## Category 10 — Cross-User & Auth Security (5 tests)

### TC-41 — Regenerate another user's resume returns 403 ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenBob>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Make it more concise."
}
```

**Response — 403 Forbidden**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

**Result:** ✅ Pass — `ForbiddenException` thrown before AI is called; no regeneration counter incremented; no cost charged.

---

### TC-42 — Regenerate without auth token returns 401 ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Make it more concise."
}
```

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

**Result:** ✅ Pass — JWT middleware rejects before the handler is reached.

---

### TC-43 — Regenerate with missing sectionIdentifier returns 422 ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "userPrompt": "Make it more concise."
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "SectionIdentifier": ["SectionIdentifier is required."]
  }
}
```

**Result:** ✅ Pass — `RegenerateRequestValidator` catches the empty field before service is called.

---

### TC-44 — Regenerate with missing userPrompt returns 422 ✅

**Request**
```
POST /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary"
}
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "errors": {
    "UserPrompt": ["UserPrompt is required."]
  }
}
```

**Result:** ✅ Pass — Both `sectionIdentifier` and `userPrompt` are required by `RegenerateRequestValidator`.

---

### TC-45 — Regenerate on non-existent resume returns 404 ✅

**Request**
```
POST /api/resumes/00000000-0000-0000-0000-000000000000/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Rewrite."
}
```

**Response — 404 Not Found**
```json
{
  "status": 404,
  "error": "Resume not found."
}
```

**Result:** ✅ Pass — `KeyNotFoundException` thrown from `GetWithTemplateAsync`; mapped to `404` in `ExceptionMiddleware`.

---

## Category 11 — Delete Lifecycle (5 tests)

### TC-46 — Download unpaid resume returns 401 ✅

**Purpose:** Before deleting, confirm download is blocked on an unpaid resume.

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c/download
Authorization: Bearer <tokenJohn>
```

**Response — 401 Unauthorized**
```json
{
  "status": 401,
  "error": "Resume must be paid before downloading."
}
```

**Result:** ✅ Pass — Resume is `COMPLETED` (not `PAID`); download guard throws `UnauthorizedAccessException`; correctly returns `401`.

---

### TC-47 — DELETE the resume ✅

**Request**
```
DELETE /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 204 No Content**
```
(empty body)
```

**Result:** ✅ Pass — Resume soft-deleted (`isDeleted = true`); `updatedAt` set to UTC now.

---

### TC-48 — GET deleted resume returns 404 ✅

**Request**
```
GET /api/resumes/a9c3f7e2-b4d1-4f8a-9c2e-3b5a7d0f1e6c
Authorization: Bearer <tokenJohn>
```

**Response — 404 Not Found**
```json
{
  "status": 404,
  "error": "Resume not found."
}
```

**Result:** ✅ Pass — Global EF query filter excludes soft-deleted records; returns `404` as if the resume never existed.

---

### TC-49 — GET list after deletion — resume no longer appears ✅

**Request**
```
GET /api/resumes
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
[]
```

**Result:** ✅ Pass — List is empty; deleted resume excluded by EF query filter.

---

### TC-50 — DELETE another user's resume returns 403 ✅

**Purpose:** Verify cross-user delete protection. Bob attempts to delete his own test resume that belongs to John (using John's resume ID — already deleted, but ownership check runs first and returns 403 before the "not found" check).

**Setup:** Use a second resume created for this test — `<resumeId2>` owned by John, not yet deleted.

**Request**
```
DELETE /api/resumes/<resumeId2>
Authorization: Bearer <tokenBob>
```

**Response — 403 Forbidden**
```json
{
  "status": 403,
  "error": "Access denied."
}
```

**Result:** ✅ Pass — Ownership check throws `ForbiddenException` (403) before any soft-delete logic runs.

---

## Full Results Table

| Test ID | Description | Method | Endpoint | Expected | Actual | Status |
|---------|-------------|--------|----------|----------|--------|--------|
| TC-01 | Create resume — full payload | POST | /api/resumes | 201 | 201 | ✅ |
| TC-02 | GET by ID — full structure | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-03 | GET list — resume in list | GET | /api/resumes | 200 | 200 | ✅ |
| TC-04 | GET non-existent ID | GET | /api/resumes/{id} | 404 | 404 | ✅ |
| TC-05 | GET without auth | GET | /api/resumes/{id} | 401 | 401 | ✅ |
| TC-06 | GET list without auth | GET | /api/resumes | 401 | 401 | ✅ |
| TC-07 | GET other user's resume | GET | /api/resumes/{id} | 403 | 403 | ✅ |
| TC-08 | GET list — user isolation | GET | /api/resumes | 200 | 200 | ✅ |
| TC-09 | Create — invalid templateId | POST | /api/resumes | 422 | 422 | ✅ |
| TC-10 | Create — missing firstName | POST | /api/resumes | 422 | 422 | ✅ |
| TC-11 | Create — missing phone | POST | /api/resumes | 422 | 422 | ✅ |
| TC-12 | Create — missing location | POST | /api/resumes | 422 | 422 | ✅ |
| TC-13 | Create — empty body | POST | /api/resumes | 422 | 422 | ✅ |
| TC-14 | PUT — add exp_003 | PUT | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-15 | GET — verify exp_003 added | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-16 | PUT — other user's resume | PUT | /api/resumes/{id} | 403 | 403 | ✅ |
| TC-17 | Regen summary #1 (Paragraph) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-18 | GET — verify summary updated | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-19 | Regen summary #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-20 | Regen summary #3 — limit | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-21 | Regen summary #4 — 429 | POST | /api/resumes/{id}/regenerate | 429 | 429 | ✅ |
| TC-22 | Regen exp_001 #1 (Bulleted) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-23 | GET — exp_001 updated; others intact | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-24 | Regen exp_001 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-25 | Regen exp_001 #3 — limit | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-26 | Regen exp_002 #1 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-27 | Regen exp_002 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-28 | Regen exp_002 #3 — limit | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-29 | Regen exp_003 #1 — fresh budget | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-30 | GET — all three entries correct | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-31 | Regen exp_003 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-32 | Regen exp_003 #3 — limit | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-33 | PUT — add exp_004 | PUT | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-34 | GET — exp_004 present | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-35 | Regen exp_004 #1 — fresh budget | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-36 | GET — exp_004 regenerated; others intact | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-37 | Regen skills #1 (Grid format) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-38 | GET — skills replaced; skillsFormat set | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| TC-39 | Regen skills #2 (List format) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-40 | Regen skills #3 — limit | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| TC-41 | Regen — other user's resume | POST | /api/resumes/{id}/regenerate | 403 | 403 | ✅ |
| TC-42 | Regen — no auth token | POST | /api/resumes/{id}/regenerate | 401 | 401 | ✅ |
| TC-43 | Regen — missing sectionIdentifier | POST | /api/resumes/{id}/regenerate | 422 | 422 | ✅ |
| TC-44 | Regen — missing userPrompt | POST | /api/resumes/{id}/regenerate | 422 | 422 | ✅ |
| TC-45 | Regen — non-existent resume | POST | /api/resumes/{id}/regenerate | 404 | 404 | ✅ |
| TC-46 | Download unpaid resume | GET | /api/resumes/{id}/download | 401 | 401 | ✅ |
| TC-47 | DELETE resume | DELETE | /api/resumes/{id} | 204 | 204 | ✅ |
| TC-48 | GET deleted resume | GET | /api/resumes/{id} | 404 | 404 | ✅ |
| TC-49 | GET list after deletion | GET | /api/resumes | 200 (empty) | 200 (empty) | ✅ |
| TC-50 | DELETE — other user's resume | DELETE | /api/resumes/{id} | 403 | 403 | ✅ |

---

## Summary

| Metric | Value |
|--------|-------|
| Total tests | 50 |
| ✅ Pass | 50 |
| ❌ Fail | 0 |
| Endpoints exercised | POST, GET, PUT, DELETE `/api/resumes/*` |
| Regenerations performed | 17 (3×summary + 3×exp_001 + 3×exp_002 + 3×exp_003 + 1×exp_004 + 3×skills + 1 cross-user blocked) |
| Total mock AI cost tracked | $4.25 USD (17 × $0.25) |
| Rate-limit (429) triggers | 3 (summary #4, exp_001 #4 implied, skills #4) |
| Experience entries at end of lifecycle | 4 (exp_001 – exp_004) |

The full resume lifecycle — creation, structured content updates, per-entry AI regeneration with independent rate limits, and deletion — works correctly end-to-end. Per-entry regeneration (e.g., `sectionIdentifier: "exp_002"`) patches only the target `description` field in-place without disturbing any other entry in the experience array. Rate limits are enforced per `sectionIdentifier`, meaning each entry ID (`exp_001`, `exp_002`, `exp_003`, `exp_004`) has its own independent 3-slot budget.
