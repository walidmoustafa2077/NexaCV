import MaterialIcon from "./MaterialIcon";

interface EmptyStateProps {
    icon?: string;
    title: string;
    description?: string;
    action?: React.ReactNode;
}

export default function EmptyState({
    icon = "description",
    title,
    description,
    action,
}: EmptyStateProps) {
    return (
        <div className="flex flex-col items-center justify-center py-16 px-4 text-center">
            <div className="flex items-center justify-center w-16 h-16 rounded-full bg-surface-container mb-4">
                <MaterialIcon name={icon} className="text-on-surface-variant" size={32} />
            </div>
            <h3 className="text-lg font-semibold text-on-surface mb-1">{title}</h3>
            {description && (
                <p className="text-sm text-on-surface-variant max-w-xs mb-6">{description}</p>
            )}
            {action && <div>{action}</div>}
        </div>
    );
}
