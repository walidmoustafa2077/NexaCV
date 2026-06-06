"use client";

import { useCallback, useState, useRef } from "react";
import Cropper, { type Area, type Point } from "react-easy-crop";
import MaterialIcon from "@/components/shared/MaterialIcon";
import { getCroppedImg } from "@/utils/cropImage";

/* ── Types ────────────────────────────────────────────────────── */

interface ImageCropModalProps {
    /** Temporary blob/data-URL of the raw selected image. */
    imageSrc: string;
    /** Called with the final cropped base64 JPEG. */
    onCropComplete: (base64: string) => void;
    /** Called when the user dismisses without saving. */
    onClose: () => void;
    /** Whether the modal is visible. */
    open: boolean;
}

/* ── Component ────────────────────────────────────────────────── */

export default function ImageCropModal({
    imageSrc,
    onCropComplete,
    onClose,
    open,
}: ImageCropModalProps) {
    const [crop, setCrop] = useState<Point>({ x: 0, y: 0 });
    const [zoom, setZoom] = useState(1);
    const [isSaving, setIsSaving] = useState(false);
    const croppedAreaPixelsRef = useRef<Area | null>(null);

    const handleCropChange = useCallback((location: Point) => {
        setCrop(location);
    }, []);

    const handleZoomChange = useCallback((newZoom: number) => {
        setZoom(newZoom);
    }, []);

    const handleCropComplete = useCallback(
        (_croppedArea: Area, croppedAreaPixels: Area) => {
            croppedAreaPixelsRef.current = croppedAreaPixels;
        },
        [],
    );

    const handleSave = useCallback(async () => {
        if (!croppedAreaPixelsRef.current) return;
        setIsSaving(true);
        try {
            const base64 = await getCroppedImg(imageSrc, croppedAreaPixelsRef.current);
            onCropComplete(base64);
        } catch {
            // If cropping fails, silently keep the modal open.
        } finally {
            setIsSaving(false);
        }
    }, [imageSrc, onCropComplete]);

    /* ── Portal / render ─────────────────────────────────────────── */

    if (!open) return null;

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            {/* Backdrop */}
            <div
                className="absolute inset-0 bg-black/60 backdrop-blur-sm"
                onClick={onClose}
            />

            {/* Modal panel */}
            <div className="relative z-10 mx-4 w-full max-w-[480px] rounded-2xl bg-white shadow-2xl flex flex-col overflow-hidden">
                {/* Header */}
                <div className="flex items-center justify-between px-5 py-4 border-b border-outline-variant/20">
                    <h2 className="font-semibold text-on-surface text-base">Crop Photo</h2>
                    <button
                        type="button"
                        onClick={onClose}
                        className="p-1 rounded-lg hover:bg-surface-container transition-colors"
                    >
                        <MaterialIcon name="close" size={20} className="text-secondary" />
                    </button>
                </div>

                {/* Cropper area (square aspect = 1/1) */}
                <div className="relative w-full aspect-square bg-black/90">
                    <Cropper
                        image={imageSrc}
                        crop={crop}
                        zoom={zoom}
                        aspect={1}
                        cropShape="round"
                        showGrid={false}
                        onCropChange={handleCropChange}
                        onZoomChange={handleZoomChange}
                        onCropComplete={handleCropComplete}
                    />
                </div>

                {/* Zoom slider */}
                <div className="px-5 py-4 border-t border-outline-variant/20">
                    <div className="flex items-center gap-3">
                        <MaterialIcon
                            name="photo_camera"
                            size={18}
                            className="text-secondary shrink-0"
                        />
                        <input
                            type="range"
                            min={1}
                            max={3}
                            step={0.1}
                            value={zoom}
                            onChange={(e) => setZoom(Number(e.target.value))}
                            className="w-full h-1.5 rounded-full appearance-none cursor-pointer
                bg-surface-container-highest accent-primary
                [&::-webkit-slider-thumb]:appearance-none
                [&::-webkit-slider-thumb]:w-4
                [&::-webkit-slider-thumb]:h-4
                [&::-webkit-slider-thumb]:rounded-full
                [&::-webkit-slider-thumb]:bg-primary
                [&::-webkit-slider-thumb]:shadow-sm
                [&::-webkit-slider-thumb]:cursor-grab"
                        />
                        <MaterialIcon
                            name="zoom_in"
                            size={18}
                            className="text-secondary shrink-0"
                        />
                    </div>
                </div>

                {/* Actions */}
                <div className="flex items-center justify-end gap-3 px-5 py-4 border-t border-outline-variant/20">
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-5 py-2 rounded-lg border border-outline-variant text-secondary font-semibold text-sm hover:bg-surface-container transition-colors"
                    >
                        Cancel
                    </button>
                    <button
                        type="button"
                        onClick={handleSave}
                        disabled={isSaving}
                        className="flex items-center gap-2 px-5 py-2 rounded-lg bg-primary text-on-primary font-semibold text-sm hover:opacity-90 transition-opacity disabled:opacity-50 disabled:cursor-not-allowed shadow-sm"
                    >
                        {isSaving ? (
                            <>
                                <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                                Saving…
                            </>
                        ) : (
                            "Save Crop"
                        )}
                    </button>
                </div>
            </div>
        </div>
    );
}