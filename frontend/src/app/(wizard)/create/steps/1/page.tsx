"use client";

import { useEffect, useRef } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useWizardStore } from "@/store/wizardStore";
import MaterialIcon from "@/components/shared/MaterialIcon";

// ─── Shared wizard header/progress ───────────────────────────────────────────

export function WizardProgress({
    step,
    total,
    title,
    subtitle,
}: {
    step: number;
    total: number;
    title: string;
    subtitle: string;
}) {
    const pct = Math.round((step / total) * 100);
    return (
        <div className="mb-8">
            <div className="flex items-center justify-between mb-3">
                <span className="font-label-caps text-label-caps text-primary uppercase">
                    Step {step} of {total}
                </span>
                <span className="font-label-caps text-label-caps text-secondary">{pct}% Complete</span>
            </div>
            <div className="h-1.5 w-full bg-surface-container-highest rounded-full overflow-hidden">
                <div
                    className="h-full bg-primary rounded-full transition-all duration-500"
                    style={{ width: `${pct}%` }}
                />
            </div>
            <div className="mt-6">
                <h1 className="font-h1 text-h1 text-on-surface">{title}</h1>
                <p className="font-body-base text-body-base text-secondary mt-1">{subtitle}</p>
            </div>
        </div>
    );
}

// ─── Step 1 schema ────────────────────────────────────────────────────────────

const step1Schema = z.object({
    firstName: z.string().min(1, "First name is required").max(50),
    middleName: z.string().max(50).optional(),
    lastName: z.string().min(1, "Last name is required").max(50),
    jobTitle: z.string().max(100).optional(),
    email: z.string().min(1, "Email is required").email("Invalid email"),
    phone: z.string().min(1, "Phone is required").max(30),
    location: z.string().min(1, "Location is required").max(100),
    zipCode: z.string().max(20).optional(),
    dateOfBirth: z.string().optional(),
    linkedinUrl: z
        .string()
        .url("Must be a valid URL")
        .or(z.literal(""))
        .optional(),
    siteUrl: z
        .string()
        .url("Must be a valid URL")
        .or(z.literal(""))
        .optional(),
});

type Step1Values = z.infer<typeof step1Schema>;

// ─── Reusable field wrapper ───────────────────────────────────────────────────

function Field({
    label,
    error,
    required,
    children,
}: {
    label: string;
    error?: string;
    required?: boolean;
    children: React.ReactNode;
}) {
    return (
        <div className="flex flex-col gap-2">
            <label className="font-label-caps text-label-caps text-secondary uppercase">
                {label}
                {required && <span className="text-error ml-1">*</span>}
            </label>
            {children}
            {error && <p className="text-xs text-error">{error}</p>}
        </div>
    );
}

const inputCls =
    "w-full h-11 px-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";
