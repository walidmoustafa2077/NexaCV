# NexaCV — Frontend Development Plan

> **Status:** Project scaffolded. Ready for implementation.
> **Stack:** Next.js 16 · React 19 · TypeScript 5 · Tailwind CSS v4 · Shadcn/UI · TanStack Query · React Hook Form · Zod · Zustand · Sonner

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [Phase 1 — Foundation & Shared Infrastructure](#2-phase-1--foundation--shared-infrastructure)
3. [Phase 2 — Authentication Screens](#3-phase-2--authentication-screens)
4. [Phase 3 — Dashboard & Resume Management](#4-phase-3--dashboard--resume-management)
5. [Phase 4 — Resume Creation Wizard](#5-phase-4--resume-creation-wizard)
6. [Phase 5 — AI Resume Editor](#6-phase-5--ai-resume-editor)
7. [Phase 6 — Payment Flow](#7-phase-6--payment-flow)
8. [Phase 7 — Static & Info Pages](#8-phase-7--static--info-pages)
9. [Screen → Route Mapping](#9-screen--route-mapping)
10. [Shared Components Catalogue](#10-shared-components-catalogue)
11. [State & Data Layer](#11-state--data-layer)
12. [Implementation Rules](#12-implementation-rules)

---

## 1. Project Structure

```
frontend/
├── src/
│   ├── app/                          # Next.js App Router
│   │   ├── (auth)/                   # Auth route group (no nav)
│   │   │   ├── login/page.tsx
│   │   │   ├── register/page.tsx
│   │   │   └── forgot-password/page.tsx
│   │   ├── (main)/                   # Authenticated app shell (with sidebar)
│   │   │   ├── dashboard/page.tsx
│   │   │   ├── resumes/
│   │   │   │   ├── page.tsx          # My Resumes list
│   │   │   │   └── [id]/
│   │   │   │       ├── page.tsx      # Resume editor / review
│   │   │   │       └── payment/page.tsx
│   │   │   └── settings/page.tsx     # Profile settings
│   │   ├── (wizard)/                 # Resume creation wizard (step layout)
│   │   │   └── create/
│   │   │       ├── layout.tsx        # Wizard shell with stepper
│   │   │       ├── template/page.tsx # Template gallery
│   │   │       ├── start/page.tsx    # Start from scratch / import PDF
│   │   │       ├── personal/page.tsx # Step 1
│   │   │       ├── education/page.tsx # Step 2
│   │   │       ├── experience/page.tsx # Step 3 (AI)
│   │   │       ├── courses/page.tsx  # Step 4a
│   │   │       ├── skills/page.tsx   # Step 4b — summary & skills
│   │   │       └── review/page.tsx   # Step 5 — review & finalize
│   │   ├── (public)/                 # Marketing / info pages (with top nav)
│   │   │   ├── page.tsx              # Landing page
│   │   │   ├── about/page.tsx
│   │   │   ├── privacy/page.tsx
│   │   │   ├── terms/page.tsx
│   │   │   └── support/page.tsx
│   │   ├── ai-generating/page.tsx    # Full-screen AI loading state
│   │   ├── layout.tsx                # Root layout (fonts, Material Symbols)
│   │   └── globals.css               # Tailwind v4 theme (MD3 colors)
│   │
│   ├── components/
│   │   ├── ui/                       # Shadcn/UI primitives (auto-generated)
│   │   ├── layout/
│   │   │   ├── TopNavBar.tsx         # Public marketing nav
│   │   │   ├── AppSidebar.tsx        # Authenticated app sidebar
│   │   │   ├── WizardStepper.tsx     # Step progress bar for wizard
│   │   │   └── PageHeader.tsx        # Reusable section header
│   │   ├── auth/
│   │   │   ├── LoginForm.tsx
│   │   │   ├── RegisterForm.tsx
│   │   │   └── ForgotPasswordForm.tsx
│   │   ├── resume/
│   │   │   ├── ResumeCard.tsx        # Card in My Resumes list
│   │   │   ├── ResumeStatusBadge.tsx # DRAFT / COMPLETED / PAID badge
│   │   │   ├── TemplateCard.tsx      # Template selection card
│   │   │   ├── RegenerateButton.tsx  # AI sparkle button with counter
│   │   │   ├── RegenerationModal.tsx # Section AI regen modal
│   │   │   └── ResumePreview.tsx     # Final resume preview panel
│   │   ├── wizard/
│   │   │   ├── PersonalInfoStep.tsx
│   │   │   ├── EducationStep.tsx
│   │   │   ├── ExperienceStep.tsx
│   │   │   ├── CoursesStep.tsx
│   │   │   ├── SkillsSummaryStep.tsx
│   │   │   └── ReviewStep.tsx
│   │   ├── payment/
│   │   │   ├── PriceBreakdown.tsx
│   │   │   └── PaymentStatusPoller.tsx
│   │   └── shared/
│   │       ├── FormField.tsx         # Label + input + error message
│   │       ├── SectionCard.tsx       # White card with shadow-sm
│   │       ├── SkeletonCard.tsx      # Loading skeleton
│   │       ├── EmptyState.tsx
│   │       └── MaterialIcon.tsx      # Typed wrapper for Material Symbols
│   │
│   ├── lib/
│   │   ├── api/
│   │   │   ├── client.ts             # Fetch wrapper (base URL, auth header)
│   │   │   ├── auth.ts               # Auth API calls
│   │   │   ├── resumes.ts            # Resume API calls
│   │   │   ├── templates.ts          # Template API calls
│   │   │   └── transactions.ts       # Payment API calls
│   │   ├── schemas/
│   │   │   ├── auth.schemas.ts       # Zod schemas for register/login
│   │   │   ├── resume.schemas.ts     # Zod schemas for wizard form
│   │   │   └── transaction.schemas.ts
│   │   ├── query/
│   │   │   └── keys.ts               # TanStack Query key factory
│   │   └── utils.ts                  # cn(), formatDate(), etc.
│   │
│   ├── hooks/
│   │   ├── useAuth.ts                # Auth state from Zustand
│   │   ├── useResumes.ts             # Resume list query
│   │   ├── useResumeDetail.ts        # Single resume query
│   │   ├── useTemplates.ts           # Templates query
│   │   └── useRegenerate.ts          # AI regeneration mutation
│   │
│   ├── store/
│   │   ├── authStore.ts              # Zustand: JWT token, user profile
│   │   └── wizardStore.ts            # Zustand: multi-step form state
│   │
│   └── types/
│       ├── api.types.ts              # All API request/response types
│       └── enums.ts                  # ResumeStatus, PaymentStatus, etc.
│
├── .env.local                        # NEXT_PUBLIC_API_URL
└── components.json                   # Shadcn config
```

---

## 2. Phase 1 — Foundation & Shared Infrastructure

> **Goal:** Everything that every other phase depends on.

### 2.1 API Client

**File:** `src/lib/api/client.ts`

- Single `apiFetch()` function wrapping native `fetch`
- Automatically injects `Authorization: Bearer <token>` from Zustand auth store
- Parses JSON and throws typed errors for 4xx/5xx
- Handles `422` validation errors separately → returns `details[]` per-field

### 2.2 TypeScript Types

**File:** `src/types/api.types.ts`

Define all request/response interfaces from the Frontend Implementation Guide:
- `AuthResponse`, `RegisterRequest`, `LoginRequest`
- `UserProfileDto`, `UpdateUserRequest`
- `TemplateDto`
- `ResumeSummaryDto`, `ResumeDetailDto`, `CreateResumeRequest`, `UpdateFinalDataRequest`
- `RegenerateRequest`, `RegenerateResponse`
- `CheckoutRequest`, `CheckoutResponse`, `TransactionDto`
- `ApiErrorResponse`, `ValidationErrorResponse`

**File:** `src/types/enums.ts`
```ts
export type ResumeStatus = "DRAFT" | "COMPLETED" | "PAID";
export type PaymentStatus = "PENDING" | "SUCCESS" | "FAILED";
export type SummaryType = "Summary" | "Objective";
export type DescriptionFormat = "Paragraph" | "Bulleted";
```

### 2.3 Zod Validation Schemas

**File:** `src/lib/schemas/auth.schemas.ts`
- `registerSchema` — firstName, lastName, username, email, password, dateOfBirth
- `loginSchema` — email, password

**File:** `src/lib/schemas/resume.schemas.ts`
- `createResumeSchema` — full wizard form validation mirroring backend rules
- `regenerateSchema` — sectionIdentifier, userPrompt

### 2.4 Zustand Stores

**File:** `src/store/authStore.ts`
```ts
// Persisted in memory only (NOT localStorage — security)
{ token, userId, isAuthenticated, setAuth, clearAuth }
```

**File:** `src/store/wizardStore.ts`
```ts
// Holds multi-step wizard data across steps
{ currentStep, formData, jobTitleSuggestions, skillSuggestions, setStep, setFormData, setSuggestions, reset }
```

### 2.5 TanStack Query Setup

**File:** `src/app/layout.tsx` (or a providers wrapper)
- Wrap app in `<QueryClientProvider>`
- Configure: `staleTime: 1000 * 60 * 5`, retry `1`

### 2.6 Sonner Toast Setup

- Add `<Toaster />` from `sonner` in root layout
- Position: `bottom-right`

### 2.7 Shared Components

| Component | Description |
|-----------|-------------|
| `MaterialIcon` | `<span className="material-symbols-outlined">{name}</span>` — typed with icon name |
| `FormField` | Label + Shadcn Input + error message. Required: `label`, `error` |
| `SectionCard` | `bg-white rounded-xl shadow-sm border border-outline-variant p-6` wrapper |
| `SkeletonCard` | Skeleton placeholder for resume cards, form sections |
| `EmptyState` | Empty list state with icon, title, and CTA button |

### 2.8 App Shell — StructureFlow Design System

> **UI Mockup:** `structureflow/` (DESIGN.md — design tokens reference, no interactive screen)

`structureflow/` is the canonical design-token source for the entire application. It defines:
- **Full MD3 color palette** — all 40+ surface, primary, secondary, tertiary, error tokens
- **Typography scale** — H1–H5 in Manrope, body/label/caption in Inter, with exact font size, weight, line height, letter spacing
- **Spacing system** — base unit 4px, container max-width 768px, section-margin 48px, form-gap 24px, input padding
- **Border radius** — `DEFAULT: 4px`, `lg: 8px`, `xl: 12px`, `full: 9999px`

All values from this file are already encoded in `src/app/globals.css` as CSS custom properties under `@theme inline`. No separate implementation file needed — this is reference only.

**AppSidebar** (the persistent navigation shell) is also established in this phase:
- `components/layout/AppSidebar.tsx` — fixed 240px left sidebar
- Logo + brand name at top
- Nav items: Dashboard (`home`), My Resumes (`description`), Settings (`settings`)
- Active item: left border `box-shadow: inset -2px 0 0 0 var(--color-primary)`, `bg-secondary-container`
- User avatar + display name + Logout (`logout`) at bottom
- Used in `(main)/layout.tsx` to wrap all authenticated pages

---

## 3. Phase 2 — Authentication Screens

> **UI Mockups:** `sign_in/`, `sign_up/`, `forgot_password/`

### Screens

#### `(auth)/login` — Sign In
- Email + Password fields with React Hook Form + `loginSchema`
- "Sign In" primary button → `POST /api/auth/login`
- On success: store token in Zustand, redirect to `/dashboard`
- On 401: toast "Invalid email or password"
- Link to Register and Forgot Password

#### `(auth)/register` — Sign Up
- firstName, lastName, username, email, password, dateOfBirth fields
- Password strength indicator (optional — nice-to-have)
- On success: store token, redirect to `/create/template`
- On 409: show inline "Email already taken" or "Username already taken"
- On 422: per-field errors below each input

#### `(auth)/forgot-password` — Forgot Password
- Email field only (static UI — no backend endpoint yet)
- Shows confirmation message after submit (stub)

### Auth Layout

- Centered card layout, max-width 480px
- Brand logo (`account_tree` Material Symbol) + "StructureFlow" text
- No sidebar — clean and focused

### Auth Guard

**File:** `src/app/(main)/layout.tsx`
- Check `authStore.isAuthenticated` — if false, redirect to `/login`
- On 401 API response anywhere → clear store, redirect to `/login`

---

## 4. Phase 3 — Dashboard & Resume Management

> **UI Mockups:** `loading_dashboard/`, `home_screen/`, `my_resumes/`

### Screens

#### `(main)/dashboard` — Loading Dashboard ← `loading_dashboard/`

This is the **skeleton loading state** shown immediately on route entry while API calls are in flight.
- Full-page skeleton matching the `home_screen/` layout — sidebar, header, and 3 resume-card skeletons
- Uses `SkeletonCard` (animated pulse) in place of every data element
- No spinners — every element has a skeleton placeholder sized to match real content
- Transitions to `home_screen/` content once `GET /api/users/me` and `GET /api/resumes` both resolve

#### `(main)/dashboard` — Home Screen ← `home_screen/`

This is the **loaded state** of the same `/dashboard` route.
- Header: "Welcome back, {firstName}" + subtitle
- Two primary CTA cards: **"Create New Resume"** (`add` icon → `/create/template`) and **"Browse Templates"** (`grid_view` icon → `/create/template`)
- **Recent Resumes** section: last 3 `ResumeSummaryDto` items rendered as `ResumeCard` components
- "View All" link → `/resumes`
- If no resumes exist: `EmptyState` with illustration + "Create Your First Resume" CTA

#### `(main)/resumes` — My Resumes
- Full list from `GET /api/resumes`
- `ResumeCard` for each resume showing: title (from personalInfo), status badge, template name, created date, action menu
- **COMPLETED:** Edit, Pay, Delete
- **PAID:** View / Download (disabled until PDF ready — show tooltip)
- Empty state with CTA to create first resume
- Skeleton loading (3 placeholder cards)

#### `(main)/resumes/[id]` — Resume Detail / Editor
- Full detail from `GET /api/resumes/{id}`
- Two-panel layout: left = editable form, right = preview
- Edit `finalData` fields inline
- Per-section `RegenerateButton` (shows `X/3` remaining count)
- "Save Changes" → `PUT /api/resumes/{id}`
- "Pay & Download" button → navigates to payment flow

### Components

**`ResumeCard`**
- Status badge: `DRAFT` = muted, `COMPLETED` = primary blue, `PAID` = tertiary (amber)
- Three-dot dropdown menu using Shadcn `DropdownMenu`

**`RegenerateButton`**
- `Sparkles` Material Symbol icon
- Shows remaining regenerations (`3 - used`)
- Disabled + tooltip when limit reached

**`ResumeStatusBadge`**
- Color-coded pill using the MD3 palette

**`AppSidebar`**
- Fixed left sidebar matching `step_3_experience_ai/` mockup
- Navigation items: Dashboard, My Resumes, Settings
- Active item has `inset -2px 0 0 0 #5775bf` border (from mockup CSS)
- Bottom: User avatar + name + logout button

---

## 5. Phase 4 — Resume Creation Wizard

> **UI Mockups:** `start_from_scratch/`, `import_from_pdf/`, `resume_templates_gallery/`, `step_1_personal_info/`, `step_2_education/`, `step_3_experience_ai/`, `step_4_courses/`, `step_4_summary_skills/`, `step_5_review_finalize/`

### Wizard Architecture

- All wizard steps live under `(wizard)/create/` route group
- `create/layout.tsx` renders the `WizardStepper` and wraps all steps
- `wizardStore` accumulates form data across steps — no data loss on back navigation
- Final submission happens at Step 5 → `POST /api/resumes`

### Stepper

Progress indicator at top of every wizard step:

```
[1] Personal → [2] Education → [3] Experience → [4] Skills → [5] Review
```

- Active step: filled primary blue circle
- Completed step: checkmark icon
- Future step: outlined circle

### Step Pages

#### `create/template` — Template Gallery ← `resume_templates_gallery/`
- Grid of `TemplateCard` components from `GET /api/templates`
- Each `TemplateCard` shows: preview thumbnail, template name, category badge (Corporate / Creative), price
- Filter row: "All", "Corporate", "Creative" toggle buttons
- Selected card gets a primary-colored border + checkmark overlay
- "Continue with this Template" sticky footer button (disabled until a template is selected)
- Select template → stores `templateId` in wizard store → navigate to `/create/start`

#### `create/start` — Start From Scratch ← `start_from_scratch/`
- Landing page before the wizard steps
- Two large option cards side by side:
  - **"Start from Scratch"** — blank form, `edit_note` icon, primary CTA → navigates to `/create/personal` (Step 1)
  - **"Import from PDF"** — upload card (see below)
- "Back to Templates" link → `/create/template`

#### `create/start` — Import from PDF ← `import_from_pdf/`

This is the **import variant** of the same `/create/start` page — toggled by selecting the "Import from PDF" option card.
- Drag-and-drop file upload zone: `upload_file` icon, "Drag & drop your PDF here" + "Browse files" button
- Accepted formats: `.pdf` only, max 5 MB
- After file selected: shows file name + size + remove button
- **Stub** — no backend upload endpoint exists yet. On "Continue": show a toast "PDF import is coming soon" and remain on this page
- The wizard data is NOT pre-filled from the PDF in the current version

#### `create/personal` — Step 1: Personal Info
**Fields:** First Name, Last Name, Email, Phone, Location, LinkedIn URL, Portfolio URL, Job Title
- Grid: `grid-cols-1 md:grid-cols-2`
- All required except LinkedIn and Portfolio

#### `create/education` — Step 2: Education
**Fields per entry:** Institution, Degree, Field of Study, Start Date, End Date, GPA (optional)
- Add/remove multiple entries
- Dynamic list with `useFieldArray` from React Hook Form

#### `create/experience` — Step 3: Experience (AI)
**Fields per entry:** Job Title, Company, Location, Start Date, End Date, Description (textarea)
- Description textarea has `Sparkles` icon → triggers AI enhancement suggestion (stored, applied on submit)
- At least 1 entry required

#### `create/courses` — Step 4a: Courses & Certifications
**Fields per entry:** Course Name, Provider, Completion Date, Certificate URL
- Optional section — can be skipped

#### `create/skills` — Step 4b: Summary & Skills
- **Summary:** Textarea (Paragraph or Objective toggle)
- **Skills:** Tag-style input — type and press Enter to add, click × to remove
- `targetFormat` selector for Paragraph / Bulleted

#### `create/review` — Step 5: Review & Finalize
- Full summary of all entered data
- Editable inline or back-navigation to any step
- **"Generate My Resume"** button → calls `POST /api/resumes`
- On loading: navigate to `/ai-generating`

### AI Generating Screen

> **UI Mockup:** `ai_generating.._/`

- Full-screen animated loading state
- Pulsing sparkle icon + "AI is crafting your resume…" message
- Auto-redirects to `/resumes/{id}` when API call resolves

---

## 6. Phase 5 — AI Resume Editor

> **UI Mockup:** `ai_generator/`, `step_5_review_finalize/`

### Resume Editor Page (`resumes/[id]`)

#### Layout
- **Left panel (60%):** Editable form sections with `SectionCard` containers
- **Right panel (40%):** Live preview of `finalData` rendering

#### Editable Sections

| Section | Content | Regenerable | sectionIdentifier |
|---------|---------|-------------|-------------------|
| Summary | Textarea | ✓ | `summary` |
| Experience | Entries with description | ✓ each | `exp_001`, `exp_002`… |
| Skills | Tag list | ✓ | `skills` |
| Personal | Info fields | — | — |
| Education | Entries | — | — |

#### Regeneration Flow

1. User clicks `RegenerateButton` on a section
2. `RegenerationModal` opens — shows current content, user types prompt
3. Optional `targetFormat` dropdown + `newTitleSuggestion` field
4. Submit → `POST /api/resumes/{id}/regenerate`
5. Loading spinner in modal
6. On success: update section in local state, show regeneration count update
7. On 429: show "Regeneration limit reached (3/3)" toast

#### `aiAvailable: false` Notice

- Show a subtle banner: "AI is in stub mode — content prefixed with 'AI-Polished:'"

#### Save Changes

- "Save" button → `PUT /api/resumes/{id}` with full `finalData`
- Debounce auto-save (optional)
- Toast on save: "Resume saved"

---

## 7. Phase 6 — Payment Flow

> **UI Mockup:** `step_5_review_finalize/` (Pay section)

### Payment Page (`resumes/[id]/payment`)

#### Price Breakdown Component

Shows before user clicks Pay:
```
Base price:        $3.00
AI regenerations:  $0.75  (3 × $0.25)
──────────────────────────
Total (EGP):       EGP 187.50
```

Currency selector: USD / EGP (others configurable)

#### Checkout Flow

1. Display `PriceBreakdown` with currency selector
2. "Pay Now" → `POST /api/transactions/checkout`
3. Redirect user to `paymentUrl` (in dev: `https://stub.payment/...`)
4. After redirect back: `PaymentStatusPoller` polls `GET /api/transactions/{id}` every 3s
5. On `paymentStatus === "SUCCESS"`:
   - Invalidate resume query
   - Show success toast
   - Show download button (disabled, tooltip: "PDF export coming soon")

#### `PaymentStatusPoller`

```ts
useQuery({
  queryKey: ["transaction", transactionId],
  queryFn: () => getTransaction(transactionId),
  refetchInterval: (data) =>
    data?.paymentStatus === "PENDING" ? 3000 : false,
  enabled: !!transactionId,
});
```

---

## 8. Phase 7 — Static & Info Pages

> **UI Mockups:** `landing_page/`, `about_us/`, `privacy_policy/`, `terms_of_service/`, `support_center/`

### Landing Page (`/`)

- Fixed `TopNavBar`: Logo, Features, Pricing, About links + Sign In + Start Building
- Hero section: headline, CTA buttons, product screenshot
- Bento grid feature cards (Minimalist Design, AI Optimization, etc.)
- Social proof row
- CTA footer section
- Redirects authenticated users to `/dashboard`

### Static Pages

| Route | Content |
|-------|---------|
| `/about` | About Us page (static copy) |
| `/privacy` | Privacy Policy (static copy) |
| `/terms` | Terms of Service (static copy) |
| `/support` | Support Center (contact form — stub) |

---

## 9. Screen → Route Mapping

| UI Mockup Folder | Next.js Route | Status |
|------------------|---------------|--------|
| `landing_page/` | `/` | Phase 7 |
| `sign_in/` | `/login` | Phase 2 |
| `sign_up/` | `/register` | Phase 2 |
| `forgot_password/` | `/forgot-password` | Phase 2 |
| `loading_dashboard/` | `/dashboard` (skeleton loading state) | Phase 3 |
| `home_screen/` | `/dashboard` (loaded state) | Phase 3 |
| `my_resumes/` | `/resumes` | Phase 3 |
| `resume_templates_gallery/` | `/create/template` | Phase 4 |
| `start_from_scratch/` | `/create/start` (default — blank form option) | Phase 4 |
| `import_from_pdf/` | `/create/start` (import variant — PDF upload toggle) | Phase 4 |
| `step_1_personal_info/` | `/create/personal` | Phase 4 |
| `step_2_education/` | `/create/education` | Phase 4 |
| `step_3_experience_ai/` | `/create/experience` | Phase 4 |
| `step_4_courses/` | `/create/courses` | Phase 4 |
| `step_4_summary_skills/` | `/create/skills` | Phase 4 |
| `step_5_review_finalize/` | `/create/review` | Phase 4 |
| `ai_generating.._/` | `/ai-generating` | Phase 4 |
| `ai_generator/` | `/resumes/[id]` | Phase 5 |
| `about_us/` | `/about` | Phase 7 |
| `privacy_policy/` | `/privacy` | Phase 7 |
| `terms_of_service/` | `/terms` | Phase 7 |
| `support_center/` | `/support` | Phase 7 |
| `structureflow/` | design token reference → `AppSidebar`, `(main)/layout.tsx` | Phase 1 |

---

## 10. Shared Components Catalogue

### Layout Components

| Component | Location | Used In |
|-----------|----------|---------|
| `TopNavBar` | `layout/TopNavBar.tsx` | Landing, About, Privacy, Terms, Support |
| `AppSidebar` | `layout/AppSidebar.tsx` | All authenticated pages |
| `WizardStepper` | `layout/WizardStepper.tsx` | All wizard steps |
| `PageHeader` | `layout/PageHeader.tsx` | Dashboard, My Resumes |

### Form Components

| Component | Notes |
|-----------|-------|
| `FormField` | Wraps Shadcn `Input` with `Label` + error. No floating labels |
| `DateRangeField` | Start + End date pair for education/experience |
| `TagInput` | Skills tag entry — type + Enter |
| `RichTextarea` | Textarea with AI sparkle button overlay |

### Resume Components

| Component | Notes |
|-----------|-------|
| `ResumeCard` | Status badge, template name, date, action menu |
| `TemplateCard` | Preview image, name, price, category badge, select button |
| `RegenerateButton` | `Sparkles` icon + `N remaining` label |
| `RegenerationModal` | Dialog: prompt input, format selector, submit |
| `ResumeStatusBadge` | DRAFT=muted, COMPLETED=primary, PAID=tertiary |
| `PriceBreakdown` | Base price + regen costs table |

### Feedback Components

| Component | Notes |
|-----------|-------|
| `SkeletonCard` | Resume card loading placeholder |
| `EmptyState` | Empty list with icon + CTA |
| `MaterialIcon` | Typed wrapper: `<MaterialIcon name="sparkles" />` |

---

## 11. State & Data Layer

### Zustand Stores

#### `authStore`
```ts
interface AuthState {
  token: string | null;
  userId: string | null;
  isAuthenticated: boolean;
  setAuth: (token: string, userId: string) => void;
  clearAuth: () => void;
}
```
Storage: **memory only** (no localStorage — JWT security)

#### `wizardStore`
```ts
interface WizardState {
  currentStep: number;          // 1–5
  templateId: number | null;
  formData: Partial<CreateResumeRequest>;
  jobTitleSuggestions: JobTitleSuggestion[];
  skillSuggestions: string[];
  setStep: (step: number) => void;
  updateFormData: (data: Partial<CreateResumeRequest>) => void;
  setSuggestions: (...) => void;
  reset: () => void;
}
```

### TanStack Query Keys

```ts
export const queryKeys = {
  user: () => ["user"] as const,
  templates: () => ["templates"] as const,
  resumes: () => ["resumes"] as const,
  resume: (id: string) => ["resumes", id] as const,
  transaction: (id: string) => ["transactions", id] as const,
};
```

### Error Handling Pattern

```ts
// Global: intercept 401 → clear auth, redirect to /login
// Per-call: catch ValidationErrorResponse (422) → map to form errors
// Per-call: catch 429 → show "Regeneration limit reached" toast
// Per-call: catch 409 → show specific conflict message
// Default: show generic "Something went wrong" toast
```

---

## 12. Implementation Rules

These rules are derived directly from the Design System and must be followed in every component:

1. **No hardcoded hex colors.** Use only Tailwind utilities that map to CSS variables (`bg-primary`, `text-on-surface`, `border-outline-variant`, etc.)

2. **No floating labels.** Every form input must have an explicit `<Label>` above it — never inside.

3. **Icons: Material Symbols only.** The HTML mockups use `material-symbols-outlined`. Do not mix Lucide icons unless no Material Symbol equivalent exists.

4. **Mobile-first.** All multi-column layouts use `grid-cols-1 md:grid-cols-2`. Wizard steps are single-column on mobile.

5. **Skeleton screens.** Never show a spinner or empty blank while data is loading. Always render `SkeletonCard` or `SkeletonList` placeholders.

6. **Form cards.** All form sections must be inside `SectionCard` (`bg-white rounded-xl shadow-sm border border-outline-variant`).

7. **Wizard state.** Never use local `useState` for wizard form data. Always write to `wizardStore` so the user can navigate back without data loss.

8. **Token storage.** JWT must be stored in Zustand (memory) only. **Never** `localStorage` or `sessionStorage`.

9. **`aiAvailable` flag.** Always check `aiAvailable` on resume responses and show a notice when it is `false`.

10. **Ephemeral suggestions.** `jobTitleSuggestions` and `skillSuggestions` are only in the `POST /api/resumes` response. Save them to `wizardStore` immediately — they will not be in any subsequent GET response.

11. **Naming convention.** Component files are named by function, not appearance: `RegenerationModal.tsx` not `SparklePopup.tsx`.

12. **Tailwind class ordering.** Follow the layout → spacing → typography → color → interactive order (`flex gap-4 p-6 text-sm text-on-surface bg-card hover:bg-surface-container`).
