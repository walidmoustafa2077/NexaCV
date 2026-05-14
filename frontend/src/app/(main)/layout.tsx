"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useAuthStore } from "@/store/authStore";
import AppSidebar from "@/components/layout/AppSidebar";
import TopBar from "@/components/layout/TopBar";

export default function MainLayout({ children }: { children: React.ReactNode }) {
    const router = useRouter();
    const isAuthenticated = useAuthStore((s) => s.isAuthenticated);
    const [mounted, setMounted] = useState(false);

    useEffect(() => {
        setMounted(true);
    }, []);

    useEffect(() => {
        if (!mounted) return;
        if (!isAuthenticated) {
            router.replace("/login");
        }
    }, [mounted, isAuthenticated, router]);

    if (!mounted || !isAuthenticated) return null;

    return (
        <div className="flex min-h-screen bg-background">
            <AppSidebar />
            <TopBar />
            <main className="flex-1 ml-64 pt-14 p-8 min-h-screen">
                <div className="pt-6">{children}</div>
            </main>
        </div>
    );
}
