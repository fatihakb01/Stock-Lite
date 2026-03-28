import { Stack, Typography, Button, Divider, CircularProgress } from "@mui/material";
import { useState } from "react";
import { useCleanJobTask, useDeleteJobTask, useJobTask, useJobTaskHistory, useRunJobTask } from "../../../hooks/useJobTasks";
import JobTaskEditForm from "../forms/JobTaskEditForm";
import ExecutionTable from "../tables/ExecutionTable";
import type { CurrentUser } from "../../../types/users/userTypes";

export default function JobTaskDetails({ jobTaskId, user } : { jobTaskId: string, user: CurrentUser }) {
  const [isEditing, setIsEditing] = useState(false);
  const [showHistory, setShowHistory] = useState(false);
  const showAll = false;

  const { data, isLoading } = useJobTask(jobTaskId);
  const runJobTask = useRunJobTask(jobTaskId);
  const cleanJobTask = useCleanJobTask(jobTaskId);
  const deleteJobTask = useDeleteJobTask(jobTaskId);
  const { data: history } = useJobTaskHistory(jobTaskId, showHistory);

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

      <Typography 
        variant="body2" 
        color="text.secondary" 
        sx={{
          whiteSpace: "normal",
          wordBreak: "break-word",
          maxWidth: 500
        }}
      >
        {data.description || "No description"}
      </Typography>

      <Divider />

      <Typography variant="caption">
        JobTask key: {data.jobTaskKey}
      </Typography>

      <Typography variant="caption">
        Jobs linked: {data.jobs.length}
      </Typography>

      {(data && data.allowManualTriggerOnly ? (
          <Typography variant="caption">
              Manual task
          </Typography>   
      ) : (
          <Typography variant="caption">
              Scheduled at: {data.nextRunAt?.replace("T", " ") ?? data.scheduledAt?.replace("T", " ")}
          </Typography>    
      ))}

      {(data && (data.nextRunAt || data.scheduledAt) && (
        <>
          <Typography variant="caption">
              Period type: {data.periodType}
          </Typography>

          <Typography variant="caption">
              Maximum retries: {data.maxRetries}
          </Typography>

          <Typography variant="caption">
              Retry delay minutes: {data.retryDelayMinutes}
          </Typography>
        </>
      ))}

      <Typography variant="caption">
        Status: {data.isArchived ? "Archived" : "Active"}
      </Typography>

      <Stack direction="row" spacing={1}>
        {user && user.role === "SuperAdmin" && (
          <>
            <Button variant="contained" onClick={() => setIsEditing(true)}>
              Edit
            </Button>
            
            <Button
              variant="contained"
              onClick={() => runJobTask.mutate()}
              disabled={runJobTask.isPending}
            >
                Run job task
            </Button>

            <Button
                variant="contained"
                onClick={() => cleanJobTask.mutate()}
                disabled={cleanJobTask.isPending}
            >
                Clean job task
            </Button>

            <Button
              sx={{ bgcolor:"red" }}
              variant="contained"
              onClick={() => deleteJobTask.mutate()}
              disabled={deleteJobTask.isPending}
            >
              Delete job task
            </Button>
          </>
        )}

        <Button
          variant="contained"
          onClick={() => setShowHistory(v => !v)}
        >
          {showHistory ? "Hide history" : "Show history"}
        </Button>

        {runJobTask.isPending && <CircularProgress size={20} />}
      </Stack>

      {showHistory && (
      history && history.length > 0 ?
        (
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
