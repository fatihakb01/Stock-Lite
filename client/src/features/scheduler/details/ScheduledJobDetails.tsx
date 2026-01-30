import {
  Stack,
  Typography,
  Button,
  Divider,
  CircularProgress
} from "@mui/material";
import { useState } from "react";
import {
  useDeleteScheduledJob,
  useRunScheduledJob,
  useScheduledJob,
  useScheduledJobHistory
} from "../../../hooks/useScheduledJobs";
import ScheduledJobEditForm from "../forms/ScheduledJobEditForm";
import ExecutionTable from "../tables/ExecutionTable";
import type { User } from "../../../types/users/userTypes";

export default function ScheduledJobDetails({ scheduledJobId, user } : 
  { scheduledJobId: string, user: User }) {
  const [isEditing, setIsEditing] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const showAll = false;

  const { data, isLoading, isError } = useScheduledJob(scheduledJobId);
  const runScheduledJob = useRunScheduledJob(scheduledJobId);
  const deleteScheduledJob = useDeleteScheduledJob(scheduledJobId);
  const { data: history } = useScheduledJobHistory(scheduledJobId, showHistory);
  
  if (isLoading) {
    return <Typography>Loading scheduled job…</Typography>;
  }

  if (isError || !data) {
    return <Typography>Scheduled job not found.</Typography>
  }

  if (isEditing) {
    return (
      <ScheduledJobEditForm
        scheduledJob={data}
        onCancel={() => setIsEditing(false)}
        onSaved={() => setIsEditing(false)}
      />
    );
  }

  return (
    <Stack spacing={2}>
      <Typography variant="h6">{data.name}</Typography>

      <Divider />

      <Typography
        sx={{
          whiteSpace: "normal",
          wordBreak: "break-word",
          maxWidth: 500
        }}
      >
        <strong>Description:</strong> {data.description || "—"}
      </Typography>
      <Typography><strong>Job type:</strong> {data.jobType}</Typography>

      {data.calculationType && (
        <Typography><strong>Calculation type:</strong> {data.calculationType}</Typography>
      )}

      {data.importDataType && (
        <Typography><strong>Import data type:</strong> {data.importDataType}</Typography>
      )}

      {data.periodType && (
        <Typography><strong>Period type:</strong> {data.periodType}</Typography>
      )}

      {data.source && (
        <Typography><strong>Source:</strong> {data.source}</Typography>
      )}

      {data.target && (
        <Typography><strong>Target:</strong> {data.target}</Typography>
      )}

      <Typography>
        <strong>Status:</strong> {data.isArchived ? "Archived" : "Active"}
      </Typography>

      <Divider />

      <Stack direction="row" spacing={1}>
        {user && user.role === "SuperAdmin" && (
        <>
          <Button variant="contained" onClick={() => setIsEditing(true)}>
            Edit
          </Button>

          <Button
            variant="contained"
            onClick={() => runScheduledJob.mutate()}
            disabled={runScheduledJob.isPending}
          >
            Run scheduled job
          </Button>

          <Button
            sx={{ bgcolor:"red" }}
            variant="contained"
            onClick={() => deleteScheduledJob.mutate()}
            disabled={deleteScheduledJob.isPending}
          >
            Delete scheduled job
          </Button>
        </>
        )}

        <Button
          variant="contained"
          onClick={() => setShowHistory(v => !v)}
        >
          {showHistory ? "Hide history" : "Show history"}
        </Button>

        {runScheduledJob.isPending && <CircularProgress size={20} />}
      </Stack>

      {showHistory && (
        history && history.length > 0 ? (
          <>
            <Divider />
            <ExecutionTable rows={history} full={showAll}/>
          </>
        ) : (
          <Typography>No history available</Typography>
        )
      )}
    </Stack>
  );
}
