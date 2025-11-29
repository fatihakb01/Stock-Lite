import { z } from "zod";

export const loginSchema = z.object({
  email: z.email("Invalid email").max(50, "Maximum of 50 characters allowed"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
