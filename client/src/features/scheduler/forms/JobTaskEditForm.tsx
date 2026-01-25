import { Stack, Button, TextField, Checkbox, FormControlLabel } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEditJobTask } from "../../../hooks/useJobTasks";
import { jobTaskSchema, type JobTaskFormValues } from "../schemas/jobTaskSchema";
import type { EditJobTaskDto, JobTaskDetail } from "../../../types/jobTasks/jobTaskTypes";

export default function JobTaskEditForm({
  jobTask,
  onCancel,
  onSaved
}: {
  jobTask: JobTaskDetail
  onCancel: () => void
  onSaved: () => void
}) {
  const form = useForm<JobTaskFormValues>({
    resolver: zodResolver(jobTaskSchema),
    defaultValues: {
      jobTaskKey: jobTask.jobTaskKey,
      name: jobTask.name,
      description: jobTask.description,
      isArchived: jobTask.isArchived
    }
  });

  const editJobTask = useEditJobTask(jobTask.id);

  const onSubmit = (values: JobTaskFormValues) => {
    const dto: EditJobTaskDto = {
      ...values,
      jobTaskKey: jobTask.jobTaskKey,
    };

    editJobTask.mutate(dto, {
      onSuccess: onSaved
    });
  };

  return (
    <form onSubmit={form.handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <TextField
          label="Name"
          {...form.register("name")}
        />

        <TextField
          label="Description"
          {...form.register("description")}
          multiline
          rows={3}
        />

        <FormControlLabel
          control={<Checkbox {...form.register("isArchived")} />}
          label="Archived"
        />

        <Stack direction="row" spacing={1}>
          <Button type="submit" variant="contained">
            Save
          </Button>
          <Button onClick={onCancel}>
            Cancel
          </Button>
        </Stack>
      </Stack>
    </form>
  );
}
