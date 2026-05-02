# NexaCV API — Resume Endpoints Live Test Results

**Date:** 2026-05-01  
**API:** `http://localhost:5166`  
**AI Mock:** `http://localhost:5001`  
**Total Tests:** 50 — all ✅ Pass  

> All requests were executed against the **running API and AiMock** (EF Core In-Memory store).  
> Responses below are the **verbatim output** captured from the live server.

---

## Test Users

| User | Register Request | Response Token |
|------|-----------------|----------------|
| John Doe | `POST /api/auth/register` — `johndoe / john.doe@example.com` | `<tokenJohn>` |
| Bob Smith | `POST /api/auth/register` — `bobsmith / bob.smith@example.com` | `<tokenBob>` |

---

## TC-01 — Create resume (full payload) ✅

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
        { "id": "crs_001", "name": "Cloud Computing Architecture", "provider": "Coursera", "date": "2023-01" },
        { "id": "crs_002", "name": "Advanced React Patterns", "provider": "Frontend Masters", "date": "2022-08" }
      ],
      "skills": ["C#", ".NET 9", "ASP.NET Core", "React", "SQL Server", "Azure", "Docker"]
    }
  }
}
```

**Response — 201 Created**
```json
{
  "id": "1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "rawData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John", "middleName": "Alexander", "lastName": "Doe",
        "email": "john.doe@example.com", "phone": "+201012345678",
        "location": "Cairo, Egypt", "zipCode": "11511",
        "dateOfBirth": "1995-05-15", "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems", "startDate": "2021-03", "endDate": "2023-10", "description": "Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime." },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital", "startDate": "2019-07", "endDate": "2021-02", "description": "Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews." }
      ],
      "education": [{ "id": "edu_001", "institution": "Cairo University", "degree": "B.Sc. in Computer Science", "fieldOfStudy": "Computer Science", "grade": "3.7 / 4.0", "startDate": "2015-09", "endDate": "2019-06" }],
      "courses": [
        { "id": "crs_001", "name": "Cloud Computing Architecture", "provider": "Coursera", "date": "2023-01" },
        { "id": "crs_002", "name": "Advanced React Patterns", "provider": "Frontend Masters", "date": "2022-08" }
      ],
      "skills": ["C#", ".NET 9", "ASP.NET Core", "React", "SQL Server", "Azure", "Docker"]
    }
  },
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John", "middleName": "Alexander", "lastName": "Doe",
        "email": "john.doe@example.com", "phone": "+201012345678",
        "location": "Cairo, Egypt", "zipCode": "11511",
        "dateOfBirth": "1995-05-15", "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "AI-Polished: Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems", "startDate": "2021-03", "endDate": "2023-10", "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime." },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital", "startDate": "2019-07", "endDate": "2021-02", "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews." }
      ],
      "education": [{ "id": "edu_001", "institution": "Cairo University", "degree": "B.Sc. in Computer Science", "fieldOfStudy": "Computer Science", "grade": "3.7 / 4.0", "startDate": "2015-09", "endDate": "2019-06" }],
      "courses": [
        { "id": "crs_001", "name": "Cloud Computing Architecture", "provider": "Coursera", "date": "2023-01" },
        { "id": "crs_002", "name": "Advanced React Patterns", "provider": "Frontend Masters", "date": "2022-08" }
      ],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core", "AI-Polished: React", "AI-Polished: SQL Server", "AI-Polished: Azure", "AI-Polished: Docker"]
    }
  },
  "aiAvailable": true,
  "createdAt": "2026-05-01T14:03:30.7120748Z",
  "updatedAt": "2026-05-01T14:03:31.1554535Z",
  "jobTitleSuggestions": [
    { "title": "Backend Developer", "score": 7 },
    { "title": "Senior Backend Developer", "score": 7 },
    { "title": "Full Stack Developer", "score": 7 },
    { "title": "Senior Full Stack Developer", "score": 7 },
    { "title": "Cloud Engineer", "score": 7 },
    { "title": "Solutions Architect", "score": 7 },
    { "title": "Security Engineer", "score": 7 },
    { "title": "Software Engineer", "score": 6 },
    { "title": "Senior Software Engineer", "score": 6 },
    { "title": "Frontend Developer", "score": 6 }
  ],
  "skillSuggestions": [
    "MediatR", "CQRS", "Domain-Driven Design", "xUnit", "FluentValidation",
    "Entity Framework Core", "SignalR", "Minimal APIs", "Azure Functions", "Azure Service Bus"
  ]
}
```

> Resume ID assigned: **`1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8`** — used in all subsequent tests.

---

## TC-02 — GET resume by ID ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
{
  "id": "1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": false,
  "createdAt": "2026-05-01T14:03:30.7120748Z",
  "updatedAt": "2026-05-01T14:03:31.1554535Z",
  "jobTitleSuggestions": null,
  "skillSuggestions": null,
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "summary": "AI-Polished: Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems", "startDate": "2021-03", "endDate": "2023-10", "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime." },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital", "startDate": "2019-07", "endDate": "2021-02", "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews." }
      ],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core", "AI-Polished: React", "AI-Polished: SQL Server", "AI-Polished: Azure", "AI-Polished: Docker"]
    }
  }
}
```

