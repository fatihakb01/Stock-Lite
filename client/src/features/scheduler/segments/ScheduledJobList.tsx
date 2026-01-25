import { Stack, Box, Typography } from "@mui/material";
import { useJobTask } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import AddScheduledJobButton from "../buttons/AddScheduledJobButton";


export default function ScheduledJobList({
  selected,
  onSelect,
}: {
  selected: SchedulerSelection;
  onSelect: (id: string) => void;
}) {
  const jobTaskId = selected?.type === "jobtask" ? selected.id : null;
  const { data } = useJobTask(jobTaskId);

  if (!jobTaskId) {
    return <Typography>Select a job task</Typography>;
  }

  return (
    <Section title="Scheduled jobs">
      <Stack spacing={1}>
        <AddScheduledJobButton />

        {data?.jobs.map(job => (
          <Box
            key={job.id}
            sx={{
              p: 1.5,
              border: "1px solid",
              borderColor: "divider",
              borderRadius: 1,
              cursor: "pointer",
            }}
            onClick={() => onSelect(job.id)}
          >
            <Typography fontWeight={500}>{job.name}</Typography>
            <Typography variant="caption">
              Order: {job.executionOrder}
            </Typography>
          </Box>
        ))}
      </Stack>
    </Section>
  );
}
