import { z } from "zod";

export const resetPasswordSchema = z.object({
  email: z.email("Invalid email").max(50, "Maximum of 50 characters allowed")
});

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;
