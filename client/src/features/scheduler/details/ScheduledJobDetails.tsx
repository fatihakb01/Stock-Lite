import { Stack, Typography, Button, Divider } from "@mui/material";
import { useState } from "react";
import { useRunScheduledJob, useScheduledJob, useScheduledJobHistory } from "../../../hooks/useScheduledJobs";
import ScheduledJobEditForm from "../forms/ScheduledJobEditForm";
import ExecutionTable from "../tables/ExecutionTable";

export default function ScheduledJobDetails({ scheduledJobId }: { scheduledJobId: string }) {
  const { data, isLoading } = useScheduledJob(scheduledJobId);
  const runScheduledJob = useRunScheduledJob(scheduledJobId);
  const { data: history } = useScheduledJobHistory(scheduledJobId);

  const [isEditing, setIsEditing] = useState(false);
  const [showHistory, setShowHistory] = useState(false);

  if (isLoading || !data) {
    return <Typography>Loading scheduled job…</Typography>;
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

      <Typography variant="body2" color="text.secondary">
        {data.description || "No description"}
      </Typography>

      <Divider />

      <Typography variant="caption">
        Execution order: {data.executionOrder}
      </Typography>

      <Typography variant="caption">
        Source → Target: {data.source} → {data.target}
      </Typography>

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
      history && history.length > 0 ?
        (
          <>
            <Divider />
            <ExecutionTable rows={history}/> 
          </>
        ) : (
          <Typography>No history available</Typography>
        )
      )}
    </Stack>
  );
}
