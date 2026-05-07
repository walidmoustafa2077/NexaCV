"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { useUser } from "@/hooks/useUser";
import { updateMe } from "@/lib/api/users";
import { queryKeys } from "@/lib/query/keys";
import { ValidationError } from "@/lib/api/client";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { Skeleton } from "@/components/shared/SkeletonCard";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

// ─── Schema ───────────────────────────────────────────────────────────────────
const profileSchema = z
    .object({
        firstName: z.string().min(1, "First name is required").max(50),
        lastName: z.string().min(1, "Last name is required").max(50),
        username: z
            .string()
            .min(3, "Username must be at least 3 characters")
            .max(30)
            .regex(/^[a-zA-Z0-9_]+$/, "Only letters, numbers, and underscores"),
        newPassword: z
            .string()
            .optional()
            .refine((v) => !v || v.length >= 8, "Password must be at least 8 characters"),
        confirmPassword: z.string().optional(),
    })
    .refine(
        (data) =>
            !data.newPassword || data.newPassword === data.confirmPassword,
        { message: "Passwords do not match", path: ["confirmPassword"] },
    );

type ProfileFormValues = z.infer<typeof profileSchema>;

// ─── Skeleton ─────────────────────────────────────────────────────────────────
function SettingsSkeleton() {
    return (
        <div className="animate-pulse space-y-6">
            <Skeleton className="h-8 w-48 rounded" />
            <div className="bg-white rounded-xl p-6 shadow-sm space-y-4">
                <Skeleton className="h-5 w-32 rounded" />
                <div className="grid grid-cols-2 gap-4">
                    {[1, 2, 3, 4].map((i) => (
                        <div key={i} className="space-y-2">
                            <Skeleton className="h-3 w-20 rounded" />
                            <Skeleton className="h-10 w-full rounded-lg" />
                        </div>
                    ))}
                </div>
            </div>
        </div>
    );
}

