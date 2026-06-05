"use client";

import { useState, useRef, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import { useWizardStore } from "@/store/wizardStore";
import { regenerateSection } from "@/lib/api/resumes";
import { ApiError, ValidationError } from "@/lib/api/client";
import { WizardProgress } from "../1/page";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type { RegenerateRequest } from "@/types/api.types";

const step6Schema = z.object({
    summary: z.string().min(10, "Summary is required (min 10 chars)").max(2000),
    skills: z.array(z.object({ category: z.string(), items: z.array(z.string()) })),
});

type Step6Values = z.infer<typeof step6Schema>;

// ─── AI Polish ────────────────────────────────────────────────────────────────

const QUICK_PROMPTS_SUMMARY = [
    "Make it more concise",
    "Add measurable impact",
    "Use stronger action verbs",
    "Tailor for a leadership role",
];

const QUICK_PROMPTS_SKILLS = [
    "Suggest missing technical skills",
    "Add cloud/DevOps skills",
    "Add testing/QA skills",
    "Suggest leadership skills",
];

function InlinePolishPanel({
    sectionId,
    resumeId,
    quickPrompts,
    targetFormat,
    onApply,
    onClose,
}: {
    sectionId: string;
    resumeId: string;
    quickPrompts: string[];
    targetFormat?: string | null;
    onApply: (text: string) => void;
    onClose: () => void;
}) {
    const [prompt, setPrompt] = useState("");
    const [errorMsg, setErrorMsg] = useState<string | null>(null);

    const { mutate, isPending } = useMutation({
        mutationFn: (req: RegenerateRequest) => regenerateSection(resumeId, req),
        onSuccess: (data) => {
            const text =
                typeof data.updatedContent === "string"
                    ? data.updatedContent
                    : JSON.stringify(data.updatedContent);
            onApply(text);
            toast.success("Section Polished", {
                description: `${data.regenCountRemaining} AI use${data.regenCountRemaining !== 1 ? "s" : ""} remaining.`,
            });
            onClose();
        },
        onError: (err: Error) => {
            let msg: string;
            if (err instanceof ValidationError && err.details.length > 0) {
                msg = err.details.map((d) => d.message).join(" ");
            } else if (err instanceof ApiError) {
                if (err.status === 0) msg = "Backend is not reachable. Make sure all services are running.";
                else if (err.status === 401) msg = "Your session has expired. Please log in again.";
                else if (err.status === 429) msg = "Max regenerations (3) reached for this section.";
                else if (err.status >= 500) msg = `Server error (${err.status}). Please try again.`;
                else msg = err.message || "Polish failed.";
            } else {
                msg = err.message || "Something went wrong.";
            }
            setErrorMsg(msg);
            toast.error(msg);
        },
    });

    function handleApply() {
        if (prompt.trim().length < 3) {
            toast.error("Please describe what to improve.");
            return;
        }
        setErrorMsg(null);
        mutate({ sectionIdentifier: sectionId, userPrompt: prompt.trim(), targetFormat });
    }

    return (
        <div className="rounded-xl border-2 border-primary/20 overflow-hidden shadow-sm mt-3">
            <div className="bg-gradient-to-r from-primary/10 to-primary-fixed/20 px-4 py-2.5 flex items-center gap-2">
                <div className="w-5 h-5 rounded-full bg-primary flex items-center justify-center shrink-0">
                    <MaterialIcon name="auto_awesome" size={12} className="text-on-primary" filled />
                </div>
                <span className="text-xs font-bold text-primary uppercase tracking-wide">AI Suggestion</span>
                <button type="button" onClick={onClose} className="ml-auto text-secondary hover:text-on-surface transition-colors" aria-label="Close">
                    <MaterialIcon name="close" size={16} />
                </button>
            </div>
            <div className="bg-white px-4 py-3 space-y-3">
                <div className="flex flex-wrap gap-1.5">
                    {quickPrompts.map((s) => (
                        <button key={s} type="button" disabled={isPending} onClick={() => setPrompt(s)}
                            className="px-2.5 py-1 text-[11px] font-medium border border-outline-variant rounded-full text-secondary hover:border-primary hover:text-primary hover:bg-primary-fixed/10 transition-all disabled:opacity-40">
                            {s}
                        </button>
                    ))}
                </div>
                <textarea
                    className="w-full rounded-lg border border-outline-variant bg-surface-container-low px-3 py-2 text-sm text-on-surface placeholder:text-secondary/60 focus:outline-none focus:ring-2 focus:ring-primary resize-none"
                    rows={2}
                    placeholder="Describe what to improve…"
                    value={prompt}
                    onChange={(e) => { setPrompt(e.target.value.slice(0, 300)); if (errorMsg) setErrorMsg(null); }}
                    disabled={isPending}
                    autoFocus
                />
                {errorMsg && (
                    <div className="flex items-start gap-2 px-3 py-2 bg-error/10 border border-error/30 rounded-lg text-xs text-error">
                        <MaterialIcon name="error" size={14} className="shrink-0 mt-0.5" />
                        <span>{errorMsg}</span>
                    </div>
                )}
                <div className="flex items-center justify-between">
                    <span className="text-[10px] text-secondary/50">{prompt.length}/300</span>
                    <div className="flex items-center gap-2">
                        <button type="button" onClick={onClose} className="px-3 py-1.5 text-sm text-secondary hover:text-on-surface transition-colors rounded-lg" disabled={isPending}>
                            Cancel
                        </button>
                        <button type="button" onClick={handleApply} disabled={isPending || prompt.trim().length < 3}
                            className="flex items-center gap-1.5 px-4 py-1.5 bg-primary text-on-primary rounded-lg text-sm font-semibold disabled:opacity-50 hover:opacity-90 transition-opacity shadow-sm">
                            {isPending ? (
                                <><div className="w-3 h-3 border-2 border-white/40 border-t-white rounded-full animate-spin" />Polishing…</>
                            ) : (
                                <><MaterialIcon name="auto_awesome" size={12} filled />Apply Polish</>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
}

// ─── Skill group card ─────────────────────────────────────────────────────────

function SkillGroupCard({
    group,
    showCategoryRow = true,
    onChange,
    onRemove,
    canRemove,
    resumeId,
    draggingSkill,
    dropBeforeIdx,
    isDropTarget,
    onSkillDragStart,
    onSkillDragEnd,
    onSkillDragOver,
    onGroupDragOver,
    onGroupDrop,
}: {
    group: { category: string; items: string[] };
    showCategoryRow?: boolean;
    onChange: (group: { category: string; items: string[] }) => void;
    onRemove: () => void;
    canRemove: boolean;
    resumeId?: string | null;
    draggingSkill?: string | null;
    dropBeforeIdx?: number | null;
    isDropTarget?: boolean;
    onSkillDragStart?: (skill: string, idx: number) => void;
    onSkillDragEnd?: () => void;
    onSkillDragOver?: (e: React.DragEvent, beforeIdx: number) => void;
    onGroupDragOver?: (e: React.DragEvent) => void;
    onGroupDrop?: () => void;
}) {
    const [input, setInput] = useState("");
    const [showPolish, setShowPolish] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    function addSkill(skill: string) {
        const trimmed = skill.trim();
        if (trimmed && !group.items.includes(trimmed)) {
            onChange({ ...group, items: [...group.items, trimmed] });
        }
        setInput("");
        inputRef.current?.focus();
    }

    function removeSkill(skill: string) {
        onChange({ ...group, items: group.items.filter((s) => s !== skill) });
    }

    function handleKeyDown(e: React.KeyboardEvent<HTMLInputElement>) {
        if ((e.key === "Enter" || e.key === ",") && input.trim()) {
            e.preventDefault();
            addSkill(input);
        }
        if (e.key === "Backspace" && !input && group.items.length > 0) {
            removeSkill(group.items[group.items.length - 1]);
        }
    }

    function handlePolishApply(text: string) {
        let items: string[] = [];
        try {
            const parsed = JSON.parse(text);
            if (Array.isArray(parsed)) items = parsed.map(String).filter(Boolean);
        } catch { /* not JSON */ }
        if (items.length === 0) items = text.split(",").map((s) => s.trim()).filter(Boolean);
        if (items.length > 0) onChange({ ...group, items });
        setShowPolish(false);
    }

    return (
        <div
            className={`border rounded-xl p-4 space-y-3 transition-all ${
                isDropTarget
                    ? "border-primary/40 bg-primary/5 shadow-md"
                    : "border-outline-variant/50 bg-surface-container-low/30"
            }`}
            onDragOver={(e) => { e.preventDefault(); onGroupDragOver?.(e); }}
            onDrop={(e) => { e.preventDefault(); onGroupDrop?.(); }}
        >
            {/* Header row — always rendered when category row is shown or when actions exist */}
            {(showCategoryRow || resumeId || canRemove) && (
                <div className="flex items-center gap-2">
                    {showCategoryRow && (
                        <>
                            <MaterialIcon name="label" size={16} className="text-secondary shrink-0" />
                            <input
                                type="text"
                                value={group.category}
                                onChange={(e) => onChange({ ...group, category: e.target.value })}
                                placeholder="Category title (optional — e.g. Frontend, Backend, Tools)"
                                className="flex-1 h-9 px-3 border border-outline-variant rounded-lg bg-white text-sm text-on-surface placeholder:text-secondary/50 focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all"
                            />
                        </>
                    )}
                    {resumeId && (
                        <button
                            type="button"
                            onClick={() => setShowPolish((v) => !v)}
                            className={`flex items-center gap-1 px-2.5 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${!showCategoryRow ? "ml-auto" : ""} ${
                                showPolish
                                    ? "bg-primary text-on-primary"
                                    : "text-primary border border-primary/30 hover:bg-primary-fixed/20"
                            }`}
                            aria-label="Polish this group with AI"
                        >
                            <MaterialIcon name="auto_awesome" size={12} filled={showPolish} />
                            <span className="hidden sm:inline">{showPolish ? "Close AI" : "Polish"}</span>
                        </button>
                    )}
                    {canRemove && (
                        <button
                            type="button"
                            onClick={onRemove}
                            className="p-1 text-secondary hover:text-error transition-colors rounded-lg hover:bg-error/10"
                            aria-label="Remove category"
                        >
                            <MaterialIcon name="delete_outline" size={18} />
                        </button>
                    )}
                </div>
            )}

            {/* Skill input */}
            <div className="relative">
                <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                    <MaterialIcon name="search" size={18} className="text-outline" />
                </span>
                <input
                    ref={inputRef}
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder="Type a skill and press Enter (e.g. React, SQL)"
                    className="w-full h-10 pl-10 pr-4 border border-outline-variant rounded-lg bg-surface-container font-input-text text-input-text focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all"
                />
                {input.length > 0 && (
                    <div className="absolute top-full left-0 right-0 bg-white border border-outline-variant rounded-lg shadow-lg z-10 max-h-40 overflow-y-auto mt-1">
                        {/* Live type-ahead from the skill input — show a "press Enter" hint */}
                        <button
                            type="button"
                            onClick={() => addSkill(input)}
                            className="w-full text-left px-4 py-2 text-sm hover:bg-surface-container-low text-on-surface transition-colors flex items-center gap-2"
                        >
                            <MaterialIcon name="add_circle" size={14} className="text-primary" />
                            Add &ldquo;{input}&rdquo;
                        </button>
                    </div>
                )}
            </div>

            {/* Added skills — drop zone + drag to reorder/move */}
            <div
                className={`flex flex-wrap items-center gap-2 min-h-[2.5rem] rounded-lg p-1 transition-all ${
                    isDropTarget ? "ring-1 ring-primary/30 bg-primary/5" : ""
                }`}
                onDragOver={(e) => { e.preventDefault(); onGroupDragOver?.(e); }}
                onDrop={(e) => { e.preventDefault(); onGroupDrop?.(); }}
            >
                {group.items.length === 0 && isDropTarget && (
                    <span className="text-xs text-primary/70 italic mx-auto pointer-events-none">
                        Drop skill here
                    </span>
                )}
                {group.items.flatMap((skill, idx) => [
                    ...(dropBeforeIdx === idx && draggingSkill != null && draggingSkill !== skill
                        ? [<div key={`ins-${idx}`} className="self-center w-0.5 h-7 bg-primary rounded-full shrink-0" />]
                        : []),
                    <span
                        key={`${idx}-${skill}`}
                        draggable={!!onSkillDragStart}
                        onDragStart={(e) => { e.stopPropagation(); onSkillDragStart?.(skill, idx); }}
                        onDragEnd={onSkillDragEnd}
                        onDragOver={(e) => { e.stopPropagation(); e.preventDefault(); onSkillDragOver?.(e, idx); }}
                        onDrop={(e) => { e.stopPropagation(); e.preventDefault(); onGroupDrop?.(); }}
                        className={`inline-flex items-center gap-1 px-2.5 py-1.5 bg-primary-fixed/30 text-on-surface rounded-lg text-sm font-medium select-none transition-all ${
                            onSkillDragStart ? "cursor-grab active:cursor-grabbing" : ""
                        } ${draggingSkill === skill ? "opacity-30 scale-95 ring-1 ring-primary/50" : ""}`}
                    >
                        {onSkillDragStart && (
                            <MaterialIcon name="drag_indicator" size={14} className="text-secondary/40 shrink-0 pointer-events-none" />
                        )}
                        {skill}
                        <button
                            type="button"
                            onClick={() => removeSkill(skill)}
                            className="text-secondary hover:text-error transition-colors"
                            aria-label={`Remove ${skill}`}
                        >
                            <MaterialIcon name="close" size={14} />
                        </button>
                    </span>,
                ])}
                {dropBeforeIdx === group.items.length && draggingSkill != null && (
                    <div className="self-center w-0.5 h-7 bg-primary rounded-full shrink-0" />
                )}
            </div>

            {/* Polish with AI panel — scoped to this group */}
            {showPolish && resumeId && (
                <InlinePolishPanel
                    sectionId="skills"
                    resumeId={resumeId}
                    quickPrompts={QUICK_PROMPTS_SKILLS}
                    onApply={handlePolishApply}
                    onClose={() => setShowPolish(false)}
                />
            )}
        </div>
    );
}

// ─── Skills groups input ───────────────────────────────────────────────────────

function SkillsGroupsInput({
    groups,
    suggestions,
    onChange,
    error,
    resumeId,
}: {
    groups: Array<{ category: string; items: string[] }>;
    suggestions: string[];
    onChange: (groups: Array<{ category: string; items: string[] }>) => void;
    error?: string;
    resumeId?: string | null;
}) {
    const [categorized, setCategorized] = useState(
        () => groups.length > 1 || groups.some((g) => g.category !== ""),
    );
    const [targetGroupIdx, setTargetGroupIdx] = useState(0);

    // Saved snapshot of categorized groups when toggling flat mode
    const savedGroups = useRef<Array<{ category: string; items: string[] }> | null>(null);

    // Drag & drop state
    const [dragging, setDragging] = useState<{ skill: string; fromGroup: number; fromIdx: number } | null>(null);
    const [dropTarget, setDropTarget] = useState<{ toGroup: number; beforeIdx: number } | null>(null);

    const allAdded = new Set(groups.flatMap((g) => g.items));
    const availableSuggestions = suggestions.filter((s) => !allAdded.has(s));

    function updateGroup(index: number, updated: { category: string; items: string[] }) {
        const next = [...groups];
        next[index] = updated;
        onChange(next);
    }

    function removeGroup(index: number) {
        onChange(groups.filter((_, i) => i !== index));
        setTargetGroupIdx((prev) => Math.min(prev, groups.length - 2));
    }

    function addGroup() {
        onChange([...groups, { category: "", items: [] }]);
    }

    function toggleCategorized(enabled: boolean) {
        setCategorized(enabled);
        if (!enabled) {
            // Save current categorized groups, then merge to a single flat list
            savedGroups.current = groups;
            onChange([{ category: "", items: groups.flatMap((g) => g.items) }]);
            setTargetGroupIdx(0);
        } else {
            if (savedGroups.current) {
                // Reconcile: restore previous categories, applying changes made in flat mode
                const flatSet = new Set(groups[0]?.items ?? []);
                const prevAll = new Set(savedGroups.current.flatMap((g) => g.items));
                const restored = savedGroups.current
                    .map((g) => ({ ...g, items: g.items.filter((s) => flatSet.has(s)) }))
                    .filter((g) => g.items.length > 0 || g.category.trim() !== "");
                // Append new skills (added while in flat mode) to the first group
                const newItems = (groups[0]?.items ?? []).filter((s) => !prevAll.has(s));
                if (newItems.length > 0) {
                    if (restored.length === 0) {
                        restored.push({ category: "", items: newItems });
                    } else {
                        restored[0] = { ...restored[0], items: [...restored[0].items, ...newItems] };
                    }
                }
                onChange(restored.length > 0 ? restored : [{ category: "", items: [] }]);
                savedGroups.current = null;
            }
        }
    }

    function addSuggestion(skill: string) {
        const idx = Math.min(targetGroupIdx, groups.length - 1);
        const next = [...groups];
        if (!next[idx].items.includes(skill)) {
            next[idx] = { ...next[idx], items: [...next[idx].items, skill] };
        }
        onChange(next);
    }

    // ── Drag & drop handlers ────────────────────────────────────────────────

    function handleSkillDragStart(skill: string, fromGroup: number, fromIdx: number) {
        setDragging({ skill, fromGroup, fromIdx });
    }

    function handleSkillDragEnd() {
        setDragging(null);
        setDropTarget(null);
    }

    function handleSkillDragOver(e: React.DragEvent, toGroup: number, beforeIdx: number) {
        e.preventDefault();
        setDropTarget({ toGroup, beforeIdx });
    }

    function handleGroupDragOver(e: React.DragEvent, toGroup: number) {
        e.preventDefault();
        // Only update if not already set by a chip's handler (chip stops propagation)
        setDropTarget((prev) =>
            prev?.toGroup === toGroup && prev.beforeIdx !== groups[toGroup].items.length
                ? prev
                : { toGroup, beforeIdx: groups[toGroup].items.length },
        );
    }

    function handleGroupDrop(toGroup: number) {
        if (!dragging || !dropTarget || dropTarget.toGroup !== toGroup) return;
        const { skill, fromGroup, fromIdx } = dragging;
        const next = groups.map((g) => ({ ...g, items: [...g.items] }));
        next[fromGroup].items.splice(fromIdx, 1);
        let insertIdx = dropTarget.beforeIdx;
        if (fromGroup === toGroup && fromIdx < insertIdx) insertIdx--;
        insertIdx = Math.max(0, Math.min(insertIdx, next[toGroup].items.length));
        next[toGroup].items.splice(insertIdx, 0, skill);
        onChange(next);
        setDragging(null);
        setDropTarget(null);
    }

    return (
        <div className="space-y-3">
            {/* Categorize toggle */}
            <button
                type="button"
                onClick={() => toggleCategorized(!categorized)}
                className="flex items-center gap-2 text-sm text-secondary hover:text-on-surface transition-colors"
            >
                <MaterialIcon
                    name={categorized ? "check_box" : "check_box_outline_blank"}
                    size={18}
                    className={categorized ? "text-primary" : "text-outline"}
                />
                Group skills by category
            </button>

            {/* Group cards */}
            {groups.map((group, i) => (
                <SkillGroupCard
                    key={i}
                    group={group}
                    showCategoryRow={categorized}
                    onChange={(updated) => updateGroup(i, updated)}
                    onRemove={() => removeGroup(i)}
                    canRemove={categorized && groups.length > 1}
                    resumeId={resumeId}
                    draggingSkill={dragging?.skill ?? null}
                    dropBeforeIdx={dropTarget?.toGroup === i ? dropTarget.beforeIdx : null}
                    isDropTarget={dropTarget?.toGroup === i}
                    onSkillDragStart={(skill, idx) => handleSkillDragStart(skill, i, idx)}
                    onSkillDragEnd={handleSkillDragEnd}
                    onSkillDragOver={(e, beforeIdx) => handleSkillDragOver(e, i, beforeIdx)}
                    onGroupDragOver={(e) => handleGroupDragOver(e, i)}
                    onGroupDrop={() => handleGroupDrop(i)}
                />
            ))}

            {/* Add Category button — only in categorized mode */}
            {categorized && (
                <button
                    type="button"
                    onClick={addGroup}
                    className="flex items-center gap-2 px-4 py-2 text-sm text-primary border border-primary/30 rounded-lg hover:bg-primary-fixed/10 transition-colors font-medium"
                >
                    <MaterialIcon name="add" size={16} />
                    Add Category
                </button>
            )}

            {error && <p className="text-xs text-error">{error}</p>}

            {/* Global AI Skill Suggestions panel */}
            {availableSuggestions.length > 0 && (
                <div className="rounded-xl border border-primary/20 overflow-hidden">
                    <div className="flex items-center gap-2 px-4 py-2.5 bg-primary/5 border-b border-primary/15">
                        <MaterialIcon name="auto_awesome" size={14} className="text-primary" filled />
                        <span className="text-xs font-bold text-primary uppercase tracking-wide">
                            AI Skill Suggestions
                        </span>
                        <span className="ml-auto text-xs text-secondary">
                            {availableSuggestions.length} available
                        </span>
                        {/* Group selector — only shown when there are multiple named groups */}
                        {categorized && groups.length > 1 && (
                            <select
                                value={targetGroupIdx}
                                onChange={(e) => setTargetGroupIdx(Number(e.target.value))}
                                className="ml-2 h-7 px-2 text-xs border border-outline-variant rounded-lg bg-white text-on-surface focus:outline-none focus:ring-1 focus:ring-primary"
                            >
                                {groups.map((g, i) => (
                                    <option key={i} value={i}>
                                        {g.category.trim() || `Group ${i + 1}`}
                                    </option>
                                ))}
                            </select>
                        )}
                    </div>
                    {categorized && groups.length > 1 && (
                        <p className="px-4 pt-2 text-xs text-secondary">
                            Adding to: <strong>{groups[Math.min(targetGroupIdx, groups.length - 1)].category || `Group ${Math.min(targetGroupIdx, groups.length - 1) + 1}`}</strong>
                        </p>
                    )}
                    <div className="px-4 py-3 flex flex-wrap gap-2 bg-white">
                        {availableSuggestions.map((s) => (
                            <button
                                key={s}
                                type="button"
                                onClick={() => addSuggestion(s)}
                                className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-primary/5 border border-primary/25 rounded-lg text-xs font-medium text-primary hover:bg-primary hover:text-on-primary transition-colors"
                            >
                                <MaterialIcon name="add" size={12} />
                                {s}
                            </button>
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
}


// ─── Page ─────────────────────────────────────────────────────────────────────

export default function Step6Page() {
    const router = useRouter();
    const { formData, updateFormData, skillSuggestions } = useWizardStore();
    const [skillGroups, setSkillGroups] = useState<Array<{ category: string; items: string[] }>>(
        formData.skills.length > 0 ? formData.skills : [{ category: "", items: [] }],
    );
    const [showSummaryPolish, setShowSummaryPolish] = useState(false);

    const resumeId = formData.createdResumeId;

    const {
        register,
        handleSubmit,
        setValue,
        watch,
        formState: { errors },
    } = useForm<Step6Values>({
        resolver: zodResolver(step6Schema),
        defaultValues: {
            summary: formData.summary,
            skills: formData.skills.length > 0 ? formData.skills : [{ category: "", items: [] }],
        },
    });

    // Keep RHF skills in sync with local state
    useEffect(() => {
        setValue("skills", skillGroups, { shouldValidate: true });
    }, [skillGroups, setValue]);

    // Auto-save summary to store on every keystroke
    useEffect(() => {
        const sub = watch((values) => {
            if (values.summary !== undefined) {
                updateFormData({ summary: values.summary });
            }
        });
        return () => sub.unsubscribe();
    }, [watch, updateFormData]);

    // Auto-save skills to store whenever local state changes
    useEffect(() => {
        updateFormData({ skills: skillGroups });
    }, [skillGroups, updateFormData]);

    function onSubmit(values: Step6Values) {
        updateFormData({ summary: values.summary, skills: skillGroups });
        router.push("/create/steps/7");
    }

    return (
        <div className="px-8 py-10 max-w-[768px] mx-auto pb-16">
            <WizardProgress
                step={6}
                total={8}
                title="Summary & Skills"
                subtitle={
                    resumeId
                        ? "Edit your summary and skills. Use Polish with AI to improve them."
                        : "Craft a compelling narrative of your professional journey and highlight your core strengths."
                }
            />

            <form onSubmit={handleSubmit(onSubmit)} noValidate className="space-y-8">
                {/* Professional Summary */}
                <section className="space-y-4">
                    <div className="flex items-center gap-3">
                        <MaterialIcon name="description" size={22} className="text-primary" />
                        <h2 className="font-h2 text-h2 text-on-surface">Professional Summary</h2>
                    </div>

                    <div className="bg-white border border-outline-variant/30 rounded-xl p-6 shadow-sm">
                        <div className="flex items-center justify-between mb-2">
                            <label className="block font-label-caps text-label-caps text-secondary uppercase">
                                Your Impact Statement <span className="text-error">*</span>
                            </label>
                            {resumeId && (
                                <button
                                    type="button"
                                    onClick={() => setShowSummaryPolish((v) => !v)}
                                    className={`flex items-center gap-1.5 px-3 py-1 rounded-lg text-xs font-bold shrink-0 transition-colors ${showSummaryPolish
                                        ? "bg-primary text-on-primary"
                                        : "text-primary border border-primary/30 hover:bg-primary-fixed/20"
                                        }`}
                                >
                                    <MaterialIcon name="auto_awesome" size={12} filled={showSummaryPolish} />
                                    {showSummaryPolish ? "Close AI" : "Polish with AI"}
                                </button>
                            )}
                        </div>
                        <textarea
                            {...register("summary")}
                            rows={6}
                            placeholder="e.g. Dedicated Software Engineer with 5+ years of experience building scalable web applications..."
                            className="w-full px-4 py-3 border border-outline-variant rounded-lg bg-white font-input-text text-input-text focus:ring-2 focus:ring-primary/20 focus:border-primary outline-none transition-all resize-none"
                        />
                        <div className="mt-2 flex items-center justify-between">
                            <span className="text-xs text-secondary font-body-sm">
                                Recommended: 200–400 characters
                            </span>
                        </div>
                        {errors.summary && (
                            <p className="text-xs text-error mt-1">{errors.summary.message}</p>
                        )}
                        {showSummaryPolish && resumeId && (
                            <InlinePolishPanel
                                sectionId="summary"
                                resumeId={resumeId}
                                quickPrompts={QUICK_PROMPTS_SUMMARY}
                                targetFormat={null}
                                onApply={(text) => {
                                    setValue("summary", text, { shouldValidate: true, shouldDirty: true });
                                }}
                                onClose={() => setShowSummaryPolish(false)}
                            />
                        )}
                    </div>
                </section>

                {/* Skills */}
                <section className="space-y-4">
                    <div className="flex items-center gap-3">
                        <MaterialIcon name="bolt" size={22} className="text-primary" />
                        <h2 className="font-h2 text-h2 text-on-surface">Technical &amp; Soft Skills</h2>
                    </div>

                    <div className="bg-white border border-outline-variant/30 rounded-xl p-6 shadow-sm space-y-4">
                        <SkillsGroupsInput
                            groups={skillGroups}
                            suggestions={skillSuggestions}
                            onChange={(updated) => {
                                setSkillGroups(updated);
                                setValue("skills", updated, { shouldValidate: true });
                            }}
                            error={errors.skills?.message}
                            resumeId={resumeId}
                        />
                    </div>
                </section>

                {/* Navigation */}
                <div className="flex items-center justify-between border-t border-outline-variant/20 pt-6">
                    <button
                        type="button"
                        onClick={() => router.push("/create/steps/5")}
                        className="flex items-center gap-2 px-6 py-2.5 rounded-lg text-secondary font-semibold hover:text-on-surface transition-colors"
                    >
                        <MaterialIcon name="arrow_back" size={18} />
                        Back
                    </button>
                    <button
                        type="submit"
                        className="flex items-center gap-2 px-8 py-2.5 rounded-lg bg-primary text-on-primary font-semibold hover:opacity-90 transition-opacity shadow-sm"
                    >
                        Save &amp; Continue
                        <MaterialIcon name="arrow_forward" size={18} />
                    </button>
                </div>
            </form>
        </div>
    );
}
