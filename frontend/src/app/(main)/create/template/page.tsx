"use client";

import { useState, useEffect, useRef, useMemo } from "react";
import { useQuery } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { toast } from "sonner";
import { getTemplates } from "@/lib/api/templates";
import { getResume, getResumes } from "@/lib/api/resumes";
import { queryKeys } from "@/lib/query/keys";
import { useWizardStore } from "@/store/wizardStore";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { renderTemplatePreview } from "@/lib/templatePreview";
import type { TemplateDto } from "@/types/api.types";

// ─── Category filters ─────────────────────────────────────────────────────────

const CATEGORIES = ["All", "Executive", "Creative", "ModernTech", "Minimalist"] as const;
type Category = (typeof CATEGORIES)[number];

const CATEGORY_LABELS: Record<string, string> = {
    All: "All Styles",
    Executive: "Executive",
    Creative: "Creative",
    ModernTech: "Modern Tech",
    Minimalist: "Minimalist",
};

function matchesFilter(template: TemplateDto, filter: Category): boolean {
    if (filter === "All") return true;
    // Match against styleCategory first, fall back to industryCategory
    const style = (template.styleCategory ?? "").toLowerCase();
    const industry = (template.industryCategory ?? "").toLowerCase();
    const key = filter.toLowerCase();
    return style === key || industry.includes(key) || style.includes(key);
}

// ─── Template thumbnail (client-side mock render) ─────────────────────────────

const A4_W = 794;
const A4_H = 1123;

