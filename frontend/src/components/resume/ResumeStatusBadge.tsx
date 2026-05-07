import type { ResumeStatus } from "@/types/enums";
import { cn } from "@/lib/utils";

interface ResumeStatusBadgeProps {
    status: ResumeStatus;
    className?: string;
}

const config: Record<ResumeStatus, { label: string; className: string }> = {
    DRAFT: {
        label: "Draft",
        className: "bg-surface-container-high text-on-surface-variant",
    },
    COMPLETED: {
        label: "Completed",
        className: "bg-primary-fixed text-on-primary-fixed-variant",
    },
    PAID: {
        label: "Paid",
        className: "bg-tertiary-container text-on-tertiary-container",
    },
};

export function ResumeStatusBadge({ status, className }: ResumeStatusBadgeProps) {
    const { label, className: colorClass } = config[status] ?? config.DRAFT;
    return (
        <span
            className={cn(
                "inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-bold uppercase tracking-wider",
                colorClass,
                className,
            )}
        >
            {label}
        </span>
    );
}
