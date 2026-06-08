"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { useState } from "react";

import { registerSchema, type RegisterFormValues } from "@/lib/schemas/auth.schemas";
import { register as registerUser } from "@/lib/api/auth";
import { useAuthStore } from "@/store/authStore";
import { ApiError, ValidationError } from "@/lib/api/client";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import MaterialIcon from "@/components/shared/MaterialIcon";

function getPasswordStrength(password: string): { score: number; label: string; color: string } {
    if (!password) return { score: 0, label: "", color: "" };
    let score = 0;
    if (password.length >= 8) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;
    const map = [
        { label: "Weak", color: "bg-error" },
        { label: "Weak", color: "bg-error" },
        { label: "Medium", color: "bg-tertiary" },
        { label: "Strong", color: "bg-primary" },
        { label: "Very strong", color: "bg-primary" },
    ];
    return { score, ...map[score] };
}

export default function RegisterForm() {
    const router = useRouter();
    const setAuth = useAuthStore((s) => s.setAuth);
    const [showPassword, setShowPassword] = useState(false);

    const {
        register,
        handleSubmit,
        watch,
        setError,
        formState: { errors, isSubmitting },
    } = useForm<RegisterFormValues>({
        resolver: zodResolver(registerSchema),
    });

    const passwordValue = watch("password", "");
    const strength = getPasswordStrength(passwordValue);

    async function onSubmit(values: RegisterFormValues) {
        try {
            const res = await registerUser(values);
            setAuth(res.accessToken, res.refreshToken, res.userId);
            router.push("/create/template");
        } catch (err) {
            if (err instanceof ValidationError) {
                err.details?.forEach((d) => {
                    const field = d.field.split(".").pop()?.toLowerCase();
                    if (field === "email") setError("email", { message: d.message });
                    else if (field === "password") setError("password", { message: d.message });
                    else toast.error(d.message);
                });
            } else if (err instanceof ApiError && err.status === 409) {
                toast.error(err.message);
            } else {
                const msg = err instanceof Error ? err.message : null;
                toast.error(msg || "Something went wrong. Please try again.");
            }
        }
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-5">
            {/* First + Last name */}
            <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="firstName" className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                        First Name
                    </Label>
                    <Input
                        id="firstName"
                        placeholder="John"
                        autoComplete="given-name"
                        className="bg-surface-container-lowest"
                        aria-invalid={!!errors.firstName}
                        {...register("firstName")}
                    />
                    {errors.firstName && <p className="text-xs text-error" role="alert">{errors.firstName.message}</p>}
                </div>
                <div className="flex flex-col gap-1.5">
                    <Label htmlFor="lastName" className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                        Last Name
                    </Label>
                    <Input
                        id="lastName"
                        placeholder="Doe"
                        autoComplete="family-name"
                        className="bg-surface-container-lowest"
                        aria-invalid={!!errors.lastName}
                        {...register("lastName")}
                    />
                    {errors.lastName && <p className="text-xs text-error" role="alert">{errors.lastName.message}</p>}
                </div>
            </div>

            {/* Email */}
            <div className="flex flex-col gap-1.5">
                <Label htmlFor="reg-email" className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                    Email Address
                </Label>
                <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 flex items-center pointer-events-none">
                        <MaterialIcon name="mail" size={18} className="text-outline leading-none" />
                    </span>
                    <Input
                        id="reg-email"
                        type="email"
                        placeholder="john@example.com"
                        autoComplete="email"
                        className="bg-surface-container-lowest pl-9"
                        aria-invalid={!!errors.email}
                        {...register("email")}
                    />
                </div>
                {errors.email && <p className="text-xs text-error" role="alert">{errors.email.message}</p>}
            </div>

            {/* Username */}
            <div className="flex flex-col gap-1.5">
                <Label htmlFor="username" className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                    Username
                </Label>
                <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 flex items-center pointer-events-none">
                        <MaterialIcon name="person" size={18} className="text-outline leading-none" />
                    </span>
                    <Input
                        id="username"
                        type="text"
                        placeholder="john_doe"
                        autoComplete="username"
                        className="bg-surface-container-lowest pl-9"
                        aria-invalid={!!errors.username}
                        {...register("username")}
                    />
                </div>
                {errors.username && <p className="text-xs text-error" role="alert">{errors.username.message}</p>}
            </div>

            {/* Password */}
            <div className="flex flex-col gap-1.5">
                <Label htmlFor="reg-password" className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                    Password
                </Label>
                <div className="relative">
                    <span className="absolute left-3 top-1/2 -translate-y-1/2 flex items-center pointer-events-none">
                        <MaterialIcon name="lock" size={18} className="text-outline leading-none" />
                    </span>
                    <Input
                        id="reg-password"
                        type={showPassword ? "text" : "password"}
                        placeholder="••••••••"
                        autoComplete="new-password"
                        className="bg-surface-container-lowest pl-9 pr-10"
                        aria-invalid={!!errors.password}
                        {...register("password")}
                    />
                    <button
                        type="button"
                        onClick={() => setShowPassword((v) => !v)}
                        className="absolute right-3 top-1/2 -translate-y-1/2 flex items-center justify-center text-on-surface-variant hover:text-on-surface"
                        tabIndex={-1}
                        aria-label={showPassword ? "Hide password" : "Show password"}
                    >
                        <MaterialIcon name={showPassword ? "visibility_off" : "visibility"} size={18} className="leading-none" />
                    </button>
                </div>
                {/* Strength indicator */}
                {passwordValue && (
                    <div className="pt-1 space-y-1">
                        <div className="flex gap-1 h-1">
                            {[1, 2, 3, 4].map((i) => (
                                <div
                                    key={i}
                                    className={`flex-1 rounded-full transition-colors ${i <= strength.score ? strength.color : "bg-surface-variant"
                                        }`}
                                />
                            ))}
                        </div>
                        <p className="text-[11px] text-on-surface-variant font-medium">
                            Strength:{" "}
                            <span
                                className={
                                    strength.score <= 1
                                        ? "text-error"
                                        : strength.score === 2
                                            ? "text-tertiary"
                                            : "text-primary"
                                }
                            >
                                {strength.label}
                            </span>
                        </p>
                    </div>
                )}
                {errors.password && <p className="text-xs text-error" role="alert">{errors.password.message}</p>}
            </div>

            <Button
                type="submit"
                disabled={isSubmitting}
                className="w-full bg-primary hover:bg-primary-container text-on-primary font-semibold py-3 rounded-lg flex items-center justify-center gap-2"
            >
                {isSubmitting ? "Creating account…" : "Create Account"}
                {!isSubmitting && <MaterialIcon name="arrow_forward" size={18} />}
            </Button>
        </form>
    );
}
