import { Box } from "@mui/material";

interface TabPanelProps {
  value: number;
  index: number;
  children: React.ReactNode;
}

export default function TabPanel({
  value,
  index,
  children,
}: TabPanelProps) {
  if (value !== index) return null;

  return (
    <Box sx={{ pt: 2 }}>
      {children}
    </Box>
  );
}
