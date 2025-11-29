import { useSearchParams } from "react-router-dom";
import { useConfirmEmail, useResetPassword } from "../../hooks/useUsers";
import { useState } from "react";
import {
  Container,
  Box,
  Paper,
  Typography,
  Button,
  CircularProgress
} from "@mui/material";
import FormError from "../../components/forms/FormError";
import FormSuccess from "../../components/forms/FormSuccess";

export default function ConfirmEmailPage() {
  const [searchParams] = useSearchParams();

  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const confirmMutation = useConfirmEmail();
  const resetPasswordMutation = useResetPassword();

  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  if (!userId || !token) {
    return (
      <Container maxWidth="sm">
        <FormError message="Invalid confirmation link." />
      </Container>
    );
  }

  const handleConfirm = async () => {
    setMessage(null);
    setError(null);

    // Confirm email
    confirmMutation.mutate(
      { userId, token },
      {
        onSuccess: (data) => {
          // Check if email has already been confirmed
          if (data.alreadyConfirmed) {
            setError("Your email has already been confirmed.");
            return;
          }

          // Trigger password reset email
          const email = data.email;

          resetPasswordMutation.mutate(
            { email:  email}, 
            {
              onSuccess: () => {
                setMessage(
                  "Your email has been confirmed! A password setup email has been sent."
                );
              },
              onError: (err) => {
                setError(`Email confirmed, but failed to send password email: ${err}`);
              }
            }
          );
        },
        onError: () => {
          setError("Failed to confirm email. The link may be expired.");
        }
      }
    );
  };

  return (
    <Container maxWidth="sm">
      <Box mt={8}>
        <Paper sx={{ p: 4, textAlign: "center" }}>
          <Typography variant="h5" gutterBottom>
            Confirm Your Email
          </Typography>

          <Typography variant="body1" sx={{ mb: 3 }}>
            Click the button below to confirm your email.
          </Typography>

          {error && (
            <FormError message={error} />
          )}

          {message && (
            <FormSuccess message={message} />
          )}

          <Button
            variant="contained"
            onClick={handleConfirm}
            disabled={confirmMutation.isPending || resetPasswordMutation.isPending}
            startIcon={
              confirmMutation.isPending || resetPasswordMutation.isPending ? (
                <CircularProgress size={20} />
              ) : undefined
            }
          >
            {confirmMutation.isPending || resetPasswordMutation.isPending
              ? "Processing..."
              : "Confirm Email"}
          </Button>
        </Paper>
      </Box>
    </Container>
  );
}
