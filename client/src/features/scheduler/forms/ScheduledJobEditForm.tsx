import {
  Stack,
  Button,
  TextField,
  Checkbox,
  FormControlLabel
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  scheduledJobSchema,
  type ScheduledJobFormValues
} from "../schemas/scheduledJobSchema";
import { useEditScheduledJob } from "../../../hooks/useScheduledJobs";
import type { EditScheduledJobDto, ScheduledJob } from "../../../types/scheduledJobs/scheduledJobTypes";

export default function ScheduledJobEditForm({
  scheduledJob,
  onCancel,
  onSaved
}: {
  scheduledJob: ScheduledJob
  onCancel: () => void
  onSaved: () => void
}) {
  const form = useForm<ScheduledJobFormValues>({
    resolver: zodResolver(scheduledJobSchema),
    defaultValues: {
      name: scheduledJob.name,
      description: scheduledJob.description,
      jobType: scheduledJob.jobType,
      calculationType: scheduledJob.calculationType,
      importDataType: scheduledJob.importDataType,
      periodType: scheduledJob.periodType,
      source: scheduledJob.source,
      target: scheduledJob.target,
      executionOrder: scheduledJob.executionOrder,
      isArchived: scheduledJob.isArchived
    }
  });

  const editScheduledJob = useEditScheduledJob(scheduledJob.id);

  const onSubmit = (values: ScheduledJobFormValues) => {
    const dto: EditScheduledJobDto = {
      jobTaskKey: scheduledJob.jobTaskKey,
      ...values
    };

    editScheduledJob.mutate(dto, {
      onSuccess: onSaved
    });
  };

  return (
    <form onSubmit={form.handleSubmit(onSubmit)}>
      <Stack spacing={2}>
        <TextField label="Name" {...form.register("name")} />

        <TextField
          label="Description"
          {...form.register("description")}
          multiline
          rows={3}
        />

        <TextField label="Source" {...form.register("source")} />
        <TextField label="Target" {...form.register("target")} />

        <TextField
          label="Execution order"
          type="number"
          {...form.register("executionOrder", { valueAsNumber: true })}
        />

        <FormControlLabel
          control={<Checkbox {...form.register("isArchived")} />}
          label="Archived"
        />

        <Stack direction="row" spacing={1}>
          <Button type="submit" variant="contained">
            Save
          </Button>
          <Button onClick={onCancel}>Cancel</Button>
        </Stack>
      </Stack>
    </form>
  );
}
