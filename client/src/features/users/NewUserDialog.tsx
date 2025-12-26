import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  CircularProgress
} from "@mui/material";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";

import { userSchema, type UserFormValues } from "./schemas/userSchema";
import { roleMap } from "./roleMap";
import { useRegisterUser, useCurrentUser } from "../../hooks/useUsers";
import FormError from "../../components/forms/FormError";
import FormSuccess from "../../components/forms/FormSuccess";
import { useState } from "react";

export default function NewUserDialog({
  open,
  onClose,
  onUserCreated
}: {
  open: boolean;
  onClose: () => void;
  onUserCreated: (id: string) => void;
}) {
  const { data: currentUser } = useCurrentUser();
  const registerMutation = useRegisterUser();

  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const {
    handleSubmit,
    register,
    reset,
    formState: { errors, isSubmitting }
  } = useForm<UserFormValues>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      userName: "",
      email: "",
      phoneNumber: "",
      role: "", 
    }
  });

  const onSubmit = async (values: UserFormValues) => {
    if (!currentUser) return;

    setErrorMessage(null);
    setSuccessMessage(null);

    const payload = {
      ...values,
      companyId: currentUser.companyId,
      password: "Temp12345!", 
    };

    registerMutation.mutate(payload, {
      onSuccess: (createdUser) => {
        handleClose();
        onUserCreated(createdUser.id);
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

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} fullWidth maxWidth="sm">
      <DialogTitle>Create New User</DialogTitle>

      <DialogContent dividers>
        <FormError message={errorMessage} />
        <FormSuccess message={successMessage} />
        <form id="new-user-form" onSubmit={handleSubmit(onSubmit)}>
          <TextField
            label="Username"
            fullWidth
            margin="normal"
            {...register("userName")}
            error={!!errors.userName}
            helperText={errors.userName?.message}
          />

          <TextField
            label="Email"
            fullWidth
            margin="normal"
            {...register("email")}
            error={!!errors.email}
            helperText={errors.email?.message}
          />

          <TextField
            label="First Name"
            fullWidth
            margin="normal"
            {...register("firstName")}
            error={!!errors.firstName}
            helperText={errors.firstName?.message}
          />

          <TextField
            label="Last Name"
            fullWidth
            margin="normal"
            {...register("lastName")}
            error={!!errors.lastName}
            helperText={errors.lastName?.message}
          />

          <TextField
            label="Phone Number"
            fullWidth
            margin="normal"
            {...register("phoneNumber")}
            error={!!errors.phoneNumber}
            helperText={errors.phoneNumber?.message}
          />

          <TextField
            select
            label="Role"
            fullWidth
            margin="normal"
            {...register("role")}
            error={!!errors.role}
            helperText={errors.role?.message}
          >
            {Object.entries(roleMap).map(([key,value]) => (
              <MenuItem key={key} value={value}>
                {value}
              </MenuItem>
            ))}
          </TextField>
        </form>
      </DialogContent>

      <DialogActions>
        <Button onClick={handleClose}>Cancel</Button>

        <Button
          variant="contained"
          type="submit"
          form="new-user-form"
          disabled={isSubmitting || registerMutation.isPending}
        >
          {registerMutation.isPending ? (
            <CircularProgress size={20} />
          ) : (
            "Create"
          )}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