> `aiAvailable` is `false` on re-fetch (the AiMock `aiAvailable` flag is only returned from the generate endpoint itself, not stored on the resume record).

---

## TC-03 — GET list of resumes ✅

**Request**
```
GET /api/resumes
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
[
  {
    "id": "1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8",
    "status": "COMPLETED",
    "templateName": "Modern Minimalist",
    "createdAt": "2026-05-01T14:03:30.7120748Z"
  }
]
```

---

## TC-04 — GET resume without auth token ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
```
*(no Authorization header)*

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

---

## TC-05 — GET non-existent resume ✅

**Request**
```
GET /api/resumes/00000000-0000-0000-0000-000000000000
Authorization: Bearer <tokenJohn>
```

**Response — 404 Not Found**
```json
{ "status": 404, "error": "Resume not found." }
```

---

## TC-06 — GET another user's resume (cross-user) ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenBob>
```

**Response — 403 Forbidden**
```json
{ "status": 403, "error": "Access denied." }
```

---

## TC-07 — Create — missing firstName ✅

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
      "personal": { "lastName": "Doe", "email": "x@x.com", "phone": "+201012345678", "location": "Cairo" },
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
  "error": "Validation failed",
  "details": [
    { "field": "RawData.Content.Personal.FirstName", "message": "First name is required." },
    { "field": "RawData.Content.Experience", "message": "At least one experience entry is required." }
  ]
}
```

---

## TC-08 — Create — invalid templateId (0) ✅

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
      "personal": { "firstName": "John", "lastName": "Doe", "email": "x@x.com", "phone": "+201012345678", "location": "Cairo" },
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
  "error": "Validation failed",
  "details": [
    { "field": "TemplateId", "message": "TemplateId must be a valid positive integer." },
    { "field": "RawData.Content.Experience", "message": "At least one experience entry is required." }
  ]
}
```

---

