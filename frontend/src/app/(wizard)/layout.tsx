"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import { WizardSidebar } from "@/components/wizard/WizardSidebar";
import { WizardTopBar } from "@/components/wizard/WizardTopBar";

export default function WizardLayout({ children }: { children: React.ReactNode }) {
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const router = useRouter();
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    useEffect(() => {
        if (!mounted) return;
        if (!isAuthenticated) router.replace("/login");
    }, [mounted, isAuthenticated, router]);

    if (!mounted || !isAuthenticated) return null;

    return (
        <div className="min-h-screen bg-surface-container-low flex">
            <WizardSidebar />
            <WizardTopBar />
            <main className="flex-1 ml-64 pt-14">{children}</main>
        </div>
    );
}
