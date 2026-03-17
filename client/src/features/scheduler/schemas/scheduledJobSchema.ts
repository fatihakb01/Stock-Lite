import { z } from "zod";

export const scheduledJobSchema = z.object({
  jobTaskKey: z.string().min(1),
  name: z.string().min(1),
  description: z.string().max(200).optional(),
  jobType: z.string(),
  calculationType: z.string().optional(),
  importDataType: z.string().optional(),
  periodType: z.string().transform(v => v === "" ? undefined : v).optional(),
  source: z.string().optional(),
  target: z.string().optional(),
  isArchived: z.boolean()
}).superRefine((data, ctx) => {
  switch (data.jobType) {
    case "Import":
      if (!data.importDataType) {
        ctx.addIssue({
          path: ["importDataType"],
          message: "Import data type is required",
          code: "custom",
        });
      }
      break;

    case "StandardCalculation":
      if (!data.calculationType) {
        ctx.addIssue({
          path: ["calculationType"],
          message: "Calculation type is required",
          code: "custom",
        });
      }
      break;

    case "CustomCalculation":
      if (!data.target) {
        ctx.addIssue({
          path: ["target"],
          message: "Target is required",
          code: "custom",
        });
      }
      break;
  }
});

export type ScheduledJobFormValues = z.infer<typeof scheduledJobSchema>;
