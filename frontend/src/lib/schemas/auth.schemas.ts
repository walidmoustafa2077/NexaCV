import { z } from "zod";

export const loginSchema = z.object({
    emailOrUsername: z
        .string()
        .min(1, "Email or username is required")
        .min(3, "Must be at least 3 characters"),
    password: z.string().min(1, "Password is required"),
});

export const registerSchema = z.object({
    firstName: z.string().min(1, "First name is required").max(100),
    lastName: z.string().min(1, "Last name is required").max(100),
    email: z
        .string()
        .min(1, "Email is required")
        .email("Enter a valid email address")
        .max(256),
    username: z
        .string()
        .min(3, "Username must be at least 3 characters")
        .max(50, "Username must be 50 characters or less")
        .regex(/^[a-zA-Z0-9._-]+$/, "Username can only contain letters, numbers, dots, dashes, and underscores"),
    password: z
        .string()
        .min(8, "Password must be at least 8 characters")
        .max(100)
        .regex(/[A-Z]/, "Password must contain at least one uppercase letter")
        .regex(/[0-9]/, "Password must contain at least one number")
        .regex(/[^A-Za-z0-9]/, "Password must contain at least one special character"),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;
