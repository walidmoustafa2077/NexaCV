import { z } from "zod";

const personalInfoSchema = z.object({
    firstName: z.string().min(1, "First name is required"),
    middleName: z.string().optional(),
    lastName: z.string().min(1, "Last name is required"),
    email: z.string().min(1, "Email is required").email("Enter a valid email address"),
    phone: z.string().min(1, "Phone is required"),
    location: z.string().min(1, "Location is required"),
    zipCode: z.string().optional(),
    dateOfBirth: z.string().optional(),
    linkedinUrl: z.string().optional(),
    siteUrl: z.string().optional(),
});

const experienceEntrySchema = z.object({
    id: z.string(),
    title: z.string().min(1, "Job title is required"),
    company: z.string().min(1, "Company is required"),
    location: z.string().optional(),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().nullable().optional(),
    description: z.string().min(1, "Description is required"),
});

const educationEntrySchema = z.object({
    id: z.string(),
    institution: z.string().min(1, "Institution is required"),
    degree: z.string().min(1, "Degree is required"),
    fieldOfStudy: z.string().min(1, "Field of study is required"),
    grade: z.string().optional(),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().min(1, "End date is required"),
});

const courseEntrySchema = z.object({
    id: z.string(),
    name: z.string().min(1, "Course name is required"),
    provider: z.string().min(1, "Provider is required"),
    date: z.string().min(1, "Date is required"),
    certificateUrl: z.string().optional(),
});

export const createResumeSchema = z.object({
    templateId: z.number().min(1, "Please select a template"),
    rawData: z.object({
        content: z.object({
            personal: personalInfoSchema,
            summary: z.string().min(1, "Summary is required"),
            experience: z.array(experienceEntrySchema).min(1, "At least one experience entry is required"),
            education: z.array(educationEntrySchema),
            courses: z.array(courseEntrySchema).optional(),
            skills: z.array(z.string()).min(1, "At least one skill is required"),
        }),
    }),
});

export const regenerateSchema = z.object({
    sectionIdentifier: z.string().min(1, "Section is required"),
    userPrompt: z.string().min(1, "Please enter a prompt"),
    targetFormat: z.string().optional(),
    newTitleSuggestion: z.string().optional(),
});

export type CreateResumeFormValues = z.infer<typeof createResumeSchema>;
export type RegenerateFormValues = z.infer<typeof regenerateSchema>;
