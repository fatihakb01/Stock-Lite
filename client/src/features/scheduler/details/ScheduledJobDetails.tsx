import {
  Stack,
  Typography,
  Button,
  Divider
} from "@mui/material";
import { useState } from "react";
import {
  useRunScheduledJob,
  useScheduledJob,
  useScheduledJobHistory
} from "../../../hooks/useScheduledJobs";
import ScheduledJobEditForm from "../forms/ScheduledJobEditForm";
import ExecutionTable from "../tables/ExecutionTable";

export default function ScheduledJobDetails({ scheduledJobId }: { scheduledJobId: string }) {
  const { data, isLoading, isError } = useScheduledJob(scheduledJobId);
  const runScheduledJob = useRunScheduledJob(scheduledJobId);
  const { data: history } = useScheduledJobHistory(scheduledJobId);

  const [isEditing, setIsEditing] = useState(false);
  const [showHistory, setShowHistory] = useState(false);

  
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

      <Typography><strong>Description:</strong> {data.description || "—"}</Typography>
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
        <Button variant="contained" onClick={() => setIsEditing(true)}>
          Edit
        </Button>

        <Button
          variant="outlined"
          onClick={() => runScheduledJob.mutate()}
          disabled={runScheduledJob.isPending}
        >
          Run scheduled job
        </Button>

        <Button
          variant="text"
          onClick={() => setShowHistory(v => !v)}
        >
          {showHistory ? "Hide history" : "Show history"}
        </Button>
      </Stack>

      {showHistory && (
        history && history.length > 0 ? (
          <>
            <Divider />
            <ExecutionTable rows={history} />
          </>
        ) : (
          <Typography>No history available</Typography>
        )
      )}
    </Stack>
  );
}
