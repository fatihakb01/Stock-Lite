import { Grid, Paper } from "@mui/material";
import { useSchedulerSelection } from "../selector/useSchedulerSelection";
import JobTaskList from "../segments/JobTaskList";
import ScheduledJobList from "../segments/ScheduledJobList";
import SchedulerDetailPanel from "../segments/SchedulerDetailPanel";

export default function SchedulerOverviewTab() {
  const selection = useSchedulerSelection();

  return (
    <Grid container spacing={2}>
      {/* LEFT: JobTasks */}
      <Grid sx={{xs:5}}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <JobTaskList
            selected={selection.selection}
            onSelect={selection.selectJobTask}
          />
        </Paper>
      </Grid>

      {/* MIDDLE: ScheduledJobs */}
      <Grid sx={{xs:5}}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <ScheduledJobList
            selected={selection.selection}
            onSelect={selection.selectScheduledJob}
          />
        </Paper>
      </Grid>

      {/* RIGHT: Details */}
      <Grid sx={{xs:5}}>
        <Paper sx={{ p: 2, height: "100%" }}>
          <SchedulerDetailPanel selection={selection.selection} />
        </Paper>
      </Grid>
    </Grid>
  );
}
