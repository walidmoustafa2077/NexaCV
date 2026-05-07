"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useWizardStore } from "@/store/wizardStore";
import { createResume, getResume } from "@/lib/api/resumes";
import { queryKeys } from "@/lib/query/keys";
import { ApiError, ValidationError } from "@/lib/api/client";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type {
    CreateResumeRequest,
    ResumeDetailDto,
    RawData,
    ExperienceEntry,
    EducationEntry,
    CourseEntry,
} from "@/types/api.types";

// Render a description that may be plain text or newline-separated bullet points.
function BulletedText({ text }: { text: string }) {
    const lines = text.split(/\n/).map((l) => l.replace(/^[-•*]\s*/, "").trim()).filter(Boolean);
    if (lines.length <= 1) {
        return <p className="text-sm text-on-surface-variant leading-relaxed">{text}</p>;
    }
    return (
        <ul className="list-disc list-inside space-y-1">
            {lines.map((line, i) => (
                <li key={i} className="text-sm text-on-surface-variant leading-relaxed">{line}</li>
            ))}
        </ul>
    );
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function applyFinalDataToStore(finalData: RawData, updateFormData: (data: any) => void) {
    const { personal, summary, experience, education, courses, skills } = finalData.content;
    updateFormData({
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
            endDate: e.endDate,
        })),
        courses: (courses ?? []).map((c) => ({
            id: c.id,
            name: c.name,
            provider: c.provider,
            date: c.date,
            certificateUrl: c.certificateUrl ?? "",
        })),
        skills,
        summaryType: finalData.settings.summaryType,
        descriptionFormat: finalData.settings.descriptionFormat,
    });
}

// ─── AI Generating animation ──────────────────────────────────────────────────
function AIGenerating() {
    return (
        <div className="flex flex-col items-center justify-center py-20 gap-6">
            <div className="relative w-24 h-24">
                <div className="absolute inset-0 rounded-full border-4 border-primary/20" />
                <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-primary animate-spin" />
                <div className="absolute inset-0 flex items-center justify-center">
                    <MaterialIcon name="auto_awesome" size={32} className="text-primary animate-pulse" filled />
                </div>
            </div>
            <div className="text-center space-y-2">
                <h3 className="font-h2 text-h2 text-on-surface">Building your resume…</h3>
                <p className="font-body-base text-secondary">
                    AI is crafting your professional document. This takes a few seconds.
                </p>
            </div>
            <div className="flex gap-1.5">
                {[0, 0.2, 0.4].map((delay, i) => (
                    <div
                        key={i}
                        className="w-2 h-2 bg-primary rounded-full animate-bounce"
                        style={{ animationDelay: `${delay}s` }}
                    />
                ))}
            </div>
        </div>
    );
}

// ─── Review section (pre-creation) ───────────────────────────────────────────
function ReviewSection({
    title,
    icon,
    children,
}: {
    title: string;
    icon: string;
    children: React.ReactNode;
}) {
    return (
        <div className="bg-white border border-outline-variant/30 rounded-xl p-5 shadow-sm">
            <div className="flex items-center gap-2 mb-4 pb-3 border-b border-outline-variant/20">
                <MaterialIcon name={icon} size={20} className="text-primary" />
                <h3 className="font-h2 text-base text-on-surface font-semibold">{title}</h3>
            </div>
            {children}
        </div>
    );
}



// ─── Post-creation plain section card ────────────────────────────────────────
function PlainSection({
    title,
    icon,
    children,
}: {
    title: string;
    icon: string;
    children: React.ReactNode;
}) {
    return (
        <div className="bg-white border border-outline-variant/30 rounded-xl shadow-sm overflow-hidden">
            <div className="flex items-center gap-2.5 px-6 py-4 border-b border-outline-variant/20">
                <MaterialIcon name={icon} size={20} className="text-primary" />
                <h3 className="font-semibold text-on-surface">{title}</h3>
            </div>
            <div className="px-6 py-5 space-y-3">
                {children}
            </div>
        </div>
    );
}


// ─── Per-entry experience card ────────────────────────────────────────────────
function ExpEntry({ e }: { e: ExperienceEntry }) {
    return (
        <div className="border border-outline-variant/20 rounded-xl p-4 space-y-2">
            <div className="flex items-start justify-between gap-2">
                <div>
                    <p className="font-semibold text-on-surface text-sm">{e.title}</p>
                    <p className="text-secondary text-xs">
                        {e.company}{e.location ? ` • ${e.location}` : ""}
                    </p>
                </div>
                <span className="text-xs text-secondary shrink-0">{e.startDate} – {e.endDate ?? "Present"}</span>
            </div>
            {e.description && <BulletedText text={e.description} />}
        </div>
    );
}

