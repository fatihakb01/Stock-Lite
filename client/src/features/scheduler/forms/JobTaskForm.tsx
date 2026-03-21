import {
    Stack,
    TextField,
    Checkbox,
    FormControlLabel,
    MenuItem
} from "@mui/material";
import type { UseFormReturn } from "react-hook-form";
import { useWatch } from "react-hook-form";
import { periodTypeMap } from "../../../types/scheduledJobs/periodTypeMap";
import type { JobTaskFormValues } from "../schemas/jobTaskSchema";
import { useEffect } from "react";
import BasicDateTimePicker from "../../../components/fields/BasicDateTimePicker";

type Props = {
    mode: "create" | "edit";
    form: UseFormReturn<JobTaskFormValues>
};

export default function JobTaskForm({ mode, form }: Props) {
    const scheduledAt = useWatch({ control: form.control, name: "scheduledAt" });
    const nextRunAt = useWatch({ control: form.control, name: "nextRunAt" });

    const dateHasValue = mode === "create"
        ? !!scheduledAt
        : !!nextRunAt;

    // Clear irrelevant fields when date field has no value anymore
    useEffect(() => {
        if (!dateHasValue) {
            form.setValue("periodType", undefined);
            form.setValue("maxRetries", undefined);
            form.setValue("retryDelayMinutes", undefined);
        }
    }, [dateHasValue]);

    return (
        <Stack spacing={2} mt={1}>
            <TextField
                label="JobTaskKey"
                {...form.register("jobTaskKey")}
                error={!!form.formState.errors.jobTaskKey}
                helperText={form.formState.errors.jobTaskKey?.message}
            />

            <TextField
                label="Name"
                {...form.register("name")}
                error={!!form.formState.errors.name}
                helperText={form.formState.errors.name?.message}
            />

            <TextField
                label="Description"
                multiline
                rows={3}
                {...form.register("description")}
                error={!!form.formState.errors.description}
                helperText={form.formState.errors.description?.message}
            />

            {mode === "create" && (
                <BasicDateTimePicker
                    name="scheduledAt"
                    control={form.control}
                    label="Scheduled At"
                />
            )}

            {mode === "edit" && (
                <BasicDateTimePicker
                    name="nextRunAt"
                    control={form.control}
                    label="Next Run At"
                />
            )}

            {dateHasValue && (
                <>
                    <TextField
                        select
                        label="Period Type"
                        {...form.register("periodType")}
                        error={!!form.formState.errors.periodType}
                        helperText={form.formState.errors.periodType?.message}
                        defaultValue={form.getValues().periodType}
                    >
                        {Object.values(periodTypeMap).map(value => (
                            <MenuItem key={value} value={value}>
                                {value}
                            </MenuItem>
                        ))}
                    </TextField>

                    <TextField
                        label="Max Retries"
                        {...form.register("maxRetries", { valueAsNumber: true })}
                        error={!!form.formState.errors.maxRetries}
                        helperText={form.formState.errors.maxRetries?.message}
                    />

                    <TextField
                        label="Retry Delay Minutes"
                        {...form.register("retryDelayMinutes", { valueAsNumber: true })}
                        error={!!form.formState.errors.retryDelayMinutes}
                        helperText={form.formState.errors.retryDelayMinutes?.message}
                    />
                </>
            )}

            {mode === "edit" && (
                <FormControlLabel
                    control={<Checkbox {...form.register("isArchived")} />}
                    label="Archived"
                />
            )}
        </Stack>
    );
}