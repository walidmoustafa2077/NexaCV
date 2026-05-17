"use client";

import { useState, useRef, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useFieldArray, useForm, useWatch } from "react-hook-form";
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

// ── Schema ────────────────────────────────────────────────────────────────────

const entrySchema = z.object({
    id: z.string(),
    name: z.string().min(1, "Project name is required").max(200),
    role: z.string().max(200).optional(),
    description: z.string().max(1000).optional(),
    link: z.string().url("Must be a valid URL").or(z.literal("")).optional(),
    technologies: z.array(z.string()),
});

const step5Schema = z.object({
    projects: z.array(entrySchema),
});

type Step5Values = z.infer<typeof step5Schema>;

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";

function newEntry() {
    return {
        id: `proj_${crypto.randomUUID()}`,
        name: "",
        role: "",
        description: "",
        link: "",
        technologies: [] as string[],
    };
}

// ── Quick prompts ─────────────────────────────────────────────────────────────

const QUICK_PROMPTS = [
    "Quantify impact with metrics",
    "Highlight technical challenges solved",
    "Make it more concise",
    "Use stronger action verbs",
];

// ── Inline AI Polish Panel ────────────────────────────────────────────────────

function InlinePolishPanel({
    entryId,
    resumeId,
    targetFormat,
    onApply,
    onClose,
}: {
    entryId: string;
    resumeId: string;
    targetFormat: string;
    onApply: (text: string) => void;
    onClose: () => void;
}) {
    const [prompt, setPrompt] = useState("");
    const [errorMsg, setErrorMsg] = useState<string | null>(null);

    const { mutate, isPending } = useMutation({
        mutationFn: (req: RegenerateRequest) => regenerateSection(resumeId, req),
        onSuccess: (data) => {
            const text = typeof data.updatedContent === "string"
                ? data.updatedContent
                : JSON.stringify(data.updatedContent);
            onApply(text);
            toast.success("Description Polished", {
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
                else if (err.status === 429) msg = "You've reached the max regenerations (3) for this section.";
                else if (err.status >= 500) msg = `Server error (${err.status}). Please try again in a moment.`;
                else msg = err.message || "Polish failed. Please try again.";
            } else {
                msg = err.message || "Something went wrong. Please try again.";
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
        mutate({ sectionIdentifier: entryId, userPrompt: prompt.trim(), targetFormat });
    }

    return (
        <div className="rounded-xl border-2 border-primary/20 overflow-hidden shadow-sm">
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/20 px-4 py-2.5 flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={12} className="text-on-primary" filled />
                </div>
                <span className="text-xs font-bold text-primary uppercase tracking-wide">AI Suggestion</span>
                <button type="button" onClick={onClose} className="ml-auto text-secondary hover:text-on-surface transition-colors" aria-label="Close AI panel">
                    <MaterialIcon name="close" size={16} />
                </button>
            </div>
            <div className="bg-white px-4 py-3 space-y-3">
                <div className="flex flex-wrap gap-1.5">
                    {QUICK_PROMPTS.map((s) => (
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

// ── Tag input (for technologies) ──────────────────────────────────────────────

function TagInput({
    tags,
    onChange,
    placeholder,
}: {
    tags: string[];
    onChange: (tags: string[]) => void;
    placeholder?: string;
}) {
    const [input, setInput] = useState("");
    const inputRef = useRef<HTMLInputElement>(null);

    function addTag(tag: string) {
        const trimmed = tag.trim();
        if (trimmed && !tags.includes(trimmed)) {
            onChange([...tags, trimmed]);
        }
        setInput("");
        inputRef.current?.focus();
    }

    function removeTag(tag: string) {
        onChange(tags.filter((t) => t !== tag));
    }

    function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if ((e.key === "Enter" || e.key === ",") && input.trim()) {
            e.preventDefault();
            addTag(input);
        }
        if (e.key === "Backspace" && !input && tags.length > 0) {
            removeTag(tags[tags.length - 1]);
        }
    }

    return (
        <div className="space-y-2">
            <input
                ref={inputRef}
                value={input}
                onChange={(e) => setInput(e.target.value)}
                onKeyDown={handleKeyDown}
                placeholder={placeholder ?? "Type and press Enter or ,"}
                className={inputCls}
            />
            {tags.length > 0 && (
                <div className="flex flex-wrap gap-2">
                    {tags.map((tag) => (
                        <span key={tag} className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary-fixed/30 text-on-surface rounded-lg text-sm font-medium">
                            {tag}
                            <button type="button" onClick={() => removeTag(tag)} className="text-secondary hover:text-error transition-colors" aria-label={`Remove ${tag}`}>
                                <MaterialIcon name="close" size={14} />
                            </button>
                        </span>
                    ))}
                </div>
            )}
        </div>
    );
}

// ── Project card ──────────────────────────────────────────────────────────────

function ProjectCard({
    index,
    register,
    control,
    errors,
    onRemove,
    setValue,
    resumeId,
    descriptionFormat,
}: {
    index: number;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    register: any;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    control: any;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    errors: any;
    onRemove: () => void;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    setValue: any;
    resumeId: string | null;
    descriptionFormat: string;
}) {
    const [showPolish, setShowPolish] = useState(false);
    const entryId = useWatch({ control, name: `projects.${index}.id` }) as string | undefined;
    const technologies = (useWatch({ control, name: `projects.${index}.technologies` }) ?? []) as string[];

    return (
        <div className="bg-white border border-outline-variant/30 rounded-xl p-7 shadow-sm">
            <input type="hidden" {...register(`projects.${index}.id`)} />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Project Name */}
                <div className="md:col-span-2 flex flex-col gap-2 relative">
                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                        Project Name <span className="text-error">*</span>
                    </label>
                    <input {...register(`projects.${index}.name`)} placeholder="e.g. E-Commerce Platform" className={inputCls} />
                    {errors.projects?.[index]?.name && (
                        <p className="text-xs text-error">{errors.projects[index].name?.message}</p>
                    )}
                    <button type="button" onClick={onRemove}
                        className="absolute top-0 right-0 p-1.5 text-error hover:bg-error-container/20 rounded-full transition-all"
                        aria-label="Remove project">
                        <MaterialIcon name="delete" size={18} />
                    </button>
                </div>

                {/* Role */}
                <div className="flex flex-col gap-2">
                    <label className="font-label-caps text-label-caps text-secondary uppercase">Your Role (optional)</label>
                    <input {...register(`projects.${index}.role`)} placeholder="e.g. Lead Developer" className={inputCls} />
                </div>

                {/* Link */}
                <div className="flex flex-col gap-2">
                    <label className="font-label-caps text-label-caps text-secondary uppercase">Project Link (optional)</label>
                    <input {...register(`projects.${index}.link`)} placeholder="e.g. https://github.com/..." className={inputCls} />
                    {errors.projects?.[index]?.link && (
                        <p className="text-xs text-error">{errors.projects[index].link?.message}</p>
                    )}
                </div>

                {/* Technologies */}
                <div className="md:col-span-2 flex flex-col gap-2">
                    <label className="font-label-caps text-label-caps text-secondary uppercase">Technologies Used (optional)</label>
                    <TagInput
                        tags={technologies}
                        onChange={(tags) => setValue(`projects.${index}.technologies`, tags, { shouldDirty: true })}
                        placeholder="e.g. React, Node.js — press Enter or comma to add"
                    />
                </div>

                {/* Description */}
                <div className="md:col-span-2 flex flex-col gap-2">
                    <div className="flex items-center justify-between gap-3">
                        <label className="font-label-caps text-label-caps text-secondary uppercase">Description (optional)</label>
                        {resumeId && (
                            <button type="button" onClick={() => setShowPolish((v) => !v)}
                                className={`flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${showPolish ? "bg-primary text-on-primary" : "text-primary border border-primary/30 hover:bg-primary-fixed/20"}`}>
                                <MaterialIcon name="auto_awesome" size={12} filled={showPolish} />
                                {showPolish ? "Close AI" : "Polish with AI"}
                            </button>
                        )}
                    </div>
                    <textarea {...register(`projects.${index}.description`)} rows={3}
                        placeholder="Describe what the project does, your contributions, and impact…"
                        className="w-full px-4 py-3 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all resize-none"
                    />
                    {errors.projects?.[index]?.description && (
                        <p className="text-xs text-error">{errors.projects[index].description?.message}</p>
                    )}
                    {showPolish && resumeId && entryId && (
                        <InlinePolishPanel
                            entryId={entryId}
                            resumeId={resumeId}
                            targetFormat={descriptionFormat}
                            onApply={(text) => setValue(`projects.${index}.description`, text, { shouldValidate: true, shouldDirty: true })}
                            onClose={() => setShowPolish(false)}
                        />
                    )}
                </div>
            </div>
        </div>
    );
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function Step5Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();

    const resumeId = formData.createdResumeId ?? null;
    const descriptionFormat = "Bulleted";

    const {
        register,
        handleSubmit,
        control,
        watch,
        setValue,
        formState: { errors },
    } = useForm<Step5Values>({
        resolver: zodResolver(step5Schema),
        defaultValues: {
            projects:
                formData.projects.length > 0
                    ? formData.projects.map((p) => ({
                        id: p.id,
                        name: p.name,
                        role: p.role,
                        description: p.description,
                        link: p.link,
                        technologies: p.technologies ?? [],
                    }))
                    : [],
        },
    });

    const { fields, append, remove } = useFieldArray({ control, name: "projects" });

    // Auto-save to store on every change
    useEffect(() => {
        const sub = watch((values) => {
            if (!values.projects) return;
            updateFormData({
                projects: values.projects
                    .filter(Boolean)
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    .map((p: any) => ({
                        id: p.id ?? "",
                        name: p.name ?? "",
                        role: p.role ?? "",
                        description: p.description ?? "",
                        link: p.link ?? "",
                        technologies: Array.isArray(p.technologies) ? p.technologies : [],
                    })),
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step5Values) {
        updateFormData({
            projects: values.projects.map((p) => ({
                id: p.id,
                name: p.name,
                role: p.role ?? "",
                description: p.description ?? "",
                link: p.link ?? "",
                technologies: Array.isArray(p.technologies) ? p.technologies : [],
            })),
        });
        router.push("/create/steps/6");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={5}
                total={9}
                title="Projects"
                subtitle="Showcase personal or professional projects that demonstrate your skills and impact. This step is optional."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
                {fields.map((field, index) => (
                    <ProjectCard
                        key={field.id}
                        index={index}
                        register={register}
                        control={control}
                        errors={errors}
                        onRemove={() => remove(index)}
                        setValue={setValue}
                        resumeId={resumeId}
                        descriptionFormat={descriptionFormat}
                    />
                ))}

                {/* Add button */}
                <button type="button" onClick={() => append(newEntry())}
                    className="w-full py-6 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group">
                    <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                    <span className="font-manrope font-semibold">
                        {fields.length === 0 ? "Add a project" : "Add another project"}
                    </span>
                </button>

                {/* Pro tip */}
                <div className="flex items-start gap-4 p-4 rounded-xl bg-secondary-container/30">
                    <MaterialIcon name="lightbulb" size={20} className="text-secondary shrink-0 mt-0.5" />
                    <p className="font-body-sm text-sm text-secondary">
                        <strong>Pro Tip:</strong> Include projects directly relevant to the roles you&apos;re targeting. Quantify outcomes where possible (e.g. &ldquo;reduced load time by 40%&rdquo;).
                    </p>
                </div>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button type="button" onClick={() => router.push("/create/steps/4")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg font-semibold text-secondary hover:bg-secondary-container transition-colors">
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button type="submit"
                        className="flex items-center gap-2 px-8 py-3 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-md">
                        {fields.length === 0 ? "Skip & Continue" : "Save & Continue"}
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