function EduEntry({ e }: { e: EducationEntry }) {
    return (
        <div className="border border-outline-variant/20 rounded-xl p-4">
            <p className="font-semibold text-on-surface text-sm">{e.institution}</p>
            <p className="text-xs text-secondary">
                {e.degree} in {e.fieldOfStudy}{e.grade ? ` • ${e.grade}` : ""}
            </p>
            <p className="text-xs text-secondary">{e.startDate} – {e.endDate ?? "Present"}</p>
        </div>
    );
}

function CourseRow({ c }: { c: CourseEntry }) {
    return (
        <div className="flex items-center justify-between text-sm py-0.5">
            <div>
                <p className="font-medium text-on-surface">{c.name}</p>
                <p className="text-xs text-secondary">
                    {c.provider} • {c.date}
                </p>
            </div>
        </div>
    );
}

// ─── Post-creation view ───────────────────────────────────────────────────────
function PostCreationView({
    resume,
    onBackToEdit,
    onViewResume,
}: {
    resume: ResumeDetailDto;
    onBackToEdit: () => void;
    onViewResume: () => void;
}) {
    // Read directly from the wizard store — always reflects the latest edits
    const { formData } = useWizardStore();

    const fullName = `${formData.firstName} ${formData.lastName}`.trim() || "Your Name";
    const location = formData.location || "";
    const email = formData.email || "";

    const experience = formData.experience.map((e) => ({
        id: e.id,
        title: e.title,
        company: e.company,
        location: e.location || null,
        startDate: e.startDate,
        endDate: e.endDate || null,
        description: e.description,
    })) as ExperienceEntry[];

    const education = formData.education.map((e) => ({
        id: e.id,
        institution: e.institution,
        degree: e.degree,
        fieldOfStudy: e.fieldOfStudy,
        grade: e.grade || null,
        startDate: e.startDate,
        endDate: e.endDate,
    })) as EducationEntry[];

    const skills = formData.skills;
    const summary = formData.summary;

    return (
        <div className="space-y-6">
            {/* Banner */}
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/30 border border-primary/20 rounded-xl px-6 py-4 flex items-center gap-4">
                <div className="w-10 h-10 rounded-xl bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={22} className="text-on-primary" filled />
                </div>
                <div className="flex-1">
                    <p className="font-bold text-on-surface">Resume created &amp; enhanced by AI!</p>
                    <p className="text-xs text-secondary">
                        Go back to any wizard step to edit your content, then return here to finalize.
                    </p>
                </div>
            </div>

            <div className="flex flex-col lg:flex-row gap-8">
                {/* Left: paper-style preview */}
                <div className="flex-1 space-y-3">
                    <h2 className="font-h2 text-h2 text-on-surface">Resume Preview</h2>
                    <div className="relative bg-white border border-outline-variant rounded-xl overflow-hidden shadow-[0_10px_25px_-5px_rgba(0,0,0,0.08)] aspect-[1/1.414] p-10 select-none">
                        {/* Resume content */}
                        <div className="space-y-6 h-full overflow-hidden">
                            {/* Header */}
                            <div className="border-b-4 border-primary pb-5">
                                <h3 className="text-xl font-bold text-slate-900 font-h1 uppercase tracking-wide">
                                    {fullName}
                                </h3>
                                <p className="text-slate-600 font-body-sm mt-1 text-sm">
                                    {[location, email].filter(Boolean).join(" | ")}
                                </p>
                            </div>

                            {/* Summary */}
                            {summary && (
                                <div className="space-y-2">
                                    <h4 className="text-[10px] font-bold text-primary uppercase tracking-widest">
                                        Professional Summary
                                    </h4>
                                    <p className="text-slate-700 text-xs leading-relaxed line-clamp-3">{summary}</p>
                                </div>
                            )}

                            {/* Experience */}
                            {experience.length > 0 && (
                                <div className="space-y-2">
                                    <h4 className="text-[10px] font-bold text-primary uppercase tracking-widest">
                                        Work Experience
                                    </h4>
                                    {experience.slice(0, 2).map((e) => (
                                        <div key={e.id}>
                                            <div className="flex justify-between font-bold text-slate-900 text-xs">
                                                <span>{e.title}, {e.company}</span>
                                                <span className="shrink-0 ml-2">{e.startDate} – {e.endDate ?? "Present"}</span>
                                            </div>
                                            {e.location && (
                                                <p className="text-slate-500 text-[10px] italic">{e.location}</p>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            )}

                            {/* Education */}
                            {education.length > 0 && (
                                <div className="space-y-2">
                                    <h4 className="text-[10px] font-bold text-primary uppercase tracking-widest">
                                        Education
                                    </h4>
                                    {education.slice(0, 2).map((e) => (
                                        <div key={e.id} className="flex justify-between text-xs font-bold text-slate-900">
                                            <span>{e.degree} in {e.fieldOfStudy}, {e.institution}</span>
                                            <span className="shrink-0 ml-2">{e.startDate} – {e.endDate ?? "Present"}</span>
                                        </div>
                                    ))}
                                </div>
                            )}

                            {/* Skills */}
                            {skills.length > 0 && (
                                <div className="space-y-1.5">
                                    <h4 className="text-[10px] font-bold text-primary uppercase tracking-widest">
                                        Skills
                                    </h4>
                                    <p className="text-xs text-slate-700">{skills.slice(0, 8).join(" • ")}</p>
                                </div>
                            )}
                        </div>

                        {/* Watermark */}
                        <div
                            className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 rotate-[-45deg] text-5xl font-black text-slate-900 opacity-[0.04] whitespace-nowrap pointer-events-none select-none"
                        >
                            PREVIEW ONLY • PREVIEW ONLY
                        </div>

                        {/* Frosted overlay */}
                        <div className="absolute inset-0 bg-white/35 backdrop-blur-[1px]" />

                        {/* Lock badge */}
                        <div className="absolute bottom-8 left-1/2 -translate-x-1/2">
                            <button
                                onClick={onViewResume}
                                className="bg-slate-900/90 text-white px-5 py-2.5 rounded-full flex items-center gap-2.5 backdrop-blur-md hover:bg-slate-900 transition-colors shadow-lg"
                            >
                                <MaterialIcon name="open_in_new" size={16} className="text-white" />
                                <span className="text-xs font-semibold uppercase tracking-wider">View full resume</span>
                            </button>
                        </div>
                    </div>
                </div>

                {/* Right: action panel */}
                <div className="w-full lg:w-[300px] space-y-4 lg:sticky lg:top-8 self-start">
                    <div className="bg-white border border-outline-variant/30 rounded-xl p-5 space-y-3 shadow-sm">
                        <h4 className="font-bold text-on-surface">What&apos;s next?</h4>
                        <p className="text-xs text-secondary leading-relaxed">
                            Go back to any wizard step to edit your content, then return here to finalize.
                        </p>
                        <button
                            onClick={onBackToEdit}
                            className="w-full flex items-center justify-center gap-2 py-2.5 border-2 border-primary text-primary rounded-xl font-bold text-sm hover:bg-primary-fixed/20 transition-colors"
                        >
                            <MaterialIcon name="arrow_back" size={18} />
                            Back to Step 1
                        </button>
                        <button
                            onClick={onViewResume}
                            className="w-full flex items-center justify-center gap-2 py-2.5 bg-primary text-on-primary rounded-xl font-bold text-sm shadow-lg shadow-primary/20 hover:opacity-90 transition-opacity"
                        >
                            <MaterialIcon name="open_in_new" size={18} />
                            View Resume
                        </button>
                    </div>
                    <div className="bg-surface-container-low border border-outline-variant/30 rounded-xl p-4 text-xs text-secondary space-y-1.5">
                        <p className="font-bold text-on-surface text-xs uppercase tracking-wide">Resume Info</p>
                        <div className="flex justify-between">
                            <span>Template</span>
                            <span className="text-on-surface font-medium">{resume.templateName}</span>
                        </div>
                        <div className="flex justify-between">
                            <span>AI mode</span>
                            <span className={resume.aiAvailable ? "text-primary font-bold" : "text-secondary"}>
                                {resume.aiAvailable ? "Active" : "Stub"}
                            </span>
                        </div>
                        <div className="flex justify-between">
                            <span>AI uses</span>
                            <span className="text-on-surface font-medium">3 per section</span>
                        </div>
                    </div>

                    {/* Trust badge */}
                    <div className="bg-white border border-outline-variant rounded-xl p-4 flex items-center gap-3">
                        <div className="flex text-amber-400">
                            {Array.from({ length: 5 }).map((_, i) => (
                                <MaterialIcon key={i} name="star" size={14} filled className="text-amber-400" />
                            ))}
                        </div>
                        <p className="text-[10px] font-bold text-slate-900 uppercase tracking-tight">
                            Trusted by 12,000+ professionals
                        </p>
                    </div>
                </div>
            </div>
        </div>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export default function Step6Page() {
    const router = useRouter();
    const queryClient = useQueryClient();
    const { formData, updateFormData, setJobTitleSuggestions, setSkillSuggestions, reset, syncToBackend } =
        useWizardStore();

    const [refreshedResume, setRefreshedResume] = useState<ResumeDetailDto | null>(null);
    const [isLoadingResume, setIsLoadingResume] = useState(false);

    const displayResume: ResumeDetailDto | null = refreshedResume;

    const { mutate, isPending } = useMutation({
        mutationFn: (req: CreateResumeRequest) => createResume(req),
        onSuccess: (data) => {
            setJobTitleSuggestions(data.jobTitleSuggestions);
            setSkillSuggestions(data.skillSuggestions);
            applyFinalDataToStore(data.finalData, updateFormData);
            updateFormData({ createdResumeId: data.id });
            toast.success("AI Enhancement Complete", {
                description: "Your resume has been polished. Review the sections below.",
            });
            router.push("/create/steps/1");
        },
        onError: (err: Error) => {
            let msg: string;
            if (err instanceof ValidationError && err.details.length > 0) {
                msg = err.details.map((d) => d.message).join(" ");
            } else if (err instanceof ApiError) {
                if (err.status === 0) msg = "Backend is not reachable. Make sure all services are running.";
                else msg = err.message || "Failed to process. Please try again.";
            } else {
                msg = err.message || "Failed to process. Please try again.";
            }
            toast.error(msg);
        },
    });

    // Sync wizard edits to backend then refresh resume when returning to this page
    useEffect(() => {
        if (formData.createdResumeId && !refreshedResume) {
            setIsLoadingResume(true);
            syncToBackend()
                .then(() => getResume(formData.createdResumeId!))
                .then(setRefreshedResume)
                .catch(() => updateFormData({ createdResumeId: null }))
                .finally(() => setIsLoadingResume(false));
        }
    }, [formData.createdResumeId, refreshedResume, updateFormData, syncToBackend]);

    function buildRequest(): CreateResumeRequest | null {
        if (!formData.templateId) return null;
        return {
            templateId: formData.templateId,
            rawData: {
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
                        endDate: e.endDate,
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
            },
        };
    }

    function handleCreate() {
        const req = buildRequest();
        if (!req) {
            toast.error("Please select a template first.");
            router.push("/create/template");
            return;
        }
        mutate(req);
    }

    function handleBackToEdit() {
        // Store already updated with finalData by onSuccess — navigate to step 1
        router.push("/create/steps/1");
    }

    function handleViewResume() {
        if (!formData.createdResumeId) return;
        const id = formData.createdResumeId;
        queryClient.invalidateQueries({ queryKey: queryKeys.resume(id) });
        reset();
        router.push(`/resumes/${id}`);
    }

    const fullName = `${formData.firstName} ${formData.lastName}`.trim() || "Your Name";

    return (
        <div className="px-8 py-10 max-w-[1024px] mx-auto">
            <WizardProgress
                step={6}
                total={6}
                title="Review & Finalize"
                subtitle={
                    displayResume
                        ? "Your resume is ready. Go back to any step to make edits."
                        : "Review your information, then create your resume."
                }
            />

            {isPending ? (
                <AIGenerating />
            ) : isLoadingResume ? (
                <div className="flex flex-col items-center justify-center py-20 gap-4">
                    <div className="relative w-16 h-16">
                        <div className="absolute inset-0 rounded-full border-4 border-primary/20" />
                        <div className="absolute inset-0 rounded-full border-4 border-transparent border-t-primary animate-spin" />
                    </div>
                    <p className="text-secondary font-body-base">Loading your AI-enhanced resume…</p>
                </div>
            ) : displayResume ? (
                <PostCreationView
                    resume={displayResume}
                    onBackToEdit={handleBackToEdit}
                    onViewResume={handleViewResume}
                />
            ) : (
                /* ── Pre-creation review ── */
                <div className="flex flex-col lg:flex-row gap-8">
                    <div className="flex-1 space-y-5">
                        <ReviewSection title="Personal Information" icon="person">
                            <div className="grid grid-cols-2 gap-2 text-sm">
                                <span className="text-secondary">Name</span>
                                <span className="text-on-surface font-medium">{fullName}</span>
                                <span className="text-secondary">Email</span>
                                <span className="text-on-surface font-medium">{formData.email || "—"}</span>
                                <span className="text-secondary">Phone</span>
                                <span className="text-on-surface font-medium">{formData.phone || "—"}</span>
                                <span className="text-secondary">Location</span>
                                <span className="text-on-surface font-medium">{formData.location || "—"}</span>
                            </div>
                        </ReviewSection>

                        <ReviewSection title={`Education (${formData.education.length})`} icon="school">
                            {formData.education.length === 0 ? (
                                <p className="text-sm text-secondary">No entries added.</p>
                            ) : (
                                <ul className="space-y-2">
                                    {formData.education.map((e) => (
                                        <li key={e.id} className="text-sm">
                                            <span className="font-medium text-on-surface">{e.institution}</span>
                                            <span className="text-secondary ml-2">
                                                — {e.degree}, {e.fieldOfStudy}
                                            </span>
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </ReviewSection>

                        <ReviewSection title={`Experience (${formData.experience.length})`} icon="work">
                            {formData.experience.length === 0 ? (
                                <p className="text-sm text-secondary">No entries added.</p>
                            ) : (
                                <ul className="space-y-2">
                                    {formData.experience.map((e) => (
                                        <li key={e.id} className="text-sm">
                                            <span className="font-medium text-on-surface">{e.title}</span>
                                            <span className="text-secondary ml-2">@ {e.company}</span>
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </ReviewSection>

                        <ReviewSection title={`Skills (${formData.skills.length})`} icon="bolt">
                            {formData.skills.length === 0 ? (
                                <p className="text-sm text-secondary">No skills added.</p>
                            ) : (
                                <div className="flex flex-wrap gap-2">
                                    {formData.skills.map((s) => (
                                        <span
                                            key={s}
                                            className="px-2.5 py-1 bg-primary-fixed/30 text-on-surface rounded-lg text-xs font-medium"
                                        >
                                            {s}
                                        </span>
                                    ))}
                                </div>
                            )}
                        </ReviewSection>

                        <ReviewSection title={`Courses (${formData.courses.length})`} icon="menu_book">
                            {formData.courses.length === 0 ? (
                                <p className="text-sm text-secondary">No courses added.</p>
                            ) : (
                                <ul className="space-y-2">
                                    {formData.courses.map((c) => (
                                        <li key={c.id} className="text-sm">
                                            <span className="font-medium text-on-surface">{c.name}</span>
                                            <span className="text-secondary ml-2">@ {c.provider} ({c.date})</span>
                                        </li>
                                    ))}
                                </ul>
                            )}
                        </ReviewSection>
                    </div>

                    {/* Right: order summary */}
                    <div className="w-full lg:w-[320px] space-y-5">
                        <div className="bg-surface-container-low border border-outline-variant rounded-xl p-6 space-y-5">
                            <h3 className="font-h2 text-lg text-on-surface">Order Summary</h3>
                            <div className="space-y-2 text-sm text-secondary">
                                <div className="flex justify-between">
                                    <span>Resume creation</span>
                                    <span className="text-on-surface font-medium">Free</span>
                                </div>
                                <div className="flex justify-between">
                                    <span>PDF download</span>
                                    <span className="text-on-surface font-medium">Paid</span>
                                </div>
                            </div>
                            <ul className="space-y-2 text-xs text-secondary">
                                {["AI-enhanced content", "ATS-optimized format", "Unlimited edits"].map((f) => (
                                    <li key={f} className="flex items-center gap-2">
                                        <MaterialIcon name="check" size={14} className="text-primary shrink-0" />
                                        {f}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            )}

            {/* Navigation bar — only in pre-creation state */}
            {!isPending && !displayResume && (
                <div className="mt-8 flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/5")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg text-secondary font-semibold hover:text-on-surface transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back to Step 5
                    </button>
                    <button
                        onClick={handleCreate}
                        className="flex items-center gap-2 px-8 py-3 rounded-xl bg-primary text-on-primary font-bold shadow-lg shadow-primary/20 hover:opacity-90 active:scale-[0.98] transition-all"
                    >
                        <MaterialIcon name="auto_awesome" size={20} />
                        Polish with AI & Review
                    </button>
                </div>
            )}
        </div>
    );
}
