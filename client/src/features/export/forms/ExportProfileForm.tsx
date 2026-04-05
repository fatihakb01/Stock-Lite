import { Stack, TextField, MenuItem } from "@mui/material";
import { type UseFormReturn, useWatch } from "react-hook-form";
import type { ExportProfileFormValues } from "../schemas/exportProfileSchema";

const exportTypeOptions = [
    { value: "StockLevels", label: "Stock Levels" },
    { value: "ConfirmedOrderAggregates", label: "Confirmed Order Aggregates" },
    { value: "Forecasts", label: "Forecasts" },
    { value: "SalesAggregates", label: "Sales Aggregates" },
    { value: "PurchaseForecasts", label: "Purchase Forecasts" },
    { value: "ProductKpis", label: "Product KPIs" },
    { value: "Seasonality", label: "Seasonality" },
];

const customTypeOptions = [
    { value: "View", label: "View" },
    { value: "Procedure", label: "Stored Procedure" },
];

type Props = {
    form: UseFormReturn<ExportProfileFormValues>;
};

export default function ExportProfileForm({ form }: Props) {
    const exportType = useWatch({ control: form.control, name: "exportType" });
    const customSource = useWatch({ control: form.control, name: "customSource" });

    const isCustom = !!customSource;
    const isStandard = !!exportType;

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
                error={!!form.formState.errors.description}
                helperText={form.formState.errors.description?.message}
            />

            {!isCustom && (
                <TextField
                    select
                    label="Export Type"
                    {...form.register("exportType")}
                    error={!!form.formState.errors.exportType}
                    helperText={form.formState.errors.exportType?.message}
                    defaultValue={form.getValues().exportType ?? ""}
                >
                    <MenuItem value=""><em>None</em></MenuItem>
                    {exportTypeOptions.map(o => (
                        <MenuItem key={o.value} value={o.value}>{o.label}</MenuItem>
                    ))}
                </TextField>
            )}

            {!isStandard && (
                <>
                    <TextField
                        label="Custom Source"
                        {...form.register("customSource")}
                        error={!!form.formState.errors.customSource}
                        helperText={form.formState.errors.customSource?.message}
                    />

                    {isCustom && (
                        <TextField
                            select
                            label="Custom Type"
                            {...form.register("customType")}
                            error={!!form.formState.errors.customType}
                            helperText={form.formState.errors.customType?.message}
                            defaultValue={form.getValues().customType ?? ""}
                        >
                            {customTypeOptions.map(o => (
                                <MenuItem key={o.value} value={o.value}>{o.label}</MenuItem>
                            ))}
                        </TextField>
                    )}
                </>
            )}

            <TextField
                label="Target Path"
                {...form.register("targetPath")}
                error={!!form.formState.errors.targetPath}
                helperText={form.formState.errors.targetPath?.message}
                placeholder="C:\Exports\my_export.csv"
            />
        </Stack>
    );
}