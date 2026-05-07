import { apiFetch } from "./client";
import type { UserProfileDto, UpdateUserRequest } from "@/types/api.types";

export function getMe(): Promise<UserProfileDto> {
    return apiFetch<UserProfileDto>("/api/users/me");
}

export function updateMe(data: UpdateUserRequest): Promise<UserProfileDto> {
    return apiFetch<UserProfileDto>("/api/users/me", {
        method: "PUT",
        body: JSON.stringify(data),
    });
}
