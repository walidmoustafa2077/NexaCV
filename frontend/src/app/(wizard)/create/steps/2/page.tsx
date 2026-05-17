"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useFieldArray, useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useWizardStore } from "@/store/wizardStore";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { MonthYearPicker } from "@/components/shared/MonthYearPicker";

const entrySchema = z.object({
    id: z.string(),
    institution: z.string().min(1, "Institution is required").max(200),
    degree: z.string().min(1, "Degree is required").max(200),
    fieldOfStudy: z.string().min(1, "Field of study is required").max(200),
    grade: z.string().max(50).optional(),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().optional(),
    isCurrentlyEnrolled: z.boolean().optional(),
});

const step2Schema = z.object({
    education: z.array(entrySchema).min(1, "Add at least one education entry"),
});

type Step2Values = z.infer<typeof step2Schema>;

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";

function newEntry() {
    return {
        id: `edu_${crypto.randomUUID()}`,

        institution: "",
        degree: "",
        fieldOfStudy: "",
        grade: "",
        startDate: "",
        endDate: "",
        isCurrentlyEnrolled: false,
    };
}

export default function Step2Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();

    const {
        register,
        handleSubmit,
        control,
        watch,
        formState: { errors },
    } = useForm<Step2Values>({
        resolver: zodResolver(step2Schema),
        defaultValues: {
            education:
                formData.education.length > 0
                    ? formData.education.map((e) => ({
                        id: e.id,
                        institution: e.institution,
                        degree: e.degree,
                        fieldOfStudy: e.fieldOfStudy,
                        grade: e.grade ?? "",
                        startDate: e.startDate,
                        endDate: e.endDate,
                        isCurrentlyEnrolled: !e.endDate,
                    }))
                    : [newEntry()],
        },
    });

    const { fields, append, remove } = useFieldArray({ control, name: "education", keyName: "_id" });

    // Auto-save to store on every change so sidebar navigation doesn't lose data
    useEffect(() => {
        const sub = watch((values) => {
            if (!values.education) return;
            updateFormData({
                education: values.education
                    .filter(Boolean)
                    .map((e: any) => ({
                        id: e.id ?? "",
                        institution: e.institution ?? "",
                        degree: e.degree ?? "",
                        fieldOfStudy: e.fieldOfStudy ?? "",
                        grade: e.grade ?? "",
                        startDate: e.startDate ?? "",
                        endDate: e.isCurrentlyEnrolled ? "" : (e.endDate ?? ""),
                    })),
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step2Values) {
        updateFormData({
            education: values.education.map((e) => ({
                id: e.id,
                institution: e.institution,
                degree: e.degree,
                fieldOfStudy: e.fieldOfStudy,
                grade: e.grade ?? "",
                startDate: e.startDate,
                endDate: e.isCurrentlyEnrolled ? "" : (e.endDate ?? ""),
            })),
        });
        router.push("/create/steps/3");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={2}
                total={8}
                title="Education Background"
                subtitle="List your academic achievements and qualifications to build a strong profile."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
                {fields.map((field, index) => (
                    <div
                        key={field._id}
                        className="bg-white border border-outline-variant/30 rounded-xl p-7 shadow-sm relative"
                    >
                        {fields.length > 1 && (
                            <button
                                type="button"
                                onClick={() => remove(index)}
                                className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                                aria-label="Remove entry"
                            >
                                <MaterialIcon name="delete" size={20} />
                            </button>
                        )}

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Institution */}
                            <div className="md:col-span-2 flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Institution Name <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`education.${index}.institution`)}
                                    placeholder="e.g. Stanford University"
                                    className={inputCls}
                                />
                                {errors.education?.[index]?.institution && (
                                    <p className="text-xs text-error">
                                        {errors.education[index].institution?.message}
                                    </p>
                                )}
                            </div>

                            {/* Degree */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Degree <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`education.${index}.degree`)}
                                    placeholder="e.g. Bachelor of Science"
                                    className={inputCls}
                                />
                                {errors.education?.[index]?.degree && (
                                    <p className="text-xs text-error">
                                        {errors.education[index].degree?.message}
                                    </p>
                                )}
                            </div>

                            {/* Field of Study */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Field of Study <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`education.${index}.fieldOfStudy`)}
                                    placeholder="e.g. Computer Science"
                                    className={inputCls}
                                />
                                {errors.education?.[index]?.fieldOfStudy && (
                                    <p className="text-xs text-error">
                                        {errors.education[index].fieldOfStudy?.message}
                                    </p>
                                )}
                            </div>

                            {/* Start / End date */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Start Date <span className="text-error">*</span>
                                </label>
                                <Controller
                                    control={control}
                                    name={`education.${index}.startDate`}
                                    render={({ field }) => (
                                        <MonthYearPicker
                                            value={field.value ?? ""}
                                            onChange={field.onChange}
                                        />
                                    )}
                                />
                                {errors.education?.[index]?.startDate && (
                                    <p className="text-xs text-error">
                                        {errors.education[index].startDate?.message}
                                    </p>
                                )}
                            </div>

                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Graduation Date
                                </label>
                                <Controller
                                    control={control}
                                    name={`education.${index}.endDate`}
                                    render={({ field: endField }) => (
                                        <Controller
                                            control={control}
                                            name={`education.${index}.isCurrentlyEnrolled`}
                                            render={({ field: enrolledField }) => (
                                                <MonthYearPicker
                                                    value={endField.value ?? ""}
                                                    onChange={endField.onChange}
                                                    allowPresent
                                                    isPresent={!!enrolledField.value}
                                                    onPresentChange={(v) => {
                                                        enrolledField.onChange(v);
                                                        if (v) endField.onChange("");
                                                    }}
                                                />
                                            )}
                                        />
                                    )}
                                />
                                {errors.education?.[index]?.endDate && (
                                    <p className="text-xs text-error">
                                        {errors.education[index].endDate?.message}
                                    </p>
                                )}
                            </div>

                            {/* Grade (optional) */}
                            <div className="md:col-span-2 flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Grade / GPA (optional)
                                </label>
                                <input
                                    {...register(`education.${index}.grade`)}
                                    placeholder="e.g. 3.8 / 4.0 or First Class Honors"
                                    className={inputCls}
                                />
                            </div>
                        </div>
                    </div>
                ))}

                {/* Add entry */}
                <button
                    type="button"
                    onClick={() => append(newEntry())}
                    className="w-full py-5 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all"
                >
                    <MaterialIcon name="add_circle" size={22} />
                    <span className="font-manrope font-semibold">Add another education entry</span>
                </button>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/1")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg border border-outline-variant text-secondary font-semibold hover:bg-surface-container transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-2.5 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-sm"
                    >
                        Save & Continue
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
