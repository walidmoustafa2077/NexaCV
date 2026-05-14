"use client";

import { useState, useRef, useEffect } from "react";
import { useRouter, usePathname } from "next/navigation";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { useAuthStore } from "@/store/authStore";
import { useUser } from "@/hooks/useUser";
import { logout } from "@/lib/api/auth";
import { toast } from "sonner";

export default function TopBar() {
    const pathname = usePathname();
    const router = useRouter();
    const { clearAuth } = useAuthStore();
    const { data: user } = useUser();
    const [dropdownOpen, setDropdownOpen] = useState(false);
    const dropdownRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClick(e: MouseEvent) {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target as Node)) {
                setDropdownOpen(false);
            }
        }
        document.addEventListener("mousedown", handleClick);
        return () => document.removeEventListener("mousedown", handleClick);
    }, []);

    async function handleLogout() {
        setDropdownOpen(false);
        try { await logout(); } catch { /* best-effort */ }
        clearAuth();
        router.push("/login");
        toast.success("You have been logged out.");
    }

    // Breadcrumb segments derived from pathname
    type Crumb = { label: string; href?: string };
    function getBreadcrumbs(): Crumb[] {
        if (pathname === "/dashboard") return [{ label: "Dashboard" }];
        if (pathname === "/resumes") return [{ label: "My Resumes" }];
        if (pathname.match(/^\/resumes\/[^/]+$/)) return [{ label: "My Resumes", href: "/resumes" }, { label: "Resume Detail" }];
        if (pathname === "/create/template") return [{ label: "Templates" }, { label: "Resume Gallery" }];
        if (pathname === "/settings") return [{ label: "Account" }, { label: "Settings" }];
        if (pathname.startsWith("/create/steps/")) {
            const step = pathname.split("/").pop();
            return [{ label: "Wizard" }, { label: `Step ${step}` }];
        }
        return [];
    }
    const crumbs = getBreadcrumbs();

    const searchPlaceholder =
        pathname.startsWith("/resumes")
            ? "Search resumes..."
            : pathname.startsWith("/create/template")
                ? "Search templates..."
                : "Search...";

    const initials = user
        ? `${user.firstName[0]}${user.lastName[0]}`.toUpperCase()
        : "U";

    return (
        <header className="fixed top-0 left-64 right-0 h-14 bg-white border-b border-slate-200 z-30 flex items-center px-6 gap-4">
            {/* Breadcrumb — left */}
            <nav className="flex items-center gap-1.5 text-sm min-w-0">
                {crumbs.map((crumb, i) => (
                    <span key={i} className="flex items-center gap-1.5 min-w-0">
                        {i > 0 && <span className="text-slate-300">/</span>}
                        {crumb.href ? (
                            <button
                                onClick={() => router.push(crumb.href!)}
                                className="text-slate-400 hover:text-primary transition-colors truncate"
                            >
                                {crumb.label}
                            </button>
                        ) : (
                            <span className={i === crumbs.length - 1 ? "font-semibold text-slate-700 truncate" : "text-slate-400 truncate"}>
                                {crumb.label}
                            </span>
                        )}
                    </span>
                ))}
            </nav>

            {/* Right side */}
            <div className="ml-auto flex items-center gap-2">
                {/* Search */}
                <div className="relative">
                    <span className="absolute inset-y-0 left-3 flex items-center pointer-events-none">
                        <MaterialIcon name="search" size={18} className="text-slate-400" />
                    </span>
                    <input
                        type="text"
                        placeholder={searchPlaceholder}
                        className="w-52 h-9 pl-9 pr-4 bg-slate-100 rounded-full text-sm text-slate-700 placeholder:text-slate-400 focus:outline-none focus:ring-2 focus:ring-primary/30 focus:bg-white focus:w-72 transition-all"
                    />
                </div>

                {/* Notifications */}
                <button className="w-9 h-9 flex items-center justify-center rounded-full text-slate-500 hover:bg-slate-100 transition-colors">
                    <MaterialIcon name="notifications" size={20} />
                </button>

                {/* Help */}
                <button className="w-9 h-9 flex items-center justify-center rounded-full text-slate-500 hover:bg-slate-100 transition-colors">
                    <MaterialIcon name="help_outline" size={20} />
                </button>

                {/* Avatar + dropdown */}
                <div className="relative" ref={dropdownRef}>
                    <button
                        onClick={() => setDropdownOpen((v) => !v)}
                        className="w-9 h-9 rounded-full bg-primary text-on-primary text-sm font-bold flex items-center justify-center hover:opacity-90 transition-opacity"
                    >
                        {initials}
                    </button>

                    {dropdownOpen && (
                        <div className="absolute right-0 top-11 w-60 bg-white rounded-xl shadow-lg border border-slate-200 overflow-hidden z-50">
                            {/* User info header */}
                            <div className="flex items-center gap-3 px-4 py-3 border-b border-slate-100">
                                <div className="w-9 h-9 rounded-full bg-primary text-on-primary text-sm font-bold flex items-center justify-center shrink-0">
                                    {initials}
                                </div>
                                <div className="min-w-0">
                                    <p className="text-sm font-semibold text-slate-900 truncate">
                                        {user ? `${user.firstName} ${user.lastName}` : "Account"}
                                    </p>
                                    <p className="text-xs text-slate-500 truncate">{user?.email}</p>
                                </div>
                            </div>

                            {/* Menu items */}
                            <div className="py-1">
                                <button
                                    onClick={() => { setDropdownOpen(false); router.push("/settings"); }}
                                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-slate-600 hover:bg-slate-50 transition-colors"
                                >
                                    <MaterialIcon name="manage_accounts" size={18} className="text-slate-400" />
                                    Account Settings
                                </button>
                                <button
                                    onClick={() => { setDropdownOpen(false); router.push("/dashboard"); }}
                                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-slate-600 hover:bg-slate-50 transition-colors"
                                >
                                    <MaterialIcon name="dashboard" size={18} className="text-slate-400" />
                                    Dashboard
                                </button>
                            </div>

                            <div className="border-t border-slate-100 py-1">
                                <button
                                    onClick={handleLogout}
                                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-500 hover:bg-red-50 transition-colors"
                                >
                                    <MaterialIcon name="logout" size={18} className="text-red-400" />
                                    Logout
                                </button>
                            </div>
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
}
