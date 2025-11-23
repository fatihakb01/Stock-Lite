import { z } from "zod";

export const resetPasswordSchema = z.object({
  email: z.email("Invalid email")
});

export type ResetPasswordFormValues = z.infer<typeof resetPasswordSchema>;
