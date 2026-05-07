import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

interface FormFieldProps extends React.InputHTMLAttributes<HTMLInputElement> {
    label: string;
    error?: string;
    hint?: string;
    required?: boolean;
}

export default function FormField({
    label,
    error,
    hint,
    required,
    id,
    className,
    ...props
}: FormFieldProps) {
    const fieldId = id ?? label.toLowerCase().replace(/\s+/g, "-");

    return (
        <div className="flex flex-col gap-1.5">
            <Label htmlFor={fieldId} className="text-sm font-medium text-on-surface">
                {label}
                {required && <span className="text-error ml-0.5">*</span>}
            </Label>
            <Input
                id={fieldId}
                className={cn(error && "border-error focus-visible:ring-error/30", className)}
                aria-invalid={!!error}
                aria-describedby={error ? `${fieldId}-error` : hint ? `${fieldId}-hint` : undefined}
                {...props}
            />
            {hint && !error && (
                <p id={`${fieldId}-hint`} className="text-xs text-on-surface-variant">
                    {hint}
                </p>
            )}
            {error && (
                <p id={`${fieldId}-error`} role="alert" className="text-xs text-error">
                    {error}
                </p>
            )}
        </div>
    );
}
