import { Typography, Stack } from "@mui/material";
import JobTaskDetails from "../details/JobTaskDetails";
import ScheduledJobDetails from "../details/ScheduledJobDetails";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";

export default function SchedulerDetailPanel({ selection }: {
  selection: SchedulerSelection
}) {
  if (!selection) {
    return <Typography>Select a job task or scheduled job</Typography>;
  }

  return (
    <Stack spacing={2}>
      {selection.type === "jobtask" && (
        <JobTaskDetails jobTaskId={selection.id} />
      )}

      {selection.type === "scheduledjob" && (
        <ScheduledJobDetails scheduledJobId={selection.id} />
      )}
    </Stack>
  );
}