const inputIconCls =
    "w-full h-11 pl-10 pr-4 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all";

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function Step1Page() {
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();
    const fileInputRef = useRef<HTMLInputElement>(null);

    function handlePhotoChange(e: React.ChangeEvent<HTMLInputElement>) {
        const file = e.target.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = () => {
            updateFormData({ photoUrl: reader.result as string });
        };
        reader.readAsDataURL(file);
    }

    function removePhoto() {
        updateFormData({ photoUrl: "" });
        if (fileInputRef.current) fileInputRef.current.value = "";
    }

    const {
        register,
        handleSubmit,
        formState: { errors },
        watch,
    } = useForm<Step1Values>({
        resolver: zodResolver(step1Schema),
        defaultValues: {
            firstName: formData.firstName,
            middleName: formData.middleName || "",
            lastName: formData.lastName,
            jobTitle: formData.jobTitle || "",
            email: formData.email,
            phone: formData.phone,
            location: formData.location,
            zipCode: formData.zipCode || "",
            dateOfBirth: formData.dateOfBirth || "",
            linkedinUrl: formData.linkedinUrl || "",
            siteUrl: formData.siteUrl || "",
        },
    });

    // Auto-save to store on every change so sidebar navigation doesn't lose data
    useEffect(() => {
        const sub = watch((values) => {
            updateFormData({
                firstName: values.firstName ?? "",
                middleName: values.middleName ?? "",
                lastName: values.lastName ?? "",
                jobTitle: values.jobTitle ?? "",
                email: values.email ?? "",
                phone: values.phone ?? "",
                location: values.location ?? "",
                zipCode: values.zipCode ?? "",
                dateOfBirth: values.dateOfBirth ?? "",
                linkedinUrl: values.linkedinUrl ?? "",
                siteUrl: values.siteUrl ?? "",
            });
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    function onSubmit(values: Step1Values) {
        updateFormData({
            firstName: values.firstName,
            middleName: values.middleName ?? "",
            lastName: values.lastName,
            jobTitle: values.jobTitle ?? "",
            email: values.email,
            phone: values.phone,
            location: values.location,
            zipCode: values.zipCode ?? "",
            dateOfBirth: values.dateOfBirth ?? "",
            linkedinUrl: values.linkedinUrl ?? "",
            siteUrl: values.siteUrl ?? "",
        });
        router.push("/create/steps/2");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto">
            <WizardProgress
                step={1}
                total={8}
                title="Let's start with the basics"
                subtitle="Provide your contact details so employers know how to reach you."
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate>
                <div className="bg-white border border-outline-variant/30 rounded-xl p-8 shadow-sm space-y-5">
                    {/* Profile Photo */}
                    <div className="flex items-center gap-5 pb-2 border-b border-outline-variant/20">
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept="image/*"
                            className="hidden"
                            onChange={handlePhotoChange}
                        />
                        <button
                            type="button"
                            onClick={() => fileInputRef.current?.click()}
                            className="relative w-20 h-20 rounded-xl border-2 border-dashed border-outline-variant bg-surface-container-low hover:border-primary hover:bg-primary/5 transition-all flex-shrink-0 overflow-hidden group"
                        >
                            {formData.photoUrl ? (
                                // eslint-disable-next-line @next/next/no-img-element
                                <img
                                    src={formData.photoUrl}
                                    alt="Profile"
                                    className="w-full h-full object-cover"
                                />
                            ) : (
                                <div className="w-full h-full flex flex-col items-center justify-center gap-0.5">
                                    <MaterialIcon name="add_a_photo" size={26} className="text-secondary group-hover:text-primary transition-colors" />
                                    <span className="text-[9px] font-bold uppercase tracking-widest text-secondary group-hover:text-primary transition-colors">
                                        Upload
                                    </span>
                                </div>
                            )}
                        </button>
                        <div className="flex-1">
                            <p className="font-semibold text-on-surface text-sm">Profile Photo</p>
                            <p className="text-xs text-secondary mt-0.5">
                                Recommended: Square image, at least 400×400px.
                            </p>
                            {formData.photoUrl && (
                                <button
                                    type="button"
                                    onClick={removePhoto}
                                    className="mt-2 flex items-center gap-1 text-xs text-error hover:opacity-80 transition-opacity"
                                >
                                    <MaterialIcon name="delete" size={14} />
                                    Remove photo
                                </button>
                            )}
                        </div>
                    </div>

                    {/* Name row */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <Field label="First Name" error={errors.firstName?.message} required>
                            <input {...register("firstName")} placeholder="e.g. Alex" className={inputCls} />
                        </Field>
                        <Field label="Last Name" error={errors.lastName?.message} required>
                            <input {...register("lastName")} placeholder="e.g. Morgan" className={inputCls} />
                        </Field>
                    </div>

                    <Field label="Middle Name" error={errors.middleName?.message}>
                        <input {...register("middleName")} placeholder="Optional" className={inputCls} />
                    </Field>

                    <Field label="Job Title" error={errors.jobTitle?.message}>
                        <input {...register("jobTitle")} placeholder="e.g. Senior Software Engineer" className={inputCls} />
                    </Field>

                    {/* Contact row */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <Field label="Email Address" error={errors.email?.message} required>
                            <div className="relative">
                                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                                    <MaterialIcon name="mail" size={18} className="text-outline" />
                                </span>
                                <input
                                    {...register("email")}
                                    type="email"
                                    placeholder="alex.morgan@example.com"
                                    className={inputIconCls}
                                />
                            </div>
                        </Field>
                        <Field label="Phone Number" error={errors.phone?.message} required>
                            <div className="relative">
                                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                                    <MaterialIcon name="call" size={18} className="text-outline" />
                                </span>
                                <input
                                    {...register("phone")}
                                    type="tel"
                                    placeholder="+1 (555) 000-0000"
                                    className={inputIconCls}
                                />
                            </div>
                        </Field>
                    </div>

                    {/* Location row */}
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <Field label="City / State" error={errors.location?.message} required>
                            <input
                                {...register("location")}
                                placeholder="San Francisco, CA"
                                className={inputCls}
                            />
                        </Field>
                        <Field label="Zip Code" error={errors.zipCode?.message}>
                            <input {...register("zipCode")} placeholder="94103" className={inputCls} />
                        </Field>
                    </div>

                    <Field label="Date of Birth" error={errors.dateOfBirth?.message}>
                        <input {...register("dateOfBirth")} type="date" className={inputCls} />
                    </Field>

                    {/* Online presence */}
                    <div className="space-y-4 pt-2">
                        <h3 className="font-manrope font-semibold text-on-surface">Online Presence</h3>
                        <Field label="LinkedIn URL" error={errors.linkedinUrl?.message}>
                            <div className="relative">
                                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                                    <MaterialIcon name="link" size={18} className="text-outline" />
                                </span>
                                <input
                                    {...register("linkedinUrl")}
                                    type="url"
                                    placeholder="https://linkedin.com/in/username"
                                    className={inputIconCls}
                                />
                            </div>
                        </Field>
                        <Field label="Portfolio / Website" error={errors.siteUrl?.message}>
                            <div className="relative">
                                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                                    <MaterialIcon name="language" size={18} className="text-outline" />
                                </span>
                                <input
                                    {...register("siteUrl")}
                                    type="url"
                                    placeholder="https://www.yoursite.com"
                                    className={inputIconCls}
                                />
                            </div>
                        </Field>
                    </div>
                </div>

                {/* Tips */}
                <div className="mt-6 grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="p-5 rounded-xl bg-secondary-container/30 border border-secondary-container flex gap-3">
                        <MaterialIcon name="lightbulb" size={22} className="text-secondary shrink-0" filled />
                        <div>
                            <h4 className="font-manrope font-bold text-on-secondary-container text-sm mb-1">
                                Expert Tip
                            </h4>
                            <p className="text-xs text-on-secondary-container/80">
                                Use keywords relevant to the roles you&apos;re applying for in your contact info.
                            </p>
                        </div>
                    </div>
                    <div className="p-5 rounded-xl bg-tertiary-fixed/30 border border-tertiary-fixed flex gap-3">
                        <MaterialIcon name="security" size={22} className="text-tertiary shrink-0" filled />
                        <div>
                            <h4 className="font-manrope font-bold text-on-tertiary-fixed-variant text-sm mb-1">
                                Privacy First
                            </h4>
                            <p className="text-xs text-on-tertiary-fixed-variant/80">
                                Your data is only used for your resume. We never share it with third parties.
                            </p>
                        </div>
                    </div>
                </div>

                {/* Navigation */}
                <div className="mt-8 flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/template")}
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
