import { Alert } from "@mui/material";

export default function FormSuccess({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <Alert severity="success" sx={{ mb: 2 }}>
      {message}
    </Alert>
  );
}
