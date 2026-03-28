import { useState } from "react";

export type SchedulerSelection =
  | { type: "jobtask"; jobTaskId: string }
  | { type: "scheduledjob"; jobTaskId: string; scheduledJobId: string }
  | null;

export function useSchedulerSelection(defaultJobTaskId?: string) {
  const [selection, setSelection] = useState<SchedulerSelection>(
      defaultJobTaskId ? { type: "jobtask", jobTaskId: defaultJobTaskId } : null
  );

  return {
    selection,

    selectJobTask: (jobTaskId: string) =>
      setSelection({ type: "jobtask", jobTaskId }),

    selectScheduledJob: (jobTaskId: string, scheduledJobId: string) =>
      setSelection({
        type: "scheduledjob",
        jobTaskId,
        scheduledJobId,
      }),

    clearSelection: () => setSelection(null),
  };
}