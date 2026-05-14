"use client";

import React, { useRef, useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { renderResumeHtml } from "@/lib/api/resumes";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { cn } from "@/lib/utils";

// A4 dimensions in CSS pixels at 96 dpi
const A4_W = 794;
const A4_H = 1123;

interface Props {
    resumeId: string;
    className?: string;
    style?: React.CSSProperties;
}

export function ResumeHtmlPreview({ resumeId, className = "", style }: Props) {
    const containerRef = useRef<HTMLDivElement>(null);
    const [scale, setScale] = useState(0);

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

    const { data: html, isLoading, isError, error } = useQuery({
        queryKey: ["resume-render", resumeId],
        queryFn: () => renderResumeHtml(resumeId),
        staleTime: 1000 * 60 * 5,
        retry: 1,
    });

    // Once we know the scale, height = A4 height scaled to container width
    const scaledH = scale > 0 ? Math.round(A4_H * scale) : undefined;

    return (
        <div
            ref={containerRef}
            className={cn("relative overflow-hidden rounded-xl bg-white", className)}
            style={{
                // While scale is unknown use aspect-ratio so the container has the right shape
                ...(scaledH ? { height: scaledH } : { aspectRatio: `${A4_W} / ${A4_H}` }),
                ...style,
            }}
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

            {/* Scaled iframe — only rendered once we have both HTML and a valid scale */}
            {html && scale > 0 && (
                <iframe
                    srcDoc={html}
                    style={{
                        position: "absolute",
                        top: 0,
                        left: 0,
                        width: A4_W,
                        height: A4_H,
                        border: "none",
                        transformOrigin: "top left",
                        transform: `scale(${scale})`,
                    }}
                    title="Resume Preview"
                    sandbox="allow-same-origin"
                />
            )}
        </div>
    );
}
