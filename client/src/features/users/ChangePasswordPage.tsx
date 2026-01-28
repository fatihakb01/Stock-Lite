import { useSearchParams, useNavigate } from "react-router-dom";
import {
  Box,
  Button,
  TextField,
  Typography,
  Paper,
  CircularProgress
} from "@mui/material";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useChangePassword } from "../../hooks/useUsers";
import { changePasswordSchema, type ChangePasswordFormValues } from "./schemas/changePasswordSchema";
import { useState } from "react";
import FormError from "../../components/forms/FormError";
import FormSuccess from "../../components/forms/FormSuccess";

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Extract URL params
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const changePasswordMutation = useChangePassword();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      newPassword: "",
      confirmPassword: ""
    }
  });

  // Basic URL validation
  if (!userId || !token) {
    return (
      <Box p={4} display="flex" justifyContent="center" alignItems="center">
        <FormError message="Invalid password reset link." />
      </Box>
    );
  }

  const onSubmit = (values: ChangePasswordFormValues) => {
    debugger;
    setErrorMessage(null);
    setSuccessMessage(null);

    changePasswordMutation.mutate(
      {
        userId,
        token,
        newPassword: values.newPassword
      },
      {
        onSuccess: () => {
          setSuccessMessage("Password changed successfully. You will be redirected to the login page...");
  
          // Redirect after 2.5 seconds
          setTimeout(() => {
            navigate("/login?passwordReset=success");
          }, 2500);
        }, 
        onError: (err: any) => {
          if (Array.isArray(err)) {
            setErrorMessage(err.join(", "));
          } else {
            setErrorMessage(
              err?.response?.data?.message ?? "Failed to change password."
            );
          }
        }
      }
    );
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="70vh"
      p={2}
    >
      <Paper elevation={3} sx={{ padding: 4, width: "100%", maxWidth: 420 }}>
        <Typography variant="h5" mb={2} fontWeight="bold">
          Reset Your Password
        </Typography>

        <Typography variant="body2" mb={3}>
          Choose a new password for your account.
        </Typography>

        <form onSubmit={handleSubmit(onSubmit)}>
          <TextField
            type="password"
            label="New Password"
            fullWidth
            margin="normal"
            {...register("newPassword")}
            error={!!errors.newPassword}
            helperText={errors.newPassword?.message}
          />

          <TextField
            type="password"
            label="Confirm Password"
            fullWidth
            margin="normal"
            {...register("confirmPassword")}
            error={!!errors.confirmPassword}
            helperText={errors.confirmPassword?.message}
          />

          {changePasswordMutation.isError && (
            <FormError message={errorMessage} />
          )}

          {changePasswordMutation.isSuccess && (
            <FormSuccess message={successMessage} />
          )}

          <Button
            type="submit"
            variant="contained"
            fullWidth
            sx={{ mt: 3 }}
            disabled={isSubmitting || changePasswordMutation.isPending}
          >
            {changePasswordMutation.isPending ? (
              <CircularProgress size={20} />
            ) : (
              "Reset Password"
            )}
          </Button>
        </form>
      </Paper>
    </Box>
  );
}
