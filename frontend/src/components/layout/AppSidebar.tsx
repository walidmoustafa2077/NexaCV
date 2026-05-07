"use client";

import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { cn } from "@/lib/utils";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { useAuthStore } from "@/store/authStore";
import { logout } from "@/lib/api/auth";
import { toast } from "sonner";

const navItems = [
    { label: "Dashboard", href: "/dashboard", icon: "dashboard" },
    { label: "My Resumes", href: "/resumes", icon: "description" },
    { label: "Templates", href: "/create/template", icon: "style" },
];

export default function AppSidebar() {
    const pathname = usePathname();
    const router = useRouter();
    const { clearAuth } = useAuthStore();

    async function handleLogout() {
        try {
            await logout();
        } catch {
            // Best-effort logout — always clear local state
        }
        clearAuth();
        router.push("/login");
        toast.success("You have been logged out.");
    }

    return (
        <aside className="fixed left-0 top-0 h-screen w-64 flex flex-col p-4 gap-2 bg-slate-50 border-r border-slate-200 z-40">
            {/* Brand */}
            <div className="flex items-center gap-3 px-2 py-4 mb-4">
                <div className="w-10 h-10 bg-primary-container rounded-lg flex items-center justify-center text-on-primary">
                    <MaterialIcon name="account_tree" size={20} filled />
                </div>
                <div>
                    <h1 className="text-lg font-bold text-slate-900 leading-tight">NexaCV</h1>
                    <p className="text-[10px] uppercase tracking-widest text-slate-500 font-semibold">Resume Builder</p>
                </div>
            </div>

            {/* Nav */}
            <nav className="flex flex-col gap-1 flex-1">
                {navItems.map((item) => {
                    const isActive =
                        pathname === item.href || pathname.startsWith(item.href + "/");
                    return (
                        <Link
                            key={item.href}
                            href={item.href}
                            className={cn(
                                "flex items-center gap-3 px-3 py-2 rounded-lg transition-all duration-150",
                                isActive
                                    ? "text-blue-600 font-semibold bg-blue-50"
                                    : "text-slate-600 hover:text-blue-600 hover:bg-slate-100",
                            )}
                        >
                            <MaterialIcon
                                name={item.icon}
                                size={20}
                                filled={isActive}
                            />
                            {item.label}
                        </Link>
                    );
                })}
            </nav>

            {/* Bottom */}
            <div className="mt-auto pt-4 border-t border-slate-200">
                <Link
                    href="/create/template"
                    className="w-full py-3 px-4 bg-primary text-on-primary rounded-lg font-semibold flex items-center justify-center gap-2 hover:opacity-90 transition-opacity shadow-sm active:scale-95 duration-200"
                >
                    <MaterialIcon name="add" size={18} />
                    Create New Resume
                </Link>
            </div>
        </aside>
    );
}
