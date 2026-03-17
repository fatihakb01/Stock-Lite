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

export default function AddJobTaskButton() {
  const [open, setOpen] = useState(false);
  const createJobTask = useCreateJobTask();

  const form = useForm<JobTaskFormValues>({
    resolver: zodResolver(jobTaskSchema),
    defaultValues: {
      jobTaskKey: "",
      name: "",
      description: "",
      scheduledAt: "",
      periodType: "",
      maxRetries: 1,
      retryDelayMinutes: 1,
      isArchived: false
    }
  });

  const onSubmit = async (values: JobTaskFormValues) => {
    await createJobTask.mutateAsync({
      jobTaskKey: values.jobTaskKey,
      name: values.name,
      description: values.description,
      scheduledAt: values.scheduledAt,
      periodType: values.periodType,
      maxRetries: values.maxRetries,
      retryDelayMinutes: values.retryDelayMinutes
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
