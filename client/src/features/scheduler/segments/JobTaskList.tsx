import { Stack, List, ListItemButton, Typography } from "@mui/material";
import { useJobTasks } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import AddJobTaskButton from "../buttons/AddJobTaskButton";

export default function JobTaskList({
  selected,
  onSelect
}: {
  selected: SchedulerSelection
  onSelect: (jobTaskId: string) => void
}) {
  const { data, isLoading } = useJobTasks();

  if (isLoading) 
    return <Typography>Jobtasks loading...</Typography>

  return (
    <Section title="Job tasks">
      <Stack spacing={1}>
        <AddJobTaskButton />

        <List dense>
          {data?.map(task => (
            <ListItemButton
              key={task.id}
              selected={selected?.type === "jobtask" && selected.jobTaskId === task.id}
              onClick={() => onSelect(task.id)}
            >
              <Typography>{task.name}</Typography>
            </ListItemButton>
          ))}
        </List>
      </Stack>
    </Section>
  );
}
