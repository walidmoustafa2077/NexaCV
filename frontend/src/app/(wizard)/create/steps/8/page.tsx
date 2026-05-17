"use client";

import { useEffect, useState, useRef } from "react";
import { useRouter } from "next/navigation";
import { useFieldArray, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useWizardStore } from "@/store/wizardStore";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";

// ── Schemas ───────────────────────────────────────────────────────────────────

const volunteerSchema = z.object({
    id: z.string(),
    organization: z.string().min(1, "Organization is required").max(200),
    role: z.string().min(1, "Role is required").max(200),
    startDate: z.string().optional(),
    endDate: z.string().optional(),
    description: z.string().max(800).optional(),
});

const otherEntrySchema = z.object({
    label: z.string().min(1, "Label is required").max(100),
    value: z.string().min(1, "Value is required").max(200),
});

const step8Schema = z.object({
    volunteers: z.array(volunteerSchema),
    hobbies: z.string().max(500), // comma-separated
    other: z.array(otherEntrySchema),
    achievements: z.array(z.object({ text: z.string().min(1, "Achievement cannot be empty").max(300) })),
});

type Step8Values = z.infer<typeof step8Schema>;

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";
const textareaCls =
    "w-full px-4 py-3 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all resize-none";

function newVolunteer() {
    return { id: `vol_${Date.now()}`, organization: "", role: "", startDate: "", endDate: "", description: "" };
}

function newOtherEntry() {
    return { label: "", value: "" };
}

function newAchievementEntry() {
    return { text: "" };
}

// ── Tag input (for hobbies) ───────────────────────────────────────────────────

const inputCls_tag =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";

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
                className={inputCls_tag}
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

// ── Page ─────────────────────────────────────────────────────────────────────

