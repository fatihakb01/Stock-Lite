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
import { useCreateScheduledJob } from "../../../hooks/useScheduledJobs";
import { createScheduledJobSchema, type CreateScheduledJobFormValues } from "../schemas/scheduledJobSchema";

export default function AddScheduledJobButton() {
  const [open, setOpen] = useState(false);
  const createScheduledJob = useCreateScheduledJob();

  const form = useForm<CreateScheduledJobFormValues>({
    resolver: zodResolver(createScheduledJobSchema),
    defaultValues: {
      name: "",
      executionOrder: 1,
      description: ""
    }
  });

  const handleSubmit = async (values: CreateScheduledJobFormValues) => {
    await createScheduledJob.mutateAsync(values);
    form.reset();
    setOpen(false);
  };

  return (
    <>
      <Button variant="contained" size="small" onClick={() => setOpen(true)}>
        Add scheduled job
      </Button>

      <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
        <DialogTitle>Add scheduled job</DialogTitle>

        <DialogContent>
          <Stack spacing={2} mt={1}>
            <TextField
              label="Name"
              {...form.register("name")}
              error={!!form.formState.errors.name}
              helperText={form.formState.errors.name?.message}
            />

            <TextField
              label="Execution order"
              type="number"
              {...form.register("executionOrder", { valueAsNumber: true })}
              error={!!form.formState.errors.executionOrder}
              helperText={form.formState.errors.executionOrder?.message}
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
            disabled={createScheduledJob.isPending}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
