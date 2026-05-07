"use client";

import { useState, useEffect } from "react";

const MONTHS = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December",
];

function getYears() {
    const current = new Date().getFullYear();
    const years: number[] = [];
    for (let y = current + 5; y >= 1960; y--) {
        years.push(y);
    }
    return years;
}

const YEARS = getYears();

const selectCls =
    "flex-1 h-11 px-3 border border-outline-variant rounded-lg bg-white font-input-text text-sm text-on-surface focus:ring-2 focus:ring-primary focus:border-transparent outline-none transition-all cursor-pointer";

interface MonthYearPickerProps {
    /** Controlled value: YYYY-MM or "" */
    value: string;
    onChange: (val: string) => void;
    /** If true, an optional "Set as present" link is shown below the pickers */
    allowPresent?: boolean;
    /** Whether the current state is "Present" (i.e. no end date) */
    isPresent?: boolean;
    onPresentChange?: (v: boolean) => void;
    /** Extra wrapper class */
    className?: string;
}

/**
 * A custom month/year picker that renders two <select> elements instead of
 * the browser-native `<input type="month">`.
 *
 * When `allowPresent` is true, a toggle allows the user to set the field to
 * "Present" (null end-date) instead of picking a specific month/year.
 */
export function MonthYearPicker({
    value,
    onChange,
    allowPresent,
    isPresent,
    onPresentChange,
    className,
}: MonthYearPickerProps) {
    const parseValue = (v: string) => {
        const parts = v ? v.split("-") : ["", ""];
        return { year: parts[0] ?? "", month: parts[1] ?? "" };
    };

    const [year, setYear] = useState(() => parseValue(value).year);
    const [month, setMonth] = useState(() => parseValue(value).month);

    // Sync local state when parent resets or pre-fills value
    useEffect(() => {
        const { year: y, month: m } = parseValue(value);
        setYear(y);
        setMonth(m);
    }, [value]);

    function handleMonthChange(newMonth: string) {
        setMonth(newMonth);
        if (year && newMonth) {
            onChange(`${year}-${newMonth.padStart(2, "0")}`);
        }
    }

    function handleYearChange(newYear: string) {
        setYear(newYear);
        if (newYear && month) {
            onChange(`${newYear}-${month.padStart(2, "0")}`);
        }
    }

    /* ─── "Present" display ─────────────────────────────────────────────── */
    if (allowPresent && isPresent) {
        return (
            <div className={`flex items-center gap-3 ${className ?? ""}`}>
                <div className="flex-1 h-11 flex items-center px-4 border border-primary/40 rounded-lg bg-primary-fixed/20 text-primary font-semibold text-sm">
                    Present
                </div>
                <button
                    type="button"
                    onClick={() => onPresentChange?.(false)}
                    className="text-xs text-secondary hover:text-primary underline whitespace-nowrap"
                >
                    Add end date
                </button>
            </div>
        );
    }

    /* ─── Normal picker ─────────────────────────────────────────────────── */
    return (
        <div className={`space-y-1.5 ${className ?? ""}`}>
            <div className="flex gap-2">
                <select
                    value={month}
                    onChange={(e) => handleMonthChange(e.target.value)}
                    className={selectCls}
                >
                    <option value="">Month</option>
                    {MONTHS.map((m, i) => (
                        <option key={m} value={String(i + 1).padStart(2, "0")}>
                            {m}
                        </option>
                    ))}
                </select>
                <select
                    value={year}
                    onChange={(e) => handleYearChange(e.target.value)}
                    className={selectCls}
                >
                    <option value="">Year</option>
                    {YEARS.map((y) => (
                        <option key={y} value={String(y)}>
                            {y}
                        </option>
                    ))}
                </select>
            </div>
            {allowPresent && (
                <button
                    type="button"
                    onClick={() => {
                        onPresentChange?.(true);
                        onChange("");
                    }}
                    className="text-xs text-primary hover:underline"
                >
                    Set as current (Present)
                </button>
            )}
        </div>
    );
}
