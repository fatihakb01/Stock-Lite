import { Box } from "@mui/material";
import { useRef } from "react";

interface DrawerResizeHandleProps {
  onResize: (newWidthPct: number) => void;
  minWidthPct?: number;
  maxWidthPct?: number;
}

export default function DrawerResizeHandle({
  onResize,
  minWidthPct = 40,
  maxWidthPct = 80,
}: DrawerResizeHandleProps) {
  const draggingRef = useRef(false);

  const onMouseDown = () => {
    draggingRef.current = true;

    const onMouseMove = (e: MouseEvent) => {
      if (!draggingRef.current) return;

      const viewportWidth = window.innerWidth;
      const newWidthPct =
        ((viewportWidth - e.clientX) / viewportWidth) * 100;

      onResize(
        Math.min(maxWidthPct, Math.max(minWidthPct, newWidthPct))
      );
    };

    const onMouseUp = () => {
      draggingRef.current = false;
      window.removeEventListener("mousemove", onMouseMove);
      window.removeEventListener("mouseup", onMouseUp);
    };

    window.addEventListener("mousemove", onMouseMove);
    window.addEventListener("mouseup", onMouseUp);
  };

  return (
    <Box
      onMouseDown={onMouseDown}
      sx={{
        position: "absolute",
        left: 0,
        top: 0,
        width: 6,
        height: "100%",
        cursor: "col-resize",
        zIndex: 1300,
        "&:hover": {
          backgroundColor: "rgba(0,0,0,0.08)",
        },
      }}
    />
  );
}