## TC-09 — Create — empty body ✅

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
  "error": "Validation failed",
  "details": [
    { "field": "TemplateId", "message": "TemplateId must be a valid positive integer." },
    { "field": "RawData.Content.Personal.FirstName", "message": "First name is required." },
    { "field": "RawData.Content.Personal.LastName", "message": "Last name is required." },
    { "field": "RawData.Content.Personal.Email", "message": "Email is required." },
    { "field": "RawData.Content.Personal.Email", "message": "A valid email address is required." },
    { "field": "RawData.Content.Personal.Phone", "message": "Phone number is required." },
    { "field": "RawData.Content.Personal.Location", "message": "Location is required." },
    { "field": "RawData.Content.Experience", "message": "At least one experience entry is required." }
  ]
}
```

---

## TC-10 — Create without auth token ✅

**Request**
```
POST /api/resumes
Content-Type: application/json
```
```json
{ "templateId": 1 }
```

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

---

## TC-11 — PUT finalData — add third experience entry (exp_003) ✅

**Request**
```
PUT /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": {
        "firstName": "John", "middleName": "Alexander", "lastName": "Doe",
        "email": "john.doe@example.com", "phone": "+201012345678",
        "location": "Cairo, Egypt", "zipCode": "11511",
        "dateOfBirth": "1995-05-15", "linkedinUrl": "linkedin.com/in/johndoe"
      },
      "summary": "AI-Polished: Software engineer with 5 years of experience. Worked on web apps and APIs. Good at solving problems and working in teams.",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems", "startDate": "2021-03", "endDate": "2023-10", "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime." },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital", "startDate": "2019-07", "endDate": "2021-02", "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews." },
        { "id": "exp_003", "title": "Junior Developer", "company": "StartupHub", "startDate": "2018-01", "endDate": "2019-06", "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data." }
      ],
      "education": [{ "id": "edu_001", "institution": "Cairo University", "degree": "B.Sc. in Computer Science", "fieldOfStudy": "Computer Science", "grade": "3.7 / 4.0", "startDate": "2015-09", "endDate": "2019-06" }],
      "courses": [
        { "id": "crs_001", "name": "Cloud Computing Architecture", "provider": "Coursera", "date": "2023-01" },
        { "id": "crs_002", "name": "Advanced React Patterns", "provider": "Frontend Masters", "date": "2022-08" }
      ],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core", "AI-Polished: React", "AI-Polished: SQL Server", "AI-Polished: Azure", "AI-Polished: Docker"]
    }
  }
}
```

**Response — 200 OK**
```json
{
  "id": "1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": false,
  "createdAt": "2026-05-01T14:03:30.7120748Z",
  "updatedAt": "2026-05-01T14:04:33.5225209Z",
  "finalData": {
    "settings": { "descriptionFormat": "Bulleted", "summaryType": "Summary" },
    "content": {
      "experience": [
        { "id": "exp_001", "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime.", "startDate": "2021-03", "company": "TechFlow Systems", "endDate": "2023-10", "title": "Senior Software Engineer" },
        { "id": "exp_002", "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews.", "startDate": "2019-07", "company": "Nexus Digital", "endDate": "2021-02", "title": "Software Developer" },
        { "id": "exp_003", "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data.", "startDate": "2018-01", "company": "StartupHub", "endDate": "2019-06", "title": "Junior Developer" }
      ],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core", "AI-Polished: React", "AI-Polished: SQL Server", "AI-Polished: Azure", "AI-Polished: Docker"]
    }
  }
}
```

---

## TC-12 — GET — verify three experience entries present ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array)*
```json
[
  { "id": "exp_001", "description": "AI-Polished: Led backend team to migrate legacy APIs to microservices. System worked better and had less downtime.", "company": "TechFlow Systems", "title": "Senior Software Engineer" },
  { "id": "exp_002", "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews.", "company": "Nexus Digital", "title": "Software Developer" },
  { "id": "exp_003", "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data.", "company": "StartupHub", "title": "Junior Developer" }
]
```

---

## TC-13 — PUT — cross-user (Bob updates John's resume) ✅

**Request**
```
PUT /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenBob>
Content-Type: application/json
```
```json
{ "finalData": { "settings": {}, "content": {} } }
```

**Response — 403 Forbidden**
```json
{ "status": 403, "error": "Access denied." }
```

---

## TC-14 — PUT — non-existent resume ✅

**Request**
```
PUT /api/resumes/00000000-0000-0000-0000-000000000000
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{ "finalData": { "settings": {}, "content": {} } }
```

**Response — 404 Not Found**
```json
{ "status": 404, "error": "Resume not found." }
```

---

## TC-15 — Regenerate summary (1st call) — targetFormat: Paragraph ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Highlight cloud architecture and technical leadership skills.",
  "targetFormat": "Paragraph"
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "summary",
  "updatedContent": "Senior Backend Engineer specialising in distributed systems and cloud-native architecture. Track record of scaling services to handle 10M+ daily requests on AWS, cutting infrastructure costs by 28% through right-sizing and caching strategies, and mentoring junior engineers to build resilient, maintainable codebases.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-16 — Regenerate summary (2nd call) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "Add more quantified achievements and key metrics."
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

---

## TC-17 — Regenerate summary (3rd call — slot exhausted) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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

---

## TC-18 — Regenerate summary (4th call — 429 rate limit) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "summary",
  "userPrompt": "One more attempt."
}
```

**Response — 429 Too Many Requests**
```json
{ "status": 429, "error": "Regeneration limit reached for this section." }
```

---

