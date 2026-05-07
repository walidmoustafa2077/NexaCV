"use client";

import { useEffect } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { toast } from "sonner";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { cn } from "@/lib/utils";
import { useWizardStore, checkStepComplete } from "@/store/wizardStore";

const steps = [
    { step: 1, label: "Personal Info", icon: "person", href: "/create/steps/1" },
    { step: 2, label: "Education", icon: "school", href: "/create/steps/2" },
    { step: 3, label: "Courses", icon: "menu_book", href: "/create/steps/3" },
    { step: 4, label: "Work Experience", icon: "work", href: "/create/steps/4" },
    { step: 5, label: "Summary & Skills", icon: "psychology", href: "/create/steps/5" },
    { step: 6, label: "Review", icon: "fact_check", href: "/create/steps/6" },
];

export function WizardSidebar() {
    const pathname = usePathname();
    const router = useRouter();
    const { formData, visitedSteps, markVisited } = useWizardStore();

    // Determine current step from path
    const currentStep = steps.find((s) => pathname.startsWith(s.href))?.step ?? 1;

    // Mark the current step as visited whenever it changes
    useEffect(() => {
        markVisited(currentStep);
    }, [currentStep, markVisited]);

    // Step 6 is unlocked only when all required fields in steps 1–5 are filled
    const isStep6Unlocked = useWizardStore((state) => {
        const fd = state.formData;
        return (
            checkStepComplete(1, fd) &&
            checkStepComplete(2, fd) &&
            checkStepComplete(4, fd) &&
            checkStepComplete(5, fd)
        );
    });

    function handleStep6Click() {
        if (isStep6Unlocked) {
            router.push("/create/steps/6");
        } else {
            toast.warning("Complete Required Fields", {
                description:
                    "Fill in Personal Info, Education, Work Experience, and Summary & Skills before reviewing.",
            });
        }
    }

    return (
        <aside className="fixed left-0 top-0 h-screen w-64 border-r border-slate-200 bg-slate-50 flex flex-col py-8 z-50">
            {/* Brand */}
            <div className="px-6 mb-8">
                <h2 className="text-slate-900 font-bold font-manrope">Resume Wizard</h2>
                <p className="text-slate-500 text-xs mt-1 font-body-sm">Progress Tracking</p>
            </div>

            {/* Steps nav */}
            <nav className="flex-1 flex flex-col gap-1">
                {steps.map(({ step, label, icon, href }) => {
                    const isActive = step === currentStep;
                    const isVisited = visitedSteps.includes(step);
                    const isComplete = checkStepComplete(step, formData);
                    const isReviewStep = step === 6;

                    // Steps 1–5 are always navigable; step 6 requires all required fields
                    const isClickable = isReviewStep ? isStep6Unlocked : true;

                    // Show warning when visited but still has missing required fields (steps 1–5 only)
                    const showWarning = !isReviewStep && isVisited && !isComplete;
                    // Show checkmark when visited and complete (but not the currently active step)
                    const showCheck = !isReviewStep && isVisited && isComplete && !isActive;

                    if (isReviewStep) {
                        return (
                            <button
                                key={step}
                                type="button"
                                onClick={handleStep6Click}
                                className={cn(
                                    "flex items-center gap-3 px-6 py-3 font-manrope text-sm transition-all duration-200 w-full text-left",
                                    isActive
                                        ? "text-primary font-bold border-r-4 border-primary bg-blue-50/50"
                                        : isClickable
                                            ? "text-slate-500 hover:text-blue-500 hover:bg-slate-100"
                                            : "text-slate-400 cursor-not-allowed opacity-60",
                                )}
                            >
                                <MaterialIcon
                                    name={icon}
                                    size={20}
                                    filled={isActive}
                                    className={isClickable ? "" : "opacity-50"}
                                />
                                <span>{label}</span>
                                {!isClickable && (
                                    <MaterialIcon
                                        name="lock"
                                        size={14}
                                        className="ml-auto text-slate-400 opacity-60"
                                    />
                                )}
                                {isClickable && !isActive && (
                                    <MaterialIcon
                                        name="check_circle"
                                        size={14}
                                        className="ml-auto text-emerald-500"
                                        filled
                                    />
                                )}
                            </button>
                        );
                    }

                    return (
                        <Link
                            key={step}
                            href={href}
                            className={cn(
                                "flex items-center gap-3 px-6 py-3 font-manrope text-sm transition-all duration-200",
                                isActive
                                    ? "text-primary font-bold border-r-4 border-primary bg-blue-50/50"
                                    : "text-slate-500 hover:text-blue-500 hover:bg-slate-100",
                            )}
                        >
                            <MaterialIcon
                                name={icon}
                                size={20}
                                filled={isActive}
                            />
                            <span>{label}</span>
                            {showWarning && (
                                <MaterialIcon
                                    name="warning"
                                    size={14}
                                    className="ml-auto text-amber-500"
                                    filled
                                />
                            )}
                            {showCheck && (
                                <MaterialIcon
                                    name="check_circle"
                                    size={14}
                                    className="ml-auto text-emerald-500"
                                    filled
                                />
                            )}
                        </Link>
                    );
                })}
            </nav>

            {/* Bottom */}
            <div className="mt-auto px-4 space-y-1">
                <Link
                    href="/dashboard"
                    className="flex items-center gap-3 px-3 py-2 text-slate-500 hover:text-blue-500 hover:bg-slate-100 text-sm transition-all duration-150 rounded-lg"
                >
                    <MaterialIcon name="arrow_back" size={18} />
                    <span className="font-manrope text-sm">Back to Dashboard</span>
                </Link>
            </div>
        </aside>
    );
}
