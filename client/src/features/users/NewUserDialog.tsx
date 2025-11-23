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

export default function NewUserDialog({
  open,
  onClose
}: {
  open: boolean;
  onClose: () => void;
}) {
  const { data: currentUser } = useCurrentUser();
  const registerMutation = useRegisterUser();

  const {
    handleSubmit,
    register,
    reset,
    formState: { isSubmitting }
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

    const payload = {
      ...values,
      companyId: currentUser.companyId,
      password: "Temp12345!", 
    };

    console.log(payload.role);

    registerMutation.mutate(payload, {
      onSuccess: () => {
        reset();
        onClose();
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
        <form id="new-user-form" onSubmit={handleSubmit(onSubmit)}>
          <TextField
            label="Username"
            fullWidth
            margin="normal"
            {...register("userName")}
          />

          <TextField
            label="Email"
            fullWidth
            margin="normal"
            {...register("email")}
          />

          <TextField
            label="First Name"
            fullWidth
            margin="normal"
            {...register("firstName")}
          />

          <TextField
            label="Last Name"
            fullWidth
            margin="normal"
            {...register("lastName")}
          />

          <TextField
            label="Phone Number"
            fullWidth
            margin="normal"
            {...register("phoneNumber")}
          />

          {/* <TextField
            label="Role"
            fullWidth
            margin="normal"
            {...register("role", { valueAsNumber: true })}
          /> */}


          <TextField
            select
            label="Role"
            fullWidth
            margin="normal"
            {...register("role")}
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