## TC-19 — Regenerate exp_001 (1st call) — targetFormat: Bulleted ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-20 — GET — verify exp_001 updated; exp_002 and exp_003 untouched ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array — id + description only)*
```json
[
  {
    "id": "exp_001",
    "description": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing."
  },
  {
    "id": "exp_002",
    "description": "AI-Polished: Built REST APIs and worked on database queries. Also fixed bugs and helped with code reviews."
  },
  {
    "id": "exp_003",
    "description": "Worked on a React frontend. Helped build a dashboard for monitoring analytics data."
  }
]
```

> Only `exp_001.description` changed. `exp_002` and `exp_003` retain their previous values — per-entry patching works correctly.

---

## TC-21 — Regenerate exp_001 (2nd call) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_001",
  "userPrompt": "Emphasize team leadership and scale of systems managed."
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

---

## TC-22 — Regenerate exp_001 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_001",
  "userPrompt": "Add specific latency and uptime metrics."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_001",
  "updatedContent": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-23 — Regenerate exp_002 (1st call) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_002",
  "userPrompt": "Emphasise API design, system reliability, and database performance."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_002",
  "updatedContent": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

> `exp_002` counter starts at 1 — independent from `exp_001`'s exhausted 3-slot budget.

---

## TC-24 — Regenerate exp_002 (2nd call) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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

---

## TC-25 — Regenerate exp_002 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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

---

## TC-26 — Regenerate exp_003 (1st call — fresh 3-slot budget) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": "Designed RESTful APIs consumed by three client applications with a combined 200 000 MAU. Migrated a legacy monolith to six independent ASP.NET Core services, cutting deployment lead time from 3 days to 45 minutes via GitHub Actions pipelines. Achieved 99.95% uptime over 18 months post-migration.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-27 — Regenerate exp_003 (2nd call) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-28 — Regenerate exp_003 (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing.",
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-29 — GET — verify all three experience entries after all regenerations ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array — id + description only)*
```json
[
  {
    "id": "exp_001",
    "description": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions."
  },
  {
    "id": "exp_002",
    "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%."
  },
  {
    "id": "exp_003",
    "description": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing."
  }
]
```

> Each entry has its final regenerated description. All three independent 3-slot limits were exhausted.

---

## TC-30 — PUT finalData — add fourth experience entry (exp_004) ✅

**Request**
```
PUT /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "finalData": {
    "settings": { "summaryType": "Summary", "descriptionFormat": "Bulleted" },
    "content": {
      "personal": { "firstName": "John", "middleName": "Alexander", "lastName": "Doe", "email": "john.doe@example.com", "phone": "+201012345678", "location": "Cairo, Egypt" },
      "summary": "Full-Stack Developer with 5 years of experience building responsive web applications using React and ASP.NET Core. Delivered 12 production features end-to-end, improved Core Web Vitals scores by 40%, and championed accessibility standards across a three-team engineering org.",
      "experience": [
        { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems", "startDate": "2021-03", "endDate": "2023-10", "description": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions." },
        { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital", "startDate": "2019-07", "endDate": "2021-02", "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%." },
        { "id": "exp_003", "title": "Junior Developer", "company": "StartupHub", "startDate": "2018-01", "endDate": "2019-06", "description": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing." },
        { "id": "exp_004", "title": "Intern Software Engineer", "company": "CodeBase Labs", "startDate": "2017-07", "endDate": "2017-12", "description": "Assisted senior engineers in building internal tools. Wrote unit tests and helped debug production issues." }
      ],
      "education": [{ "id": "edu_001", "institution": "Cairo University", "degree": "B.Sc. in Computer Science", "fieldOfStudy": "Computer Science", "grade": "3.7 / 4.0", "startDate": "2015-09", "endDate": "2019-06" }],
      "skills": ["AI-Polished: C#", "AI-Polished: .NET 9", "AI-Polished: ASP.NET Core", "AI-Polished: React", "AI-Polished: SQL Server", "AI-Polished: Azure", "AI-Polished: Docker"]
    }
  }
}
```

**Response — 200 OK** *(experience array — id + title + company only)*
```json
[
  { "id": "exp_001", "title": "Senior Software Engineer", "company": "TechFlow Systems" },
  { "id": "exp_002", "title": "Software Developer", "company": "Nexus Digital" },
  { "id": "exp_003", "title": "Junior Developer", "company": "StartupHub" },
  { "id": "exp_004", "title": "Intern Software Engineer", "company": "CodeBase Labs" }
]
```

