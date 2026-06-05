"use client";

import { create } from "zustand";
import type { JobTitleSuggestion, ResumeDetailDto, RawData } from "@/types/api.types";
import { updateResume } from "@/lib/api/resumes";

export interface WizardFormData {
    templateId: number | null;
    // Personal
    firstName: string;
    middleName: string;
    lastName: string;
    jobTitle: string;
    email: string;
    phone: string;
    location: string;
    zipCode: string;
    dateOfBirth: string;
    linkedinUrl: string;
    siteUrl: string;
    // Summary
    summary: string;
    // Experience
    experience: Array<{
        id: string;
        title: string;
        company: string;
        location: string;
        startDate: string;
        endDate: string;
        description: string;
    }>;
    // Education
    education: Array<{
        id: string;
        institution: string;
        degree: string;
        fieldOfStudy: string;
        grade: string;
        startDate: string;
        endDate: string;
    }>;
    // Courses
    courses: Array<{
        id: string;
        name: string;
        provider: string;
        date: string;
        certificateUrl: string;
    }>;
    // Skills (grouped by optional category)
    skills: Array<{ category: string; items: string[] }>;
    // Projects
    projects: Array<{
        id: string;
        name: string;
        role: string;
        description: string;
        link: string;
        technologies: string[];
    }>;
    // Languages
    languages: Array<{
        language: string;
        level: string;
    }>;
    // Volunteers
    volunteers: Array<{
        id: string;
        organization: string;
        role: string;
        startDate: string;
        endDate: string;
        description: string;
    }>;
    // Hobbies
    hobbies: string[];
    // Other (custom key-value pairs)
    other: Array<{
        label: string;
        value: string;
    }>;
    // Achievements (bullet-point list)
    achievements: string[];
    // Profile photo (base64 data URL, local only)
    photoUrl: string;
    // Resume display name
    resumeName: string;
    // Metadata
    createdResumeId: string | null;
}

const defaultFormData: WizardFormData = {
    templateId: null,
    firstName: "",
    middleName: "",
    lastName: "",
    jobTitle: "",
    email: "",
    phone: "",
    location: "",
    zipCode: "",
    dateOfBirth: "",
    linkedinUrl: "",
    siteUrl: "",
    summary: "",
    experience: [],
    education: [],
    courses: [],
    skills: [{ category: "", items: [] }],
    projects: [],
    languages: [],
    volunteers: [],
    hobbies: [],
    other: [],
    achievements: [],
    photoUrl: "",
    resumeName: "",
    createdResumeId: null,
};

interface WizardState {
    currentStep: number; // 1–8
    formData: WizardFormData;
    jobTitleSuggestions: JobTitleSuggestion[];
    skillSuggestions: string[];
    /** Steps the user has navigated to (1-based). Used for warning indicators. */
    visitedSteps: number[];
    setStep: (step: number) => void;
    updateFormData: (data: Partial<WizardFormData>) => void;
    setJobTitleSuggestions: (suggestions: JobTitleSuggestion[]) => void;
    setSkillSuggestions: (suggestions: string[]) => void;
    markVisited: (step: number) => void;
    reset: () => void;
    /** Populate the store from an existing resume so the user can re-edit it. */
    initFromResume: (resume: ResumeDetailDto) => void;
    /**
     * Fire-and-forget sync of the current wizard data to the backend.
     * Only runs when a resume has already been created (createdResumeId is set).
     * Silently swallows errors — navigation should never be blocked.
     */
    syncToBackend: () => Promise<void>;
}

