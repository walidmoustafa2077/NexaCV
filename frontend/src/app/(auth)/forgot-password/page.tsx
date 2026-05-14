"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import Link from "next/link";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import MaterialIcon from "@/components/shared/MaterialIcon";

const schema = z.object({
    email: z.string().min(1, "Email is required").email("Enter a valid email address"),
});
type FormValues = z.infer<typeof schema>;

export default function ForgotPasswordPage() {
    const [submitted, setSubmitted] = useState(false);

    const {
        register,
        handleSubmit,
        getValues,
        formState: { errors, isSubmitting },
    } = useForm<FormValues>({ resolver: zodResolver(schema) });

    function onSubmit(_values: FormValues) {
        // Stub — no backend endpoint yet
        setSubmitted(true);
    }

    return (
        <div className="w-full flex flex-col items-center">
            {/* Brand */}
            <div className="mb-12 text-center">
                <div className="flex items-center justify-center gap-2 mb-4">
                    <div className="w-10 h-10 bg-primary rounded-lg flex items-center justify-center">
                        <MaterialIcon name="architecture" className="text-on-primary" size={22} filled />
                    </div>
                    <span className="font-[Manrope] text-xl font-bold text-primary">NexaCV</span>
                </div>
            </div>

            {/* Card */}
            <div className="w-full max-w-[480px] bg-white p-8 lg:p-12 rounded-xl shadow-sm border border-outline-variant">
                {submitted ? (
                    /* Confirmation state */
                    <div className="text-center space-y-4">
                        <div className="flex justify-center">
                            <div className="w-14 h-14 rounded-full bg-primary/10 flex items-center justify-center">
                                <MaterialIcon name="mark_email_read" size={32} className="text-primary" filled />
                            </div>
                        </div>
                        <h1 className="font-[Manrope] text-[24px] font-bold text-on-surface">Check your email</h1>
                        <p className="text-sm text-on-surface-variant">
                            If <span className="font-medium text-on-surface">{getValues("email")}</span> is linked
                            to an account, you&apos;ll receive a reset link shortly.
                        </p>
                        <Link
                            href="/login"
                            className="inline-flex items-center gap-2 mt-4 text-sm text-primary hover:underline font-medium"
                        >
                            <MaterialIcon name="arrow_back" size={16} />
                            Back to Login
                        </Link>
                    </div>
                ) : (
                    <>
                        <header className="mb-10 text-center">
                            <h1 className="font-[Manrope] text-[30px] leading-[36px] font-bold tracking-tight text-on-surface mb-4">
                                Forgot Password
                            </h1>
                            <p className="text-[16px] leading-[24px] text-on-surface-variant">
                                Enter the email address associated with your account and we&apos;ll send you a secure
                                link to reset your password.
                            </p>
                        </header>

                        <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-6">
                            <div className="flex flex-col gap-1.5">
                                <Label
                                    htmlFor="fp-email"
                                    className="text-xs font-semibold text-on-surface-variant uppercase tracking-wider"
                                >
                                    Email Address
                                </Label>
                                <div className="relative">
                                    <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                                        <MaterialIcon name="mail" size={18} className="text-outline" />
                                    </span>
                                    <Input
                                        id="fp-email"
                                        type="email"
                                        placeholder="name@company.com"
                                        autoComplete="email"
                                        className="pl-9"
                                        aria-invalid={!!errors.email}
                                        {...register("email")}
                                    />
                                </div>
                                {errors.email && (
                                    <p className="text-xs text-error" role="alert">
                                        {errors.email.message}
                                    </p>
                                )}
                            </div>

                            <Button
                                type="submit"
                                disabled={isSubmitting}
                                className="w-full bg-primary hover:bg-primary-container text-on-primary font-semibold py-4 rounded-lg flex items-center justify-center gap-2"
                            >
                                <span>Send Reset Link</span>
                                <MaterialIcon name="send" size={18} />
                            </Button>
                        </form>

                        <div className="mt-8 text-center">
                            <Link
                                href="/login"
                                className="inline-flex items-center gap-2 text-sm text-primary hover:text-primary-container transition-colors font-medium"
                            >
                                <MaterialIcon name="arrow_back" size={16} />
                                Back to Login
                            </Link>
                        </div>
                    </>
                )}
            </div>
        </div>
    );
}
