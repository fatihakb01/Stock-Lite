import { z } from "zod";

export const scheduledJobSchema = z.object({
  name: z.string().min(1),
  description: z.string().optional(),
  jobType: z.string(),
  calculationType: z.string().optional(),
  importDataType: z.string().optional(),
  periodType: z.string().optional(),
  source: z.string().optional(),
  target: z.string().optional(),
  executionOrder: z.number().int().positive(),
  isArchived: z.boolean(),
});

export const createScheduledJobSchema = z.object({
  jobTaskKey: z.string().min(1),
  name: z.string().min(1),
  description: z.string().optional(),
  jobType: z.string(),
  calculationType: z.string().optional(),
  importDataType: z.string().optional(),
  periodType: z.string().optional(),
  source: z.string().optional(),
  target: z.string().optional(),
  executionOrder: z.number().int().positive(),
});

export type ScheduledJobFormValues = z.infer<typeof scheduledJobSchema>;
export type CreateScheduledJobFormValues = z.infer<typeof createScheduledJobSchema>;
