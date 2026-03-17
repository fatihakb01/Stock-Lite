import { z } from "zod";

export const jobTaskSchema = z.object({
    jobTaskKey: z.string().min(1),
    name: z
        .string()
        .min(1, "Name is required")
        .max(50, "Name is too long"),
    description: z.string().max(200).optional(),
    scheduledAt: z.string().optional(),
    nextRunAt: z.string().optional(),
    periodType: z.string(),
    maxRetries: z.number().min(1),
    retryDelayMinutes: z.number().min(1),
    isArchived: z.boolean()
});

export type JobTaskFormValues = z.infer<typeof jobTaskSchema>;
