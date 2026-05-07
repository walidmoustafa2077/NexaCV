"use client";

import { useState, useRef, useEffect, useCallback } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import type { ResumeSummaryDto } from "@/types/api.types";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { cn } from "@/lib/utils";
import { getResume } from "@/lib/api/resumes";
import { useWizardStore } from "@/store/wizardStore";
import { useRenameResume } from "@/hooks/useResumes";

interface ResumeCardProps {
    resume: ResumeSummaryDto;
    onDelete?: (id: string) => void;
    className?: string;
}

function formatEditedDate(iso: string) {
    const date = new Date(iso);
    return date.toLocaleDateString("en-US", { month: "short", day: "numeric" });
}

/** Dark-bg decorative resume document */
function ResumePlaceholder() {
    return (
        <div className="absolute inset-0 flex flex-col items-center justify-center bg-[#1e2235] p-6">
            <div className="w-28 h-36 bg-white/10 border border-white/15 rounded-lg p-3 flex flex-col gap-1.5 shadow-xl">
                <div className="h-2.5 w-3/5 rounded-full bg-blue-300/70 mb-0.5" />
                <div className="h-1.5 w-4/5 rounded-full bg-white/20" />
                <div className="h-px w-full bg-white/15 mt-0.5" />
                <div className="space-y-1 mt-0.5">
                    {[90, 70, 85, 60, 78, 65, 80, 55].map((w, i) => (
                        <div key={i} className="h-1 rounded-full bg-white/15" style={{ width: `${w}%` }} />
                    ))}
                </div>
                <div className="h-1.5 w-1/3 rounded-full bg-blue-400/50 mt-1" />
                <div className="flex gap-1 mt-1">
                    {[24, 32, 20, 28].map((w, i) => (
                        <div key={i} className="h-3 rounded-full bg-blue-400/20 border border-blue-400/25" style={{ width: `${w}px` }} />
                    ))}
                </div>
            </div>
        </div>
    );
}

