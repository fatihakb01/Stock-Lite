import { Alert } from "@mui/material";

export default function FormInfo({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <Alert severity="info" sx={{ mb: 2 }}>
      {message}
    </Alert>
  );
}
