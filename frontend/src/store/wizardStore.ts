"use client";

import { create } from "zustand";
import type { JobTitleSuggestion, ResumeDetailDto, RawData } from "@/types/api.types";
import { updateResume } from "@/lib/api/resumes";

export interface WizardFormData {
    templateId: number | null;
    summaryType: "Summary" | "Objective";
    descriptionFormat: "Paragraph" | "Bulleted";
    // Personal
    firstName: string;
    middleName: string;
    lastName: string;
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
    // Skills
    skills: string[];
    // Profile photo (base64 data URL, local only)
    photoUrl: string;
    // Resume display name
    resumeName: string;
    // Metadata
    createdResumeId: string | null;
}

const defaultFormData: WizardFormData = {
    templateId: null,
    summaryType: "Summary",
    descriptionFormat: "Bulleted",
    firstName: "",
    middleName: "",
    lastName: "",
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
    skills: [],
    photoUrl: "",
    resumeName: "",
    createdResumeId: null,
};

interface WizardState {
    currentStep: number; // 1–6
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
        const { personal, summary, experience, education, courses, skills } = data.content;
        set({
            currentStep: 1,
            formData: {
                templateId: resume.templateId,
                summaryType: data.settings.summaryType,
                descriptionFormat: data.settings.descriptionFormat,
                firstName: personal.firstName,
                middleName: personal.middleName ?? "",
                lastName: personal.lastName,
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
                skills,
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
            settings: {
                summaryType: formData.summaryType,
                descriptionFormat: formData.descriptionFormat,
            },
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
                skills: formData.skills,
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
 * Step 3 (Courses) is always optional — always returns true.
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
            return true; // Optional step
        case 4:
            return (
                formData.experience.length > 0 &&
                formData.experience.every(
                    (e) => e.title.trim() && e.company.trim() && e.description.trim(),
                )
            );
        case 5:
            return formData.summary.trim().length >= 10 && formData.skills.length >= 3;
        default:
            return true;
    }
}
