"use client";

import { useEffect, useRef, useState } from "react";
import { usePathname, useRouter } from "next/navigation";
import { toast } from "sonner";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { useWizardStore } from "@/store/wizardStore";
import { useRenameResume } from "@/hooks/useResumes";

const steps = [
    { step: 1, label: "Personal Info" },
    { step: 2, label: "Education" },
    { step: 3, label: "Courses" },
    { step: 4, label: "Work Experience" },
    { step: 5, label: "Projects" },
    { step: 6, label: "Summary & Skills" },
    { step: 7, label: "Languages" },
    { step: 8, label: "Review" },
];

export function WizardTopBar() {
    const pathname = usePathname();
    const router = useRouter();
    const { formData, updateFormData } = useWizardStore();
    const { mutate: renameResume, isPending } = useRenameResume();

    const [isEditing, setIsEditing] = useState(false);
    const [editValue, setEditValue] = useState("");
    const inputRef = useRef<HTMLInputElement>(null);

    // Current step from path
    const currentStep = steps.find((s) => pathname.startsWith(`/create/steps/${s.step}`))?.step ?? null;
    const stepLabel = steps.find((s) => s.step === currentStep)?.label ?? "";

    // Display name: custom name > template-derived label
    const displayName = formData.resumeName || "Untitled Resume";

    useEffect(() => {
        if (isEditing) inputRef.current?.select();
    }, [isEditing]);

    function handleEditStart() {
        setEditValue(formData.resumeName || "");
        setIsEditing(true);
    }

    function handleEditSubmit() {
        const trimmed = editValue.trim();
        setIsEditing(false);
        if (!trimmed) return;
        if (trimmed === formData.resumeName) return;

        // Optimistically update local store
        updateFormData({ resumeName: trimmed });

        // Persist to backend if resume exists
        if (formData.createdResumeId) {
            renameResume({ id: formData.createdResumeId, name: trimmed }, {
                onError: () => toast.error("Failed to save resume name."),
            });
        }
    }

    return (
        <header className="fixed top-0 left-64 right-0 h-14 bg-white border-b border-slate-200 z-30 flex items-center px-6 gap-4">
            {/* Resume name — editable */}
            <div className="flex items-center gap-2 min-w-0">
                <MaterialIcon name="description" size={18} className="text-primary shrink-0" />
                {isEditing ? (
                    <input
                        ref={inputRef}
                        value={editValue}
                        onChange={(e) => setEditValue(e.target.value)}
                        onKeyDown={(e) => {
                            if (e.key === "Enter") handleEditSubmit();
                            if (e.key === "Escape") setIsEditing(false);
                        }}
                        onBlur={handleEditSubmit}
                        disabled={isPending}
                        maxLength={100}
                        className="text-sm font-semibold text-slate-800 border-b-2 border-primary bg-transparent outline-none py-0.5 w-56"
                    />
                ) : (
                    <button
                        onClick={handleEditStart}
                        className="text-sm font-semibold text-slate-800 hover:text-primary transition-colors truncate max-w-[220px] flex items-center gap-1.5 group"
                        title="Click to rename"
                    >
                        <span className="truncate">{displayName}</span>
                        <MaterialIcon name="edit" size={14} className="text-slate-300 group-hover:text-primary transition-colors shrink-0" />
                    </button>
                )}
                {isPending && <div className="w-3 h-3 border-2 border-primary/30 border-t-primary rounded-full animate-spin shrink-0" />}
            </div>

            {/* Divider */}
            {stepLabel && <span className="text-slate-200 text-sm shrink-0">/</span>}

            {/* Current step label */}
            {stepLabel && (
                <span className="text-sm text-slate-500 shrink-0">{stepLabel}</span>
            )}

            {/* Right side */}
            <div className="ml-auto flex items-center gap-2">
                {/* Step progress pills */}
                {currentStep && (
                    <div className="hidden sm:flex items-center gap-1">
                        {steps.map((s) => (
                            <div
                                key={s.step}
                                title={s.label}
                                className={`h-1.5 rounded-full transition-all ${s.step < currentStep
                                        ? "w-4 bg-primary"
                                        : s.step === currentStep
                                            ? "w-6 bg-primary"
                                            : "w-4 bg-slate-200"
                                    }`}
                            />
                        ))}
                        <span className="text-xs text-slate-400 ml-1">{currentStep}/8</span>
                    </div>
                )}

                {/* Back to dashboard */}
                <button
                    onClick={() => router.push("/dashboard")}
                    className="flex items-center gap-1.5 px-3 py-1.5 text-xs text-slate-500 hover:text-primary hover:bg-slate-100 rounded-lg transition-colors"
                >
                    <MaterialIcon name="arrow_back" size={14} />
                    Dashboard
                </button>
            </div>
        </header>
    );
}