export function ResumeCard({ resume, onDelete, className }: ResumeCardProps) {
    const [isLoadingWizard, setIsLoadingWizard] = useState(false);
    const [menuOpen, setMenuOpen] = useState(false);
    const [isRenaming, setIsRenaming] = useState(false);
    const [renameValue, setRenameValue] = useState(resume.name ?? "");
    const router = useRouter();
    const initFromResume = useWizardStore((s) => s.initFromResume);
    const { mutate: renameResume, isPending: isRenamePending } = useRenameResume();
    const menuRef = useRef<HTMLDivElement>(null);
    const renameInputRef = useRef<HTMLInputElement>(null);

    // Close menu on outside click
    useEffect(() => {
        function handleClick(e: MouseEvent) {
            if (menuRef.current && !menuRef.current.contains(e.target as Node)) setMenuOpen(false);
        }
        if (menuOpen) document.addEventListener("mousedown", handleClick);
        return () => document.removeEventListener("mousedown", handleClick);
    }, [menuOpen]);

    // Focus rename input when opening
    useEffect(() => {
        if (isRenaming) renameInputRef.current?.select();
    }, [isRenaming]);

    async function handleEditInWizard() {
        setMenuOpen(false);
        setIsLoadingWizard(true);
        try {
            const detail = await getResume(resume.id);
            initFromResume(detail);
            router.push("/create/steps/1");
        } catch {
            toast.error("Failed to load resume. Please try again.");
            setIsLoadingWizard(false);
        }
    }

    function handleDelete() {
        setMenuOpen(false);
        if (!confirm("Delete this resume? This cannot be undone.")) return;
        onDelete?.(resume.id);
    }

    function handleRenameStart() {
        setMenuOpen(false);
        setRenameValue(resume.name ?? resume.templateName);
        setIsRenaming(true);
    }

    const handleRenameSubmit = useCallback(() => {
        const trimmed = renameValue.trim();
        if (!trimmed) { setIsRenaming(false); return; }
        if (trimmed === (resume.name ?? "")) { setIsRenaming(false); return; }
        renameResume({ id: resume.id, name: trimmed }, {
            onSuccess: () => toast.success("Resume renamed."),
            onError: () => toast.error("Failed to rename resume."),
            onSettled: () => setIsRenaming(false),
        });
    }, [renameValue, resume.id, resume.name, renameResume]);

    const displayName = resume.name || resume.templateName;

    return (
        <div className={cn("group relative bg-white rounded-2xl border border-slate-200 shadow-sm hover:shadow-xl hover:-translate-y-0.5 transition-all flex flex-col overflow-hidden", className)}>
            {/* Dark preview area */}
            <div className="relative h-44 overflow-hidden">
                <ResumePlaceholder />

                {/* Three-dot menu */}
                <div className="absolute top-3 right-3 z-20" ref={menuRef}>
                    <button
                        onClick={(e) => { e.stopPropagation(); setMenuOpen((v) => !v); }}
                        className="w-8 h-8 bg-white/15 backdrop-blur-sm rounded-lg flex items-center justify-center hover:bg-white/25 transition-colors"
                        aria-label="More options"
                    >
                        <MaterialIcon name="more_vert" size={18} className="text-white" />
                    </button>
                    {menuOpen && (
                        <>
                            <div className="fixed inset-0 z-10" onClick={() => setMenuOpen(false)} />
                            <div className="absolute right-0 top-9 z-20 w-44 bg-white rounded-xl border border-slate-200 shadow-lg overflow-hidden py-1">
                                <button
                                    onClick={handleEditInWizard}
                                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-50 transition-colors"
                                >
                                    <MaterialIcon name="edit_note" size={17} className="text-primary" />
                                    Edit in Wizard
                                </button>
                                <Link
                                    href={`/resumes/${resume.id}`}
                                    onClick={() => setMenuOpen(false)}
                                    className="flex items-center gap-2.5 px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-50 transition-colors"
                                >
                                    <MaterialIcon name="visibility" size={17} className="text-secondary" />
                                    View Preview
                                </Link>
                                <button
                                    onClick={handleRenameStart}
                                    className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-slate-700 hover:bg-slate-50 transition-colors"
                                >
                                    <MaterialIcon name="drive_file_rename_outline" size={17} className="text-amber-500" />
                                    Rename
                                </button>
                                {onDelete && (
                                    <button
                                        onClick={handleDelete}
                                        className="w-full flex items-center gap-2.5 px-4 py-2.5 text-sm text-red-500 hover:bg-red-50 transition-colors"
                                    >
                                        <MaterialIcon name="delete" size={17} className="text-red-400" />
                                        Delete
                                    </button>
                                )}
                            </div>
                        </>
                    )}
                </div>

                {/* Edit overlay on hover */}
                <div className="absolute inset-0 bg-black/40 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center gap-3 z-10">
                    <button
                        onClick={handleEditInWizard}
                        disabled={isLoadingWizard}
                        className="bg-white text-slate-900 px-4 py-2 rounded-xl font-semibold flex items-center gap-2 shadow-lg hover:bg-slate-100 disabled:opacity-60 text-sm"
                    >
                        {isLoadingWizard ? (
                            <div className="w-4 h-4 border-2 border-slate-400 border-t-slate-800 rounded-full animate-spin" />
                        ) : (
                            <MaterialIcon name="edit" size={16} />
                        )}
                        Edit
                    </button>
                    <Link
                        href={`/resumes/${resume.id}`}
                        className="bg-white/20 border border-white/30 text-white px-4 py-2 rounded-xl font-semibold flex items-center gap-2 shadow-lg hover:bg-white/30 text-sm backdrop-blur-sm"
                    >
                        <MaterialIcon name="visibility" size={16} />
                        View
                    </Link>
                </div>
            </div>

            {/* Card footer */}
            <div className="px-4 py-3.5 flex flex-col gap-2">
                {/* Title / rename */}
                {isRenaming ? (
                    <div className="flex items-center gap-2">
                        <input
                            ref={renameInputRef}
                            value={renameValue}
                            onChange={(e) => setRenameValue(e.target.value)}
                            onKeyDown={(e) => {
                                if (e.key === "Enter") handleRenameSubmit();
                                if (e.key === "Escape") setIsRenaming(false);
                            }}
                            onBlur={handleRenameSubmit}
                            disabled={isRenamePending}
                            className="flex-1 text-sm font-semibold text-slate-800 border-b-2 border-primary bg-transparent outline-none py-0.5"
                            maxLength={100}
                        />
                        {isRenamePending && <div className="w-3.5 h-3.5 border-2 border-primary/40 border-t-primary rounded-full animate-spin" />}
                    </div>
                ) : (
                    <h3 className="font-semibold text-sm text-slate-800 truncate">{displayName}</h3>
                )}

                {/* Meta row */}
                <div className="flex items-center gap-3 text-xs text-slate-400">
                    <span className="flex items-center gap-1">
                        <MaterialIcon name="calendar_today" size={12} className="text-slate-400" />
                        Edited {formatEditedDate(resume.updatedAt)}
                    </span>
                    <span className="flex items-center gap-1">
                        <MaterialIcon name="visibility" size={12} className="text-slate-400" />
                        {resume.downloadCount} {resume.downloadCount === 1 ? "Download" : "Downloads"}
                    </span>
                </div>
            </div>
        </div>
    );
}
