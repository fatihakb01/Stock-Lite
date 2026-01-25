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

import { Stack, Typography } from "@mui/material";
import { useEffect, useState } from "react";
import { useJobTask } from "../../../hooks/useJobTasks";
import Section from "../../../components/general/Section";
import AddScheduledJobButton from "../buttons/AddScheduledJobButton";
import { useReorderScheduledJobs } from "../../../hooks/useScheduledJobs";
import { SortableScheduledJobItem } from "../item/SortableScheduledJobItem";

export default function ScheduledJobList({ selected, onSelect }: any) {
  const jobTaskId = selected?.type === "jobtask" ? selected.id : null;
  const { data } = useJobTask(jobTaskId);
  const reorderMutation = useReorderScheduledJobs();

  const [jobs, setJobs] = useState<any[]>([]);

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

  return (
    <Section title="Scheduled jobs">
      <AddScheduledJobButton />

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
                // executionOrder={job.executionOrder}
                onClick={() => onSelect(job.id)}
              />
            ))}
          </Stack>
        </SortableContext>
      </DndContext>
    </Section>
  );
}
