import { DateTimePicker } from "@mui/x-date-pickers/DateTimePicker";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { AdapterDayjs } from "@mui/x-date-pickers/AdapterDayjs";
import { type Control, type FieldPath, type FieldValues, Controller } from "react-hook-form";
import dayjs from "dayjs";

type Props<T extends FieldValues> = {
    name: FieldPath<T>;
    control: Control<T>;
    label: string;
};

export default function BasicDateTimePicker<T extends FieldValues>({
    name,
    control,
    label
}: Props<T>) {
    return (
        <LocalizationProvider dateAdapter={AdapterDayjs}>
            <Controller
                name={name}
                control={control}
                render={({ field, fieldState }) => (
                    <DateTimePicker
                        label={label}
                        value={field.value ? dayjs(field.value) : null}
                        onChange={(date) =>
                            field.onChange(date ? date.toISOString() : undefined)
                        }
                        slotProps={{
                            textField: {
                                fullWidth: true,
                                error: !!fieldState.error,
                                helperText: fieldState.error?.message
                            },
                            field: {
                                clearable: true
                            }
                        }}
                    />
                )}
            />
        </LocalizationProvider>
    );
}