import type { JobTaskExecution } from "../../../types/jobTasks/jobTaskTypes";
import { Table, TableBody, TableCell, TableHead, TableRow } from "@mui/material";

interface Props {
    rows: JobTaskExecution[]
    full: boolean
}

export default function ExecutionTable({ rows, full } : Props) {
    return (
        <Table size="small">
          <TableHead>
            <TableRow>
              {full && (<TableCell>JobTask Name</TableCell>)}
              {full && (<TableCell>ScheduledJob Name</TableCell>)}
              {full && (<TableCell>Trigger Type</TableCell>)}
              <TableCell>Started</TableCell>
              <TableCell>Finished</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Error Message</TableCell>
            </TableRow>
          </TableHead>
            <TableBody>
                {rows?.map((row, i) => (
                    <TableRow key={i}>
                        {full && (<TableCell>{row.jobTaskName}</TableCell>)}
                        {full && (<TableCell>{row.scheduledJobName}</TableCell>)}
                        {full && (<TableCell>{row.triggerType}</TableCell>)}
                        <TableCell>{row.startedAt.replace("T", " ") ?? "-"}</TableCell>
                        <TableCell>{row.finishedAt?.replace("T", " ") ?? "-"}</TableCell>
                        <TableCell>{ row.status }</TableCell>
                        <TableCell   
                            sx={{
                            whiteSpace: "normal",
                            wordBreak: "break-word",
                            maxWidth: full ? "100%" : 300,
                        }}>
                            {row.errorMessage ?? "—"}
                        </TableCell>
                    </TableRow>
                ))}
            </TableBody>
        </Table>
    )
}