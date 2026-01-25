import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
  Paper
} from "@mui/material";
import { useRecentHistory } from "../../../hooks/useJobTasks";
import { statusMap } from "../../../types/jobTasks/jobExecutionStatusMap";

export default function RecentHistoryTab() {
  const { data, isLoading } = useRecentHistory();

  if (isLoading) {
    return <Typography>Loading execution history…</Typography>;
  }

  if (!data?.length) {
    return <Typography>No executions found.</Typography>;
  }

  return (
    <Paper>
      <Table size="small">
        <TableHead>
          <TableRow>
            <TableCell>Started</TableCell>
            <TableCell>Finished</TableCell>
            <TableCell>Status</TableCell>
            <TableCell>Error Message</TableCell>
          </TableRow>
        </TableHead>

        <TableBody>
          {data?.map(job =>
            <TableRow key={job.startedAt.toString()}>
              <TableCell>{job.startedAt.toString()}</TableCell>
              <TableCell>{job.finishedAt?.toString() ?? "—"}</TableCell>
              <TableCell>{statusMap[job.status]}</TableCell>
              <TableCell>{job.errorMessage?.toString() ?? "—"}</TableCell>
            </TableRow>
            )
          }
        </TableBody>
      </Table>
    </Paper>
  );
}
