"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import Link from "next/link";
import { useState } from "react";

import { loginSchema, type LoginFormValues } from "@/lib/schemas/auth.schemas";
import { login } from "@/lib/api/auth";
import { useAuthStore } from "@/store/authStore";
import { ApiError, ValidationError } from "@/lib/api/client";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import MaterialIcon from "@/components/shared/MaterialIcon";

export default function LoginForm() {
    const router = useRouter();
    const setAuth = useAuthStore((s) => s.setAuth);
    const [showPassword, setShowPassword] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting },
    } = useForm<LoginFormValues>({
        resolver: zodResolver(loginSchema),
    });

    async function onSubmit(values: LoginFormValues) {
        try {
            const res = await login(values);
            setAuth(res.accessToken, res.refreshToken, res.userId);
            router.push("/dashboard");
        } catch (err) {
            if (err instanceof ValidationError) {
                toast.error("Please check your input and try again.");
            } else if (err instanceof ApiError && err.status === 401) {
                toast.error("Invalid email or password.");
            } else {
                const msg = err instanceof Error ? err.message : null;
                toast.error(msg || "Something went wrong. Please try again.");
            }
        }
    }

    return (
        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
            {/* Email or Username */}
            <div className="flex flex-col gap-1.5">
                <Label
                    htmlFor="emailOrUsername"
                    className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider"
                >
                    Email or Username
                </Label>
                <Input
                    id="emailOrUsername"
                    type="text"
                    placeholder="name@company.com or username"
                    autoComplete="username"
                    className="bg-surface-container-lowest"
                    aria-invalid={!!errors.emailOrUsername}
                    {...register("emailOrUsername")}
                />
                {errors.emailOrUsername && (
                    <p className="text-xs text-error" role="alert">
                        {errors.emailOrUsername.message}
                    </p>
                )}
            </div>

            {/* Password */}
            <div className="flex flex-col gap-1.5">
                <div className="flex items-center justify-between">
                    <Label
                        htmlFor="password"
                        className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider"
                    >
                        Password
                    </Label>
                    <Link
                        href="/forgot-password"
                        className="text-sm text-primary hover:underline font-medium"
                    >
                        Forgot password?
                    </Link>
                </div>
                <div className="relative">
                    <Input
                        id="password"
                        type={showPassword ? "text" : "password"}
                        placeholder="••••••••"
                        autoComplete="current-password"
                        className="bg-surface-container-lowest pr-10"
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
                        <MaterialIcon name={showPassword ? "visibility_off" : "visibility"} size={20} className="leading-none" />
                    </button>
                </div>
                {errors.password && (
                    <p className="text-xs text-error" role="alert">
                        {errors.password.message}
                    </p>
                )}
            </div>

            <Button
                type="submit"
                disabled={isSubmitting}
                className="w-full bg-primary hover:bg-primary-container text-on-primary font-semibold py-3 rounded-lg"
            >
                {isSubmitting ? "Signing in…" : "Sign In"}
            </Button>
        </form>
    );
}
