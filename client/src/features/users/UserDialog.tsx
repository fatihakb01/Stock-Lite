import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  CircularProgress,
  Box
} from "@mui/material";

import { useUser, useCurrentUser } from "../../hooks/useUsers";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useEditUser, useDeleteUser } from "../../hooks/useUsers";
import { userSchema, type UserFormValues } from "./schemas/userSchema";
import { roleMap } from "../../types/users/roleMap";
import { useEffect, useState } from "react";
import FormError from "../../components/forms/FormError";
import FormSuccess from "../../components/forms/FormSuccess";

export default function UserDialog({
  open,
  userId,
  onClose
}: {
  open: boolean;
  userId: string | null;
  onClose: () => void;
}) {
  const { data: currentUser } = useCurrentUser();
  const { data: user, isLoading } = useUser(userId);
  const editUserMutation = useEditUser(userId);
  const deleteUserMutation = useDeleteUser(userId);

  const [confirmDeleteOpen, setConfirmDeleteOpen] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  const isSuperAdmin = currentUser && roleMap[Number(currentUser.role)] === "SuperAdmin";
  const sameUserSelected = user?.id === currentUser?.id;

  const { handleSubmit, reset, register, formState: { errors }} = useForm<UserFormValues>({
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

  // Load values into form once user data arrives
  useEffect(() => {
    if (user) {
      reset({
        firstName: user.firstName ?? "",
        lastName: user.lastName ?? "",
        userName: user.userName,
        email: user.email,
        phoneNumber: user.phoneNumber ?? "",
        role: roleMap[Number(user.role)],
      });
    }
  }, [user]);

  // Reset messages after re-opening a form
  useEffect(() => {
    if (open) {
      setErrorMessage(null);
      setSuccessMessage(null);
    }
  }, [open, userId]);

  const onSubmit = (values: UserFormValues) => {
    if (!isSuperAdmin || !userId) return;

    setErrorMessage(null);
    setSuccessMessage(null);

    editUserMutation.mutate(values, {
      onSuccess: () => {
        setSuccessMessage("User updated successfully.");
      },
      onError: (err: any) => {
        if (Array.isArray(err)) {
          setErrorMessage(err.join(", "));
        } else {
          setErrorMessage(
            err?.response?.data?.message ?? "Failed to update user."
          );
        }
      }
    });
  };

  const handleDelete = () => {
    if (!userId) return;

    setErrorMessage(null);
    setSuccessMessage(null);

    deleteUserMutation.mutate(undefined, {
      onSuccess: () => {
        setSuccessMessage("User deleted successfully.");
        setConfirmDeleteOpen(false);
        onClose();
      },
      onError: (err: any) => {
        console.error("Delete failed:", err);
        setErrorMessage(
          err?.response?.data?.message ?? "Failed to delete user."
        );
      }
    });
  };

  return (
    <>
      {/* MAIN USER DIALOG */}
      <Dialog open={open} onClose={onClose} fullWidth>
        <DialogTitle>User Info</DialogTitle>
        <DialogContent dividers>
          {isLoading || !user ? (
            <Box display="flex" justifyContent="center" py={4}>
              <CircularProgress />
            </Box>
          ) : (
            <>
            <FormError message={errorMessage} />
            <FormSuccess message={successMessage} />
            
            <form id="user-form" onSubmit={handleSubmit(onSubmit)}>
              <TextField
                label="First Name"
                fullWidth
                margin="normal"
                {...register("firstName")}
                error={!!errors.firstName}
                helperText={errors.firstName?.message}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Last Name"
                fullWidth
                margin="normal"
                {...register("lastName")}
                error={!!errors.lastName}
                helperText={errors.lastName?.message}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="UserName"
                fullWidth
                margin="normal"
                {...register("userName")}
                error={!!errors.userName}
                helperText={errors.userName?.message}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Email"
                fullWidth
                margin="normal"
                {...register("email")}
                error={!!errors.email}
                helperText={errors.email?.message}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Phone Number"
                fullWidth
                margin="normal"
                {...register("phoneNumber")}
                error={!!errors.phoneNumber}
                helperText={errors.phoneNumber?.message}
                disabled={!isSuperAdmin}
              />

              <TextField
                select
                label="Role"
                fullWidth
                margin="normal"
                {...register("role")}
                error={!!errors.role}
                helperText={errors.role?.message}
                defaultValue={roleMap[Number(user.role)]}
                disabled={!isSuperAdmin || sameUserSelected}
              >
                {Object.entries(roleMap).map(([label, value]) => (
                  <MenuItem key={label} value={value}>
                    {value}
                  </MenuItem>
                ))}
              </TextField>
            </form>
            </>
          )}
        </DialogContent>

        <DialogActions sx={{ justifyContent: "space-between" }}>
          {/* DELETE LEFT */}
          {(isSuperAdmin && !sameUserSelected) &&  (
            <Button color="error" onClick={() => setConfirmDeleteOpen(true)}>
              Delete
            </Button>
          )}

          <Button onClick={onClose}>Close</Button>

          {isSuperAdmin && (
            <Button type="submit" form="user-form" variant="contained">
              Save
            </Button>
          )}
        </DialogActions>
      </Dialog>

      {/* CONFIRM DELETE DIALOG */}
      <Dialog
        open={confirmDeleteOpen}
        onClose={() => setConfirmDeleteOpen(false)}
      >
        <DialogTitle>Confirm Delete</DialogTitle>
        <DialogContent>
          Are you sure you want to delete this user?
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setConfirmDeleteOpen(false)}>
            Cancel
          </Button>
          <Button color="error" onClick={handleDelete} variant="contained">
            Yes, delete
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}