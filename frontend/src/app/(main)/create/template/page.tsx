"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { getTemplates } from "@/lib/api/templates";
import { queryKeys } from "@/lib/query/keys";
import { useWizardStore } from "@/store/wizardStore";
import MaterialIcon from "@/components/shared/MaterialIcon";
import type { TemplateDto } from "@/types/api.types";

// ─── Category tabs ────────────────────────────────────────────────────────────

const CATEGORIES = ["All Styles", "Creative", "Executive", "Minimalist"];

function matchesFilter(template: TemplateDto, filter: string): boolean {
    if (filter === "All Styles") return true;
    const cat = template.industryCategory.toLowerCase();
    if (filter === "Creative") return cat.includes("creative") || cat.includes("design") || cat.includes("art");
    if (filter === "Executive") return cat.includes("executive") || cat.includes("leadership") || cat.includes("senior") || cat.includes("management");
    if (filter === "Minimalist") return cat.includes("minimalist") || cat.includes("corporate") || cat.includes("professional") || cat.includes("engineering");
    return true;
}

// ─── Decorative template thumbnail ───────────────────────────────────────────

const CARD_THEMES = [
    { bg: "bg-[#1a3a3a]", accent: "bg-teal-400", lineSm: "bg-teal-300/20", lineBase: "bg-white/20" },
    { bg: "bg-[#1e2235]", accent: "bg-slate-300", lineSm: "bg-slate-400/15", lineBase: "bg-white/15" },
    { bg: "bg-[#fafaf8]", accent: "bg-slate-700", lineSm: "bg-slate-300/80", lineBase: "bg-slate-300", isDark: false },
    { bg: "bg-[#1a2436]", accent: "bg-blue-400", lineSm: "bg-blue-300/20", lineBase: "bg-white/20" },
    { bg: "bg-[#1e1e2e]", accent: "bg-purple-400", lineSm: "bg-purple-300/20", lineBase: "bg-white/20" },
    { bg: "bg-[#2a1a1a]", accent: "bg-rose-400", lineSm: "bg-rose-300/20", lineBase: "bg-white/20" },
];

function TemplateThumbnail({ index }: { index: number }) {
    const theme = CARD_THEMES[index % CARD_THEMES.length];
    const isDark = theme.isDark !== false;
    const docBg = isDark ? "bg-white/10 border-white/15" : "bg-white border-slate-200";

    return (
        <div className={`w-full h-full ${theme.bg} flex flex-col items-center justify-center p-6`}>
            <div className={`w-24 h-32 ${docBg} border rounded-md p-2.5 flex flex-col gap-1.5 shadow-lg`}>
                <div className={`h-2.5 w-3/5 rounded-full ${theme.accent} opacity-80`} />
                <div className={`h-1.5 w-4/5 rounded-full ${theme.lineBase}`} />
                <div className={`h-px w-full ${theme.lineBase} mt-0.5`} />
                <div className="space-y-1 mt-0.5">
                    {[90, 70, 85, 60, 78, 65].map((w, i) => (
                        <div key={i} className={`h-1 rounded-full ${theme.lineSm}`} style={{ width: `${w}%` }} />
                    ))}
                </div>
                <div className={`h-1.5 w-1/3 rounded-full ${theme.accent} opacity-50 mt-1`} />
                <div className="space-y-1">
                    {[75, 60, 70].map((w, i) => (
                        <div key={i} className={`h-1 rounded-full ${theme.lineSm}`} style={{ width: `${w}%` }} />
                    ))}
                </div>
            </div>
        </div>
    );
}

// ─── Skeleton card ────────────────────────────────────────────────────────────

function TemplateSkeleton() {
    return (
        <div className="bg-white rounded-2xl border border-slate-100 overflow-hidden animate-pulse">
            <div className="aspect-[3/4] bg-slate-100" />
            <div className="p-4 space-y-2">
                <div className="h-4 w-32 bg-slate-200 rounded-full" />
                <div className="h-3 w-24 bg-slate-100 rounded-full" />
            </div>
        </div>
    );
}

// ─── Custom Canvas card ───────────────────────────────────────────────────────

