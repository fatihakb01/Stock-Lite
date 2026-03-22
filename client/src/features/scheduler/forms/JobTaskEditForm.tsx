import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEditJobTask } from "../../../hooks/useJobTasks";
import {
    jobTaskSchema,
  type JobTaskFormValues
} from "../schemas/jobTaskSchema";
import type { EditJobTaskDto, JobTaskDetail } from "../../../types/jobTasks/jobTaskTypes";
import JobTaskForm from "./JobTaskForm";

export default function JobTaskEditForm({
  jobTask,
  onCancel,
  onSaved
}: {
  jobTask: JobTaskDetail;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const editJobTask = useEditJobTask(jobTask.id);

  const form = useForm<JobTaskFormValues>({
    resolver: zodResolver(jobTaskSchema),
    defaultValues: {
      jobTaskKey: jobTask.jobTaskKey,
      name: jobTask.name,
      description: jobTask.description || undefined,
      nextRunAt: jobTask.nextRunAt || jobTask.scheduledAt || undefined,
      scheduledAt: undefined,
      periodType: jobTask.periodType || undefined,
      maxRetries: jobTask.maxRetries || undefined,
      retryDelayMinutes: jobTask.retryDelayMinutes || undefined,
      isArchived: jobTask.isArchived
    }
  });

    const onSubmit = async (values: JobTaskFormValues) => {
      const nextRunAt = values.nextRunAt || undefined;
      const periodType = values.periodType === "None" ? undefined : values.periodType;

      const dto: EditJobTaskDto = {
        jobTaskKey: values.jobTaskKey,
        name: values.name,
        description: values.description || undefined,
        nextRunAt: nextRunAt,
        scheduledAt: nextRunAt,
        periodType: periodType || undefined,
        maxRetries: values.maxRetries || undefined,
        retryDelayMinutes: values.retryDelayMinutes || undefined,
        isArchived: values.isArchived
      };
      
      await editJobTask.mutateAsync(dto);
      onSaved();
  };

  return (
    <Dialog open fullWidth onClose={onCancel}>
      <form
        onSubmit={form.handleSubmit(
          onSubmit,
          errors => console.log("EDIT JOBTASK ERRORS", errors)
        )}
      >
        <DialogTitle>Edit job task</DialogTitle>

        <DialogContent>
          <JobTaskForm mode="edit" form={form} />
        </DialogContent>

        <DialogActions>
          <Button onClick={onCancel}>Cancel</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={editJobTask.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
