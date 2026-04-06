import { z } from "zod";

export const exportProfileSchema = z.object({
    name: z.string()
        .min(1, "Name is required")
        .max(50, "Name is too long"),
    description: z.string().max(200).optional(),
    exportType: z.string().optional(),
    customType: z.string().optional(),
    customSource: z.string().optional(),
    targetPath: z.string().optional()
}).superRefine((data, ctx) => {

    // Either exportType or customSource must be set, not both
    const hasExportType = !!data.exportType;
    const hasCustomSource = !!data.customSource;

    if (hasExportType === hasCustomSource) {
        ctx.addIssue({
            code: "custom",
            message: "Either CustomSource or ExportType must be filled in, but not both",
            path: ["exportType"]
        });
    }

    // CustomSource and CustomType must both be set or both be empty
    const hasCustomType = !!data.customType;

    if (hasCustomSource !== hasCustomType) {
        ctx.addIssue({
            code: "custom",
            message: "Custom Source and Custom Type must both be filled in or both be empty",
            path: ["customType"]
        });
    }

    // Check whether extension is valid (.xlsx or .csv)
    const targetPath = data.targetPath?.toLowerCase();

    if (targetPath && !targetPath.endsWith(".xlsx") && !targetPath.endsWith(".csv")) {
        ctx.addIssue({
            code: "custom",
            message: "Please specify a valid extension (.xlsx or .csv)",
            path: ["targetPath"]
        });
    }
})

export type ExportProfileFormValues = z.infer<typeof exportProfileSchema>;