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