export const useWizardStore = create<WizardState>((set, get) => ({
    currentStep: 1,
    formData: defaultFormData,
    jobTitleSuggestions: [],
    skillSuggestions: [],
    visitedSteps: [],

    setStep: (step) => set({ currentStep: step }),

    markVisited: (step) =>
        set((state) => ({
            visitedSteps: state.visitedSteps.includes(step)
                ? state.visitedSteps
                : [...state.visitedSteps, step],
        })),

    updateFormData: (data) =>
        set((state) => ({ formData: { ...state.formData, ...data } })),

    setJobTitleSuggestions: (suggestions) =>
        set({ jobTitleSuggestions: suggestions }),

    setSkillSuggestions: (suggestions) => set({ skillSuggestions: suggestions }),

    reset: () =>
        set({ currentStep: 1, formData: defaultFormData, jobTitleSuggestions: [], skillSuggestions: [], visitedSteps: [] }),

    initFromResume: (resume) => {
        // Prefer finalData (AI-polished) over rawData
        const data = resume.finalData ?? resume.rawData;
        const { personal, summary, experience, education, courses, skills, projects, languages, volunteers, hobbies, other, achievements } = data.content;
        set({
            currentStep: 1,
            formData: {
                templateId: resume.templateId,
                firstName: personal.firstName,
                middleName: personal.middleName ?? "",
                lastName: personal.lastName,
                jobTitle: personal.jobTitle ?? "",
                email: personal.email,
                phone: personal.phone,
                location: personal.location,
                zipCode: personal.zipCode ?? "",
                dateOfBirth: personal.dateOfBirth ?? "",
                linkedinUrl: personal.linkedinUrl ?? "",
                siteUrl: personal.siteUrl ?? "",
                summary,
                experience: experience.map((e) => ({
                    id: e.id,
                    title: e.title,
                    company: e.company,
                    location: e.location ?? "",
                    startDate: e.startDate,
                    endDate: e.endDate ?? "",
                    description: e.description,
                })),
                education: education.map((e) => ({
                    id: e.id,
                    institution: e.institution,
                    degree: e.degree,
                    fieldOfStudy: e.fieldOfStudy,
                    grade: e.grade ?? "",
                    startDate: e.startDate,
                    endDate: e.endDate ?? "",
                })),
                courses: (courses ?? []).map((c) => ({
                    id: c.id,
                    name: c.name,
                    provider: c.provider,
                    date: c.date,
                    certificateUrl: c.certificateUrl ?? "",
                })),
                skills: (() => {
                    const byCategory = new Map<string, string[]>();
                    for (const s of skills ?? []) {
                        const cat = (typeof s === "string" ? "" : (s.category ?? ""));
                        const name = typeof s === "string" ? s : s.name;
                        if (!byCategory.has(cat)) byCategory.set(cat, []);
                        byCategory.get(cat)!.push(name);
                    }
                    return byCategory.size > 0
                        ? Array.from(byCategory.entries()).map(([category, items]) => ({ category, items }))
                        : [{ category: "", items: [] }];
                })(),
                projects: (projects ?? []).map((p) => ({
                    id: p.id,
                    name: p.name,
                    role: p.role ?? "",
                    description: p.description ?? "",
                    link: p.link ?? "",
                    technologies: p.technologies ?? [],
                })),
                languages: (languages ?? []).map((l) => ({
                    language: l.language,
                    level: l.level ?? "",
                })),
                volunteers: (volunteers ?? []).map((v) => ({
                    id: v.id ?? "",
                    organization: v.organization,
                    role: v.role,
                    startDate: v.startDate ?? "",
                    endDate: v.endDate ?? "",
                    description: v.description ?? "",
                })),
                hobbies: hobbies ?? [],
                other: (other ?? []).map((o) => ({ label: o.label, value: o.value })),
                achievements: (achievements ?? []) as string[],
                photoUrl: personal.photoUrl ?? "",
                resumeName: resume.name ?? "",
                createdResumeId: resume.id,
            },
            jobTitleSuggestions: resume.jobTitleSuggestions ?? [],
            skillSuggestions: resume.skillSuggestions ?? [],
        });
    },

    syncToBackend: async () => {
        const { formData } = get();
        if (!formData.createdResumeId) return;
        const finalData: RawData = {
            content: {
                personal: {
                    firstName: formData.firstName,
                    middleName: formData.middleName || null,
                    lastName: formData.lastName,
                    email: formData.email,
                    phone: formData.phone,
                    location: formData.location,
                    zipCode: formData.zipCode || null,
                    dateOfBirth: formData.dateOfBirth || null,
                    linkedinUrl: formData.linkedinUrl || null,
                    siteUrl: formData.siteUrl || null,
                    photoUrl: formData.photoUrl || null,
                },
                summary: formData.summary,
                experience: formData.experience.map((e) => ({
                    id: e.id,
                    title: e.title,
                    company: e.company,
                    location: e.location || null,
                    startDate: e.startDate,
                    endDate: e.endDate || null,
                    description: e.description,
                })),
                education: formData.education.map((e) => ({
                    id: e.id,
                    institution: e.institution,
                    degree: e.degree,
                    fieldOfStudy: e.fieldOfStudy,
                    grade: e.grade || null,
                    startDate: e.startDate,
                    endDate: e.endDate || "",
                })),
                courses: formData.courses.map((c) => ({
                    id: c.id,
                    name: c.name,
                    provider: c.provider,
                    date: c.date,
                    certificateUrl: c.certificateUrl || null,
                })),
                skills: formData.skills.flatMap((group) =>
                    group.items.map((name) => ({ name, category: group.category || null }))
                ),
                projects: formData.projects.length > 0
                    ? formData.projects.map((p) => ({
                        id: p.id,
                        name: p.name,
                        role: p.role || null,
                        description: p.description || null,
                        link: p.link || null,
                        technologies: p.technologies.length > 0 ? p.technologies : null,
                    }))
                    : null,
                languages: formData.languages.length > 0
                    ? formData.languages.map((l) => ({
                        language: l.language,
                        level: (l.level as import('@/types/api.types').LanguageLevel) || null,
                    }))
                    : null,
                volunteers: formData.volunteers.length > 0
                    ? formData.volunteers.map((v) => ({
                        id: v.id || null,
                        organization: v.organization,
                        role: v.role,
                        startDate: v.startDate || null,
                        endDate: v.endDate || null,
                        description: v.description || null,
                    }))
                    : null,
                hobbies: formData.hobbies.length > 0 ? formData.hobbies : null,
                other: formData.other.length > 0
                    ? formData.other.map((o) => ({ label: o.label, value: o.value }))
                    : null,
                achievements: formData.achievements.length > 0 ? formData.achievements : null,
            },
        };
        try {
            await updateResume(formData.createdResumeId, { finalData });
        } catch (err) {
            console.error("[syncToBackend] PUT failed:", err);
        }
    },
}));