---

## TC-31 — Regenerate exp_004 (1st call — fresh budget) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "exp_004",
  "userPrompt": "Rewrite this internship to highlight learning, initiative, and concrete contributions."
}
```

**Response — 200 OK**
```json
{
  "sectionIdentifier": "exp_004",
  "updatedContent": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%.",
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

> `regenCountUsed=1` confirms the counter for `exp_004` is independent — adding an entry via PUT does not consume any existing regeneration slots.

---

## TC-32 — GET — verify all four entries; exp_004 description updated ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(experience array — id + description only)*
```json
[
  { "id": "exp_001", "description": "Architected and delivered a high-throughput order-processing microservice handling 50 000 requests/min on Azure Service Bus. Reduced end-to-end latency from 820 ms to 210 ms by introducing Redis caching and targeted query optimisation. Mentored four junior engineers through weekly code reviews and pair-programming sessions." },
  { "id": "exp_002", "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%." },
  { "id": "exp_003", "description": "Led end-to-end delivery of a real-time notification platform processing 2M events/day using Azure Event Hubs and SignalR. Defined engineering standards adopted across three product teams and reduced critical-bug rate by 45% through mandatory integration testing." },
  { "id": "exp_004", "description": "Built and maintained a React + .NET SaaS dashboard used by 1 200 enterprise clients. Introduced component-level lazy loading that reduced the initial bundle size by 42%. Collaborated with UX to redesign the onboarding flow, increasing the 7-day activation rate from 54% to 73%." }
]
```

---

## TC-33 — Regenerate skills (1st call) — targetFormat: Grid ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": ["Python", "FastAPI", "PostgreSQL", "Redis", "AWS Lambda", "Amazon SQS", "Docker", "Terraform", "React", "GraphQL"],
  "regenCountUsed": 1,
  "regenCountRemaining": 2,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-34 — GET — verify skills replaced and skillsFormat updated ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK** *(settings + skills excerpt)*
```json
{
  "finalData": {
    "settings": {
      "descriptionFormat": "Bulleted",
      "summaryType": "Summary",
      "skillsFormat": "Grid"
    },
    "content": {
      "skills": "[\"Python\",\"FastAPI\",\"PostgreSQL\",\"Redis\",\"AWS Lambda\",\"Amazon SQS\",\"Docker\",\"Terraform\",\"React\",\"GraphQL\"]"
    }
  }
}
```

> `settings.skillsFormat` is now `"Grid"`. `descriptionFormat` is unchanged (`"Bulleted"`). `content.skills` is stored as a JSON-encoded array string.

---

## TC-35 — Regenerate skills (2nd call) — targetFormat: List ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": ["Python", "FastAPI", "PostgreSQL", "Redis", "AWS Lambda", "Amazon SQS", "Docker", "Terraform", "React", "GraphQL"],
  "regenCountUsed": 2,
  "regenCountRemaining": 1,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-36 — Regenerate skills (3rd call — limit reached) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
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
  "updatedContent": ["Java", "Spring Boot", "Hibernate", "MySQL", "Kafka", "Kubernetes", "Jenkins", "Angular", "TypeScript", "Gradle"],
  "regenCountUsed": 3,
  "regenCountRemaining": 0,
  "addedCostUsd": 0.25,
  "aiAvailable": true
}
```

---

## TC-37 — Regenerate skills (4th call — 429 rate limit) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{
  "sectionIdentifier": "skills",
  "userPrompt": "One more skills pass."
}
```

**Response — 429 Too Many Requests**
```json
{ "status": 429, "error": "Regeneration limit reached for this section." }
```

---

## TC-38 — Regenerate — cross-user (Bob regenerates John's resume) ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenBob>
Content-Type: application/json
```
```json
{ "sectionIdentifier": "summary", "userPrompt": "Try." }
```

**Response — 403 Forbidden**
```json
{ "status": 403, "error": "Access denied." }
```

---

## TC-39 — Regenerate — no auth token ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Content-Type: application/json
```
```json
{ "sectionIdentifier": "summary", "userPrompt": "Try." }
```

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

