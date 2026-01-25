import { z } from "zod";

export const jobTaskSchema = z.object({
  jobTaskKey: z.string().min(1),
  name: z.string().min(1, "Name is required").max(50, "Name is too long"),
  description: z.string().max(200).optional(),
  isArchived: z.boolean(),
});

export const createJobTaskSchema = z.object({
  jobTaskKey: z.string().min(1),
  name: z.string().min(1, "Name is required").max(50, "Name is too long"),
  description: z.string().max(200).optional()
});

export type JobTaskFormValues = z.infer<typeof jobTaskSchema>;
export type CreateJobTaskFormValues = z.infer<typeof createJobTaskSchema>;
