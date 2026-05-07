import { cn } from "@/lib/utils";

interface SectionCardProps {
    children: React.ReactNode;
    className?: string;
}

export default function SectionCard({ children, className }: SectionCardProps) {
    return (
        <div
            className={cn(
                "bg-white rounded-xl shadow-sm border border-outline-variant p-6",
                className,
            )}
        >
            {children}
        </div>
    );
}