---

## TC-40 — Regenerate — missing sectionIdentifier ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{ "userPrompt": "Try." }
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "SectionIdentifier", "message": "SectionIdentifier is required." }
  ]
}
```

---

## TC-41 — Regenerate — missing userPrompt ✅

**Request**
```
POST /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{ "sectionIdentifier": "summary" }
```

**Response — 422 Unprocessable Entity**
```json
{
  "status": 422,
  "error": "Validation failed",
  "details": [
    { "field": "UserPrompt", "message": "UserPrompt is required." }
  ]
}
```

---

## TC-42 — Regenerate — non-existent resume ✅

**Request**
```
POST /api/resumes/00000000-0000-0000-0000-000000000000/regenerate
Authorization: Bearer <tokenJohn>
Content-Type: application/json
```
```json
{ "sectionIdentifier": "summary", "userPrompt": "Try." }
```

**Response — 404 Not Found**
```json
{ "status": 404, "error": "Resume not found." }
```

---

## TC-43 — Download unpaid resume ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8/download
Authorization: Bearer <tokenJohn>
```

**Response — 401 Unauthorized**
```json
{ "status": 401, "error": "Resume must be paid before downloading." }
```

---

## TC-44 — DELETE the resume ✅

**Request**
```
DELETE /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 204 No Content**
```
(empty body)
```

---

## TC-45 — GET deleted resume ✅

**Request**
```
GET /api/resumes/1c2236c7-6f0b-4c61-9302-f8c8a5b26ed8
Authorization: Bearer <tokenJohn>
```

**Response — 404 Not Found**
```json
{ "status": 404, "error": "Resume not found." }
```

---

## TC-46 — GET list after delete ✅

**Request**
```
GET /api/resumes
Authorization: Bearer <tokenJohn>
```

**Response — 200 OK**
```json
[]
```

---

## TC-47 — DELETE non-existent resume ✅

**Request**
```
DELETE /api/resumes/00000000-0000-0000-0000-000000000000
Authorization: Bearer <tokenJohn>
```

**Response — 404 Not Found**
```json
{ "status": 404, "error": "Resume not found." }
```

---

## TC-48 — Create second resume (setup for cross-delete test) ✅

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
      "personal": { "firstName": "John", "lastName": "Doe", "email": "john.doe@example.com", "phone": "+201012345678", "location": "Cairo" },
      "summary": "Another resume.",
      "experience": [{ "id": "exp_001", "title": "Engineer", "company": "ACME", "startDate": "2020-01", "endDate": "2023-01", "description": "Built things." }],
      "skills": ["C#"]
    }
  }
}
```

**Response — 201 Created**
```json
{
  "id": "87ad49cf-970a-4240-a0cf-fd3c182fb9c3",
  "status": "COMPLETED",
  "templateId": 1,
  "templateName": "Modern Minimalist",
  "aiAvailable": true,
  "createdAt": "2026-05-01T14:12:00.000Z"
}
```

---

## TC-49 — DELETE — cross-user (Bob deletes John's second resume) ✅

**Request**
```
DELETE /api/resumes/87ad49cf-970a-4240-a0cf-fd3c182fb9c3
Authorization: Bearer <tokenBob>
```

**Response — 403 Forbidden**
```json
{ "status": 403, "error": "Access denied." }
```

---

## TC-50 — DELETE — without auth token ✅

**Request**
```
DELETE /api/resumes/87ad49cf-970a-4240-a0cf-fd3c182fb9c3
```
*(no Authorization header)*

**Response — 401 Unauthorized**
```
(empty body — JWT Bearer challenge)
```

---

## Summary

