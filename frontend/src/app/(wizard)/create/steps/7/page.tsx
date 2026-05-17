"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useFieldArray, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useWizardStore } from "@/store/wizardStore";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";

// ── Schema ────────────────────────────────────────────────────────────────────

const LANGUAGE_LEVELS = [
    "Native",
    "Fluent",
    "Advanced",
    "Intermediate",
    "Basic",
] as const;

const entrySchema = z.object({
    language: z.string().min(1, "Language name is required").max(100),
    level: z.string().optional(),
});

const step7Schema = z.object({
    languages: z.array(entrySchema),
});

type Step7Values = z.infer<typeof step7Schema>;

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";
const selectCls =
    "w-full h-11 pl-3 pr-9 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all appearance-none cursor-pointer text-sm";

function newEntry() {
    return { language: "", level: "" };
}

// ── Page ─────────────────────────────────────────────────────────────────────

export default function Step7Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();

    const {
        register,
        handleSubmit,
        control,
        watch,
        formState: { errors },
    } = useForm<Step7Values>({
        resolver: zodResolver(step7Schema),
        defaultValues: {
            languages:
                formData.languages.length > 0
                    ? formData.languages.map((l) => ({
                        language: l.language,
                        level: l.level ?? "",
                    }))
                    : [],
        },
    });

    const { fields, append, remove } = useFieldArray({ control, name: "languages" });

    // Auto-save to store on every change
    useEffect(() => {
        const sub = watch((values) => {
            if (!values.languages) return;
            updateFormData({
                languages: values.languages
                    .filter(Boolean)
                    // eslint-disable-next-line @typescript-eslint/no-explicit-any
                    .map((l: any) => ({
                        language: l.language ?? "",
                        level: l.level ?? "",
                    })),
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step7Values) {
        updateFormData({
            languages: values.languages.map((l) => ({
                language: l.language,
                level: l.level ?? "",
            })),
        });
        router.push("/create/steps/8");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={7}
                total={8}
                title="Languages"
                subtitle="Add languages you speak. This step is optional — skip if not applicable."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
                {fields.map((field, index) => (
                    <div
                        key={field.id}
                        className="bg-white border border-outline-variant/30 rounded-xl p-6 shadow-sm relative"
                    >
                        <button
                            type="button"
                            onClick={() => remove(index)}
                            className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                            aria-label="Remove language"
                        >
                            <MaterialIcon name="delete" size={20} />
                        </button>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Language */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Language <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`languages.${index}.language`)}
                                    placeholder="e.g. English, Arabic, French"
                                    className={inputCls}
                                />
                                {errors.languages?.[index]?.language && (
                                    <p className="text-xs text-error">
                                        {errors.languages[index].language?.message}
                                    </p>
                                )}
                            </div>

                            {/* Level */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Proficiency Level (optional)
                                </label>
                                <div className="relative">
                                    <select
                                        {...register(`languages.${index}.level`)}
                                        className={selectCls}
                                    >
                                        <option value="">Select level</option>
                                        {LANGUAGE_LEVELS.map((lvl) => (
                                            <option key={lvl} value={lvl}>
                                                {lvl}
                                            </option>
                                        ))}
                                    </select>
                                    <span className="pointer-events-none absolute inset-y-0 right-2.5 flex items-center">
                                        <MaterialIcon name="expand_more" size={18} className="text-secondary" />
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>
                ))}

                {/* Add button */}
                <button
                    type="button"
                    onClick={() => append(newEntry())}
                    className="w-full py-6 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                >
                    <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                    <span className="font-manrope font-semibold">
                        {fields.length === 0 ? "Add a language" : "Add another language"}
                    </span>
                </button>

                {/* Pro tip */}
                <div className="flex items-start gap-4 p-4 rounded-xl bg-secondary-container/30">
                    <MaterialIcon name="lightbulb" size={20} className="text-secondary shrink-0 mt-0.5" />
                    <p className="font-body-sm text-sm text-secondary">
                        <strong>Pro Tip:</strong> Only include languages you can confidently use in a professional setting. Multilingual candidates often stand out in global or diverse teams.
                    </p>
                </div>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/6")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg font-semibold text-secondary hover:bg-secondary-container transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-3 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-md"
                    >
                        {fields.length === 0 ? "Skip & Continue" : "Save & Continue"}
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
