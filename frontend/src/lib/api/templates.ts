import { apiFetch } from "./client";
import type { TemplateDto } from "@/types/api.types";

export function getTemplates(industryCategory?: string): Promise<TemplateDto[]> {
    const query = industryCategory
        ? `?industryCategory=${encodeURIComponent(industryCategory)}`
        : "";
    return apiFetch<TemplateDto[]>(`/api/templates${query}`);
}

export function getTemplate(id: number): Promise<TemplateDto> {
    return apiFetch<TemplateDto>(`/api/templates/${id}`);
}
