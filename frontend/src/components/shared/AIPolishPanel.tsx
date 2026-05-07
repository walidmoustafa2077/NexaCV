"use client";

import { useState } from "react";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { regenerateSection } from "@/lib/api/resumes";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type { RegenerateRequest } from "@/types/api.types";

// ─── Quick-prompt suggestions per section ─────────────────────────────────────
const QUICK_PROMPTS: Record<string, string[]> = {
    summary:    ["Make it more concise", "Add leadership emphasis", "More ATS-friendly", "Highlight key achievements"],
    experience: ["Quantify achievements with numbers", "Use stronger action verbs", "Add impact metrics", "Make it more concise"],
    education:  ["Highlight academic achievements", "Add relevant coursework", "Make it more professional"],
    skills:     ["Add more technical skills", "Reorganize by relevance", "Include soft skills"],
    courses:    ["Highlight relevance to career", "Add certification details", "Make descriptions more impactful"],
};

interface AIPolishPanelProps {
    /** Section or entry identifier sent to the backend */
    sectionId: string;
    resumeId: string;
    /** Whether the real AI backend is available (false = stub mode) */
    aiAvailable: boolean;
    /** Remaining regeneration uses — null means not yet fetched */
    regenRemaining: number | null;
    /** Called after a successful regeneration so the parent can update remaining count */
    onRegenSuccess: (remaining: number) => void;
    /** Called to trigger a full data refresh in the parent */
    onRefresh: () => void;
    onClose: () => void;
    /**
     * Optional: "category" key used to look up quick-prompt suggestions.
     * Falls back to lower-cased sectionId if omitted.
     */
    promptCategory?: string;
}

export function AIPolishPanel({
    sectionId,
    resumeId,
    aiAvailable,
    regenRemaining,
    onRegenSuccess,
    onRefresh,
    onClose,
    promptCategory,
}: AIPolishPanelProps) {
    const [prompt, setPrompt] = useState("");
    const category = (promptCategory ?? sectionId).toLowerCase();
    const suggestions = QUICK_PROMPTS[category] ?? QUICK_PROMPTS["experience"] ?? [];
    const remaining = regenRemaining ?? 3;
    const exhausted = remaining <= 0;

    const { mutate, isPending } = useMutation({
        mutationFn: (req: RegenerateRequest) => regenerateSection(resumeId, req),
        onSuccess: (data) => {
            onRegenSuccess(data.regenCountRemaining);
            toast.success(
                `✨ Section improved! ${data.regenCountRemaining} use${data.regenCountRemaining !== 1 ? "s" : ""} remaining.`,
            );
            onRefresh();
            onClose();
        },
        onError: (err: Error) => {
            toast.error(err.message || "Regeneration failed. Please try again.");
        },
    });

    function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        if (prompt.trim().length < 10) {
            toast.error("Please describe what to improve (min 10 characters).");
            return;
        }
        mutate({ sectionIdentifier: sectionId, userPrompt: prompt.trim() });
    }

    return (
        <form
            onSubmit={handleSubmit}
            className="mt-3 rounded-xl overflow-hidden border border-primary/20 shadow-sm"
        >
            {/* Header */}
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/20 px-4 py-3 flex items-center gap-2">
                <div className="w-6 h-6 rounded-full bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={14} className="text-on-primary" filled />
                </div>
                <span className="text-sm font-bold text-primary">Polish with AI</span>
                <div className="ml-auto flex items-center gap-2">
                    {!aiAvailable && (
                        <span className="text-[10px] bg-tertiary-container text-on-surface px-2 py-0.5 rounded-full font-bold">
                            STUB
                        </span>
                    )}
                    <span
                        className={`text-[11px] font-bold px-2 py-0.5 rounded-full ${
                            exhausted
                                ? "bg-error-container text-error"
                                : remaining === 1
                                    ? "bg-orange-100 text-orange-700"
                                    : "bg-primary-fixed/60 text-primary"
                        }`}
                    >
                        {exhausted ? "No uses left" : `${remaining} use${remaining !== 1 ? "s" : ""} left`}
                    </span>
                </div>
            </div>

            <div className="bg-white px-4 py-3 space-y-3">
                {/* Quick suggestions */}
                {suggestions.length > 0 && (
                    <div className="flex flex-wrap gap-1.5">
                        {suggestions.map((s) => (
                            <button
                                key={s}
                                type="button"
                                disabled={isPending || exhausted}
                                onClick={() => setPrompt(s)}
                                className="px-2.5 py-1 text-[11px] font-medium border border-outline-variant rounded-full text-secondary hover:border-primary hover:text-primary hover:bg-primary-fixed/10 transition-all disabled:opacity-40"
                            >
                                {s}
                            </button>
                        ))}
                    </div>
                )}

                {/* Textarea */}
                <div className="relative">
                    <textarea
                        className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-3 py-2 text-sm text-on-surface placeholder:text-secondary/60 focus:outline-none focus:ring-2 focus:ring-primary resize-none pr-12"
                        rows={3}
                        placeholder={
                            exhausted
                                ? "Maximum uses reached for this section."
                                : "Describe what to improve…"
                        }
                        value={prompt}
                        onChange={(e) => setPrompt(e.target.value.slice(0, 500))}
                        disabled={isPending || exhausted}
                        autoFocus={!exhausted}
                    />
                    <span
                        className={`absolute bottom-2.5 right-3 text-[10px] ${
                            prompt.length > 450 ? "text-error" : "text-secondary/50"
                        }`}
                    >
                        {prompt.length}/500
                    </span>
                </div>

                {/* Actions */}
                <div className="flex items-center justify-end gap-2 pb-1">
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-4 py-1.5 text-sm text-secondary hover:text-on-surface transition-colors rounded-lg hover:bg-surface-container"
                        disabled={isPending}
                    >
                        Cancel
                    </button>
                    <button
                        type="submit"
                        disabled={isPending || prompt.trim().length < 10 || exhausted}
                        className="flex items-center gap-1.5 px-5 py-1.5 bg-primary text-on-primary rounded-lg text-sm font-semibold disabled:opacity-50 hover:opacity-90 transition-opacity shadow-sm"
                    >
                        {isPending ? (
                            <>
                                <div className="w-3.5 h-3.5 border-2 border-white/40 border-t-white rounded-full animate-spin" />
                                Generating…
                            </>
                        ) : (
                            <>
                                <MaterialIcon name="auto_awesome" size={14} filled />
                                Apply
                            </>
                        )}
                    </button>
                </div>
            </div>
        </form>
    );
}
