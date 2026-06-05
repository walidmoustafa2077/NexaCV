"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useWizardStore } from "@/store/wizardStore";
import { createResume, getResume } from "@/lib/api/resumes";
import { queryKeys } from "@/lib/query/keys";
import { ApiError, ValidationError } from "@/lib/api/client";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { ResumeHtmlPreview } from "@/components/resume/ResumeHtmlPreview";
import type {
    CreateResumeRequest,
    ResumeDetailDto,
    RawData,
    ExperienceEntry,
    EducationEntry,
    CourseEntry,
    LanguageLevel,
    VolunteerEntry,
} from "@/types/api.types";

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function applyFinalDataToStore(finalData: RawData, updateFormData: (data: any) => void) {
    const { personal, summary, experience, education, courses, skills, projects, languages, volunteers, hobbies, other } = finalData.content;
    updateFormData({
        firstName: personal.firstName, middleName: personal.middleName ?? "", lastName: personal.lastName,
        email: personal.email, phone: personal.phone, location: personal.location,
        zipCode: personal.zipCode ?? "", dateOfBirth: personal.dateOfBirth ?? "",
        linkedinUrl: personal.linkedinUrl ?? "", siteUrl: personal.siteUrl ?? "", photoUrl: personal.photoUrl ?? "",
        summary,
        experience: experience.map((e: ExperienceEntry) => ({ id: e.id, title: e.title, company: e.company, location: e.location ?? "", startDate: e.startDate, endDate: e.endDate ?? "", description: e.description })),
        education: education.map((e: EducationEntry) => ({ id: e.id, institution: e.institution, degree: e.degree, fieldOfStudy: e.fieldOfStudy, grade: e.grade ?? "", startDate: e.startDate, endDate: e.endDate })),
        courses: (courses ?? []).map((c: CourseEntry) => ({ id: c.id, name: c.name, provider: c.provider, date: c.date, certificateUrl: c.certificateUrl ?? "" })),
        skills: (() => {
            const byCategory = new Map<string, string[]>();
            for (const s of skills ?? []) {
                const cat = typeof s === "string" ? "" : (s.category ?? "");
                const name = typeof s === "string" ? s : s.name;
                if (!byCategory.has(cat)) byCategory.set(cat, []);
                byCategory.get(cat)!.push(name);
            }
            return byCategory.size > 0
                ? Array.from(byCategory.entries()).map(([category, items]) => ({ category, items }))
                : [{ category: "", items: [] }];
        })(),
        projects: (projects ?? []).map((p) => ({ id: p.id, name: p.name, role: p.role ?? "", description: p.description ?? "", link: p.link ?? "", technologies: p.technologies ?? [] })),
        languages: (languages ?? []).map((l) => ({ language: l.language, level: l.level ?? "" })),
        volunteers: (volunteers ?? []).map((v: VolunteerEntry) => ({ id: v.id ?? "", organization: v.organization, role: v.role, startDate: v.startDate ?? "", endDate: v.endDate ?? "", description: v.description ?? "" })),
        hobbies: hobbies ?? [],
        other: (other ?? []).map((o) => ({ label: o.label, value: o.value })),
    });
}

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
                <p className="font-body-base text-secondary">AI is crafting your professional document. This takes a few seconds.</p>
            </div>
            <div className="flex gap-1.5">
                {[0, 0.2, 0.4].map((delay, i) => (
                    <div key={i} className="w-2 h-2 bg-primary rounded-full animate-bounce" style={{ animationDelay: `${delay}s` }} />
                ))}
            </div>
        </div>
    );
}

