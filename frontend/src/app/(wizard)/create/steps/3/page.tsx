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
    name: z.string().min(1, "Course name is required").max(200),
    provider: z.string().min(1, "Provider is required").max(200),
    date: z.string().min(1, "Completion date is required"),
    certificateUrl: z.string().url("Must be a valid URL").or(z.literal("")).optional(),
});

const step3Schema = z.object({
    courses: z.array(entrySchema),
});

type Step3Values = z.infer<typeof step3Schema>;

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";

function newEntry() {
    return {
        id: `course_${crypto.randomUUID()}`,

        name: "",
        provider: "",
        date: "",
        certificateUrl: "",
    };
}

export default function Step3Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();

    const {
        register,
        handleSubmit,
        control,
        watch,
        formState: { errors },
    } = useForm<Step3Values>({
        resolver: zodResolver(step3Schema),
        defaultValues: {
            courses:
                formData.courses.length > 0
                    ? formData.courses.map((c) => ({
                        id: c.id,
                        name: c.name,
                        provider: c.provider,
                        date: c.date,
                        certificateUrl: c.certificateUrl ?? "",
                    }))
                    : [],
        },
    });

    const { fields, append, remove } = useFieldArray({ control, name: "courses" });

    // Auto-save to store on every change so sidebar navigation doesn't lose data
    useEffect(() => {
        const sub = watch((values) => {
            if (!values.courses) return;
            updateFormData({
                courses: values.courses
                    .filter(Boolean)
                    .map((c: any) => ({
                        id: c.id ?? "",
                        name: c.name ?? "",
                        provider: c.provider ?? "",
                        date: c.date ?? "",
                        certificateUrl: c.certificateUrl ?? "",
                    })),
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step3Values) {
        updateFormData({
            courses: values.courses.map((c) => ({
                id: c.id,
                name: c.name,
                provider: c.provider,
                date: c.date,
                certificateUrl: c.certificateUrl ?? "",
            })),
        });
        router.push("/create/steps/4");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={3}
                total={6}
                title="Certifications & Coursework"
                subtitle="List your relevant professional development."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
                {fields.map((field, index) => (
                    <div
                        key={field.id}
                        className="bg-white border border-outline-variant/30 rounded-xl p-7 shadow-sm relative"
                    >
                        <button
                            type="button"
                            onClick={() => remove(index)}
                            className="absolute top-4 right-4 p-2 text-error hover:bg-error-container/20 rounded-full transition-all"
                            aria-label="Remove entry"
                        >
                            <MaterialIcon name="delete" size={20} />
                        </button>

                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                            {/* Course Name */}
                            <div className="md:col-span-2 flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Course / Certification Name <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`courses.${index}.name`)}
                                    placeholder="e.g. Google Data Analytics Professional Certificate"
                                    className={inputCls}
                                />
                                {errors.courses?.[index]?.name && (
                                    <p className="text-xs text-error">
                                        {errors.courses[index].name?.message}
                                    </p>
                                )}
                            </div>

                            {/* Provider */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Institution / Platform <span className="text-error">*</span>
                                </label>
                                <input
                                    {...register(`courses.${index}.provider`)}
                                    placeholder="e.g. Coursera, Udemy, Microsoft"
                                    className={inputCls}
                                />
                                {errors.courses?.[index]?.provider && (
                                    <p className="text-xs text-error">
                                        {errors.courses[index].provider?.message}
                                    </p>
                                )}
                            </div>

                            {/* Date */}
                            <div className="flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Completion Date <span className="text-error">*</span>
                                </label>
                                <Controller
                                    control={control}
                                    name={`courses.${index}.date`}
                                    render={({ field }) => (
                                        <MonthYearPicker
                                            value={field.value ?? ""}
                                            onChange={field.onChange}
                                        />
                                    )}
                                />
                                {errors.courses?.[index]?.date && (
                                    <p className="text-xs text-error">
                                        {errors.courses[index].date?.message}
                                    </p>
                                )}
                            </div>

                            {/* Certificate URL */}
                            <div className="md:col-span-2 flex flex-col gap-2">
                                <label className="font-label-caps text-label-caps text-secondary uppercase">
                                    Certificate URL (optional)
                                </label>
                                <input
                                    {...register(`courses.${index}.certificateUrl`)}
                                    placeholder="e.g. https://coursera.org/verify/..."
                                    className={inputCls}
                                />
                                {errors.courses?.[index]?.certificateUrl && (
                                    <p className="text-xs text-error">
                                        {errors.courses[index].certificateUrl?.message}
                                    </p>
                                )}
                            </div>
                        </div>
                    </div>
                ))}

                {/* Add button — always visible */}
                <button
                    type="button"
                    onClick={() => append(newEntry())}
                    className="w-full py-6 border-2 border-dashed border-outline-variant rounded-xl flex items-center justify-center gap-3 text-secondary hover:border-primary hover:text-primary hover:bg-primary/5 transition-all group"
                >
                    <MaterialIcon name="add_circle" size={22} className="group-hover:scale-110 transition-transform text-primary" />
                    <span className="font-manrope font-semibold">
                        {fields.length === 0 ? "Add a course or certification" : "Add another course"}
                    </span>
                </button>

                {/* Pro tip */}
                <div className="flex items-start gap-4 p-4 rounded-xl bg-secondary-container/30">
                    <MaterialIcon name="lightbulb" size={20} className="text-secondary shrink-0 mt-0.5" />
                    <p className="font-body-sm text-sm text-secondary">
                        <strong>Pro Tip:</strong> Only include certifications that are relevant to the role you&apos;re applying for. Focus on recent professional development within the last 5 years.
                    </p>
                </div>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/2")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg font-semibold text-secondary hover:bg-secondary-container transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-3 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-md"
                    >
                        Save &amp; Continue
                    </button>
                </div>
            </form>
        </div>
    );
}