export default function Step8Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();

    const [hobbies, setHobbies] = useState<string[]>(formData.hobbies ?? []);
    const [showHobbies, setShowHobbies] = useState((formData.hobbies ?? []).length > 0);

    const {
        register,
        handleSubmit,
        control,
        watch,
        formState: { errors },
    } = useForm<Step8Values>({
        resolver: zodResolver(step8Schema),
        defaultValues: {
            volunteers: formData.volunteers.length > 0
                ? formData.volunteers.map((v) => ({
                    id: v.id || `vol_${Date.now()}`,
                    organization: v.organization,
                    role: v.role,
                    startDate: v.startDate ?? "",
                    endDate: v.endDate ?? "",
                    description: v.description ?? "",
                }))
                : [],
            hobbies: "",
            other: formData.other.length > 0
                ? formData.other.map((o) => ({ label: o.label, value: o.value }))
                : [],
            achievements: formData.achievements.length > 0
                ? formData.achievements.map((a) => ({ text: a }))
                : [],
        },
    });

    const {
        fields: volunteerFields,
        append: appendVolunteer,
        remove: removeVolunteer,
    } = useFieldArray({ control, name: "volunteers" });

    const {
        fields: otherFields,
        append: appendOther,
        remove: removeOther,
    } = useFieldArray({ control, name: "other" });

    const {
        fields: achievementFields,
        append: appendAchievement,
        remove: removeAchievement,
    } = useFieldArray({ control, name: "achievements" });

    // Auto-save volunteers/other to store on every change
    useEffect(() => {
        const sub = watch((values) => {
            const volunteers = (values.volunteers ?? []).filter(Boolean).map((v) => ({
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                id: (v as any).id ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                organization: (v as any).organization ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                role: (v as any).role ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                startDate: (v as any).startDate ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                endDate: (v as any).endDate ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                description: (v as any).description ?? "",
            }));
            const other = (values.other ?? []).filter(Boolean).map((o) => ({
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                label: (o as any).label ?? "",
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                value: (o as any).value ?? "",
            }));
            const achievements = (values.achievements ?? []).filter(Boolean).map(
                // eslint-disable-next-line @typescript-eslint/no-explicit-any
                (a) => (a as any).text ?? "",
            ).filter(Boolean);
            updateFormData({ volunteers, other, achievements });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    // Auto-save hobbies tags to store
    useEffect(() => {
        updateFormData({ hobbies });
    }, [hobbies, updateFormData]);

    function onSubmit(values: Step8Values) {
        const volunteers = values.volunteers.map((v) => ({
            id: v.id,
            organization: v.organization,
            role: v.role,
            startDate: v.startDate ?? "",
            endDate: v.endDate ?? "",
            description: v.description ?? "",
        }));
        const other = values.other.map((o) => ({ label: o.label, value: o.value }));
        const achievements = values.achievements.map((a) => a.text).filter(Boolean);
        updateFormData({ volunteers, hobbies, other, achievements });
        router.push("/create/steps/9");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={8}
                total={9}
                title="Extras"
                subtitle="Add volunteer work, hobbies, and other info. This step is optional — skip if not applicable."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-10">

                {/* ── Volunteer Work ── */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 mb-1">
                        <MaterialIcon name="volunteer_activism" size={20} className="text-primary" />
                        <h2 className="font-h2 text-base text-on-surface font-semibold">Volunteer Work</h2>
                    </div>

                    {volunteerFields.map((field, index) => (
                        <div
                            key={field.id}
                            className="bg-white border border-outline-variant/30 rounded-xl p-6 pr-16 shadow-sm relative"
                        >
                            <button
                                type="button"
                                onClick={() => removeVolunteer(index)}
                                className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                                aria-label="Remove volunteer entry"
                            >
                                <MaterialIcon name="delete" size={20} />
                            </button>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {/* Organization */}
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        Organization <span className="text-error">*</span>
                                    </label>
                                    <input
                                        {...register(`volunteers.${index}.organization`)}
                                        placeholder="e.g. Red Cross, Local Food Bank"
                                        className={inputCls}
                                    />
                                    {errors.volunteers?.[index]?.organization && (
                                        <p className="text-xs text-error">{errors.volunteers[index].organization?.message}</p>
                                    )}
                                </div>

                                {/* Role */}
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        Role / Title <span className="text-error">*</span>
                                    </label>
                                    <input
                                        {...register(`volunteers.${index}.role`)}
                                        placeholder="e.g. Team Coordinator, Tutor"
                                        className={inputCls}
                                    />
                                    {errors.volunteers?.[index]?.role && (
                                        <p className="text-xs text-error">{errors.volunteers[index].role?.message}</p>
                                    )}
                                </div>

                                {/* Start Date */}
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        Start Date (optional)
                                    </label>
                                    <input
                                        {...register(`volunteers.${index}.startDate`)}
                                        placeholder="e.g. 2021-06"
                                        className={inputCls}
                                    />
                                </div>

                                {/* End Date */}
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        End Date (optional)
                                    </label>
                                    <input
                                        {...register(`volunteers.${index}.endDate`)}
                                        placeholder="e.g. 2022-08 or Present"
                                        className={inputCls}
                                    />
                                </div>
                            </div>

                            {/* Description */}
                            <div className="flex flex-col gap-2 mt-4">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Description (optional)
                                </label>
                                <textarea
                                    {...register(`volunteers.${index}.description`)}
                                    placeholder="Briefly describe your contributions and impact…"
                                    rows={3}
                                    className={textareaCls}
                                />
                            </div>
                        </div>
                    ))}

                    <button
                        type="button"
                        onClick={() => appendVolunteer(newVolunteer())}
                        className="w-full py-5 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                    >
                        <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                        <span className="font-manrope font-semibold">
                            {volunteerFields.length === 0 ? "Add volunteer experience" : "Add another volunteer role"}
                        </span>
                    </button>
                </div>

                {/* ── Hobbies ── */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 mb-1">
                        <MaterialIcon name="interests" size={20} className="text-primary" />
                        <h2 className="font-h2 text-base text-on-surface font-semibold">Hobbies &amp; Interests</h2>
                    </div>

                    {showHobbies && (
                        <div className="bg-white border border-outline-variant/30 rounded-xl p-6 pr-16 shadow-sm relative">
                            <button
                                type="button"
                                onClick={() => { setShowHobbies(false); setHobbies([]); }}
                                className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                                aria-label="Remove hobbies"
                            >
                                <MaterialIcon name="delete" size={20} />
                            </button>
                            <label className="font-label-caps text-label-caps text-secondary uppercase block mb-2">
                                Hobbies &amp; Interests (optional)
                            </label>
                            <TagInput
                                tags={hobbies}
                                onChange={setHobbies}
                                placeholder="e.g. Photography, Hiking — press Enter or comma to add"
                            />
                        </div>
                    )}

                    {!showHobbies && (
                        <button
                            type="button"
                            onClick={() => setShowHobbies(true)}
                            className="w-full py-5 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                        >
                            <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                            <span className="font-manrope font-semibold">Add hobbies &amp; interests</span>
                        </button>
                    )}
                </div>

                {/* ── Other Info ── */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 mb-1">
                        <MaterialIcon name="info" size={20} className="text-primary" />
                        <h2 className="font-h2 text-base text-on-surface font-semibold">Other Information</h2>
                    </div>
                    <p className="text-sm text-secondary -mt-2">
                        Add custom key-value fields like Military Status, Driving License, Nationality, etc.
                    </p>

                    {otherFields.map((field, index) => (
                        <div
                            key={field.id}
                            className="bg-white border border-outline-variant/30 rounded-xl p-5 pr-16 shadow-sm relative"
                        >
                            <button
                                type="button"
                                onClick={() => removeOther(index)}
                                className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                                aria-label="Remove entry"
                            >
                                <MaterialIcon name="delete" size={20} />
                            </button>

                            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        Label <span className="text-error">*</span>
                                    </label>
                                    <input
                                        {...register(`other.${index}.label`)}
                                        placeholder="e.g. Military Status"
                                        className={inputCls}
                                    />
                                    {errors.other?.[index]?.label && (
                                        <p className="text-xs text-error">{errors.other[index].label?.message}</p>
                                    )}
                                </div>
                                <div className="flex flex-col gap-2">
                                    <label className="font-label-caps text-label-caps text-secondary uppercase">
                                        Value <span className="text-error">*</span>
                                    </label>
                                    <input
                                        {...register(`other.${index}.value`)}
                                        placeholder="e.g. Completed"
                                        className={inputCls}
                                    />
                                    {errors.other?.[index]?.value && (
                                        <p className="text-xs text-error">{errors.other[index].value?.message}</p>
                                    )}
                                </div>
                            </div>
                        </div>
                    ))}

                    <button
                        type="button"
                        onClick={() => appendOther(newOtherEntry())}
                        className="w-full py-5 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                    >
                        <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                        <span className="font-manrope font-semibold">
                            {otherFields.length === 0 ? "Add custom field" : "Add another field"}
                        </span>
                    </button>
                </div>

                {/* ── Key Achievements ── */}
                <div className="space-y-4">
                    <div className="flex items-center gap-2 mb-1">
                        <MaterialIcon name="emoji_events" size={20} className="text-primary" />
                        <h2 className="font-h2 text-base text-on-surface font-semibold">Key Achievements</h2>
                    </div>
                    <p className="text-sm text-secondary -mt-2">
                        Add notable career achievements shown as bullet points in select templates.
                    </p>

                    {achievementFields.map((field, index) => (
                        <div
                            key={field.id}
                            className="bg-white border border-outline-variant/30 rounded-xl p-5 pr-16 shadow-sm relative"
                        >
                            <button
                                type="button"
                                onClick={() => removeAchievement(index)}
                                className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                                aria-label="Remove achievement"
                            >
                                <MaterialIcon name="delete" size={20} />
                            </button>
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Achievement <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`achievements.${index}.text`)}
                                    placeholder="e.g. Grew ARR from $2M to $8M in 18 months"
                                    className={inputCls}
                                />
                                {errors.achievements?.[index]?.text && (
                                    <p className="text-xs text-error">{errors.achievements[index].text?.message}</p>
                                )}
                            </div>
                        </div>
                    ))}

                    <button
                        type="button"
                        onClick={() => appendAchievement(newAchievementEntry())}
                        className="w-full py-5 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                    >
                        <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                        <span className="font-manrope font-semibold">
                            {achievementFields.length === 0 ? "Add achievement" : "Add another achievement"}
                        </span>
                    </button>
                </div>

                {/* Pro tip */}
                <div className="flex items-start gap-4 p-4 rounded-xl bg-secondary-container/30">
                    <MaterialIcon name="lightbulb" size={20} className="text-secondary shrink-0 mt-0.5" />
                    <p className="font-body-sm text-sm text-secondary">
                        <strong>Pro Tip:</strong> Volunteer work shows initiative and values. Other fields like Military Status or Driving License can be important in certain regions or industries.
                    </p>
                </div>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/7")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg font-semibold text-secondary hover:bg-secondary-container transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-3 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-md"
                    >
                        {volunteerFields.length === 0 && otherFields.length === 0 && hobbies.length === 0 && achievementFields.length === 0
                            ? "Skip & Continue"
                            : "Save & Continue"}
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
