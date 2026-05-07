import Link from "next/link";
import RegisterForm from "@/components/auth/RegisterForm";
import MaterialIcon from "@/components/shared/MaterialIcon";

export default function RegisterPage() {
    return (
        <div className="w-full min-h-screen flex flex-col">
            {/* Header */}
            <header className="h-16 px-6 flex items-center justify-center md:justify-start w-full max-w-[1440px] mx-auto">
                <div className="flex items-center gap-2">
                    <div className="w-8 h-8 bg-primary rounded-lg flex items-center justify-center">
                        <MaterialIcon name="account_tree" className="text-on-primary" size={18} filled />
                    </div>
                    <span className="font-[Manrope] text-xl font-bold text-primary">NexaCV</span>
                </div>
            </header>

            {/* Main */}
            <main className="flex-grow flex items-center justify-center p-4">
                <div className="w-full max-w-[768px] flex flex-col md:flex-row gap-12 items-stretch">
                    {/* Left branding panel */}
                    <div className="hidden md:flex flex-col justify-center flex-1 space-y-6">
                        <div className="space-y-4">
                            <h1 className="font-[Manrope] text-[30px] leading-[36px] font-bold tracking-tight text-primary-container">
                                Build your future with precision.
                            </h1>
                            <p className="text-[16px] leading-[24px] text-secondary max-w-sm">
                                Join thousands of professionals using NexaCV to architect their career narratives with our AI-powered resume builder.
                            </p>
                        </div>
                        <div className="space-y-3">
                            {[
                                "AI-Powered Content Analysis",
                                "ATS-Optimized Layouts",
                                "Real-time Visual Formatting",
                            ].map((feature) => (
                                <div key={feature} className="flex items-center gap-3 text-on-surface-variant">
                                    <MaterialIcon name="check_circle" size={20} className="text-primary" filled />
                                    <span className="text-sm">{feature}</span>
                                </div>
                            ))}
                        </div>
                    </div>

                    {/* Register card */}
                    <div className="flex-1 bg-surface-container-low rounded-xl p-8 border border-outline-variant shadow-sm">
                        <div className="mb-6">
                            <h2 className="font-[Manrope] text-[24px] leading-[32px] font-semibold text-on-surface">
                                Create Account
                            </h2>
                            <p className="text-sm text-on-surface-variant">
                                Start building your professional profile today.
                            </p>
                        </div>

                        <RegisterForm />

                        {/* Divider */}
                        <div className="relative my-6">
                            <div className="absolute inset-0 flex items-center">
                                <span className="w-full border-t border-outline-variant" />
                            </div>
                            <div className="relative flex justify-center">
                                <span className="bg-surface-container-low px-4 text-xs font-semibold text-on-surface-variant uppercase tracking-wider">
                                    Or sign up with
                                </span>
                            </div>
                        </div>

                        {/* Social (stub) */}
                        <div className="grid grid-cols-2 gap-3">
                            <button
                                type="button"
                                disabled
                                className="flex items-center justify-center gap-2 py-2 border border-outline-variant rounded-lg text-sm font-medium text-on-surface-variant opacity-50 cursor-not-allowed"
                            >
                                <MaterialIcon name="g_translate" size={16} />
                                Google
                            </button>
                            <button
                                type="button"
                                disabled
                                className="flex items-center justify-center gap-2 py-2 border border-outline-variant rounded-lg text-sm font-medium text-on-surface-variant opacity-50 cursor-not-allowed"
                            >
                                <MaterialIcon name="terminal" size={16} />
                                GitHub
                            </button>
                        </div>

                        <p className="mt-7 text-center text-sm text-on-surface-variant">
                            Already have an account?{" "}
                            <Link href="/login" className="text-primary font-bold hover:underline">
                                Sign In
                            </Link>
                        </p>
                    </div>
                </div>
            </main>

            {/* Footer */}
            <footer className="h-16 flex items-center justify-center">
                <p className="text-[11px] text-outline font-medium tracking-wide uppercase">
                    © {new Date().getFullYear()} NexaCV. All rights reserved.
                </p>
            </footer>
        </div>
    );
}
