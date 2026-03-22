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
    maxRetries: z.number().min(1).optional(),
    retryDelayMinutes: z.number().min(1).optional(),
    isArchived: z.boolean()
}).superRefine((data, ctx) => {
    const dateHasValue = !!data.scheduledAt || !!data.nextRunAt;

    if (!dateHasValue) return;

    if (!data.periodType || data.periodType === "None") {
        ctx.addIssue({
            code: "custom",
            message: "Period type is required when a date is set",
            path: ["periodType"]
        });
    }

    if (data.maxRetries === undefined || data.maxRetries === null) {
        ctx.addIssue({
            code: "custom",
            message: "Max retries is required when a date is set",
            path: ["maxRetries"]
        });
    }

    if (data.retryDelayMinutes === undefined || data.retryDelayMinutes === null) {
        ctx.addIssue({
            code: "custom",
            message: "Retry delay is required when a date is set",
            path: ["retryDelayMinutes"]
        });
    }
});

export type JobTaskFormValues = z.infer<typeof jobTaskSchema>;