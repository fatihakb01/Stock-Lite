import { z } from "zod";

export const userSchema = z.object({
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  userName: z.string().min(2),
  email: z.email(),
  phoneNumber: z.string().optional(),
  role: z.string(),
});

export type UserFormValues = z.infer<typeof userSchema>;