function TemplateThumbnail({ template }: { template: TemplateDto }) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [scale, setScale] = useState(0.35);

    // Pre-render once; memoised so it doesn't recalculate on every re-render
    const srcdoc = useMemo(
        () => (template.htmlContent ? renderTemplatePreview(template.htmlContent) : ""),
        [template.htmlContent],
    );

    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;
        const update = () => setScale(el.offsetWidth / A4_W);
        update();
        const obs = new ResizeObserver(update);
        obs.observe(el);
        return () => obs.disconnect();
    }, []);

    if (!srcdoc) {
        // No HTML content yet — show a neutral placeholder
        return (
            <div ref={containerRef} className="w-full h-full flex items-center justify-center bg-slate-100">
                <span className="text-xs text-slate-400">No preview</span>
            </div>
        );
    }

    return (
        <div ref={containerRef} className="w-full h-full overflow-hidden relative bg-slate-50">
            <iframe
                srcDoc={srcdoc}
                style={{
                    position: "absolute",
                    top: 0,
                    left: 0,
                    width: A4_W,
                    height: A4_H,
                    border: "none",
                    transformOrigin: "top left",
                    transform: `scale(${scale})`,
                    pointerEvents: "none",
                }}
                title={`${template.name} preview`}
                sandbox="allow-same-origin"
            />
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

// ─── Style badge metadata ─────────────────────────────────────────────────────

const STYLE_BADGE: Record<string, { label: string; className: string }> = {
    executive: { label: "Executive", className: "bg-amber-400/20 text-amber-700 border border-amber-400/40" },
    creative: { label: "Creative", className: "bg-sky-400/20 text-sky-700 border border-sky-400/40" },
    moderntech: { label: "Modern Tech", className: "bg-cyan-400/20 text-cyan-700 border border-cyan-400/40" },
    minimalist: { label: "Minimalist", className: "bg-slate-200/60 text-slate-600 border border-slate-300" },
};

function getStyleBadge(template: TemplateDto) {
    const key = (template.styleCategory ?? "").toLowerCase().replace(/\s/g, "");
    return STYLE_BADGE[key] ?? { label: template.styleCategory ?? "Standard", className: "bg-slate-100 text-slate-600 border border-slate-200" };
}

// ─── Template card ────────────────────────────────────────────────────────────

function TemplateCard({
    template,
    index,
    onSelect,
    disabled,
}: {
    template: TemplateDto;
    index: number;
    onSelect: () => void;
    disabled?: boolean;
}) {
    const badge = getStyleBadge(template);

    return (
        <button
            onClick={onSelect}
            disabled={disabled}
            className="group flex flex-col bg-white rounded-2xl border border-slate-100 overflow-hidden hover:shadow-xl hover:-translate-y-0.5 transition-all duration-200 text-left shadow-sm w-full disabled:pointer-events-none disabled:opacity-60"
        >
            {/* Thumbnail */}
            <div className="aspect-[3/4] relative overflow-hidden">
                <TemplateThumbnail template={template} />
                {/* Badges */}
                {index === 0 && (
                    <span className="absolute top-3 left-3 bg-amber-400 text-amber-900 text-[10px] font-bold uppercase tracking-wider px-2.5 py-0.5 rounded-full shadow">
                        Popular
                    </span>
                )}
                {template.supportsWord && (
                    <span className="absolute top-3 right-3 bg-white/90 text-slate-700 text-[9px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-full shadow flex items-center gap-1">
                        <MaterialIcon name="description" size={11} /> DOCX
                    </span>
                )}
                {/* Hover overlay */}
                <div className="absolute inset-0 bg-black/35 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                    <span className="bg-white text-slate-900 px-5 py-2 rounded-full font-semibold text-sm shadow-lg">
                        Use Template
                    </span>
                </div>
            </div>
            {/* Info */}
            <div className="px-4 py-3 flex items-start justify-between gap-2">
                <div>
                    <h3 className="font-semibold text-sm text-slate-800 group-hover:text-primary transition-colors">
                        {template.name}
                    </h3>
                    <p className="text-xs text-slate-400 mt-0.5">{template.industryCategory}</p>
                </div>
                <span className={`text-[9px] font-bold uppercase tracking-wide px-2 py-0.5 rounded-full flex-shrink-0 mt-0.5 ${badge.className}`}>
                    {badge.label}
                </span>
            </div>
        </button>
    );
}

// ─── Page ─────────────────────────────────────────────────────────────────────

export default function TemplatePage() {
    const router = useRouter();
    const { updateFormData, reset, initFromResume } = useWizardStore();
    const [activeFilter, setActiveFilter] = useState<Category>("All");
    const [isNavigating, setIsNavigating] = useState(false);

    const { data: templates, isLoading } = useQuery({
        queryKey: queryKeys.templates(),
        queryFn: () => getTemplates(),
    });

    async function handleSelect(templateId: number) {
        if (isNavigating) return;
        setIsNavigating(true);
        try {
            // Fetch the user's resumes to find the most recently created one
            const resumes = await getResumes();
            const sorted = [...resumes].sort(
                (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
            );
            const lastResume = sorted[0];

            if (lastResume) {
                // Fetch full detail and pre-populate the wizard
                const detail = await getResume(lastResume.id);
                initFromResume(detail);
                // Override: new resume — clear identity fields, set the chosen template
                updateFormData({ templateId, createdResumeId: null, resumeName: "" });
                toast.info("Pre-filled from your last resume", {
                    description: "Your previous information was loaded. Edit anything you like.",
                    action: {
                        label: "Start fresh",
                        onClick: () => {
                            reset();
                            updateFormData({ templateId });
                        },
                    },
                    duration: 8000,
                });
            } else {
                reset();
                updateFormData({ templateId });
            }
            router.push("/create/steps/1");
        } catch {
            // If fetching previous resumes fails, fall back to a clean wizard
            reset();
            updateFormData({ templateId });
            router.push("/create/steps/1");
        } finally {
            setIsNavigating(false);
        }
    }

    const filtered = (templates ?? []).filter((t) => matchesFilter(t, activeFilter));

    // Sort: executive → creative → moderntech
    const ORDER: Record<string, number> = { executive: 0, creative: 1, moderntech: 2 };
    const sorted = [...filtered].sort((a, b) => {
        const aO = ORDER[(a.styleCategory ?? "").toLowerCase()] ?? 3;
        const bO = ORDER[(b.styleCategory ?? "").toLowerCase()] ?? 3;
        return aO - bO;
    });

    const counts: Record<string, number> = {};
    (templates ?? []).forEach((t) => {
        const key = (t.styleCategory ?? "default").toLowerCase().replace(/\s/g, "");
        counts[key] = (counts[key] ?? 0) + 1;
    });

    return (
        <div className="w-full min-h-screen flex flex-col">
            {/* Loading overlay while fetching last resume data */}
            {isNavigating && (
                <div className="fixed inset-0 z-50 bg-black/20 backdrop-blur-sm flex items-center justify-center">
                    <div className="bg-white rounded-2xl shadow-xl px-8 py-6 flex flex-col items-center gap-3">
                        <div className="w-8 h-8 border-4 border-primary border-t-transparent rounded-full animate-spin" />
                        <p className="text-sm font-medium text-slate-700">Preparing your resume…</p>
                    </div>
                </div>
            )}
            {/* Header */}
            <div className="flex flex-col md:flex-row md:items-start justify-between gap-4 mb-8">
                <div>
                    <h1 className="text-2xl font-bold text-slate-900">Explore Resume Templates</h1>
                    <p className="text-sm text-slate-500 mt-1">
                        Choose from {templates?.length ?? 0} premium designs — Executive, Creative, and Modern Tech.
                    </p>
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
                            {CATEGORY_LABELS[cat]}
                            {cat !== "All" && templates && (
                                <span className="ml-1.5 text-[10px] opacity-70">
                                    ({counts[(cat.toLowerCase().replace(/\s/g, ""))] ?? 0})
                                </span>
                            )}
                        </button>
                    ))}
                </div>
            </div>

            {/* Grid */}
            <div className="flex-1">
                <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-5">
                    {isLoading
                        ? Array.from({ length: 8 }).map((_, i) => <TemplateSkeleton key={i} />)
                        : sorted.map((t, i) => (
                            <TemplateCard
                                key={t.id}
                                template={t}
                                index={i}
                                onSelect={() => handleSelect(t.id)}
                                disabled={isNavigating}
                            />
                        ))
                    }
                </div>
                {!isLoading && sorted.length === 0 && (
                    <div className="flex flex-col items-center gap-3 py-20 text-slate-400">
                        <MaterialIcon name="search_off" size={40} />
                        <p className="text-sm">No templates found for this filter.</p>
                    </div>
                )}
            </div>

            {/* Footer */}
            <footer className="mt-12 pt-6 border-t border-slate-100 flex items-center justify-between text-xs text-slate-400">
                <span>© {new Date().getFullYear()} NexaCV. All resume templates are AI-optimized.</span>
                <div className="flex items-center gap-4">
                    <button className="hover:text-slate-600 transition-colors">Privacy</button>
                    <button className="hover:text-slate-600 transition-colors">Terms</button>
                    <button className="hover:text-slate-600 transition-colors">Support</button>
                </div>
            </footer>
        </div>
    );
}

