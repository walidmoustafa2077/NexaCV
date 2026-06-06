/**
 * Creates a cropped JPEG base64 data URL from an image source and pixel crop area.
 *
 * Uses an off-screen `<canvas>` to draw the cropped region, then exports
 * as a `data:image/jpeg;base64` string at 90 % quality — ready to store
 * in `useWizardStore` as `photoUrl`.
 */

export interface PixelCrop {
    x: number;
    y: number;
    width: number;
    height: number;
}

export async function getCroppedImg(
    imageSrc: string,
    pixelCrop: PixelCrop,
): Promise<string> {
    const image = await loadImage(imageSrc);
    const canvas = document.createElement("canvas");
    const ctx = canvas.getContext("2d")!;

    // Set output size – cap at 512 px on the longest side for performance,
    // then let the canvas scale proportionally.
    const maxDim = 512;
    const scale = Math.min(maxDim / pixelCrop.width, maxDim / pixelCrop.height, 1);
    canvas.width = Math.round(pixelCrop.width * scale);
    canvas.height = Math.round(pixelCrop.height * scale);

    // Draw cropped region (bilinear filtering is fine for JPEG)
    ctx.drawImage(
        image,
        pixelCrop.x,
        pixelCrop.y,
        pixelCrop.width,
        pixelCrop.height, // source rect
        0,
        0,
        canvas.width,
        canvas.height, // dest rect
    );

    return canvas.toDataURL("image/jpeg", 0.9);
}

/* ── internal helpers ─────────────────────────────────────────── */

function loadImage(src: string): Promise<HTMLImageElement> {
    return new Promise((resolve, reject) => {
        const img = new Image();
        img.crossOrigin = "anonymous";
        img.onload = () => resolve(img);
        img.onerror = (_ev, _src, _lineno, _colno, err) =>
            reject(err ?? new Error(`Failed to load image: ${src}`));
        img.src = src;
    });
}