// ─── Step completeness helper (used by WizardSidebar) ─────────────────────────

/**
 * Returns true when the step has all its required fields filled.
 * Steps 3 (Courses), 5 (Projects), 7 (Languages), 8 (Extras) are optional — always return true.
 * Step 9 (Review) is unlocked when steps 1, 2, 4, 6 are all complete.
 */
export function checkStepComplete(step: number, formData: WizardFormData): boolean {
    switch (step) {
        case 1:
            return !!(
                formData.firstName.trim() &&
                formData.lastName.trim() &&
                formData.email.trim() &&
                formData.phone.trim() &&
                formData.location.trim()
            );
        case 2:
            return (
                formData.education.length > 0 &&
                formData.education.every(
                    (e) => e.institution.trim() && e.degree.trim() && e.fieldOfStudy.trim() && e.startDate.trim(),
                )
            );
        case 3:
            return true; // Courses — optional step
        case 4:
            return (
                formData.experience.length > 0 &&
                formData.experience.every(
                    (e) => e.title.trim() && e.company.trim() && e.description.trim(),
                )
            );
        case 5:
            return true; // Projects — optional step
        case 6:
            return formData.summary.trim().length >= 10 && formData.skills.some((g) => g.items.length > 0);
        case 7:
            return true; // Languages — optional step
        case 8:
            return true; // Extras (Volunteers, Hobbies, Other) — optional step
        default:
            return true;
    }
}
