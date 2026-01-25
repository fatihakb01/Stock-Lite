import { Typography} from "@mui/material";
import { useRecentHistory } from "../../../hooks/useJobTasks";
import ExecutionTable from "../tables/ExecutionTable";

export default function RecentHistoryTab() {
  const { data, isLoading } = useRecentHistory();

  if (isLoading) {
    return <Typography>Loading execution history…</Typography>;
  }

  if (!data?.length) {
    return <Typography>No executions found.</Typography>;
  }

  return (
    (data && data.length > 0 ?
      (
        <ExecutionTable rows={data}/> 
      ) : (
        <Typography>No history available</Typography>
      )
    )
  );
}