function CustomCanvasCard({ onSelect }: { onSelect: () => void }) {
    return (
        <div
            onClick={onSelect}
            className="group flex flex-col bg-white rounded-2xl border-2 border-dashed border-slate-200 overflow-hidden hover:border-primary/40 hover:shadow-lg transition-all duration-200 cursor-pointer"
        >
            <div className="aspect-[3/4] flex flex-col items-center justify-center gap-4 bg-slate-50 group-hover:bg-primary/5 transition-colors">
                <div className="w-12 h-12 rounded-full border-2 border-dashed border-slate-300 group-hover:border-primary/50 flex items-center justify-center transition-colors">
                    <MaterialIcon name="add" size={24} className="text-slate-400 group-hover:text-primary transition-colors" />
                </div>
                <div className="text-center px-4">
                    <p className="font-semibold text-slate-600 group-hover:text-primary transition-colors text-sm">Custom Canvas</p>
                    <p className="text-xs text-slate-400 mt-1">Start from scratch and build your own unique layout.</p>
                    <span className="text-xs text-primary font-medium mt-2 inline-block">Start Blank Page</span>
                </div>
            </div>
        </div>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function TemplatePage() {
    const router = useRouter();
    const { updateFormData, reset } = useWizardStore();
    const [activeFilter, setActiveFilter] = useState("All Styles");

    const { data: templates, isLoading } = useQuery({
        queryKey: queryKeys.templates(),
        queryFn: () => getTemplates(),
    });

    function handleSelect(templateId: number) {
        reset();
        updateFormData({ templateId });
        router.push("/create/steps/1");
    }

    const filtered = (templates ?? []).filter((t) => matchesFilter(t, activeFilter));

    return (
        <div className="w-full min-h-screen flex flex-col">
            {/* Header row */}
            <div className="flex flex-col md:flex-row md:items-start justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Explore Resume Templates</h1>
                    <p className="text-sm text-slate-500 mt-1">Select a foundation that reflects your professional identity.</p>
                </div>
                {/* Filter tabs */}
                <div className="flex items-center gap-1.5 flex-wrap shrink-0">
                    {CATEGORIES.map((cat) => (
                        <button
                            key={cat}
                            onClick={() => setActiveFilter(cat)}
                            className={`px-4 py-1.5 rounded-full text-sm font-medium transition-all ${activeFilter === cat
                                    ? "bg-primary text-white shadow-sm"
                                    : "bg-white border border-slate-200 text-slate-600 hover:border-primary/40 hover:text-primary"
                                }`}
                        >
                            {cat}
                        </button>
                    ))}
                </div>
            </div>

            {/* Grid */}
            <div className="flex-1">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5">
                    {isLoading
                        ? Array.from({ length: 6 }).map((_, i) => <TemplateSkeleton key={i} />)
                        : <>
                            {filtered.map((t, i) => (
                                <button
                                    key={t.id}
                                    onClick={() => handleSelect(t.id)}
                                    className="group flex flex-col bg-white rounded-2xl border border-slate-100 overflow-hidden hover:shadow-xl hover:-translate-y-0.5 transition-all duration-200 text-left shadow-sm"
                                >
                                    {/* Thumbnail */}
                                    <div className="aspect-[3/4] relative overflow-hidden">
                                        <TemplateThumbnail index={i} />
                                        {i === 0 && (
                                            <span className="absolute top-3 left-3 bg-amber-400 text-amber-900 text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full">
                                                Popular
                                            </span>
                                        )}
                                        <div className="absolute inset-0 bg-black/30 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                                            <span className="bg-white text-slate-900 px-5 py-2 rounded-full font-semibold text-sm shadow-lg">
                                                Use Template
                                            </span>
                                        </div>
                                    </div>
                                    {/* Info */}
                                    <div className="px-4 py-3">
                                        <h3 className="font-semibold text-sm text-slate-800 group-hover:text-primary transition-colors">{t.name}</h3>
                                        <p className="text-xs text-slate-400 mt-0.5">{t.industryCategory}</p>
                                    </div>
                                </button>
                            ))}
                            <CustomCanvasCard onSelect={() => handleSelect(0)} />
                        </>
                    }
                </div>
            </div>

            {/* Footer */}
            <footer className="mt-12 pt-6 border-t border-slate-100 flex items-center justify-between text-xs text-slate-400">
                <span>© 2024 NexaCV. All resume templates are AI-optimized.</span>
                <div className="flex items-center gap-4">
                    <button className="hover:text-slate-600 transition-colors">Privacy</button>
                    <button className="hover:text-slate-600 transition-colors">Terms</button>
                    <button className="hover:text-slate-600 transition-colors">Support</button>
                </div>
            </footer>
        </div>
    );
}
