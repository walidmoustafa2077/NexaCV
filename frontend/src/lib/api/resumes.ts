import { apiFetch } from "./client";
import type {
    CreateResumeRequest,
    CreateResumeResponse,
    ResumeDetailDto,
    ResumeSummaryDto,
    UpdateFinalDataRequest,
    RegenerateRequest,
    RegenerateResponse,
} from "@/types/api.types";

export function createResume(data: CreateResumeRequest): Promise<CreateResumeResponse> {
    return apiFetch<CreateResumeResponse>("/api/resumes", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function getResumes(): Promise<ResumeSummaryDto[]> {
    return apiFetch<ResumeSummaryDto[]>("/api/resumes");
}

export function getResume(id: string): Promise<ResumeDetailDto> {
    return apiFetch<ResumeDetailDto>(`/api/resumes/${id}`);
}

export function updateResume(id: string, data: UpdateFinalDataRequest): Promise<ResumeDetailDto> {
    return apiFetch<ResumeDetailDto>(`/api/resumes/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export function deleteResume(id: string): Promise<void> {
    return apiFetch<void>(`/api/resumes/${id}`, { method: "DELETE" });
}

export function renameResume(id: string, name: string): Promise<ResumeSummaryDto> {
    return apiFetch<ResumeSummaryDto>(`/api/resumes/${id}/name`, {
        method: "PATCH",
        body: JSON.stringify({ name }),
    });
}

export function regenerateSection(
    id: string,
    data: RegenerateRequest,
): Promise<RegenerateResponse> {
    return apiFetch<RegenerateResponse>(`/api/resumes/${id}/regenerate`, {
        method: "POST",
        body: JSON.stringify(data),
    });
}

/**
 * Returns a fully rendered HTML string of the resume with all template
 * placeholders replaced by the resume's finalData.
 * The response is text/html so we read it as raw text, not JSON.
 */
export async function renderResumeHtml(id: string): Promise<string> {
    const { useAuthStore } = await import("@/store/authStore");
    const token = useAuthStore.getState().token;
    const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5166";

    const res = await fetch(`${API_URL}/api/resumes/${id}/render`, {
        headers: token ? { Authorization: `Bearer ${token}` } : {},
    });
    if (!res.ok) throw new Error(`Render failed: ${res.status}`);
    return res.text();
}
