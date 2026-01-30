import { Stack, List, ListItemButton, Typography } from "@mui/material";
import { useJobTasks } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import AddJobTaskButton from "../buttons/AddJobTaskButton";
import type { User } from "../../../types/users/userTypes";

type Props = {
  selected: SchedulerSelection
  onSelect: (jobTaskId: string) => void
  user: User
}

export default function JobTaskList({ selected, onSelect, user } : Props) {
  const { data, isLoading } = useJobTasks();

  if (isLoading) 
    return <Typography>Jobtasks loading...</Typography>

  return (
    <Section title="Job tasks">
      <Stack spacing={1}>
        {user && user.role === "SuperAdmin" && (
          <AddJobTaskButton />
        )}

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
