import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  scheduledJobSchema,
  type ScheduledJobFormValues
} from "../schemas/scheduledJobSchema";
import { useEditScheduledJob } from "../../../hooks/useScheduledJobs";
import type { ScheduledJob, EditScheduledJobDto } from "../../../types/scheduledJobs/scheduledJobTypes";
import ScheduledJobForm from "./ScheduledJobForm";

export default function ScheduledJobEditForm({
  scheduledJob,
  onCancel,
  onSaved
}: {
  scheduledJob: ScheduledJob;
  onCancel: () => void;
  onSaved: () => void;
}) {
  const editScheduledJob = useEditScheduledJob(scheduledJob.id);

  const form = useForm<ScheduledJobFormValues>({
    resolver: zodResolver(scheduledJobSchema),
    defaultValues: {
      jobTaskKey: scheduledJob.jobTaskKey,
      name: scheduledJob.name,
      description: scheduledJob.description || undefined,
      jobType: scheduledJob.jobType,
      calculationType: scheduledJob.calculationType || undefined,
      importDataType: scheduledJob.importDataType || undefined,
      periodType: scheduledJob.periodType || undefined,
      source: scheduledJob.source || undefined,
      target: scheduledJob.target || undefined,
      isArchived: scheduledJob.isArchived
    }
  });

  const onSubmit = async (values: ScheduledJobFormValues) => {
    const dto: EditScheduledJobDto = { ...values };
    await editScheduledJob.mutateAsync(dto);
    onSaved();
  };

  return (
    <Dialog open fullWidth onClose={onCancel}>
      <form
        onSubmit={form.handleSubmit(
          onSubmit,
          errors => console.log("EDIT FORM ERRORS", errors)
        )}
      >
        <DialogTitle>Edit scheduled job</DialogTitle>

        <DialogContent>
          <ScheduledJobForm mode="edit" form={form} />
        </DialogContent>

        <DialogActions>
          <Button onClick={onCancel}>Cancel</Button>
          <Button
            type="submit"
            variant="contained"
            disabled={editScheduledJob.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
