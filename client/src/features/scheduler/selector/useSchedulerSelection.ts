import { useState } from "react";

export type SchedulerSelection =
  | { type: "jobtask"; id: string }
  | { type: "scheduledjob"; id: string }
  | null;

export function useSchedulerSelection() {
  const [selection, setSelection] = useState<SchedulerSelection>(null);

  return {
    selection,
    selectJobTask: (id: string) =>
      setSelection({ type: "jobtask", id }),
    selectScheduledJob: (id: string) =>
      setSelection({ type: "scheduledjob", id }),
    clearSelection: () => setSelection(null),
  };
}
