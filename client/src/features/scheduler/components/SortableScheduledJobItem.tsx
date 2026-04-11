import { CSS } from "@dnd-kit/utilities";
import { useSortable } from "@dnd-kit/sortable";
import { Box, Typography } from "@mui/material";
import SchedulerStatusBadge from "./SchedulerStatusBadge";

type Props = {
  id: string
  name: string
  result: string
  archived: boolean
  onClick: () => void
};

export function SortableScheduledJobItem({
  id,
  name,
  result,
  archived,
  onClick,
}: Props) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
  } = useSortable({ id });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  return (
    <Box
      ref={setNodeRef}
      style={style}
      {...attributes}
      {...listeners}
      onClick={onClick}
      sx={{
        p: 1.5,
        border: "1px solid",
        borderColor: "divider",
        borderRadius: 1,
        cursor: "grab",
        bgcolor: archived ? "background.transparent" : "background.paper",
      }}
    >
      <Typography fontWeight={500}>{name}</Typography>
      <Typography>{archived ?  "Archived job" : "Active job"}</Typography>
      <SchedulerStatusBadge status={result} label={"Job"} />
    </Box>
  );
}
