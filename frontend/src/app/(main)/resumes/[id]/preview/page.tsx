"use client";

import { useParams, useRouter } from "next/navigation";
import { useResumeDetail } from "@/hooks/useResumeDetail";
import { ResumeHtmlPreview } from "@/components/resume/ResumeHtmlPreview";
import MaterialIcon from "@/components/shared/MaterialIcon";

export default function ResumePreviewPage() {
    const params = useParams<{ id: string }>();
    const router = useRouter();
    const { data: resume } = useResumeDetail(params.id);

    return (
        <div className="min-h-screen bg-slate-100 flex flex-col">
            {/* Top bar */}
            <div className="sticky top-0 z-10 bg-white border-b border-slate-200 flex items-center gap-4 px-5 py-3 shadow-sm">
                <button
                    onClick={() => router.push(`/resumes/${params.id}`)}
                    className="flex items-center gap-1.5 text-sm text-slate-600 hover:text-slate-900 transition-colors"
                >
                    <MaterialIcon name="arrow_back" size={18} />
                    Back
                </button>
                <div className="flex-1">
                    <h1 className="font-semibold text-slate-900 text-sm">
                        {resume ? `${resume.templateName} — Preview` : "Resume Preview"}
                    </h1>
                    {resume?.finalData?.content?.personal && (
                        <p className="text-xs text-slate-400">
                            {resume.finalData.content.personal.firstName}{" "}
                            {resume.finalData.content.personal.lastName}
                        </p>
                    )}
                </div>
                <span className="text-[10px] font-bold uppercase tracking-wide bg-primary/10 text-primary px-2.5 py-1 rounded-full">
                    Live Preview
                </span>
            </div>

            {/* Iframe preview — A4 proportions */}
            <div className="flex-1 flex items-start justify-center py-8 px-4">
                <div className="w-full max-w-[860px]">
                    <ResumeHtmlPreview
                        resumeId={params.id}
                        className="w-full shadow-xl"
                    />
                </div>
            </div>
        </div>
    );
}
