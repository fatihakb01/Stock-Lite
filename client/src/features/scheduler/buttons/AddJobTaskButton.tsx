import { useState } from "react";
import {
  Button,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Stack,
  TextField
} from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useCreateJobTask } from "../../../hooks/useJobTasks";
import { createJobTaskSchema, type CreateJobTaskFormValues } from "../schemas/jobTaskSchema";

export default function AddJobTaskButton() {
  const [open, setOpen] = useState(false);
  const createJobTask = useCreateJobTask();

  const form = useForm<CreateJobTaskFormValues>({
    resolver: zodResolver(createJobTaskSchema),
    defaultValues: {
      name: "",
      description: ""
    }
  });

  const handleSubmit = async (values: CreateJobTaskFormValues) => {
    await createJobTask.mutateAsync(values);
    form.reset();
    setOpen(false);
  };

  return (
    <>
      <Button variant="contained" size="small" onClick={() => setOpen(true)}>
        Add job task
      </Button>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <DialogTitle>Add job task</DialogTitle>

        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Name"
              {...form.register("name")}
              error={!!form.formState.errors.name}
              helperText={form.formState.errors.name?.message}
            />

            <TextField
              label="Description"
              multiline
              rows={3}
              {...form.register("description")}
            />
          </Stack>
        </DialogContent>

        <DialogActions>
          <Button onClick={() => setOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            onClick={form.handleSubmit(handleSubmit)}
            disabled={createJobTask.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
