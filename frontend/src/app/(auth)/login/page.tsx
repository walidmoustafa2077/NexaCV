import Link from "next/link";
import LoginForm from "@/components/auth/LoginForm";
import MaterialIcon from "@/components/shared/MaterialIcon";

export default function LoginPage() {
    return (
        <div className="w-full max-w-[480px] mx-auto flex flex-col items-center">
            {/* Brand */}
            <div className="mb-8 flex items-center gap-2">
                <div className="w-9 h-9 bg-primary rounded-lg flex items-center justify-center">
                    <MaterialIcon name="architecture" className="text-on-primary" size={20} filled />
                </div>
                <span className="font-[Manrope] text-xl font-bold text-primary">NexaCV</span>
            </div>

            {/* Card */}
            <div className="w-full bg-surface-container-low p-8 md:p-10 rounded-xl border border-outline-variant shadow-sm">
                <div className="mb-8 text-center">
                    <h1 className="font-[Manrope] text-[30px] leading-[36px] font-bold tracking-tight text-on-surface mb-2">
                        Welcome Back
                    </h1>
                    <p className="text-sm text-on-surface-variant">
                        Access your AI-powered resume builder workspace.
                    </p>
                </div>

                <LoginForm />

                {/* Divider */}
                <div className="relative my-7">
                    <div className="absolute inset-0 flex items-center">
                        <div className="w-full border-t border-outline-variant" />
                    </div>
                    <div className="relative flex justify-center">
                        <span className="px-3 bg-surface-container-low text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                            Or continue with
                        </span>
                    </div>
                </div>

                {/* Social (stub) */}
                <div className="grid grid-cols-2 gap-3">
                    <button
                        type="button"
                        disabled
                        className="flex items-center justify-center gap-2 py-2.5 px-4 bg-surface-container-lowest border border-outline-variant rounded-lg text-sm font-semibold text-on-surface opacity-50 cursor-not-allowed"
                    >
                        <MaterialIcon name="g_translate" size={16} />
                        Google
                    </button>
                    <button
                        type="button"
                        disabled
                        className="flex items-center justify-center gap-2 py-2.5 px-4 bg-surface-container-lowest border border-outline-variant rounded-lg text-sm font-semibold text-on-surface opacity-50 cursor-not-allowed"
                    >
                        <MaterialIcon name="work" size={16} />
                        LinkedIn
                    </button>
                </div>
            </div>

            {/* Footer link */}
            <p className="mt-7 text-sm text-on-surface-variant text-center">
                Don&apos;t have an account?{" "}
                <Link href="/register" className="text-primary font-semibold hover:underline">
                    Start building for free
                </Link>
            </p>

            {/* Trust badges */}
            <div className="mt-8 flex items-center justify-center gap-6 opacity-50">
                <div className="flex items-center gap-1.5">
                    <MaterialIcon name="lock" size={14} filled />
                    <span className="text-[11px] font-semibold uppercase tracking-wider">Bank-grade security</span>
                </div>
                <div className="flex items-center gap-1.5">
                    <MaterialIcon name="verified_user" size={14} filled />
                    <span className="text-[11px] font-semibold uppercase tracking-wider">Privacy focused</span>
                </div>
            </div>

            {/* Background decoration */}
            <div className="fixed inset-0 -z-10 overflow-hidden pointer-events-none">
                <div className="absolute -top-[20%] -left-[10%] w-[60%] h-[60%] bg-primary opacity-[0.03] rounded-full blur-[120px]" />
                <div className="absolute -bottom-[20%] -right-[10%] w-[50%] h-[50%] bg-tertiary opacity-[0.03] rounded-full blur-[120px]" />
            </div>
        </div>
    );
}
