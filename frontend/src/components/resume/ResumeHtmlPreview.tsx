"use client";

import React, { useRef, useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { renderResumeHtml } from "@/lib/api/resumes";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { cn } from "@/lib/utils";

// A4 dimensions in CSS pixels at 96 dpi
const A4_W = 794;
const A4_H = 1123;
/** Visible gap between page cards (in scaled pixels). */
const PAGE_GAP = 16;

interface Props {
    resumeId: string;
    className?: string;
    style?: React.CSSProperties;
}

export function ResumeHtmlPreview({ resumeId, className = "", style }: Props) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [scale, setScale] = useState(0);
    const [pages, setPages] = useState(1);
    // Tracks the last diagnostics filename saved so two iframes don't save twice
    const savedDiagRef = useRef<string | null>(null);

    // Measure container width and recompute scale whenever it changes
    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;
        const update = () => {
            if (el.offsetWidth > 0) setScale(el.offsetWidth / A4_W);
        };
        update();
        const obs = new ResizeObserver(update);
        obs.observe(el);
        return () => obs.disconnect();
    }, []);

    // Receive messages from the diagnostic script running inside the iframe(s)
    useEffect(() => {
        const handler = (e: MessageEvent) => {
            if (e.data?.type === "nexacv-layout") {
                setPages(Math.max(1, Number(e.data.pages) || 1));
            }
            if (e.data?.type === "nexacv-diagnostics" && e.data.log && e.data.filename) {
                // Two iframes (page 1 + page 2) both run the script; only save once.
                if (savedDiagRef.current === e.data.filename) return;
                savedDiagRef.current = e.data.filename;

                const blob = new Blob(
                    [JSON.stringify(e.data.log, null, 2)],
                    { type: "application/json" },
                );
                const url = URL.createObjectURL(blob);
                const a = document.createElement("a");
                a.href = url;
                a.download = e.data.filename;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);
            }
        };
        window.addEventListener("message", handler);
        return () => window.removeEventListener("message", handler);
    }, []);

    // Reset to one page and clear saved diagnostics ref whenever a new resume loads
    useEffect(() => { setPages(1); savedDiagRef.current = null; }, [resumeId]);

    const { data: html, isLoading, isError, error } = useQuery({
        queryKey: ["resume-render", resumeId],
        queryFn: () => renderResumeHtml(resumeId),
        staleTime: 1000 * 60 * 5,
        retry: 1,
    });

    // Height of one A4 page in the scaled (container) coordinate space
    const scaledPageH = scale > 0 ? Math.round(A4_H * scale) : undefined;
    // Total container height: pages stacked with a visible gap between them
    const totalHeight = scaledPageH
        ? scaledPageH * pages + (pages - 1) * PAGE_GAP
        : undefined;

    const isReady = html != null && scale > 0;

    const iframeBase: React.CSSProperties = {
        position: "absolute",
        left: 0,
        width: A4_W,
        border: "none",
        overflow: "hidden",
        transformOrigin: "top left",
        transform: `scale(${scale})`,
    };

    return (
        <div
            ref={containerRef}
            className={cn("relative", className)}
            style={{
                ...(totalHeight ? { height: totalHeight } : { aspectRatio: `${A4_W} / ${A4_H}` }),
                ...style,
            }}
        >
            {/* ── Page 1 card ──────────────────────────────────────────────── */}
            <div
                className="absolute overflow-hidden rounded-xl bg-white"
                style={{ top: 0, left: 0, right: 0, height: scaledPageH ?? "100%" }}
            >
                {/* Loading spinner */}
                {(isLoading || scale === 0) && (
                    <div className="absolute inset-0 flex items-center justify-center bg-slate-50">
                        <div className="flex flex-col items-center gap-3 text-slate-400">
                            <div className="w-8 h-8 border-2 border-primary/30 border-t-primary rounded-full animate-spin" />
                            <p className="text-sm">Rendering preview…</p>
                        </div>
                    </div>
                )}

                {/* Error state */}
                {isError && (
                    <div className="absolute inset-0 flex items-center justify-center bg-slate-50">
                        <div className="flex flex-col items-center gap-3 text-slate-400 p-8 text-center">
                            <MaterialIcon name="broken_image" size={40} />
                            <p className="text-sm font-medium text-slate-600">Preview unavailable</p>
                            <p className="text-xs text-slate-400">
                                {error instanceof Error ? error.message : "Failed to load"}
                            </p>
                        </div>
                    </div>
                )}

                {/* Page 1 iframe */}
                {isReady && (
                    <iframe
                        srcDoc={html}
                        scrolling="no"
                        style={{ ...iframeBase, top: 0, height: A4_H }}
                        title="Resume Preview"
                        sandbox="allow-scripts"
                    />
                )}
            </div>

            {/* ── Page 2 card — appears after optimizer reports pages≥2 ───── */}
            {isReady && pages >= 2 && scaledPageH != null && (
                <div
                    className="absolute overflow-hidden rounded-xl bg-white"
                    style={{ top: scaledPageH + PAGE_GAP, left: 0, right: 0, height: scaledPageH }}
                >
                    {/*
                     * The second iframe renders the same HTML. We shift it up by exactly
                     * one scaled page-height so the content that lives at document y=A4_H
                     * appears at the top of this card's viewport.
                     *
                     * Math: top = -(A4_H * scale)
                     *   Visual top-left stays at -(A4_H×s), visual height = 2×A4_H×s
                     *   ⇒ visible window = document y ∈ [A4_H, 2×A4_H] ✓
                     */}
                    <iframe
                        key={`${resumeId}-p2`}
                        srcDoc={html}
                        scrolling="no"
                        style={{
                            ...iframeBase,
                            top: -(A4_H * scale),
                            height: A4_H * 2,
                        }}
                        title="Resume Preview — Page 2"
                        sandbox="allow-scripts"
                    />
                </div>
            )}
        </div>
    );
}
