"use client";

import Link from "next/link";
import { useUser } from "@/hooks/useUser";
import { useResumes } from "@/hooks/useResumes";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { ResumeCard } from "@/components/resume/ResumeCard";
import { Skeleton } from "@/components/shared/SkeletonCard";

// ─── Skeleton loading layout ─────────────────────────────────────────────────

function DashboardSkeleton() {
    return (
        <div className="space-y-10 animate-pulse">
            {/* Welcome */}
            <div className="space-y-2">
                <Skeleton className="h-9 w-64" />
                <Skeleton className="h-5 w-80 opacity-60" />
            </div>
            {/* Stats */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <Skeleton className="h-24 rounded-xl" />
                <Skeleton className="h-24 rounded-xl" />
            </div>
            {/* Quick start */}
            <div className="space-y-3">
                <Skeleton className="h-7 w-32" />
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <Skeleton className="h-32 rounded-xl" />
                    <Skeleton className="h-32 rounded-xl" />
                    <Skeleton className="h-32 rounded-xl" />
                </div>
            </div>
            {/* Recent */}
            <div className="space-y-3">
                <Skeleton className="h-7 w-40" />
                <Skeleton className="h-16 rounded-xl" />
                <Skeleton className="h-16 rounded-xl" />
                <Skeleton className="h-16 rounded-xl" />
            </div>
        </div>
    );
}

// ─── Profile completion heuristic ─────────────────────────────────────────────

function calcCompletion(
    user: { firstName: string; lastName: string; username: string; email: string } | undefined,
    resumeCount: number,
): number {
    if (!user) return 0;
    let pct = 60; // account exists
    if (user.firstName && user.lastName) pct += 15;
    if (user.username) pct += 10;
    if (resumeCount >= 1) pct += 10;
    if (resumeCount >= 3) pct += 5;
    return Math.min(pct, 100);
}

// ─── Recent activity row ──────────────────────────────────────────────────────

function RecentRow({
    id,
    templateName,
    name,
    createdAt,
}: {
    id: string;
    templateName: string;
    name?: string | null;
    createdAt: string;
}) {
    const date = new Date(createdAt).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
        year: "numeric",
    });
    return (
        <Link
            href={`/resumes/${id}`}
            className="p-4 flex items-center justify-between hover:bg-surface-container-high/40 transition-colors"
        >
            <div className="flex items-center gap-4">
                {/* Mini resume preview thumbnail */}
                <div className="w-10 h-12 bg-surface-container rounded-lg border border-outline-variant/20 flex items-center justify-center shrink-0">
                    <MaterialIcon name="description" size={20} className="text-primary/60" />
                </div>
                <div>
                    <p className="font-body-base font-semibold text-on-surface">{name || templateName}</p>
                    <p className="text-xs text-secondary">{date}</p>
                </div>
            </div>
            <MaterialIcon name="chevron_right" size={20} className="text-on-surface-variant" />
        </Link>
    );
}

// ─── Quick Start card ─────────────────────────────────────────────────────────

