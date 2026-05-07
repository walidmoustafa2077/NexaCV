import { z } from "zod";

export const loginSchema = z.object({
    email: z.string().min(1, "Email is required").email("Enter a valid email address"),
    password: z.string().min(1, "Password is required"),
});

export const registerSchema = z.object({
    firstName: z.string().min(1, "First name is required").max(50),
    lastName: z.string().min(1, "Last name is required").max(50),
    username: z.string().min(1, "Username is required").max(50),
    email: z
        .string()
        .min(1, "Email is required")
        .email("Enter a valid email address")
        .max(150),
    password: z
        .string()
        .min(8, "Password must be at least 8 characters")
        .max(128)
        .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
        .regex(/[0-9]/, "Password must contain at least one number")
        .regex(/[^A-Za-z0-9]/, "Password must contain at least one special character"),
    dateOfBirth: z.string().optional(),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;
