import { z } from "zod";

export const checkoutSchema = z.object({
    resumeId: z.string().uuid("Invalid resume ID"),
    currency: z.enum(["USD", "EGP", "EUR", "GBP", "SAR", "AED"]),
});

export type CheckoutFormValues = z.infer<typeof checkoutSchema>;
