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

export default function AddScheduledJobButton({ jobTaskKey }: { jobTaskKey: string }) {
  const [open, setOpen] = useState(false);
  const createScheduledJob = useCreateScheduledJob();

  const form = useForm<ScheduledJobFormValues>({
    resolver: zodResolver(scheduledJobSchema),
    defaultValues: {
      jobTaskKey,
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
    console.log("SUBMIT VALUES", values);
    await createScheduledJob.mutateAsync(values);
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
            errors => console.log("FORM ERRORS", errors)
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
