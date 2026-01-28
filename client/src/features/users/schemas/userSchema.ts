import { z } from "zod";

export const userSchema = z.object({
  firstName: z.string().max(50, "Maximum of 20 characters allowed").optional(),
  lastName: z.string().max(50, "Maximum of 20 characters allowed").optional(),
  userName: z.string().max(50, "Maximum of 50 characters allowed").min(2, "Minimum of 2 characters required"),
  email: z.email("Invalid email").max(50, "Maximum of 50 characters allowed"),
  phoneNumber: z.string().max(20, "Maximum of 20 characters allowed").optional(),
  role: z.string(),
});

export type UserFormValues = z.infer<typeof userSchema>;
