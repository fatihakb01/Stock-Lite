import {
  Stack,
  TextField,
  Checkbox,
  FormControlLabel,
  MenuItem
} from "@mui/material";
import type { UseFormReturn } from "react-hook-form";
import { periodTypeMap } from "../../../types/scheduledJobs/periodTypeMap";
import type { JobTaskFormValues } from "../schemas/jobTaskSchema";

type Props = {
    mode: "create" | "edit";
    form: UseFormReturn<JobTaskFormValues>
};
export default function JobTaskForm({ mode, form }: Props) {
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
          <TextField
              label="Scheduled At"
              {...form.register("scheduledAt")}
              error={!!form.formState.errors.scheduledAt}
              helperText={form.formState.errors.scheduledAt?.message}
          />
      )}

      {mode === "edit" && (
          <TextField
              label="Next Run At"
              {...form.register("nextRunAt")}
              error={!!form.formState.errors.nextRunAt}
              helperText={form.formState.errors.nextRunAt?.message}
          />
      )}

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
          label="MaxRetries"
          {...form.register("maxRetries", { valueAsNumber: true })}
          error={!!form.formState.errors.maxRetries}
          helperText={form.formState.errors.maxRetries?.message}
      />

      <TextField
          label="RetryDelayMinutes"
          {...form.register("retryDelayMinutes", { valueAsNumber: true })}
          error={!!form.formState.errors.retryDelayMinutes}
          helperText={form.formState.errors.retryDelayMinutes?.message}
      />

      {mode === "edit" && (
        <FormControlLabel
          control={<Checkbox {...form.register("isArchived")} />}
          label="Archived"
        />
      )}
    </Stack>
  );
}
