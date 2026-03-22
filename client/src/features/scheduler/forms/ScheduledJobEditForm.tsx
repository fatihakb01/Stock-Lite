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
    const calculationType = values.calculationType === "None" ? undefined : values.calculationType;
    const importDataType = values.importDataType === "None" ? undefined : values.importDataType;
    const periodType = values.periodType === "None" ? undefined : values.periodType;

    const dto: EditScheduledJobDto = {
        jobTaskKey: scheduledJob.jobTaskKey,
        name: values.name,
        description: values.description,
        jobType: values.jobType,
        calculationType: calculationType || undefined,
        importDataType: importDataType || undefined,
        periodType: periodType || undefined,
        source: values.source,
        target: values.target,
        isArchived: values.isArchived
    };

    await editScheduledJob.mutateAsync(dto);
    onSaved();
  };

  return (
    <Dialog open fullWidth onClose={onCancel}>
      <form
        onSubmit={form.handleSubmit(
          onSubmit,
          errors => console.log("EDIT SCHEDULEDJOB ERRORS", errors)
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
