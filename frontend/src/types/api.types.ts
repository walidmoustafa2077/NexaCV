import type {
    ResumeStatus,
    PaymentStatus,
    Currency,
} from "./enums";

// ─── Auth ──────────────────────────────────────────────────────────────────

export interface RegisterRequest {
    firstName: string;
    lastName: string;
    username: string;
    email: string;
    password: string;
    dateOfBirth?: string; // YYYY-MM-DD
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface AuthResponse {
    userId: string;
    token: string;
    expiresIn: number;
}

// ─── Users ─────────────────────────────────────────────────────────────────

export interface UserProfileDto {
    id: string;
    firstName: string;
    lastName: string;
    username: string;
    email: string;
    createdAt: string;
    lastLogin: string | null;
}

export interface UpdateUserRequest {
    firstName?: string | null;
    lastName?: string | null;
    username?: string | null;
    password?: string | null;
}

// ─── Templates ─────────────────────────────────────────────────────────────

export interface TemplateDto {
    id: number;
    name: string;
    industryCategory: string;
    /** Design style category returned by the backend: "Executive" | "Creative" | "ModernTech" | "Minimalist" */
    styleCategory: string | null;
    basePriceUsd: number;
    supportsWord: boolean;
    /**
     * Raw HTML of the template with {{Placeholder}} tokens.
     * Substitute client-side with mock or real data and render via srcdoc.
     */
    htmlContent: string | null;
}

// ─── Resume Raw Data ────────────────────────────────────────────────────────

export interface PersonalInfo {
    firstName: string;
    middleName?: string | null;
    lastName: string;
    /** Current or desired job title rendered under the candidate's name. */
    jobTitle?: string | null;
    email: string;
    phone: string;
    location: string;
    zipCode?: string | null;
    dateOfBirth?: string | null;
    linkedinUrl?: string | null;
    siteUrl?: string | null;
    /** URL or base-64 data URI for the candidate's photo (used by photo-enabled templates). */
    photoUrl?: string | null;
}

export interface ExperienceEntry {
    id: string; // e.g. "exp_001"
    title: string;
    company: string;
    location?: string | null;
    startDate: string; // YYYY-MM
    endDate: string | null; // YYYY-MM or null (present)
    description: string;
}

export interface EducationEntry {
    id: string; // e.g. "edu_001"
    institution: string;
    degree: string;
    fieldOfStudy: string;
    grade?: string | null;
    startDate: string; // YYYY-MM
    endDate: string; // YYYY-MM
}

export interface CourseEntry {
    id: string; // e.g. "crs_001"
    name: string;
    provider: string;
    date: string; // YYYY-MM
    certificateUrl?: string | null;
}

export type SkillType = "Technical" | "Soft";

export interface SkillEntry {
    name: string;
    type?: SkillType | null;
    category?: string | null;
}

export interface RawData {
    content: ResumeContent;
}

export type LanguageLevel = "Native" | "Fluent" | "Professional" | "Basic";

export interface LanguageEntry {
    language: string;
    level?: LanguageLevel | null;
}

export interface ProjectEntry {
    id: string;
    name: string;
    role?: string | null;
    description?: string | null;
    link?: string | null;
    technologies?: string[] | null;
}

export interface OtherEntry {
    label: string;
    value: string;
}

export interface VolunteerEntry {
    id?: string | null;
    organization: string;
    role: string;
    startDate?: string | null;
    endDate?: string | null;
    description?: string | null;
}

export interface ResumeContent {
    personal: PersonalInfo;
    summary: string;
    experience: ExperienceEntry[];
    education: EducationEntry[];
    courses?: CourseEntry[];
    projects?: ProjectEntry[] | null;
    /** Skills list. Each entry has at minimum a `name`; `type` and `category` are optional AI enrichments. */
    skills: SkillEntry[];
    languages?: LanguageEntry[] | null;
    volunteers?: VolunteerEntry[] | null;
    hobbies?: string[] | null;
    other?: OtherEntry[] | null;
    achievements?: string[] | null;
}

// ─── Resume Requests ────────────────────────────────────────────────────────

export interface CreateResumeRequest {
    templateId: number;
    rawData: RawData;
}

export interface UpdateFinalDataRequest {
    finalData: RawData;
}

export interface RegenerateRequest {
    sectionIdentifier: string;
    userPrompt: string;
    targetFormat?: string | null;
    newTitleSuggestion?: string | null;
}

// ─── Resume Responses ───────────────────────────────────────────────────────

export interface JobTitleSuggestion {
    title: string;
    score: number;
}

export interface ResumeSummaryDto {
    id: string;
    status: ResumeStatus;
    templateName: string;
    createdAt: string;
    updatedAt: string;
    name?: string | null;
    downloadCount: number;
}

export interface ResumeDetailDto {
    id: string;
    status: ResumeStatus;
    templateId: number;
    templateName: string;
    rawData: RawData;
    finalData: RawData;
    aiAvailable: boolean;
    createdAt: string;
    updatedAt: string;
    name?: string | null;
    jobTitleSuggestions?: JobTitleSuggestion[] | null;
    skillSuggestions?: string[] | null;
}

export interface CreateResumeResponse extends ResumeDetailDto {
    jobTitleSuggestions: JobTitleSuggestion[];
    skillSuggestions: string[];
}

export interface RegenerateResponse {
    sectionIdentifier: string;
    updatedContent: unknown;
    regenCountUsed: number;
    regenCountRemaining: number;
    addedCostUsd: number;
    aiAvailable: boolean;
}

// ─── Transactions ───────────────────────────────────────────────────────────

export interface CheckoutRequest {
    resumeId: string;
    currency: Currency;
}

export interface CheckoutResponse {
    transactionId: string;
    paymentUrl: string;
    baseAmount: number;
    regenAmount: number;
    totalAmount: number;
    currency: Currency;
    exchangeRateUsed: number;
}

export interface TransactionDto {
    id: string;
    resumeId: string;
    totalAmount: number;
    currency: Currency;
    paymentStatus: PaymentStatus;
    paymentUrl: string;
    createdAt: string;
    updatedAt: string;
}

// ─── Errors ─────────────────────────────────────────────────────────────────

export interface ApiErrorResponse {
    status: number;
    error: string;
}

export interface ValidationDetail {
    field: string;
    message: string;
}

export interface ValidationErrorResponse extends ApiErrorResponse {
    details: ValidationDetail[];
}
