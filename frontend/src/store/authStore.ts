"use client";

import { create } from "zustand";
import { persist, createJSONStorage } from "zustand/middleware";
import { setTokenProvider } from "@/lib/api/client";

interface AuthState {
    token: string | null;
    refreshToken: string | null;
    userId: string | null;
    isAuthenticated: boolean;
    setAuth: (accessToken: string, refreshToken: string, userId: string) => void;
    clearAuth: () => void;
}

export const useAuthStore = create<AuthState>()(
    persist(
        (set) => ({
            token: null,
            refreshToken: null,
            userId: null,
            isAuthenticated: false,

            setAuth: (accessToken, refreshToken, userId) => {
                set({ token: accessToken, refreshToken, userId, isAuthenticated: true });
            },

            clearAuth: () => {
                set({ token: null, refreshToken: null, userId: null, isAuthenticated: false });
            },
        }),
        {
            name: "nexacv-auth",
            storage: createJSONStorage(() => localStorage),
            partialize: (state) => ({
                token: state.token,
                refreshToken: state.refreshToken,
                userId: state.userId,
                isAuthenticated: state.isAuthenticated,
            }),
        },
    ),
);

// Wire the API client to always read the latest token from the store.
// This runs once when the module loads on the client.
if (typeof window !== "undefined") {
    setTokenProvider(() => useAuthStore.getState().token);
}
