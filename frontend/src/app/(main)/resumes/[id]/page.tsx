"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { toast } from "sonner";
import { useResumeDetail } from "@/hooks/useResumeDetail";
import { checkout } from "@/lib/api/transactions";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { ResumeStatusBadge } from "@/components/resume/ResumeStatusBadge";
import { ResumeHtmlPreview } from "@/components/resume/ResumeHtmlPreview";
import { Skeleton } from "@/components/shared/SkeletonCard";
import { useWizardStore } from "@/store/wizardStore";
import type {
    ResumeDetailDto,
} from "@/types/api.types";

// ─── Skeleton ─────────────────────────────────────────────────────────────────
function ResumeDetailSkeleton() {
    return (
        <div className="animate-pulse space-y-6">
            <div className="flex items-center gap-4">
                <Skeleton className="w-8 h-8 rounded-lg" />
                <Skeleton className="h-7 w-48 rounded" />
                <Skeleton className="h-5 w-20 rounded-full" />
            </div>
            {[1, 2, 3].map((i) => (
                <div key={i} className="bg-white rounded-xl p-6 shadow-sm space-y-3">
                    <Skeleton className="h-5 w-32 rounded" />
                    <Skeleton className="h-3 w-full rounded" />
                    <Skeleton className="h-3 w-4/5 rounded" />
                    <Skeleton className="h-3 w-3/5 rounded" />
                </div>
            ))}
        </div>
    );
}

