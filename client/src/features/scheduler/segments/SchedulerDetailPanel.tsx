import { Typography, Stack } from "@mui/material";
import JobTaskDetails from "../details/JobTaskDetails";
import ScheduledJobDetails from "../details/ScheduledJobDetails";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import type { CurrentUser } from "../../../types/users/userTypes";

export default function SchedulerDetailPanel({ selection, user }: {
  selection: SchedulerSelection,
  user: CurrentUser
}) {
  if (!selection) {
    return <Typography>Select a job task or scheduled job</Typography>;
  }

  return (
    <Stack spacing={2}>
      {selection.type === "jobtask" && (
        <JobTaskDetails jobTaskId={selection.jobTaskId} user={user} />
      )}

      {selection.type === "scheduledjob" && (
        <ScheduledJobDetails scheduledJobId={selection.scheduledJobId} user={user} />
      )}
    </Stack>
  );
}
