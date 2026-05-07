"use client";

import { useState, useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { useFieldArray, useForm, Controller, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useWizardStore } from "@/store/wizardStore";
import type { JobTitleSuggestion } from "@/types/api.types";
import { regenerateSection } from "@/lib/api/resumes";
import { ApiError, ValidationError } from "@/lib/api/client";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type { RegenerateRequest } from "@/types/api.types";

// ── Schema ────────────────────────────────────────────────────────────────────

const entrySchema = z.object({
    id: z.string(),
    title: z.string().min(1, "Job title is required").max(200),
    company: z.string().min(1, "Company is required").max(200),
    location: z.string().max(200).optional(),
    startDate: z.string().optional(),
    endDate: z.string().nullable().optional(),
    description: z.string().min(1, "Description is required").max(2000),
    isPresent: z.boolean().optional(),
});

const step4Schema = z.object({
    experience: z.array(entrySchema).min(1, "Add at least one experience entry"),
});

type Step4Values = z.infer<typeof step4Schema>;

// ── Style constants ───────────────────────────────────────────────────────────

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";
const selectCls =
    "w-full h-11 pl-3 pr-9 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all appearance-none cursor-pointer text-sm";

// ── Helpers ───────────────────────────────────────────────────────────────────

function newEntry() {
    return {
        id: `exp_${crypto.randomUUID()}`,

        title: "",
        company: "",
        location: "",
        startDate: "",
        endDate: "",
        description: "",
        isPresent: false,
    };
}

// ── Month / Year picker ───────────────────────────────────────────────────────

const MONTHS = [
    { value: "01", label: "January" },
    { value: "02", label: "February" },
    { value: "03", label: "March" },
    { value: "04", label: "April" },
    { value: "05", label: "May" },
    { value: "06", label: "June" },
    { value: "07", label: "July" },
    { value: "08", label: "August" },
    { value: "09", label: "September" },
    { value: "10", label: "October" },
    { value: "11", label: "November" },
    { value: "12", label: "December" },
];
const CURRENT_YEAR = new Date().getFullYear();
const YEARS = Array.from({ length: 50 }, (_, i) => CURRENT_YEAR - i);

function MonthYearPicker({
    value,
    onChange,
    disabled,
}: {
    value: string;
    onChange: (v: string) => void;
    disabled?: boolean;
}) {
    const parts = value?.match(/^(\d{4})-(\d{2})$/);

    // Local state tracks partial selections (e.g. month chosen but year not yet).
    // Without this, a controlled input resets to "" whenever only one is filled.
    const [month, setMonth] = useState(parts ? parts[2] : "");
    const [year, setYear] = useState(parts ? parts[1] : "");

    // Sync inbound value changes (e.g. form reset)
    useEffect(() => {
        const p = value?.match(/^(\d{4})-(\d{2})$/);
        setMonth(p ? p[2] : "");
        setYear(p ? p[1] : "");
    }, [value]);

    function handleMonthChange(m: string) {
        setMonth(m);
        if (m && year) onChange(`${year}-${m}`);
        else if (!m && !year) onChange("");
    }

    function handleYearChange(y: string) {
        setYear(y);
        if (month && y) onChange(`${y}-${month}`);
        else if (!month && !y) onChange("");
    }

    return (
        <div className="flex gap-2">
            <div className="relative flex-1 min-w-0">
                <select
                    value={month}
                    onChange={(e) => handleMonthChange(e.target.value)}
                    disabled={disabled}
                    className={selectCls}
                >
                    <option value="">Month</option>
                    {MONTHS.map((m) => (
                        <option key={m.value} value={m.value}>
                            {m.label}
                        </option>
                    ))}
                </select>
                <span className="pointer-events-none absolute inset-y-0 right-2.5 flex items-center">
                    <MaterialIcon name="expand_more" size={18} className="text-secondary" />
                </span>
            </div>
            <div className="relative flex-1 min-w-0">
                <select
                    value={year}
                    onChange={(e) => handleYearChange(e.target.value)}
                    disabled={disabled}
                    className={selectCls}
                >
                    <option value="">Year</option>
                    {YEARS.map((y) => (
                        <option key={y} value={String(y)}>
                            {y}
                        </option>
                    ))}
                </select>
                <span className="pointer-events-none absolute inset-y-0 right-2.5 flex items-center">
                    <MaterialIcon name="expand_more" size={18} className="text-secondary" />
                </span>
            </div>
        </div>
    );
}

// ── Quick prompts ─────────────────────────────────────────────────────────────

const QUICK_PROMPTS = [
    "Quantify achievements with numbers",
    "Use stronger action verbs",
    "Add impact metrics",
    "Make it more concise",
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
    const inflightRef = useRef(false);

    const { mutate, isPending } = useMutation({
        mutationFn: (req: RegenerateRequest) => regenerateSection(resumeId, req),
        onSuccess: (data) => {
            inflightRef.current = false;
            const text =
                typeof data.updatedContent === "string"
                    ? data.updatedContent
                    : JSON.stringify(data.updatedContent);
            onApply(text);
            toast.success("Description Polished", {
                description: `${data.regenCountRemaining} AI use${data.regenCountRemaining !== 1 ? "s" : ""} remaining.`,
            });
            onClose();
        },
        onError: (err: Error) => {
            inflightRef.current = false;
            let msg: string;
            if (err instanceof ValidationError && err.details.length > 0) {
                msg = err.details.map((d) => d.message).join(" ");
            } else if (err instanceof ApiError) {
                if (err.status === 0) {
                    msg = "Backend is not reachable. Make sure all services are running.";
                } else if (err.status === 401) {
                    msg = "Your session has expired. Please log in again.";
                } else if (err.status === 429) {
                    msg = "You've reached the max regenerations (3) for this section.";
                } else if (err.status >= 500) {
                    msg = `Server error (${err.status}). Please try again in a moment.`;
                } else {
                    msg = err.message || "Polish failed. Please try again.";
                }
            } else {
                msg = err.message || "Something went wrong. Please try again.";
            }
            setErrorMsg(msg);
            toast.error(msg);
        },
    });

    function handleApply() {
        if (inflightRef.current || isPending) return;
        if (!entryId) {
            setErrorMsg("Could not identify this experience entry. Please close and reopen the panel.");
            return;
        }
        if (prompt.trim().length < 3) {
            toast.error("Please describe what to improve.");
            return;
        }
        inflightRef.current = true;
        setErrorMsg(null);
        mutate({ sectionIdentifier: entryId, userPrompt: prompt.trim(), targetFormat });
    }

    return (
        // ⚠️ Use <div> not <form> — this panel is nested inside the outer step form.
        // A nested <form> causes the outer form to submit when "Apply" is clicked,
        // navigating away from the page before the mutation can complete.
        <div className="rounded-xl border-2 border-primary/20 overflow-hidden shadow-sm">
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/20 px-4 py-2.5 flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={12} className="text-on-primary" filled />
                </div>
                <span className="text-xs font-bold text-primary uppercase tracking-wide">
                    AI Suggestion
                </span>
                <button
                    type="button"
                    onClick={onClose}
                    className="ml-auto text-secondary hover:text-on-surface transition-colors"
                    aria-label="Close AI panel"
                >
                    <MaterialIcon name="close" size={16} />
                </button>
            </div>

            <div className="bg-white px-4 py-3 space-y-3">
                <div className="flex flex-wrap gap-1.5">
                    {QUICK_PROMPTS.map((s) => (
                        <button
                            key={s}
                            type="button"
                            disabled={isPending}
                            onClick={() => setPrompt(s)}
                            className="px-2.5 py-1 text-[11px] font-medium border border-outline-variant rounded-full text-secondary hover:border-primary hover:text-primary hover:bg-primary-fixed/10 transition-all disabled:opacity-40"
                        >
                            {s}
                        </button>
                    ))}
                </div>
                <textarea
                    className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-3 py-2 text-sm text-on-surface placeholder:text-secondary/60 focus:outline-none focus:ring-2 focus:ring-primary resize-none"
                    rows={2}
                    placeholder="Describe what to improve…"
                    value={prompt}
                    onChange={(e) => {
                        setPrompt(e.target.value.slice(0, 300));
                        if (errorMsg) setErrorMsg(null);
                    }}
                    disabled={isPending}
                    autoFocus
                />

                {/* Inline error message */}
                {errorMsg && (
                    <div className="flex items-start gap-2 px-3 py-2 bg-error/10 border border-error/30 rounded-lg text-xs text-error">
                        <MaterialIcon name="error" size={14} className="shrink-0 mt-0.5" />
                        <span>{errorMsg}</span>
                    </div>
                )}

                <div className="flex items-center justify-between">
                    <span className="text-[10px] text-secondary/50">{prompt.length}/300</span>
                    <div className="flex items-center gap-2">
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-3 py-1.5 text-sm text-secondary hover:text-on-surface transition-colors rounded-lg"
                            disabled={isPending}
                        >
                            Cancel
                        </button>
                        <button
                            type="button"
                            onClick={handleApply}
                            disabled={isPending || prompt.trim().length < 3}
                            className="flex items-center gap-1.5 px-4 py-1.5 bg-primary text-on-primary rounded-lg text-sm font-semibold disabled:opacity-50 hover:opacity-90 transition-opacity shadow-sm"
                        >
                            {isPending ? (
                                <>
                                    <div className="w-3 h-3 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                                    Polishing…
                                </>
                            ) : (
                                <>
                                    <MaterialIcon name="auto_awesome" size={12} filled />
                                    Apply Polish
                                </>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

// ── Experience card ───────────────────────────────────────────────────────────

function ExperienceCard({
    index,
    register,
    control,
    errors,
    canRemove,
    onRemove,
    setValue,
    resumeId,
    descriptionFormat,
    titleSuggestions,
}: {
    index: number;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    register: any;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    control: any;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    errors: any;
    canRemove: boolean;
    onRemove: () => void;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    setValue: any;
    resumeId: string | null;
    descriptionFormat: string;
    titleSuggestions?: JobTitleSuggestion[];
}) {
    const [showPolish, setShowPolish] = useState(false);
    const isPresent = useWatch({ control, name: `experience.${index}.isPresent` });
    const entryId = useWatch({ control, name: `experience.${index}.id` }) as string | undefined;

    return (
        <div className="bg-white border border-outline-variant/30 rounded-xl shadow-sm overflow-hidden">
            {/* Hidden id keeps the entry's stable ID in RHF form state */}
            <input type="hidden" {...register(`experience.${index}.id`)} />
            <div className="p-6 space-y-4">
                {/* Title + Company */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="flex flex-col gap-2">
                        <label className="font-label-caps text-label-caps text-secondary uppercase">
                            Job Title <span className="text-error">*</span>
                        </label>
                        <input
                            {...register(`experience.${index}.title`)}
                            placeholder="e.g. Senior Product Designer"
                            className={inputCls}
                        />
                        {errors.experience?.[index]?.title && (
                            <p className="text-xs text-error">
                                {errors.experience[index].title?.message}
                            </p>
                        )}
                    </div>
                    <div className="flex flex-col gap-2">
                        <label className="font-label-caps text-label-caps text-secondary uppercase">
                            Company <span className="text-error">*</span>
                        </label>
                        <input
                            {...register(`experience.${index}.company`)}
                            placeholder="e.g. Acme Corp"
                            className={inputCls}
                        />
                        {errors.experience?.[index]?.company && (
                            <p className="text-xs text-error">
                                {errors.experience[index].company?.message}
                            </p>
                        )}
                    </div>
                </div>

                {/* AI Job Title Suggestions — full width below both fields */}
                {titleSuggestions && titleSuggestions.length > 0 && (
                    <div className="rounded-xl border border-primary/20 overflow-hidden">
                        <div className="flex items-center gap-2 px-4 py-2.5 bg-primary/5 border-b border-primary/15">
                            <MaterialIcon name="auto_awesome" size={14} className="text-primary" filled />
                            <span className="text-xs font-bold text-primary uppercase tracking-wide">
                                AI Job Title Suggestions
                            </span>
                            <span className="ml-auto text-xs text-secondary">
                                {titleSuggestions.length} available
                            </span>
                        </div>
                        <div className="px-4 py-3 flex flex-wrap gap-2 bg-white">
                            {[...titleSuggestions]
                                .sort((a, b) => b.score - a.score)
                                .slice(0, 5)
                                .map((s) => (
                                    <button
                                        key={s.title}
                                        type="button"
                                        onClick={() =>
                                            setValue(
                                                `experience.${index}.title`,
                                                s.title,
                                                { shouldValidate: true },
                                            )
                                        }
                                        className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary/5 border border-primary/25 rounded-lg text-xs font-medium text-primary hover:bg-primary hover:text-on-primary transition-colors"
                                    >
                                        <MaterialIcon name="add" size={12} />
                                        {s.title}
                                    </button>
                                ))}
                        </div>
                    </div>
                )}

                {/* Location */}
                <div className="flex flex-col gap-2">
                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                        Location{" "}
                        <span className="text-secondary normal-case font-normal text-xs">(optional)</span>
                    </label>
                    <input
                        {...register(`experience.${index}.location`)}
                        placeholder="e.g. New York, NY"
                        className={inputCls}
                    />
                </div>

                {/* Dates */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Start date */}
                    <div className="flex flex-col gap-2">
                        <label className="font-label-caps text-label-caps text-secondary uppercase">
                            Start Date{" "}
                            <span className="text-secondary normal-case font-normal text-xs">(optional)</span>
                        </label>
                        <Controller
                            control={control}
                            name={`experience.${index}.startDate`}
                            render={({ field }) => (
                                <MonthYearPicker
                                    value={field.value ?? ""}
                                    onChange={field.onChange}
                                />
                            )}
                        />
                    </div>

                    {/* End date */}
                    <div className="flex flex-col gap-2">
                        <div className="flex items-center justify-between">
                            <label className="font-label-caps text-label-caps text-secondary uppercase">
                                End Date{" "}
                                <span className="text-secondary normal-case font-normal text-xs">
                                    (optional)
                                </span>
                            </label>
                            <label className="flex items-center gap-1.5 cursor-pointer select-none">
                                <Controller
                                    control={control}
                                    name={`experience.${index}.isPresent`}
                                    render={({ field }) => (
                                        <input
                                            type="checkbox"
                                            checked={field.value ?? false}
                                            onChange={(e) => {
                                                field.onChange(e.target.checked);
                                                if (e.target.checked) {
                                                    setValue(`experience.${index}.endDate`, "");
                                                }
                                            }}
                                            className="w-3.5 h-3.5 accent-primary rounded"
                                        />
                                    )}
                                />
                                <span className="text-[11px] font-semibold text-secondary">
                                    Present
                                </span>
                            </label>
                        </div>

                        {isPresent ? (
                            <div className="flex items-center h-11 px-4 border-2 border-primary/30 rounded-lg bg-primary-fixed/20">
                                <MaterialIcon name="work" size={16} className="text-primary mr-2" filled />
                                <span className="text-sm font-bold text-primary">
                                    Currently working here
                                </span>
                            </div>
                        ) : (
                            <Controller
                                control={control}
                                name={`experience.${index}.endDate`}
                                render={({ field }) => (
                                    <MonthYearPicker
                                        value={field.value ?? ""}
                                        onChange={field.onChange}
                                    />
                                )}
                            />
                        )}
                    </div>
                </div>

                {/* Description */}
                <div className="flex flex-col gap-2">
                    <div className="flex items-center justify-between gap-3">
                        <label className="font-label-caps text-label-caps text-secondary uppercase">
                            Description & Key Achievements <span className="text-error">*</span>
                        </label>
                        {resumeId && (
                            <button
                                type="button"
                                onClick={() => setShowPolish((v) => !v)}
                                className={`flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${showPolish
                                    ? "bg-primary text-on-primary"
                                    : "text-primary border border-primary/30 hover:bg-primary-fixed/20"
                                    }`}
                            >
                                <MaterialIcon name="auto_awesome" size={12} filled={showPolish} />
                                {showPolish ? "Close AI" : "Polish with AI"}
                            </button>
                        )}
                    </div>
                    <textarea
                        {...register(`experience.${index}.description`)}
                        rows={4}
                        placeholder="Describe your responsibilities, key achievements, and impact..."
                        className="w-full px-4 py-3 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all resize-none"
                    />
                    {errors.experience?.[index]?.description && (
                        <p className="text-xs text-error">
                            {errors.experience[index].description?.message}
                        </p>
                    )}
                    {showPolish && resumeId && entryId && (
                        <InlinePolishPanel
                            entryId={entryId}
                            resumeId={resumeId}
                            targetFormat={descriptionFormat}
                            onApply={(text) =>
                                setValue(`experience.${index}.description`, text, {
                                    shouldValidate: true,
                                    shouldDirty: true,
                                })
                            }
                            onClose={() => setShowPolish(false)}
                        />
                    )}
                </div>
            </div>

            {/* Card footer */}
            {canRemove && (
                <div className="px-6 py-3 bg-surface-container-low border-t border-outline-variant/20 flex justify-end">
                    <button
                        type="button"
                        onClick={onRemove}
                        className="flex items-center gap-1.5 text-error text-sm font-semibold hover:opacity-80 transition-opacity"
                    >
                        <MaterialIcon name="delete" size={18} />
                        Remove Position
                    </button>
                </div>
            )}
        </div>
    );
}

// ── Page ──────────────────────────────────────────────────────────────────────

export default function Step4Page() {
    const router = useRouter();
    const { formData, updateFormData, jobTitleSuggestions } = useWizardStore();

    const {
        register,
        handleSubmit,
        control,
        setValue,
        watch,
        formState: { errors },
    } = useForm<Step4Values>({
        resolver: zodResolver(step4Schema),
        defaultValues: {
            experience:
                formData.experience.length > 0
                    ? formData.experience.map((e) => ({
                        id: e.id,
                        title: e.title,
                        company: e.company,
                        location: e.location ?? "",
                        startDate: e.startDate,
                        endDate: e.endDate ?? "",
                        description: e.description,
                        isPresent: !e.endDate,
                    }))
                    : [newEntry()],
        },
    });

    const { fields, append, remove } = useFieldArray({ control, name: "experience", keyName: "_id" });

    // Auto-save to store on every change so sidebar navigation doesn't lose data
    useEffect(() => {
        const sub = watch((values) => {
            if (!values.experience) return;
            updateFormData({
                experience: values.experience
                    .filter(Boolean)
                    .map((e: any) => ({
                        id: e.id ?? "",
                        title: e.title ?? "",
                        company: e.company ?? "",
                        location: e.location ?? "",
                        startDate: e.startDate ?? "",
                        endDate: e.isPresent ? "" : (e.endDate ?? ""),
                        description: e.description ?? "",
                    })),
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step4Values) {
        updateFormData({
            experience: values.experience.map((e) => ({
                id: e.id,
                title: e.title,
                company: e.company,
                location: e.location ?? "",
                startDate: e.startDate ?? "",
                endDate: e.isPresent ? "" : (e.endDate ?? ""),
                description: e.description,
            })),
        });
        router.push("/create/steps/5");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={4}
                total={6}
                title="Work Experience"
                subtitle={
                    formData.createdResumeId
                        ? "Edit your positions below. Use Polish with AI to enhance each description."
                        : "Detail your professional journey."
                }
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
                {fields.map((field, index) => (
                    <ExperienceCard
                        key={field._id}
                        index={index}
                        register={register}
                        control={control}
                        errors={errors}
                        canRemove={fields.length > 1}
                        onRemove={() => remove(index)}
                        setValue={setValue}
                        resumeId={formData.createdResumeId}
                        descriptionFormat={formData.descriptionFormat}
                        titleSuggestions={jobTitleSuggestions}
                    />
                ))}

                {/* Add entry */}
                <button
                    type="button"
                    onClick={() => append(newEntry())}
                    className="w-full py-6 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all"
                >
                    <MaterialIcon name="add_circle" size={24} />
                    <span className="font-manrope font-semibold">Add Work Experience</span>
                </button>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/3")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg text-secondary font-semibold hover:text-on-surface transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back to Courses
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-2.5 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-sm"
                    >
                        Continue to Summary
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
