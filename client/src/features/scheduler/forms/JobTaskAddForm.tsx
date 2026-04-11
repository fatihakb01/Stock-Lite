import { useState } from "react";
import {
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateJobTask } from "../../../hooks/useJobTasks";
import {
    jobTaskSchema,
  type JobTaskFormValues
} from "../schemas/jobTaskSchema";
import JobTaskForm from "../forms/JobTaskForm";

export default function JobTaskAddForm() {
  const [open, setOpen] = useState(false);
  const createJobTask = useCreateJobTask();

  const form = useForm<JobTaskFormValues>({
    resolver: zodResolver(jobTaskSchema),
    defaultValues: {
      jobTaskKey: "",
      name: "",
      scheduledAt: "",
      description: "",
      periodType: undefined,
      maxRetries: undefined,
      retryDelayMinutes: undefined,
      isArchived: false
    }
  });

  const onSubmit = async (values: JobTaskFormValues) => {
    const periodType = values.periodType === "None" ? undefined : values.periodType;

    await createJobTask.mutateAsync({
      jobTaskKey: values.jobTaskKey,
      name: values.name,
      description: values.description,
      scheduledAt: values.scheduledAt || undefined,
      periodType: periodType || undefined,
      maxRetries: values.maxRetries || undefined,
      retryDelayMinutes: values.retryDelayMinutes || undefined
    });
    form.reset();
    setOpen(false);
  };

  return (
    <>
      <Button variant="contained" size="small" onClick={() => setOpen(true)}>
        Add job task
      </Button>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <form
          onSubmit={form.handleSubmit(
            onSubmit,
            errors => console.log("CREATE JOBTASK ERRORS", errors)
          )}
        >
          <DialogTitle>Add job task</DialogTitle>

          <DialogContent>
            <JobTaskForm mode="create" form={form} />
          </DialogContent>

          <DialogActions>
            <Button onClick={() => setOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createJobTask.isPending}
            >
              Save
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  );
}
