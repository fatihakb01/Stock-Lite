import { Grid, Paper } from "@mui/material";
import { useSchedulerSelection } from "../selector/useSchedulerSelection";
import JobTaskList from "../segments/JobTaskList";
import ScheduledJobList from "../segments/ScheduledJobList";
import SchedulerDetailPanel from "../segments/SchedulerDetailPanel";
import { useCurrentUser } from "../../../hooks/useUsers";

export default function SchedulerOverviewTab() {
  const selection = useSchedulerSelection();
  const { data: currentUser } = useCurrentUser();

  return (
    <Grid container spacing={2} sx={{ m:"auto" }}>
      {/* LEFT: JobTasks */}
      <Grid sx={{ xs: 5 }}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <JobTaskList
            selected={selection.selection}
            onSelect={selection.selectJobTask}
            user={currentUser!}
          />
        </Paper>
      </Grid>

      {/* MIDDLE: ScheduledJobs */}
      <Grid sx={{ xs: 5 }}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <ScheduledJobList
            selected={selection.selection}
            onSelect={selection.selectScheduledJob}
            user={currentUser!}
          />
        </Paper>
      </Grid>

      {/* RIGHT: Details */}
      <Grid sx={{ xs: 5 }}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <SchedulerDetailPanel selection={selection.selection} user={currentUser!} />
        </Paper>
      </Grid>
    </Grid>
  );
}
