import type { ApiErrorResponse, ValidationErrorResponse } from "@/types/api.types";

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5166";

// Lazily imported so the store isn't bundled into server components
let getToken: (() => string | null) | null = null;

export function setTokenProvider(fn: () => string | null) {
    getToken = fn;
}

export class ApiError extends Error {
    constructor(
        public readonly status: number,
        message: string,
    ) {
        super(message);
        this.name = "ApiError";
    }
}

export class ValidationError extends ApiError {
    constructor(
        public readonly details: ValidationErrorResponse["details"],
        message: string,
    ) {
        super(422, message);
        this.name = "ValidationError";
    }
}

export async function apiFetch<T>(
    path: string,
    options: RequestInit = {},
): Promise<T> {
    const token = getToken?.();

    const headers: Record<string, string> = {
        "Content-Type": "application/json",
        ...(options.headers as Record<string, string>),
    };

    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    let response: Response;
    try {
        response = await fetch(`${API_URL}${path}`, {
            ...options,
            headers,
        });
    } catch {
        // Network failure — backend unreachable or CORS error
        throw new ApiError(0, "Unable to reach the server. Make sure the backend is running.");
    }

    // 204 No Content — return undefined cast to T
    if (response.status === 204) {
        return undefined as T;
    }

    let body: unknown;
    try {
        body = await response.json();
    } catch {
        throw new ApiError(response.status, `HTTP ${response.status}`);
    }

    if (!response.ok) {
        if (response.status === 422) {
            const err = body as ValidationErrorResponse;
            throw new ValidationError(err.details ?? [], err.error ?? "Validation failed");
        }
        const err = body as ApiErrorResponse;
        throw new ApiError(response.status, err.error ?? `HTTP ${response.status}`);
    }

    return body as T;
}
