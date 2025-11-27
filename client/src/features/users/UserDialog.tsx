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
import { roleMap } from "./roleMap";
import { useEffect, useState } from "react";

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

  const isSuperAdmin = currentUser && roleMap[Number(currentUser.role)] === "SuperAdmin";
  const sameUserSelected = user?.id === currentUser?.id;

  const { handleSubmit, reset, register } = useForm<UserFormValues>({
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

  const onSubmit = (values: UserFormValues) => {
    if (!isSuperAdmin || !userId) return;

    editUserMutation.mutate(values, {
      onSuccess: () => {
        console.log("User updated");
        onClose();
      },
      onError: (err) => {
        console.error("Failed to update user:", err);
      }
    });
  };

  const handleDelete = () => {
    if (!userId) return;

    deleteUserMutation.mutate(undefined, {
      onSuccess: () => {
        console.log("User deleted");
        setConfirmDeleteOpen(false);
        onClose();
      },
      onError: (err) => console.error("Delete failed", err),
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
            <form id="user-form" onSubmit={handleSubmit(onSubmit)}>
              <TextField
                label="First Name"
                fullWidth
                margin="normal"
                {...register("firstName")}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Last Name"
                fullWidth
                margin="normal"
                {...register("lastName")}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="UserName"
                fullWidth
                margin="normal"
                {...register("userName")}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Email"
                fullWidth
                margin="normal"
                {...register("email")}
                disabled={!isSuperAdmin}
              />

              <TextField
                label="Phone Number"
                fullWidth
                margin="normal"
                {...register("phoneNumber")}
                disabled={!isSuperAdmin}
              />

              <TextField
                select
                label="Role"
                fullWidth
                margin="normal"
                {...register("role")}
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