"use client";

import { useState, useRef, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useWizardStore } from "@/store/wizardStore";
import { regenerateSection } from "@/lib/api/resumes";
import { ApiError, ValidationError } from "@/lib/api/client";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type { RegenerateRequest } from "@/types/api.types";

const step5Schema = z.object({
    summary: z.string().min(10, "Summary is required (min 10 chars)").max(2000),
    skills: z.array(z.string()).min(3, "Add at least 3 skills"),
});

type Step5Values = z.infer<typeof step5Schema>;

// ─── AI Polish ────────────────────────────────────────────────────────────────

const QUICK_PROMPTS_SUMMARY = [
    "Make it more concise",
    "Add measurable impact",
    "Use stronger action verbs",
    "Tailor for a leadership role",
];

const QUICK_PROMPTS_SKILLS = [
    "Suggest missing technical skills",
    "Add cloud/DevOps skills",
    "Add testing/QA skills",
    "Suggest leadership skills",
];

function InlinePolishPanel({
    sectionId,
    resumeId,
    quickPrompts,
    targetFormat,
    onApply,
    onClose,
}: {
    sectionId: string;
    resumeId: string;
    quickPrompts: string[];
    targetFormat?: string | null;
    onApply: (text: string) => void;
    onClose: () => void;
}) {
    const [prompt, setPrompt] = useState("");
    const [errorMsg, setErrorMsg] = useState<string | null>(null);

    const { mutate, isPending } = useMutation({
        mutationFn: (req: RegenerateRequest) => regenerateSection(resumeId, req),
        onSuccess: (data) => {
            const text =
                typeof data.updatedContent === "string"
                    ? data.updatedContent
                    : JSON.stringify(data.updatedContent);
            onApply(text);
            toast.success("Section Polished", {
                description: `${data.regenCountRemaining} AI use${data.regenCountRemaining !== 1 ? "s" : ""} remaining.`,
            });
            onClose();
        },
        onError: (err: Error) => {
            let msg: string;
            if (err instanceof ValidationError && err.details.length > 0) {
                msg = err.details.map((d) => d.message).join(" ");
            } else if (err instanceof ApiError) {
                if (err.status === 0) msg = "Backend is not reachable. Make sure all services are running.";
                else if (err.status === 401) msg = "Your session has expired. Please log in again.";
                else if (err.status === 429) msg = "Max regenerations (3) reached for this section.";
                else if (err.status >= 500) msg = `Server error (${err.status}). Please try again.`;
                else msg = err.message || "Polish failed.";
            } else {
                msg = err.message || "Something went wrong.";
            }
            setErrorMsg(msg);
            toast.error(msg);
        },
    });

    function handleApply() {
        if (prompt.trim().length < 3) {
            toast.error("Please describe what to improve.");
            return;
        }
        setErrorMsg(null);
        mutate({ sectionIdentifier: sectionId, userPrompt: prompt.trim(), targetFormat });
    }

    return (
        <div className="rounded-xl border-2 border-primary/20 overflow-hidden shadow-sm mt-3">
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/20 px-4 py-2.5 flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={12} className="text-on-primary" filled />
                </div>
                <span className="text-xs font-bold text-primary uppercase tracking-wide">AI Suggestion</span>
                <button type="button" onClick={onClose} className="ml-auto text-secondary hover:text-on-surface transition-colors" aria-label="Close">
                    <MaterialIcon name="close" size={16} />
                </button>
            </div>
            <div className="bg-white px-4 py-3 space-y-3">
                <div className="flex flex-wrap gap-1.5">
                    {quickPrompts.map((s) => (
                        <button key={s} type="button" disabled={isPending} onClick={() => setPrompt(s)}
                            className="px-2.5 py-1 text-[11px] font-medium border border-outline-variant rounded-full text-secondary hover:border-primary hover:text-primary hover:bg-primary-fixed/10 transition-all disabled:opacity-40">
                            {s}
                        </button>
                    ))}
                </div>
                <textarea
                    className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-3 py-2 text-sm text-on-surface placeholder:text-secondary/60 focus:outline-none focus:ring-2 focus:ring-primary resize-none"
                    rows={2}
                    placeholder="Describe what to improve…"
                    value={prompt}
                    onChange={(e) => { setPrompt(e.target.value.slice(0, 300)); if (errorMsg) setErrorMsg(null); }}
                    disabled={isPending}
                    autoFocus
                />
                {errorMsg && (
                    <div className="flex items-start gap-2 px-3 py-2 bg-error/10 border border-error/30 rounded-lg text-xs text-error">
                        <MaterialIcon name="error" size={14} className="shrink-0 mt-0.5" />
                        <span>{errorMsg}</span>
                    </div>
                )}
                <div className="flex items-center justify-between">
                    <span className="text-[10px] text-secondary/50">{prompt.length}/300</span>
                    <div className="flex items-center gap-2">
                        <button type="button" onClick={onClose} className="px-3 py-1.5 text-sm text-secondary hover:text-on-surface transition-colors rounded-lg" disabled={isPending}>
                            Cancel
                        </button>
                        <button type="button" onClick={handleApply} disabled={isPending || prompt.trim().length < 3}
                            className="flex items-center gap-1.5 px-4 py-1.5 bg-primary text-on-primary rounded-lg text-sm font-semibold disabled:opacity-50 hover:opacity-90 transition-opacity shadow-sm">
                            {isPending ? (
                                <><div className="w-3 h-3 border-2 border-white/40 border-t-white rounded-full animate-spin" />Polishing…</>
                            ) : (
                                <><MaterialIcon name="auto_awesome" size={12} filled />Apply Polish</>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

// ─── Skills input ─────────────────────────────────────────────────────────────

function SkillsInput({
    skills,
    suggestions,
    onChange,
    error,
}: {
    skills: string[];
    suggestions: string[];
    onChange: (skills: string[]) => void;
    error?: string;
}) {
    const [input, setInput] = useState("");
    const inputRef = useRef<HTMLInputElement>(null);

    // Filtered suggestions not already added
    const filteredSuggestions = suggestions.filter(
        (s) =>
            !skills.includes(s) &&
            s.toLowerCase().includes(input.toLowerCase()) &&
            input.length > 0,
    );

    function addSkill(skill: string) {
        const trimmed = skill.trim();
        if (trimmed && !skills.includes(trimmed)) {
            onChange([...skills, trimmed]);
        }
        setInput("");
        inputRef.current?.focus();
    }

    function removeSkill(skill: string) {
        onChange(skills.filter((s) => s !== skill));
    }

    function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if ((e.key === "Enter" || e.key === ",") && input.trim()) {
            e.preventDefault();
            addSkill(input);
        }
        if (e.key === "Backspace" && !input && skills.length > 0) {
            removeSkill(skills[skills.length - 1]);
        }
    }

    return (
        <div className="space-y-3">
            {/* Input */}
            <div className="relative">
                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                    <MaterialIcon name="search" size={18} className="text-outline" />
                </span>
                <input
                    ref={inputRef}
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder="Type a skill and press Enter (e.g. React, SQL)"
                    className="w-full h-11 pl-10 pr-4 border border-outline-variant rounded-lg bg-surface-container font-input-text text-input-text focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all"
                />
                {filteredSuggestions.length > 0 && (
                    <div className="absolute top-full left-0 right-0 bg-white border border-outline-variant rounded-lg shadow-lg z-10 max-h-40 overflow-y-auto mt-1">
                        {filteredSuggestions.map((s) => (
                            <button
                                key={s}
                                type="button"
                                onClick={() => addSkill(s)}
                                className="w-full text-left px-4 py-2 text-sm hover:bg-surface-container-low text-on-surface transition-colors"
                            >
                                {s}
                            </button>
                        ))}
                    </div>
                )}
            </div>

            {error && <p className="text-xs text-error">{error}</p>}

            {/* Added skills */}
            {skills.length > 0 && (
                <div className="flex flex-wrap gap-2">
                    {skills.map((skill) => (
                        <span
                            key={skill}
                            className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary-fixed/30 text-on-surface rounded-lg text-sm font-medium"
                        >
                            {skill}
                            <button
                                type="button"
                                onClick={() => removeSkill(skill)}
                                className="text-secondary hover:text-error transition-colors"
                                aria-label={`Remove ${skill}`}
                            >
                                <MaterialIcon name="close" size={14} />
                            </button>
                        </span>
                    ))}
                </div>
            )}

            {/* AI Skill Suggestions panel */}
            {suggestions.length > 0 && suggestions.some((s) => !skills.includes(s)) && (
                <div className="rounded-xl border border-primary/20 overflow-hidden">
                    <div className="flex items-center gap-2 px-4 py-2.5 bg-primary/5 border-b border-primary/15">
                        <MaterialIcon name="auto_awesome" size={14} className="text-primary" filled />
                        <span className="text-xs font-bold text-primary uppercase tracking-wide">
                            AI Skill Suggestions
                        </span>
                        <span className="ml-auto text-xs text-secondary">
                            {suggestions.filter((s) => !skills.includes(s)).length} available
                        </span>
                    </div>
                    <div className="px-4 py-3 flex flex-wrap gap-2 bg-white">
                        {suggestions
                            .filter((s) => !skills.includes(s))
                            .map((s) => (
                                <button
                                    key={s}
                                    type="button"
                                    onClick={() => addSkill(s)}
                                    className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary/5 border border-primary/25 rounded-lg text-xs font-medium text-primary hover:bg-primary hover:text-on-primary transition-colors"
                                >
                                    <MaterialIcon name="add" size={12} />
                                    {s}
                                </button>
                            ))}
                    </div>
                </div>
            )}
        </div>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function Step5Page() {
    const router = useRouter();
    const { formData, updateFormData, skillSuggestions } = useWizardStore();
    const [skills, setSkills] = useState<string[]>(formData.skills);
    const [showSummaryPolish, setShowSummaryPolish] = useState(false);
    const [showSkillsPolish, setShowSkillsPolish] = useState(false);

    const resumeId = formData.createdResumeId;

    const {
        register,
        handleSubmit,
        setValue,
        watch,
        formState: { errors },
    } = useForm<Step5Values>({
        resolver: zodResolver(step5Schema),
        defaultValues: {
            summary: formData.summary,
            skills: formData.skills,
        },
    });

    // Keep RHF skills in sync with local state
    useEffect(() => {
        setValue("skills", skills, { shouldValidate: true });
    }, [skills, setValue]);

    // Auto-save summary to store on every keystroke
    useEffect(() => {
        const sub = watch((values) => {
            if (values.summary !== undefined) {
                updateFormData({ summary: values.summary });
            }
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    // Auto-save skills to store whenever local state changes
    useEffect(() => {
        updateFormData({ skills });
    }, [skills, updateFormData]);

    function onSubmit(values: Step5Values) {
        updateFormData({ summary: values.summary, skills });
        router.push("/create/steps/6");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto pb-16">
            <WizardProgress
                step={5}
                total={6}
                title="Summary & Skills"
                subtitle={
                    resumeId
                        ? "Edit your summary and skills. Use Polish with AI to improve them."
                        : "Craft a compelling narrative of your professional journey and highlight your core strengths."
                }
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-8">
                {/* Professional Summary */}
                <section className="space-y-4">
                    <div className="flex items-center gap-3">
                        <MaterialIcon name="description" size={22} className="text-primary" />
                        <h2 className="font-h2 text-h2 text-on-surface">Professional Summary</h2>
                    </div>

                    <div className="bg-white border border-outline-variant/30 rounded-xl p-6 shadow-sm">
                        <div className="flex items-center justify-between mb-2">
                            <label className="block font-label-caps text-label-caps text-secondary uppercase">
                                Your Impact Statement <span className="text-error">*</span>
                            </label>
                            {resumeId && (
                                <button
                                    type="button"
                                    onClick={() => { setShowSummaryPolish((v) => !v); setShowSkillsPolish(false); }}
                                    className={`flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${showSummaryPolish
                                        ? "bg-primary text-on-primary"
                                        : "text-primary border border-primary/30 hover:bg-primary-fixed/20"
                                        }`}
                                >
                                    <MaterialIcon name="auto_awesome" size={12} filled={showSummaryPolish} />
                                    {showSummaryPolish ? "Close AI" : "Polish with AI"}
                                </button>
                            )}
                        </div>
                        <textarea
                            {...register("summary")}
                            rows={6}
                            placeholder="e.g. Dedicated Software Engineer with 5+ years of experience building scalable web applications..."
                            className="w-full px-4 py-3 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all resize-none"
                        />
                        <div className="mt-2 flex items-center justify-between">
                            <span className="text-xs text-secondary font-body-sm">
                                Recommended: 200–400 characters
                            </span>
                        </div>
                        {errors.summary && (
                            <p className="text-xs text-error mt-1">{errors.summary.message}</p>
                        )}
                        {showSummaryPolish && resumeId && (
                            <InlinePolishPanel
                                sectionId="summary"
                                resumeId={resumeId}
                                quickPrompts={QUICK_PROMPTS_SUMMARY}
                                targetFormat={formData.summaryType}
                                onApply={(text) => {
                                    setValue("summary", text, { shouldValidate: true, shouldDirty: true });
                                }}
                                onClose={() => setShowSummaryPolish(false)}
                            />
                        )}
                    </div>
                </section>

                {/* Skills */}
                <section className="space-y-4">
                    <div className="flex items-center gap-3">
                        <MaterialIcon name="bolt" size={22} className="text-primary" />
                        <h2 className="font-h2 text-h2 text-on-surface">Technical &amp; Soft Skills</h2>
                    </div>

                    <div className="bg-white border border-outline-variant/30 rounded-xl p-6 shadow-sm space-y-4">
                        {resumeId && (
                            <div className="flex justify-end">
                                <button
                                    type="button"
                                    onClick={() => { setShowSkillsPolish((v) => !v); setShowSummaryPolish(false); }}
                                    className={`flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${showSkillsPolish
                                        ? "bg-primary text-on-primary"
                                        : "text-primary border border-primary/30 hover:bg-primary-fixed/20"
                                        }`}
                                >
                                    <MaterialIcon name="auto_awesome" size={12} filled={showSkillsPolish} />
                                    {showSkillsPolish ? "Close AI" : "Polish with AI"}
                                </button>
                            </div>
                        )}
                        <SkillsInput
                            skills={skills}
                            suggestions={skillSuggestions}
                            onChange={(updated) => {
                                setSkills(updated);
                                setValue("skills", updated, { shouldValidate: true });
                            }}
                            error={errors.skills?.message}
                        />
                        {showSkillsPolish && resumeId && (
                            <InlinePolishPanel
                                sectionId="skills"
                                resumeId={resumeId}
                                quickPrompts={QUICK_PROMPTS_SKILLS}
                                onApply={(text) => {
                                    // AI may return a JSON array string or comma-separated list
                                    try {
                                        const parsed = JSON.parse(text);
                                        if (Array.isArray(parsed)) {
                                            const next = parsed.map(String).filter(Boolean);
                                            setSkills(next);
                                            setValue("skills", next, { shouldValidate: true });
                                            return;
                                        }
                                    } catch { /* not JSON, treat as CSV */ }
                                    // Fallback: split by comma
                                    const next = text.split(",").map((s) => s.trim()).filter(Boolean);
                                    if (next.length > 0) {
                                        setSkills(next);
                                        setValue("skills", next, { shouldValidate: true });
                                    }
                                }}
                                onClose={() => setShowSkillsPolish(false)}
                            />
                        )}
                    </div>
                </section>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/4")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg text-secondary font-semibold hover:text-on-surface transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-2.5 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-sm"
                    >
                        Review Resume
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
