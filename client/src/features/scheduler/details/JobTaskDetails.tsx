import { Stack, Typography, Button, Divider } from "@mui/material";
import { useState } from "react";
import { useJobTask, useJobTaskHistory, useRunJobTask } from "../../../hooks/useJobTasks";
import JobTaskEditForm from "../forms/JobTaskEditForm";
import { statusMap } from "../../../types/jobTasks/jobExecutionStatusMap";

export default function JobTaskDetails({jobTaskId} : {jobTaskId: string;}) {
  const { data, isLoading } = useJobTask(jobTaskId);
  const runJobTask = useRunJobTask(jobTaskId);
  const { data: history } = useJobTaskHistory(jobTaskId);

  const [isEditing, setIsEditing] = useState(false);
  const [showHistory, setShowHistory] = useState(false);

  if (isLoading || !data) {
    return <Typography>Loading job task…</Typography>;
  }

  if (isEditing) {
    return (
      <JobTaskEditForm
        jobTask={data}
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
        JobTask key: {data.jobTaskKey}
      </Typography>

      <Typography variant="caption">
        Jobs linked: {data.jobs.length}
      </Typography>

      <Stack direction="row" spacing={1}>
        <Button variant="contained" onClick={() => setIsEditing(true)}>
          Edit
        </Button>

        <Button 
          variant="outlined"
          onClick={() => runJobTask.mutate()}
          disabled={runJobTask.isPending}
        >
          Run job task
        </Button>

        <Button
          variant="text"
          onClick={() => setShowHistory(v => !v)}
        >
          {showHistory ? "Hide history" : "Show history"}
        </Button>
      </Stack>

      {showHistory && (
        <>
          <Divider />

          {history?.jobs.flatMap(job =>
            job.jobExecutions.map(exec => (
              <Typography
                key={`${job.id}-${exec.startedAt.toString()}`}
                variant="caption"
              >
                {exec.startedAt.toString()} — {exec.finishedAt?.toString()} — {statusMap[exec.status]} — {exec.errorMessage}
              </Typography>
            ))
          )}
        </>
      )}
    </Stack>
  );
}
