"use client";

import { create } from "zustand";
import { setTokenProvider } from "@/lib/api/client";

interface AuthState {
    token: string | null;
    userId: string | null;
    isAuthenticated: boolean;
    setAuth: (token: string, userId: string) => void;
    clearAuth: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
    token: null,
    userId: null,
    isAuthenticated: false,

    setAuth: (token, userId) => {
        set({ token, userId, isAuthenticated: true });
    },

    clearAuth: () => {
        set({ token: null, userId: null, isAuthenticated: false });
    },
}));

// Wire the API client to always read the latest token from the store.
// This runs once when the module loads on the client.
if (typeof window !== "undefined") {
    setTokenProvider(() => useAuthStore.getState().token);
}
