import type { JobTaskExecution } from "../../../types/jobTasks/jobTaskTypes";
import { Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";
import { statusMap } from "../../../types/jobTasks/jobExecutionStatusMap";

interface Props {
    rows: JobTaskExecution[]
}

export default function ExecutionTable({rows} : Props) {
    return (
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>JobTask Name</TableCell>
              <TableCell>ScheduledJob Name</TableCell>
              <TableCell>Started</TableCell>
              <TableCell>Finished</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Error Message</TableCell>
            </TableRow>
          </TableHead>
            <TableBody>
                {rows?.map((row, i) => (
                    <TableRow key={i}>
                        <TableCell>{row.jobTaskName}</TableCell>
                        <TableCell>{row.scheduledJobName}</TableCell>
                        <TableCell>{row.startedAt ?? "-"}</TableCell>
                        <TableCell>{row.finishedAt ?? "-"}</TableCell>
                        <TableCell>{ statusMap[row.status] }</TableCell>
                        <TableCell>{ row.errorMessage ?? "—" }</TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    )
}