import {
  DndContext,
  closestCenter,
  PointerSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core";
import {
  SortableContext,
  verticalListSortingStrategy,
  arrayMove,
} from "@dnd-kit/sortable";

import { Box, Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { useJobTask } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import AddScheduledJobButton from "../buttons/AddScheduledJobButton";
import { useReorderScheduledJobs } from "../../../hooks/useScheduledJobs";
import { SortableScheduledJobItem } from "../item/SortableScheduledJobItem";
import type { ScheduledJob } from "../../../types/scheduledJobs/scheduledJobTypes";
import type { SchedulerSelection } from "../selector/useSchedulerSelection";
import type { CurrentUser } from "../../../types/users/userTypes";

type Props = {
  selected: SchedulerSelection
  onSelect: (jobTaskId: string, scheduledJobId: string) => void
  user: CurrentUser
}

export default function ScheduledJobList({ selected, onSelect, user } : Props) {
  const jobTaskId =
  selected?.type === "jobtask"
    ? selected.jobTaskId
    : selected?.type === "scheduledjob"
    ? selected.jobTaskId
    : null;

  const { data, isLoading, isError } = useJobTask(jobTaskId);
  const reorderMutation = useReorderScheduledJobs();
  const [jobs, setJobs] = useState<ScheduledJob[]>([]);

  useEffect(() => {
    if (data?.jobs) {
      setJobs(data.jobs);
    }
  }, [data]);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: { distance: 5 },
    })
  );

  if (!jobTaskId) {
    return <Typography>Select a job task</Typography>;
  }

  const handleDragEnd = (event: any) => {
    const { active, over } = event;

    if (!over || active.id === over.id) return;

    setJobs((prev) => {
      const oldIndex = prev.findIndex(j => j.id === active.id);
      const newIndex = prev.findIndex(j => j.id === over.id);

      const reordered = arrayMove(prev, oldIndex, newIndex);

      reorderMutation.mutate({
        orderedScheduledJobIds: reordered.map(j => j.id),
      });

      return reordered;
    });
  };

  if (isLoading || !data) {
    return <Typography>Loading jobs...</Typography>
  }

  if (isError || !data) {
    return <Typography>No jobs found.</Typography>
  }
  
  const jobTaskKey = data && data.jobTaskKey;

  return (
    <Section title="Scheduled jobs">
      {user && user.role === "SuperAdmin" && (
        <AddScheduledJobButton jobTaskKey={jobTaskKey}/>
      )}

      <Box mt={2}>
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={jobs.map(j => j.id)}
            strategy={verticalListSortingStrategy}
          >
            <Stack spacing={1}>
              {jobs.map(job => (
                <SortableScheduledJobItem
                  key={job.id}
                  id={job.id}
                  name={job.name}
                  result={job.result}
                  archived={job.isArchived}
                  onClick={() => onSelect(jobTaskId, job.id)}
                />
              ))}
            </Stack>
          </SortableContext>
        </DndContext>
      </Box>
    </Section>
  );
}
