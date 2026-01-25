import { CSS } from "@dnd-kit/utilities";
import { useSortable } from "@dnd-kit/sortable";
import { Box, Typography } from "@mui/material";

type Props = {
  id: string;
  name: string;
  // executionOrder: number;
  onClick: () => void;
};

export function SortableScheduledJobItem({
  id,
  name,
  // executionOrder,
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
        bgcolor: "background.paper",
      }}
    >
      <Typography fontWeight={500}>{name}</Typography>
      {/* <Typography variant="caption">
        Order: {executionOrder}
      </Typography> */}
    </Box>
  );
}
