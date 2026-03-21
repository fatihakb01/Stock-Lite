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
    periodType: z.string().optional(),
    maxRetries: z.number().optional(),
    retryDelayMinutes: z.number().optional(),
    isArchived: z.boolean()
});

export type JobTaskFormValues = z.infer<typeof jobTaskSchema>;
