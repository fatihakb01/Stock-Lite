import {
  Box,
  Paper,
  Typography,
  TextField,
  Button,
  CircularProgress
} from "@mui/material";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  resetPasswordSchema,
  type ResetPasswordFormValues
} from "./schemas/resetPasswordSchema";

import { useResetPassword } from "../../hooks/useUsers";
import { useState } from "react";
import FormInfo from "../../components/forms/FormInfo";
import FormError from "../../components/forms/FormError";

export default function ForgotPasswordPage() {
  const resetPasswordMutation = useResetPassword();

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: { email: "" }
  });

  const onSubmit = (values: ResetPasswordFormValues) => {
    resetPasswordMutation.mutate(values, {
      onSuccess: () => {
        setSuccess(true);
      },
      onError: (err: any) => {
        console.error("Failed to create user:", err);
        if (Array.isArray(err)) {
          // Validation errors from backend (400)
          setErrorMessage(err.join(", "));
        } else {
          setErrorMessage(
            err?.response?.data?.message ?? "Failed to create user."
          );
        }
      }
    });
  };


  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="70vh"
      p={2}
    >
      <Paper elevation={3} sx={{ p: 4, width: "100%", maxWidth: 420 }}>
        <Typography variant="h5" mb={2} fontWeight="bold">
          Reset Password
        </Typography>

        <Typography variant="body2" mb={3}>
          Enter your email address and we will send you instructions to reset your password.
        </Typography>

        {/* Success Message */}
        {success && (
          <FormInfo message="If an account with that email exists, you will receive an email with password reset instructions." />
        )}

        {/* Error Message */}
        {resetPasswordMutation.isError && (
          <FormError message={errorMessage} />
        )}

        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            label="Email Address"
            fullWidth
            margin="normal"
            {...register("email")}
            error={!!errors.email}
            helperText={errors.email?.message}
          />

          <Button
            type="submit"
            variant="contained"
            fullWidth
            sx={{ mt: 3 }}
            disabled={isSubmitting || resetPasswordMutation.isPending}
          >
            {resetPasswordMutation.isPending ? (
              <CircularProgress size={20} />
            ) : (
              "Send Reset Instructions"
            )}
          </Button>
        </form>
      </Paper>
    </Box>
  );
}
