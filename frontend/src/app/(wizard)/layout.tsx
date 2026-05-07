"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { WizardSidebar } from "@/components/wizard/WizardSidebar";
import { WizardTopBar } from "@/components/wizard/WizardTopBar";

export default function WizardLayout({ children }: { children: React.ReactNode }) {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const router = useRouter();

    useEffect(() => {
        if (!isAuthenticated) router.replace("/login");
    }, [isAuthenticated, router]);

    if (!isAuthenticated) return null;

    return (
        <div className="min-h-screen bg-surface-container-low flex">
            <WizardSidebar />
            <WizardTopBar />
            <main className="flex-1 ml-64 pt-14">{children}</main>
        </div>
    );
}
