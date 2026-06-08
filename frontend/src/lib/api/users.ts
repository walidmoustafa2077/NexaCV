import { apiFetch } from "./client";
import type { UserProfileDto, UpdateUserRequest } from "@/types/api.types";

export function getMe(): Promise<UserProfileDto> {
    return apiFetch<UserProfileDto>("/api/profile/me");
}

export function updateMe(data: UpdateUserRequest): Promise<UserProfileDto> {
    return apiFetch<UserProfileDto>("/api/profile/me", {
        method: "PUT",
        body: JSON.stringify(data),
    });
}
