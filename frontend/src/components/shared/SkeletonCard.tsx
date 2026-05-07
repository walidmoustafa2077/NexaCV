import { cn } from "@/lib/utils";

interface SkeletonProps {
    className?: string;
}

function Skeleton({ className }: SkeletonProps) {
    return (
        <div className={cn("animate-pulse rounded-md bg-surface-container-high", className)} />
    );
}

export function SkeletonCard({ className }: SkeletonProps) {
    return (
        <div
            className={cn(
                "bg-white rounded-xl border border-outline-variant p-5 space-y-3",
                className,
            )}
        >
            <div className="flex items-start justify-between">
                <Skeleton className="h-5 w-40" />
                <Skeleton className="h-5 w-20 rounded-full" />
            </div>
            <Skeleton className="h-4 w-28" />
            <Skeleton className="h-4 w-36" />
        </div>
    );
}

export { Skeleton };
