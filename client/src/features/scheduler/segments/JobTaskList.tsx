import { Stack, List, ListItemButton, Typography, Box } from "@mui/material";
import { useJobTasks } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import AddJobTaskButton from "../buttons/AddJobTaskButton";
import type { CurrentUser } from "../../../types/users/userTypes";
import SchedulerStatusBadge from "../chips/SchedulerStatusBadge";

type Props = {
  selected: SchedulerSelection
  onSelect: (jobTaskId: string) => void
  user: CurrentUser
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
              <Box sx={{ display: "flex", alignItems: "center", gap: 1, width: "100%" }}>
                  <Typography>{task.name}</Typography>
                  <SchedulerStatusBadge status={task.result} label={"Task"} />
              </Box>
            </ListItemButton>
          ))}
        </List>
      </Stack>
    </Section>
  );
}