function ReviewSection({ title, icon, children }: { title: string; icon: string; children: React.ReactNode }) {
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

function PostCreationView({ resume, onBackToEdit, onViewResume }: {
    resume: ResumeDetailDto;
    onBackToEdit: () => void;
    onViewResume: () => void;
}) {
    return (
        <div className="space-y-6">
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/30 border border-primary/20 rounded-xl px-6 py-4 flex items-center gap-4">
                <div className="w-10 h-10 rounded-xl bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={22} className="text-on-primary" filled />
                </div>
                <div className="flex-1">
                    <p className="font-bold text-on-surface">Resume created &amp; enhanced by AI!</p>
                    <p className="text-xs text-secondary">Go back to any wizard step to edit your content, then return here to finalize.</p>
                </div>
            </div>

            <div className="flex flex-col lg:flex-row gap-8">
                <div className="flex-1 space-y-3">
                    <h2 className="font-h2 text-h2 text-on-surface">Resume Preview</h2>
                    <div className="relative overflow-hidden rounded-xl shadow-[0_10px_25px_-5px_rgba(0,0,0,0.08)]">
                        <ResumeHtmlPreview resumeId={resume.id} />
                        <div className="absolute inset-0 flex items-center justify-center pointer-events-none select-none overflow-hidden">
                            <span className="rotate-[-45deg] text-5xl font-black text-black/[0.04] whitespace-nowrap">PREVIEW ONLY &bull; PREVIEW ONLY</span>
                        </div>
                        <div className="absolute inset-0 bg-white/40 backdrop-blur-[2px] pointer-events-none" />
                        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 z-10">
                            <button onClick={onViewResume} className="bg-slate-900/90 text-white px-5 py-2.5 rounded-full flex items-center gap-2.5 backdrop-blur-md hover:bg-slate-900 transition-colors shadow-lg">
                                <MaterialIcon name="open_in_new" size={16} className="text-white" />
                                <span className="text-xs font-semibold uppercase tracking-wider">View full resume</span>
                            </button>
                        </div>
                    </div>
                </div>

                <div className="w-full lg:w-[300px] space-y-4 lg:sticky lg:top-8 self-start">
                    <div className="bg-white border border-outline-variant/30 rounded-xl p-5 space-y-3 shadow-sm">
                        <h4 className="font-bold text-on-surface">What&apos;s next?</h4>
                        <p className="text-xs text-secondary leading-relaxed">Go back to any wizard step to edit your content, then return here to finalize.</p>
                        <button onClick={onBackToEdit} className="w-full flex items-center justify-center gap-2 py-2.5 border-2 border-primary text-primary rounded-xl font-bold text-sm hover:bg-primary-fixed/20 transition-colors">
                            <MaterialIcon name="arrow_back" size={18} />Back to Step 1
                        </button>
                        <button onClick={onViewResume} className="w-full flex items-center justify-center gap-2 py-2.5 bg-primary text-on-primary rounded-xl font-bold text-sm shadow-lg shadow-primary/20 hover:opacity-90 transition-opacity">
                            <MaterialIcon name="open_in_new" size={18} />View Resume
                        </button>
                    </div>
                    <div className="bg-surface-container-low border border-outline-variant/30 rounded-xl p-4 text-xs text-secondary space-y-1.5">
                        <p className="font-bold text-on-surface text-xs uppercase tracking-wide">Resume Info</p>
                        <div className="flex justify-between"><span>Template</span><span className="text-on-surface font-medium">{resume.templateName}</span></div>
                        <div className="flex justify-between"><span>AI mode</span><span className={resume.aiAvailable ? "text-primary font-bold" : "text-secondary"}>{resume.aiAvailable ? "Active" : "Stub"}</span></div>
                        <div className="flex justify-between"><span>AI uses</span><span className="text-on-surface font-medium">3 per section</span></div>
                    </div>
                    <div className="bg-white border border-outline-variant rounded-xl p-4 flex items-center gap-3">
                        <div className="flex text-amber-400">{Array.from({ length: 5 }).map((_, i) => (<MaterialIcon key={i} name="star" size={14} filled className="text-amber-400" />))}</div>
                        <p className="text-[10px] font-bold text-slate-900 uppercase tracking-tight">Trusted by 12,000+ professionals</p>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default function Step9Page() {
    const router = useRouter();
    const queryClient = useQueryClient();
    const { formData, updateFormData, setJobTitleSuggestions, setSkillSuggestions, reset, syncToBackend } = useWizardStore();

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
            toast.success("AI Enhancement Complete", { description: "Your resume has been polished. Review the sections below." });
            setRefreshedResume(data);
        },
        onError: (err: Error) => {
            let msg: string;
            if (err instanceof ValidationError && err.details.length > 0) msg = err.details.map((d) => d.message).join(" ");
            else if (err instanceof ApiError) {
                if (err.status === 0) msg = "Backend is not reachable. Make sure all services are running.";
                else msg = err.message || "Failed to process. Please try again.";
            } else msg = err.message || "Failed to process. Please try again.";
            toast.error(msg);
        },
    });

    useEffect(() => {
        if (formData.createdResumeId && !refreshedResume) {
            const id = formData.createdResumeId;
            setIsLoadingResume(true);
            queryClient.removeQueries({ queryKey: ["resume-render", id] });
            syncToBackend()
                .then(() => getResume(id))
                .then(setRefreshedResume)
                .catch(() => updateFormData({ createdResumeId: null }))
                .finally(() => setIsLoadingResume(false));
        }
    }, [formData.createdResumeId, refreshedResume, updateFormData, syncToBackend, queryClient]);

    function buildRequest(): CreateResumeRequest | null {
        if (!formData.templateId) return null;
        return {
            templateId: formData.templateId,
            rawData: {
                content: {
                    personal: { firstName: formData.firstName, middleName: formData.middleName || null, lastName: formData.lastName, jobTitle: formData.jobTitle || null, email: formData.email, phone: formData.phone, location: formData.location, zipCode: formData.zipCode || null, dateOfBirth: formData.dateOfBirth || null, linkedinUrl: formData.linkedinUrl || null, siteUrl: formData.siteUrl || null, photoUrl: formData.photoUrl || null },
                    summary: formData.summary,
                    experience: formData.experience.map((e) => ({ id: e.id, title: e.title, company: e.company, location: e.location || null, startDate: e.startDate, endDate: e.endDate || null, description: e.description })),
                    education: formData.education.map((e) => ({ id: e.id, institution: e.institution, degree: e.degree, fieldOfStudy: e.fieldOfStudy, grade: e.grade || null, startDate: e.startDate, endDate: e.endDate })),
                    courses: formData.courses.map((c) => ({ id: c.id, name: c.name, provider: c.provider, date: c.date, certificateUrl: c.certificateUrl || null })),
                    skills: formData.skills.flatMap((group) =>
                        group.items.map((name) => ({ name, category: group.category || null }))
                    ),
                    projects: formData.projects.length > 0 ? formData.projects.map((p) => ({ id: p.id, name: p.name, role: p.role || null, description: p.description || null, link: p.link || null, technologies: p.technologies.length > 0 ? p.technologies : null })) : undefined,
                    languages: formData.languages.length > 0 ? formData.languages.map((l) => ({ language: l.language, level: (l.level as LanguageLevel) || null })) : undefined,
                    volunteers: formData.volunteers.length > 0 ? formData.volunteers.map((v) => ({ id: v.id || null, organization: v.organization, role: v.role, startDate: v.startDate || null, endDate: v.endDate || null, description: v.description || null })) : undefined,
                    hobbies: formData.hobbies.length > 0 ? formData.hobbies : undefined,
                    other: formData.other.length > 0 ? formData.other.map((o) => ({ label: o.label, value: o.value })) : undefined,
                },
            },
        };
    }

    function handleCreate() {
        const req = buildRequest();
        if (!req) { toast.error("Please select a template first."); router.push("/create/template"); return; }
        mutate(req);
    }
    function handleBackToEdit() { router.push("/create/steps/1"); }
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
            <WizardProgress step={9} total={9} title="Review & Finalize"
                subtitle={displayResume ? "Your resume is ready. Go back to any step to make edits." : "Review your information, then create your resume."} />

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
                <PostCreationView resume={displayResume} onBackToEdit={handleBackToEdit} onViewResume={handleViewResume} />
            ) : (
                /* ── Pre-creation review — mirrors wizard step order exactly ── */
                <div className="flex flex-col lg:flex-row gap-8">
                    <div className="flex-1 space-y-5">

                        {/* Step 1 – Personal Info */}
                        <ReviewSection title="Personal Information" icon="person">
                            <div className="grid grid-cols-2 gap-2 text-sm">
                                <span className="text-secondary">Name</span><span className="text-on-surface font-medium">{fullName}</span>
                                <span className="text-secondary">Email</span><span className="text-on-surface font-medium">{formData.email || "—"}</span>
                                <span className="text-secondary">Phone</span><span className="text-on-surface font-medium">{formData.phone || "—"}</span>
                                <span className="text-secondary">Location</span><span className="text-on-surface font-medium">{formData.location || "—"}</span>
                            </div>
                        </ReviewSection>

                        {/* Step 2 – Education */}
                        <ReviewSection title={`Education (${formData.education.length})`} icon="school">
                            {formData.education.length === 0 ? <p className="text-sm text-secondary">No entries added.</p> : (
                                <ul className="space-y-2">{formData.education.map((e) => (<li key={e.id} className="text-sm"><span className="font-medium text-on-surface">{e.institution}</span><span className="text-secondary ml-2">— {e.degree}, {e.fieldOfStudy}</span></li>))}</ul>
                            )}
                        </ReviewSection>

                        {/* Step 3 – Courses (optional) */}
                        <ReviewSection title={`Courses (${formData.courses.length})`} icon="menu_book">
                            {formData.courses.length === 0
                                ? <p className="text-sm text-secondary italic opacity-70">Optional — none added.</p>
                                : <ul className="space-y-2">{formData.courses.map((c) => (<li key={c.id} className="text-sm"><span className="font-medium text-on-surface">{c.name}</span><span className="text-secondary ml-2">@ {c.provider} ({c.date})</span></li>))}</ul>
                            }
                        </ReviewSection>

                        {/* Step 4 – Work Experience */}
                        <ReviewSection title={`Experience (${formData.experience.length})`} icon="work">
                            {formData.experience.length === 0 ? <p className="text-sm text-secondary">No entries added.</p> : (
                                <ul className="space-y-2">{formData.experience.map((e) => (<li key={e.id} className="text-sm"><span className="font-medium text-on-surface">{e.title}</span><span className="text-secondary ml-2">@ {e.company}</span></li>))}</ul>
                            )}
                        </ReviewSection>

                        {/* Step 5 – Projects (optional) */}
                        <ReviewSection title={`Projects (${formData.projects.length})`} icon="code">
                            {formData.projects.length === 0
                                ? <p className="text-sm text-secondary italic opacity-70">Optional — none added.</p>
                                : <ul className="space-y-2">{formData.projects.map((p) => (<li key={p.id} className="text-sm"><span className="font-medium text-on-surface">{p.name}</span>{p.role && <span className="text-secondary ml-2">— {p.role}</span>}{p.technologies.length > 0 && <span className="text-secondary ml-2">({p.technologies.join(", ")})</span>}</li>))}</ul>
                            }
                        </ReviewSection>

                        {/* Step 6 – Summary & Skills */}
                        <ReviewSection title={`Summary & Skills (${formData.skills.reduce((n, g) => n + g.items.length, 0)} skills)`} icon="psychology">
                            <div className="space-y-4">
                                <div>
                                    <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">Professional Summary</p>
                                    {formData.summary.trim()
                                        ? <p className="text-sm text-on-surface leading-relaxed line-clamp-3">{formData.summary}</p>
                                        : <p className="text-sm text-secondary italic">No summary written yet.</p>
                                    }
                                </div>
                                <div>
                                    <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">Skills</p>
                                    {formData.skills.every((g) => g.items.length === 0)
                                        ? <p className="text-sm text-secondary">No skills added.</p>
                                        : <div className="space-y-3">{formData.skills.filter((g) => g.items.length > 0).map((group, i) => (
                                            <div key={i}>
                                                {group.category && <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">{group.category}</p>}
                                                <div className="flex flex-wrap gap-2">{group.items.map((s) => (<span key={s} className="px-2.5 py-1 bg-primary-fixed/30 text-on-surface rounded-lg text-xs font-medium">{s}</span>))}</div>
                                            </div>
                                        ))}</div>
                                    }
                                </div>
                            </div>
                        </ReviewSection>

                        {/* Step 7 – Languages (optional) */}
                        <ReviewSection title={`Languages (${formData.languages.length})`} icon="translate">
                            {formData.languages.length === 0
                                ? <p className="text-sm text-secondary italic opacity-70">Optional — none added.</p>
                                : <div className="flex flex-wrap gap-2">{formData.languages.map((l, i) => (<span key={i} className="px-2.5 py-1 bg-primary-fixed/30 text-on-surface rounded-lg text-xs font-medium">{l.language}{l.level ? ` — ${l.level}` : ""}</span>))}</div>
                            }
                        </ReviewSection>

                        {/* Step 8 – Extras (optional) */}
                        <ReviewSection title="Extras" icon="volunteer_activism">
                            <div className="space-y-4">
                                {/* Volunteer Work */}
                                <div>
                                    <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">Volunteer Work ({formData.volunteers.length})</p>
                                    {formData.volunteers.length === 0
                                        ? <p className="text-sm text-secondary italic opacity-70">None added.</p>
                                        : <ul className="space-y-1">{formData.volunteers.map((v, i) => (<li key={i} className="text-sm"><span className="font-medium text-on-surface">{v.role}</span><span className="text-secondary ml-2">@ {v.organization}</span></li>))}</ul>
                                    }
                                </div>
                                {/* Hobbies */}
                                <div>
                                    <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">Hobbies ({formData.hobbies.length})</p>
                                    {formData.hobbies.length === 0
                                        ? <p className="text-sm text-secondary italic opacity-70">None added.</p>
                                        : <div className="flex flex-wrap gap-2">{formData.hobbies.map((h, i) => (<span key={i} className="px-2.5 py-1 bg-secondary-container/40 text-on-surface rounded-lg text-xs font-medium">{h}</span>))}</div>
                                    }
                                </div>
                                {/* Other */}
                                <div>
                                    <p className="text-xs font-semibold text-secondary uppercase tracking-wide mb-1.5">Other Info ({formData.other.length})</p>
                                    {formData.other.length === 0
                                        ? <p className="text-sm text-secondary italic opacity-70">None added.</p>
                                        : <div className="grid grid-cols-2 gap-1 text-sm">{formData.other.map((o, i) => (<React.Fragment key={i}><span className="text-secondary">{o.label}</span><span className="text-on-surface font-medium">{o.value}</span></React.Fragment>))}</div>
                                    }
                                </div>
                            </div>
                        </ReviewSection>

                    </div>

                    {/* Right: order summary */}
                    <div className="w-full lg:w-[320px] space-y-5">
                        <div className="bg-surface-container-low border border-outline-variant rounded-xl p-6 space-y-5">
                            <h3 className="font-h2 text-lg text-on-surface">Order Summary</h3>
                            <div className="space-y-2 text-sm text-secondary">
                                <div className="flex justify-between"><span>Resume creation</span><span className="text-on-surface font-medium">Free</span></div>
                                <div className="flex justify-between"><span>PDF download</span><span className="text-on-surface font-medium">Paid</span></div>
                            </div>
                            <ul className="space-y-2 text-xs text-secondary">
                                {["AI-enhanced content", "ATS-optimized format", "Unlimited edits"].map((f) => (
                                    <li key={f} className="flex items-center gap-2"><MaterialIcon name="check" size={14} className="text-primary shrink-0" />{f}</li>
                                ))}
                            </ul>
                        </div>
                    </div>
                </div>
            )}

            {!isPending && !displayResume && (
                <div className="mt-8 flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button type="button" onClick={() => router.push("/create/steps/8")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg text-secondary font-semibold hover:text-on-surface transition-colors">
                        <MaterialIcon name="arrow_back" size={18} />Back
                    </button>
                    <button onClick={handleCreate}
                        className="flex items-center gap-2 px-8 py-3 rounded-xl bg-primary text-on-primary font-bold shadow-lg shadow-primary/20 hover:opacity-90 active:scale-[0.98] transition-all">
                        <MaterialIcon name="auto_awesome" size={20} />Polish with AI &amp; Review
                    </button>
                </div>
            )}
        </div>
    );
}