function QuickStartCard({
    icon,
    title,
    description,
    href,
    disabled,
}: {
    icon: string;
    title: string;
    description: string;
    href?: string;
    disabled?: boolean;
}) {
    const inner = (
        <div
            className={`bg-surface-container p-6 rounded-xl border border-outline-variant/30 hover:border-primary/40 transition-all text-left group h-full ${disabled ? "opacity-50 cursor-not-allowed" : "cursor-pointer"
                }`}
        >
            <div className="w-10 h-10 rounded-full bg-white flex items-center justify-center mb-4 group-hover:bg-primary-fixed transition-colors">
                <MaterialIcon name={icon} size={22} className="text-primary" />
            </div>
            <h4 className="font-body-base font-bold mb-1 text-on-surface">{title}</h4>
            <p className="font-body-sm text-body-sm text-secondary">{description}</p>
        </div>
    );

    if (disabled || !href) return <div>{inner}</div>;
    return <Link href={href}>{inner}</Link>;
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function DashboardPage() {
    const { data: user, isLoading: userLoading } = useUser();
    const { data: resumes, isLoading: resumesLoading } = useResumes();

    const isLoading = userLoading || resumesLoading;

    if (isLoading) return <DashboardSkeleton />;

    const resumeList = resumes ?? [];
    const recentResumes = [...resumeList]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5);

    const completion = calcCompletion(user, resumeList.length);

    return (
        <div className="space-y-10">
            {/* Welcome */}
            <section className="space-y-1">
                <h1 className="font-h1 text-h1 text-on-surface">
                    Welcome back, {user?.firstName ?? "there"}
                </h1>
                <p className="font-body-base text-body-base text-secondary">
                    Continue refining your professional identity today.
                </p>
            </section>

            {/* Stats bento */}
            <section className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Resumes created */}
                <div className="bg-surface-container-low p-6 rounded-xl border border-outline-variant/30 flex items-center gap-4">
                    <div className="w-12 h-12 rounded-lg bg-primary-fixed flex items-center justify-center shrink-0">
                        <MaterialIcon name="description" size={28} className="text-primary" />
                    </div>
                    <div>
                        <p className="font-label-caps text-label-caps text-on-surface-variant">
                            RESUMES CREATED
                        </p>
                        <h3 className="font-h1 text-h1 text-primary">{resumeList.length}</h3>
                    </div>
                </div>

                {/* Profile completion */}
                <div className="bg-surface-container-low p-6 rounded-xl border border-outline-variant/30 flex flex-col gap-3">
                    <div className="flex items-center justify-between">
                        <p className="font-label-caps text-label-caps text-on-surface-variant">
                            PROFILE COMPLETION
                        </p>
                        <span className="font-body-sm text-body-sm font-semibold text-primary">
                            {completion}%
                        </span>
                    </div>
                    <div className="w-full bg-surface-container-highest h-2 rounded-full overflow-hidden">
                        <div
                            className="bg-primary h-full rounded-full transition-all duration-700"
                            style={{ width: `${completion}%` }}
                        />
                    </div>
                    <p className="font-body-sm text-body-sm text-on-surface-variant">
                        {completion < 100
                            ? "Create your first resume to complete your profile."
                            : "Your profile is complete — keep building!"}
                    </p>
                </div>
            </section>

            {/* Quick Start */}
            <section className="space-y-4">
                <h2 className="font-h2 text-h2 text-on-surface">Quick Start</h2>
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                    <QuickStartCard
                        icon="auto_awesome"
                        title="AI Generator"
                        description="Start with an AI-generated draft"
                        href="/create/template"
                    />
                    <QuickStartCard
                        icon="upload_file"
                        title="Import PDF"
                        description="Enhance your existing resume"
                        disabled
                    />
                    <QuickStartCard
                        icon="edit_note"
                        title="Blank Canvas"
                        description="Build from scratch manually"
                        href="/create/template"
                    />
                </div>
            </section>

            {/* Recent Activity */}
            <section className="space-y-4 pb-8">
                <div className="flex items-center justify-between">
                    <h2 className="font-h2 text-h2 text-on-surface">Recent Activity</h2>
                    <Link
                        href="/resumes"
                        className="text-primary font-semibold text-sm hover:underline"
                    >
                        View All
                    </Link>
                </div>

                {recentResumes.length === 0 ? (
                    <div className="bg-surface-container-low rounded-xl border border-outline-variant/30 p-10 flex flex-col items-center gap-3">
                        <MaterialIcon name="description" size={40} className="text-outline" />
                        <p className="text-on-surface-variant font-body-base text-center">
                            No resumes yet.{" "}
                            <Link href="/create/template" className="text-primary hover:underline font-semibold">
                                Create your first one
                            </Link>
                            .
                        </p>
                    </div>
                ) : (
                    <div className="bg-surface-container-low rounded-xl border border-outline-variant/30 divide-y divide-outline-variant/20 overflow-hidden">
                        {recentResumes.map((r) => (
                            <RecentRow key={r.id} id={r.id} templateName={r.templateName} name={r.name} createdAt={r.createdAt} />
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}
