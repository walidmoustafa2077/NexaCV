import { identityFetch } from "./client";
import type { AuthResponse, LoginRequest, RegisterRequest, TokenRequest } from "@/types/api.types";

/** Register a new user via the Identity service. */
export function register(data: RegisterRequest): Promise<AuthResponse> {
    return identityFetch<AuthResponse>("/api/auth/register", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

/** Login via the Identity service. */
export function login(data: LoginRequest): Promise<AuthResponse> {
    return identityFetch<AuthResponse>("/api/auth/login", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

/** Exchange a Refresh Token for a new Access Token + Refresh Token pair. */
export function refreshTokens(data: TokenRequest): Promise<AuthResponse> {
    return identityFetch<AuthResponse>("/api/auth/refresh", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

/** Explicitly revoke a Refresh Token (e.g. on logout). */
export function revokeToken(data: TokenRequest): Promise<void> {
    return identityFetch<void>("/api/auth/revoke", {
        method: "POST",
        body: JSON.stringify(data),
    });
}
