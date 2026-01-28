import { useEffect } from "react";
import {
  Stack,
  TextField,
  MenuItem,
  Checkbox,
  FormControlLabel
} from "@mui/material";
import { type UseFormReturn } from "react-hook-form";
import { jobTypeMap } from "../../../types/scheduledJobs/jobTypeMap";
import { calculationTypeMap } from "../../../types/scheduledJobs/calculationTypeMap";
import { importDataTypeMap } from "../../../types/scheduledJobs/importDataTypeMap";
import { periodTypeMap } from "../../../types/scheduledJobs/periodTypeMap";
import { type ScheduledJobFormValues } from "../schemas/scheduledJobSchema";

type Props = {
  mode: "create" | "edit";
  form: UseFormReturn<ScheduledJobFormValues>;
};

export default function ScheduledJobForm({
  mode,
  form
}: Props) {

  const jobType = form.watch("jobType");

  // Clear irrelevant fields when jobType changes
  useEffect(() => {
    if (!jobType) return;

    switch (jobType) {
      case "Import":
        form.setValue("calculationType", undefined);
        form.setValue("periodType", undefined);
        form.setValue("target", undefined);
        break;

      case "StandardCalculation":
        form.setValue("importDataType", undefined);
        form.setValue("target", undefined);
        break;

      case "CustomCalculation":
        form.setValue("importDataType", undefined);
        form.setValue("calculationType", undefined);
        form.setValue("periodType", undefined);
        break;
    }
  }, [jobType, form]);

  return (
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

      <TextField
        select
        label="Job Type"
        {...form.register("jobType")}
        error={!!form.formState.errors.jobType}
        helperText={form.formState.errors.jobType?.message}
        defaultValue={form.getValues().jobType}
      >
        {Object.values(jobTypeMap).map(value => (
          <MenuItem key={value} value={value}>
            {value}
          </MenuItem>
        ))}
      </TextField>

      {jobType === "Import" && (
        <>
          <TextField
            select
            label="Import Data Type"
            {...form.register("importDataType")}
            error={!!form.formState.errors.importDataType}
            helperText={form.formState.errors.importDataType?.message}
            defaultValue={form.getValues().importDataType}
          >
            {Object.values(importDataTypeMap).map(value => (
              <MenuItem key={value} value={value}>
                {value}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            label="Source"
            {...form.register("source")}
          />
        </>
      )}

      {jobType === "StandardCalculation" && (
        <>
          <TextField
            select
            label="Calculation Type"
            {...form.register("calculationType")}
            error={!!form.formState.errors.calculationType}
            helperText={form.formState.errors.calculationType?.message}
            defaultValue={form.getValues().calculationType}
          >
            {Object.values(calculationTypeMap).map(value => (
              <MenuItem key={value} value={value}>
                {value}
              </MenuItem>
            ))}
          </TextField>

          <TextField
            select
            label="Period Type"
            {...form.register("periodType")}
            error={!!form.formState.errors.periodType}
            helperText={form.formState.errors.periodType?.message}
            defaultValue={form.getValues().periodType}
          >
            {Object.values(periodTypeMap).map(value => (
              <MenuItem key={value} value={value}>
                {value}
              </MenuItem>
            ))}
          </TextField>
        </>
      )}

      {jobType === "CustomCalculation" && (
        <TextField
          label="Target"
          {...form.register("target")}
          error={!!form.formState.errors.target}
          helperText={form.formState.errors.target?.message}
          defaultValue={form.getValues().target}
        />
      )}

      {(mode === 'edit') && (
        <FormControlLabel
          control={<Checkbox {...form.register("isArchived")} />}
          label="Archived"
        />
      )}
    </Stack>
  );
}
