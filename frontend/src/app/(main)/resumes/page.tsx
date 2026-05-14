"use client";

import { useState } from "react";
import Link from "next/link";
import { toast } from "sonner";
import { useResumes, useDeleteResume } from "@/hooks/useResumes";
import { ResumeCard } from "@/components/resume/ResumeCard";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { SkeletonCard } from "@/components/shared/SkeletonCard";

// ─── Skeleton grid ────────────────────────────────────────────────────────────

function ResumesGridSkeleton() {
    return (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
            {Array.from({ length: 6 }).map((_, i) => (
                <SkeletonCard key={i} className="h-[220px]" />
            ))}
        </div>
    );
}

// ─── Create New card ──────────────────────────────────────────────────────────

function CreateNewCard() {
    return (
        <Link href="/create/template" className="block h-full">
            <div className="group relative flex flex-col items-center justify-center border-2 border-dashed border-slate-200 rounded-2xl hover:border-primary/40 hover:bg-primary/5 transition-all cursor-pointer overflow-hidden h-full min-h-[220px]">
                <div className="w-12 h-12 rounded-full border-2 border-dashed border-slate-300 group-hover:border-primary/50 flex items-center justify-center mb-3 transition-colors">
                    <MaterialIcon name="add" size={24} className="text-slate-400 group-hover:text-primary transition-colors" />
                </div>
                <span className="font-semibold text-sm text-slate-600 group-hover:text-primary transition-colors">Create New Resume</span>
                <p className="text-xs text-slate-400 text-center mt-1 px-6">Start with a professional template.</p>
            </div>
        </Link>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function MyResumesPage() {
    const { data: resumes, isLoading } = useResumes();
    const { mutate: deleteResume, isPending: isDeleting } = useDeleteResume();
    const [sortAsc, setSortAsc] = useState(false);

    const resumeList = resumes ?? [];

    const sorted = [...resumeList].sort((a, b) => {
        const diff =
            new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime();
        return sortAsc ? diff : -diff;
    });

    function handleDelete(id: string) {
        deleteResume(id, {
            onSuccess: () => toast.success("Resume deleted."),
            onError: () => toast.error("Failed to delete resume. Please try again."),
        });
    }

    return (
        <div className="space-y-8">
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-end justify-between gap-4">
                <div className="space-y-1">
                    <h1 className="font-h1 text-h1 text-on-surface">My Resumes</h1>
                    <p className="font-body-base text-body-base text-on-surface-variant">
                        Manage and organize your professional profiles.
                    </p>
                </div>

                {/* Controls */}
                <div className="flex items-center gap-3">
                    <button
                        onClick={() => setSortAsc((v) => !v)}
                        className="flex items-center gap-2 px-4 py-2 border border-outline-variant rounded-lg font-semibold text-sm text-on-surface hover:bg-surface-container-low transition-colors"
                        aria-label="Toggle sort order"
                    >
                        <MaterialIcon name="filter_list" size={20} />
                        Sort: {sortAsc ? "Oldest" : "Recent"}
                    </button>

                    <Link
                        href="/create/template"
                        className="flex items-center gap-2 px-4 py-2 bg-primary text-on-primary rounded-lg font-semibold text-sm hover:opacity-90 transition-opacity"
                    >
                        <MaterialIcon name="add" size={20} />
                        New Resume
                    </Link>
                </div>
            </div>

            {/* Grid */}
            {isLoading ? (
                <ResumesGridSkeleton />
            ) : (
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-5">
                    <CreateNewCard />
                    {sorted.map((resume) => (
                        <ResumeCard
                            key={resume.id}
                            resume={resume}
                            onDelete={isDeleting ? undefined : handleDelete}
                        />
                    ))}
                </div>
            )}

            {/* Empty state (no resumes yet — CreateNewCard is always shown) */}
            {!isLoading && resumeList.length === 0 && (
                <p className="text-center text-on-surface-variant font-body-sm mt-4">
                    No resumes yet — click &ldquo;Create New Resume&rdquo; to get started.
                </p>
            )}
        </div>
    );
}
