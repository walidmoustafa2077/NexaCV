import { apiFetch } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest } from "@/types/api.types";

export function register(data: RegisterRequest): Promise<AuthResponse> {
    return apiFetch<AuthResponse>("/api/auth/register", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function login(data: LoginRequest): Promise<AuthResponse> {
    return apiFetch<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export function logout(): Promise<void> {
    return apiFetch<void>("/api/auth/logout", { method: "POST" });
}
