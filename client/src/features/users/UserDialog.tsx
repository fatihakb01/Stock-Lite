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

import { userSchema, type UserFormValues } from "./schemas/userSchema";
import { roleMap } from "./roleMap";
import { useEffect } from "react";

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

  const isSuperAdmin = currentUser && roleMap[Number(currentUser.role)] === "SuperAdmin";

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
        role: user.role, 
      });
    }
  }, [user]);

  const onSubmit = (values: UserFormValues) => {
    if (!isSuperAdmin) return;

    console.log("Sending update:", values);
    // call mutation here
  };

  return (
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
              disabled={!isSuperAdmin}
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

      <DialogActions>
        <Button onClick={onClose}>Close</Button>
        {isSuperAdmin && (
          <Button type="submit" form="user-form" variant="contained">
            Save
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}