// ─── Field wrapper ─────────────────────────────────────────────────────────────
function Field({
    label,
    error,
    children,
}: {
    label: string;
    error?: string;
    children: React.ReactNode;
}) {
    return (
        <div className="space-y-1.5">
            <Label className="text-xs font-semibold text-secondary uppercase tracking-wide">
                {label}
            </Label>
            {children}
            {error && <p className="text-xs text-error">{error}</p>}
        </div>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export default function SettingsPage() {
    const { data: user, isLoading } = useUser();
    const queryClient = useQueryClient();

    const {
        register,
        handleSubmit,
        reset,
        setError,
        formState: { errors, isDirty, isSubmitting },
    } = useForm<ProfileFormValues>({
        resolver: zodResolver(profileSchema),
        defaultValues: {
            firstName: "",
            lastName: "",
            username: "",
            newPassword: "",
            confirmPassword: "",
        },
    });

    // Populate form when user data loads
    useEffect(() => {
        if (user) {
            reset({
                firstName: user.firstName ?? "",
                lastName: user.lastName ?? "",
                username: user.username ?? "",
                newPassword: "",
                confirmPassword: "",
            });
        }
    }, [user, reset]);

    const { mutate } = useMutation({
        mutationFn: updateMe,
        onSuccess: (updated) => {
            toast.success("Profile updated successfully!");
            queryClient.setQueryData(queryKeys.user(), updated);
            reset({
                firstName: updated.firstName ?? "",
                lastName: updated.lastName ?? "",
                username: updated.username ?? "",
                newPassword: "",
                confirmPassword: "",
            });
        },
        onError: (err: Error) => {
            if (err instanceof ValidationError) {
                err.details.forEach(({ field, message }) => {
                    const key = field as keyof ProfileFormValues;
                    setError(key, { message });
                });
            } else {
                const msg = err instanceof Error ? err.message : null;
                toast.error(msg || "Failed to update profile.");
            }
        },
    });

    function onSubmit(values: ProfileFormValues) {
        mutate({
            firstName: values.firstName || null,
            lastName: values.lastName || null,
            username: values.username || null,
            password: values.newPassword || null,
        });
    }

    if (isLoading) {
        return (
            <div className="px-8 py-10 max-w-2xl mx-auto">
                <SettingsSkeleton />
            </div>
        );
    }

    return (
        <div className="px-8 py-10 max-w-2xl mx-auto space-y-8">
            {/* Header */}
            <div>
                <h1 className="font-bold text-2xl text-on-surface">Account Settings</h1>
                <p className="text-secondary text-sm mt-1">
                    Manage your profile and security preferences.
                </p>
            </div>

            {/* Profile card */}
            <div className="bg-white border border-outline-variant/30 rounded-xl shadow-sm overflow-hidden">
                <div className="flex items-center gap-3 px-6 py-4 border-b border-outline-variant/20">
                    <MaterialIcon name="manage_accounts" size={20} className="text-primary" />
                    <h2 className="font-semibold text-on-surface">Profile Information</h2>
                </div>

                {/* Avatar / identity summary */}
                <div className="px-6 pt-5 pb-2 flex items-center gap-4">
                    <div className="w-14 h-14 rounded-full bg-primary-container flex items-center justify-center shrink-0">
                        <span className="text-on-primary text-xl font-black">
                            {(user?.firstName?.[0] ?? user?.email?.[0] ?? "?").toUpperCase()}
                        </span>
                    </div>
                    <div>
                        <p className="font-bold text-on-surface">
                            {user?.firstName
                                ? `${user.firstName} ${user.lastName ?? ""}`
                                : user?.username ?? "—"}
                        </p>
                        <p className="text-sm text-secondary">{user?.email}</p>
                    </div>
                </div>

                <form onSubmit={handleSubmit(onSubmit)} className="px-6 pb-6 space-y-5 pt-4">
                    <div className="grid grid-cols-2 gap-4">
                        <Field label="First Name" error={errors.firstName?.message}>
                            <Input
                                {...register("firstName")}
                                placeholder="John"
                                className="h-10"
                            />
                        </Field>
                        <Field label="Last Name" error={errors.lastName?.message}>
                            <Input
                                {...register("lastName")}
                                placeholder="Doe"
                                className="h-10"
                            />
                        </Field>
                    </div>

                    <Field label="Username" error={errors.username?.message}>
                        <div className="relative">
                            <span className="absolute left-3 top-1/2 -translate-y-1/2 flex items-center pointer-events-none text-secondary text-sm leading-none">@</span>
                            <Input
                                {...register("username")}
                                placeholder="johndoe"
                                className="h-10 pl-7"
                            />
                        </div>
                    </Field>

                    <Field label="Email">
                        <Input
                            value={user?.email ?? ""}
                            disabled
                            className="h-10 opacity-60 cursor-not-allowed"
                        />
                        <p className="text-[11px] text-secondary">Email cannot be changed.</p>
                    </Field>

                    <div className="border-t border-outline-variant/20 pt-4 space-y-4">
                        <p className="text-xs font-bold text-secondary uppercase tracking-wide flex items-center gap-2">
                            <MaterialIcon name="lock" size={14} />
                            Change Password
                            <span className="normal-case font-normal tracking-normal text-secondary/70">— leave blank to keep current</span>
                        </p>
                        <div className="grid grid-cols-2 gap-4">
                            <Field label="New Password" error={errors.newPassword?.message}>
                                <Input
                                    {...register("newPassword")}
                                    type="password"
                                    placeholder="Min 8 characters"
                                    className="h-10"
                                />
                            </Field>
                            <Field label="Confirm Password" error={errors.confirmPassword?.message}>
                                <Input
                                    {...register("confirmPassword")}
                                    type="password"
                                    placeholder="Repeat password"
                                    className="h-10"
                                />
                            </Field>
                        </div>
                    </div>

                    <div className="flex items-center justify-end gap-3 pt-2">
                        <button
                            type="button"
                            onClick={() =>
                                reset({
                                    firstName: user?.firstName ?? "",
                                    lastName: user?.lastName ?? "",
                                    username: user?.username ?? "",
                                    newPassword: "",
                                    confirmPassword: "",
                                })
                            }
                            disabled={!isDirty || isSubmitting}
                            className="px-4 py-2 text-sm text-secondary hover:text-on-surface disabled:opacity-40 transition-colors"
                        >
                            Reset
                        </button>
                        <button
                            type="submit"
                            disabled={!isDirty || isSubmitting}
                            className="flex items-center gap-2 px-6 py-2 bg-primary text-on-primary rounded-xl font-semibold text-sm shadow-sm hover:opacity-90 disabled:opacity-50 transition-all"
                        >
                            {isSubmitting ? (
                                <div className="w-3.5 h-3.5 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                            ) : (
                                <MaterialIcon name="save" size={16} />
                            )}
                            Save Changes
                        </button>
                    </div>
                </form>
            </div>

            {/* Account info */}
            <div className="bg-white border border-outline-variant/30 rounded-xl shadow-sm overflow-hidden">
                <div className="flex items-center gap-3 px-6 py-4 border-b border-outline-variant/20">
                    <MaterialIcon name="info" size={20} className="text-secondary" />
                    <h2 className="font-semibold text-on-surface">Account Details</h2>
                </div>
                <div className="px-6 py-5 space-y-3 text-sm">
                    <div className="flex justify-between">
                        <span className="text-secondary">Member since</span>
                        <span className="text-on-surface font-medium">
                            {user?.createdAt
                                ? new Date(user.createdAt).toLocaleDateString(undefined, {
                                    year: "numeric",
                                    month: "long",
                                    day: "numeric",
                                })
                                : "—"}
                        </span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-secondary">Last login</span>
                        <span className="text-on-surface font-medium">
                            {user?.lastLogin
                                ? new Date(user.lastLogin).toLocaleDateString(undefined, {
                                    year: "numeric",
                                    month: "short",
                                    day: "numeric",
                                })
                                : "—"}
                        </span>
                    </div>
                    <div className="flex justify-between">
                        <span className="text-secondary">User ID</span>
                        <span className="text-on-surface font-mono text-xs">{user?.id}</span>
                    </div>
                </div>
            </div>
        </div>
    );
}
