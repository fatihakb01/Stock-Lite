import {
  Stack,
  TextField,
  Checkbox,
  FormControlLabel
} from "@mui/material";
import type { UseFormReturn } from "react-hook-form";
import type { JobTaskFormValues } from "../schemas/jobTaskSchema";

type Props = {
  mode: "create" | "edit";
  form: UseFormReturn<JobTaskFormValues>;
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

      {mode === "edit" && (
        <FormControlLabel
          control={<Checkbox {...form.register("isArchived")} />}
          label="Archived"
        />
      )}
    </Stack>
  );
}
