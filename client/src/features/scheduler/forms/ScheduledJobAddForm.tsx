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
import { useCreateScheduledJob } from "../../../hooks/useScheduledJobs";
import {
  scheduledJobSchema,
  type ScheduledJobFormValues
} from "../schemas/scheduledJobSchema";
import ScheduledJobForm from "../forms/ScheduledJobForm";

export default function ScheduledJobAddForm({ jobTaskKey }: { jobTaskKey: string }) {
  const [open, setOpen] = useState(false);
  const createScheduledJob = useCreateScheduledJob();

  const form = useForm<ScheduledJobFormValues>({
    resolver: zodResolver(scheduledJobSchema),
    defaultValues: {
      jobTaskKey: jobTaskKey,
      name: "",
      description: undefined,
      jobType: "",
      calculationType: undefined,
      importDataType: undefined,
      periodType: undefined,
      source: undefined,
      target: undefined,
      isArchived: false
    }
  });

  const onSubmit = async (values: ScheduledJobFormValues) => {
    const calculationType = values.calculationType === "None" ? undefined : values.calculationType;
    const importDataType = values.importDataType === "None" ? undefined : values.importDataType;
    const periodType = values.periodType === "None" ? undefined : values.periodType;

    await createScheduledJob.mutateAsync({
        jobTaskKey: jobTaskKey,
        name: values.name,
        description: values.description,
        jobType: values.jobType,
        calculationType: calculationType,
        importDataType: importDataType,
        periodType: periodType || undefined,
        source: values.source,
        target: values.target
    });
    form.reset();
    setOpen(false);
  };

  return (
    <>
      <Button variant="contained" size="small" onClick={() => setOpen(true)}>
        Add scheduled job
      </Button>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <form
          onSubmit={form.handleSubmit(
            onSubmit,
            errors => console.log("CREATE SCHEDULEDJOB ERRORS", errors)
          )}
        >
          <DialogTitle>Add scheduled job</DialogTitle>

          <DialogContent>
            <ScheduledJobForm mode="create" form={form} />
          </DialogContent>

          <DialogActions>
            <Button onClick={() => setOpen(false)}>Cancel</Button>
            <Button
              type="submit"
              variant="contained"
              disabled={createScheduledJob.isPending}
            >
              Save
            </Button>
          </DialogActions>
        </form>
      </Dialog>
    </>
  );
}