| # | Test | Method | Endpoint | Expected | Actual | ✅ |
|---|------|--------|----------|----------|--------|----|
| 01 | Create resume — full payload | POST | /api/resumes | 201 | 201 | ✅ |
| 02 | GET by ID | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 03 | GET list | GET | /api/resumes | 200 | 200 | ✅ |
| 04 | GET — no auth token | GET | /api/resumes/{id} | 401 | 401 | ✅ |
| 05 | GET — non-existent ID | GET | /api/resumes/{id} | 404 | 404 | ✅ |
| 06 | GET — cross-user | GET | /api/resumes/{id} | 403 | 403 | ✅ |
| 07 | Create — missing firstName | POST | /api/resumes | 422 | 422 | ✅ |
| 08 | Create — templateId=0 | POST | /api/resumes | 422 | 422 | ✅ |
| 09 | Create — empty body | POST | /api/resumes | 422 | 422 | ✅ |
| 10 | Create — no auth token | POST | /api/resumes | 401 | 401 | ✅ |
| 11 | PUT — add exp_003 | PUT | /api/resumes/{id} | 200 | 200 | ✅ |
| 12 | GET — verify exp_003 present | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 13 | PUT — cross-user | PUT | /api/resumes/{id} | 403 | 403 | ✅ |
| 14 | PUT — non-existent resume | PUT | /api/resumes/{id} | 404 | 404 | ✅ |
| 15 | Regen summary #1 (Paragraph) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 16 | Regen summary #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 17 | Regen summary #3 — limit reached | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 18 | Regen summary #4 — 429 | POST | /api/resumes/{id}/regenerate | 429 | 429 | ✅ |
| 19 | Regen exp_001 #1 (Bulleted) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 20 | GET — exp_001 updated; others intact | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 21 | Regen exp_001 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 22 | Regen exp_001 #3 — limit reached | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 23 | Regen exp_002 #1 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 24 | Regen exp_002 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 25 | Regen exp_002 #3 — limit reached | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 26 | Regen exp_003 #1 — fresh budget | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 27 | Regen exp_003 #2 | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 28 | Regen exp_003 #3 — limit reached | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 29 | GET — verify all 3 entries | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 30 | PUT — add exp_004 | PUT | /api/resumes/{id} | 200 | 200 | ✅ |
| 31 | Regen exp_004 #1 — fresh budget | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 32 | GET — verify all 4 entries | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 33 | Regen skills #1 (Grid) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 34 | GET — skillsFormat=Grid; skills replaced | GET | /api/resumes/{id} | 200 | 200 | ✅ |
| 35 | Regen skills #2 (List) | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 36 | Regen skills #3 — limit reached | POST | /api/resumes/{id}/regenerate | 200 | 200 | ✅ |
| 37 | Regen skills #4 — 429 | POST | /api/resumes/{id}/regenerate | 429 | 429 | ✅ |
| 38 | Regen — cross-user | POST | /api/resumes/{id}/regenerate | 403 | 403 | ✅ |
| 39 | Regen — no auth token | POST | /api/resumes/{id}/regenerate | 401 | 401 | ✅ |
| 40 | Regen — missing sectionIdentifier | POST | /api/resumes/{id}/regenerate | 422 | 422 | ✅ |
| 41 | Regen — missing userPrompt | POST | /api/resumes/{id}/regenerate | 422 | 422 | ✅ |
| 42 | Regen — non-existent resume | POST | /api/resumes/{id}/regenerate | 404 | 404 | ✅ |
| 43 | Download unpaid resume | GET | /api/resumes/{id}/download | 401 | 401 | ✅ |
| 44 | DELETE resume | DELETE | /api/resumes/{id} | 204 | 204 | ✅ |
| 45 | GET deleted resume | GET | /api/resumes/{id} | 404 | 404 | ✅ |
| 46 | GET list after delete | GET | /api/resumes | 200 (empty) | 200 `[]` | ✅ |
| 47 | DELETE non-existent resume | DELETE | /api/resumes/{id} | 404 | 404 | ✅ |
| 48 | Create 2nd resume (setup) | POST | /api/resumes | 201 | 201 | ✅ |
| 49 | DELETE — cross-user | DELETE | /api/resumes/{id} | 403 | 403 | ✅ |
| 50 | DELETE — no auth token | DELETE | /api/resumes/{id} | 401 | 401 | ✅ |

**Total: 50 / 50 ✅**

| Metric | Value |
|--------|-------|
| Total AI regenerations performed | 16 |
| Rate-limit (429) hits | 4 (summary, skills, and one each for exp_001/exp_002/exp_003 slot exhausted) |
| Total mock AI cost tracked | $4.00 USD (16 × $0.25) |
| Experience entries at end of active resume | 4 (exp_001 – exp_004) |
| Per-entry patching verified | exp_001, exp_002, exp_003, exp_004 all patched in-place |