// ─── Checkout panel ───────────────────────────────────────────────────────────
function CheckoutPanel({ resume }: { resume: ResumeDetailDto }) {
    const router = useRouter();
    const [currency, setCurrency] = useState<"USD" | "EGP">("USD");
    const [isPending, setIsPending] = useState(false);

    async function handleCheckout() {
        setIsPending(true);
        try {
            const res = await checkout({ resumeId: resume.id, currency });
            router.push(res.paymentUrl || `/resumes/${resume.id}/payment?tx=${res.transactionId}`);
        } catch (err: unknown) {
            const msg = err instanceof Error ? err.message : "Checkout failed.";
            toast.error(msg);
        } finally {
            setIsPending(false);
        }
    }

    return (
        <div className="bg-gradient-to-br from-primary/5 to-primary-fixed/30 border border-primary/20 rounded-xl p-6 space-y-4">
            <div className="flex items-center gap-3">
                <div className="w-10 h-10 rounded-xl bg-primary flex items-center justify-center">
                    <MaterialIcon name="download" size={20} className="text-on-primary" filled />
                </div>
                <div>
                    <h3 className="font-bold text-on-surface">Download PDF</h3>
                    <p className="text-xs text-secondary">One-time payment to unlock your resume</p>
                </div>
            </div>

            <div className="flex items-center gap-3">
                <span className="text-sm text-secondary font-medium">Currency:</span>
                <div className="flex gap-2">
                    {(["USD", "EGP"] as const).map((c) => (
                        <button
                            key={c}
                            onClick={() => setCurrency(c)}
                            className={`px-3 py-1 rounded-full text-xs font-bold border transition-colors ${currency === c
                                ? "bg-primary text-on-primary border-primary"
                                : "border-outline-variant text-secondary hover:border-primary/50"
                                }`}
                        >
                            {c}
                        </button>
                    ))}
                </div>
            </div>

            <button
                onClick={handleCheckout}
                disabled={isPending}
                className="w-full flex items-center justify-center gap-2 py-3 bg-primary text-on-primary rounded-xl font-bold shadow-lg shadow-primary/20 hover:opacity-90 active:scale-[0.98] transition-all disabled:opacity-60"
            >
                {isPending ? (
                    <div className="w-4 h-4 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                ) : (
                    <MaterialIcon name="rocket_launch" size={20} />
                )}
                {isPending ? "Processing…" : `Checkout in ${currency}`}
            </button>

            <p className="text-[10px] text-secondary text-center">
                Secure payment • Instant PDF delivery • 30-day access
            </p>
        </div>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────
export default function ResumeDetailPage() {
    const params = useParams<{ id: string }>();
    const router = useRouter();
    const { data: resume, isLoading, isError } = useResumeDetail(params.id);
    const initFromResume = useWizardStore((s) => s.initFromResume);

    function handleEditInWizard() {
        if (!resume) return;
        initFromResume(resume);
        router.push("/create/steps/1");
    }

    if (isLoading) {
        return (
            <div className="max-w-[1024px] mx-auto py-4">
                <ResumeDetailSkeleton />
            </div>
        );
    }

    if (isError || !resume) {
        return (
            <div className="flex flex-col items-center gap-4 py-24">
                <MaterialIcon name="error_outline" size={48} className="text-error" />
                <h2 className="font-h2 text-h2 text-on-surface">Resume not found</h2>
                <p className="text-secondary">This resume may have been deleted or is unavailable.</p>
                <button
                    onClick={() => router.push("/resumes")}
                    className="px-5 py-2 bg-primary text-on-primary rounded-lg font-semibold"
                >
                    Back to My Resumes
                </button>
            </div>
        );
    }

    const raw = resume.finalData?.content ?? resume.rawData?.content;
    const personal = raw?.personal;

    return (
        <div className="max-w-[1024px] mx-auto space-y-6">
            {/* Header */}
            <div className="flex items-center gap-3 flex-wrap">
                <button
                    onClick={() => router.push("/resumes")}
                    className="flex items-center gap-1.5 text-sm text-secondary hover:text-on-surface transition-colors shrink-0"
                >
                    <MaterialIcon name="arrow_back" size={18} />
                    My Resumes
                </button>
                <span className="text-slate-300 shrink-0">|</span>
                <h1 className="font-bold text-xl text-on-surface">
                    {personal
                        ? `${personal.firstName} ${personal.lastName}`
                        : "Resume"}{" "}
                    <span className="font-normal text-secondary text-base">— {resume.templateName}</span>
                </h1>
                <ResumeStatusBadge status={resume.status} />
                {resume.aiAvailable && (
                    <span className="flex items-center gap-1 text-[10px] font-bold text-primary bg-primary-fixed px-2 py-0.5 rounded-full uppercase tracking-wide">
                        <MaterialIcon name="auto_awesome" size={12} filled />
                        AI Active
                    </span>
                )}
                <p className="text-xs text-secondary ml-auto shrink-0">
                    Created {new Date(resume.createdAt).toLocaleDateString()}
                </p>
            </div>

            <div className="flex flex-col lg:flex-row gap-6">
                {/* Left: rendered HTML preview */}
                <div className="flex-1 space-y-3">
                    <div className="flex items-center justify-between">
                        <h2 className="font-h2 text-h2 text-on-surface">Resume Preview</h2>
                        <a
                            href={`/resumes/${resume.id}/preview`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="flex items-center gap-1.5 text-xs font-semibold text-primary hover:underline"
                        >
                            <MaterialIcon name="open_in_new" size={14} />
                            Full Preview
                        </a>
                    </div>
                    <ResumeHtmlPreview
                        resumeId={resume.id}
                    />
                </div>

                {/* Right: action panel */}
                <div className="w-full lg:w-[300px] space-y-4 lg:sticky lg:top-8 self-start">
                    {/* Status info */}
                    <div className="bg-white border border-outline-variant/30 rounded-xl shadow-sm p-5 space-y-3">
                        <h4 className="font-semibold text-on-surface text-sm">Resume Status</h4>
                        <div className="flex items-center gap-2">
                            <ResumeStatusBadge status={resume.status} />
                        </div>
                        <div className="text-xs text-secondary space-y-1">
                            <div className="flex justify-between">
                                <span>Template</span>
                                <span className="text-on-surface font-medium">{resume.templateName}</span>
                            </div>
                            <div className="flex justify-between">
                                <span>Last updated</span>
                                <span className="text-on-surface font-medium">
                                    {new Date(resume.updatedAt).toLocaleDateString()}
                                </span>
                            </div>
                            <div className="flex justify-between">
                                <span>AI mode</span>
                                <span
                                    className={
                                        resume.aiAvailable
                                            ? "text-primary font-bold"
                                            : "text-secondary"
                                    }
                                >
                                    {resume.aiAvailable ? "Active" : "Stub"}
                                </span>
                            </div>
                        </div>
                    </div>

                    {/* Edit in Wizard */}
                    <button
                        onClick={handleEditInWizard}
                        className="w-full flex items-center justify-center gap-2 py-2.5 border-2 border-primary text-primary rounded-xl font-bold text-sm hover:bg-primary-fixed/20 transition-colors"
                    >
                        <MaterialIcon name="edit_note" size={18} />
                        Edit in Wizard
                    </button>

                    {/* Checkout */}
                    {resume.status !== "PAID" && <CheckoutPanel resume={resume} />}

                    {resume.status === "PAID" && (
                        <div className="bg-white border border-primary/30 rounded-xl p-5 text-center space-y-3">
                            <div className="w-12 h-12 rounded-full bg-primary-fixed mx-auto flex items-center justify-center">
                                <MaterialIcon name="check_circle" size={28} className="text-primary" filled />
                            </div>
                            <p className="font-bold text-on-surface">Resume Purchased</p>
                            <p className="text-xs text-secondary">
                                Your PDF is ready to download.
                            </p>
                            <button className="w-full flex items-center justify-center gap-2 py-2.5 bg-primary text-on-primary rounded-xl font-bold text-sm hover:opacity-90 transition-opacity">
                                <MaterialIcon name="download" size={16} />
                                Download PDF
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}
