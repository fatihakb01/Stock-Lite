import { useState } from "react";
import { Button, Dialog, DialogTitle, DialogContent, DialogActions } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { exportProfileSchema, type ExportProfileFormValues } from "../schemas/exportProfileSchema";
import { useCreateExportProfile } from "../../../hooks/useExportProfiles";
import ExportProfileForm from "../forms/ExportProfileForm";
import type { CurrentUser } from "../../../types/users/userTypes";

type Props = {
    user: CurrentUser;
}

export default function AddExportProfileButton({ user } : Props) {
    const [open, setOpen] = useState(false);
    const createExportProfile = useCreateExportProfile();
    const isAdminOrSuperAdmin = user && (user.role === "Admin" || user.role === "SuperAdmin");

    const form = useForm<ExportProfileFormValues>({
        resolver: zodResolver(exportProfileSchema),
        defaultValues: {
            name: "",
            description: "",
            exportType: undefined,
            customType: undefined,
            customSource: "",
            targetPath: ""
        }
    });

    const onSubmit = async (values: ExportProfileFormValues) => {
        await createExportProfile.mutateAsync({
            name: values.name,
            description: values.description || undefined,
            exportType: values.exportType || undefined,
            customType: values.customType || undefined,
            customSource: values.customSource || undefined,
            targetPath: values.targetPath || undefined
        });
        form.reset();
        setOpen(false);
    };

    return (
        <>
            <Button
                variant="contained"
                size="small"
                onClick={() => setOpen(true)}
                disabled={!isAdminOrSuperAdmin}
            >
                Add export profile
            </Button>

            <Dialog open={open} onClose={() => setOpen(false)} fullWidth>
                <form onSubmit={form.handleSubmit(onSubmit, errors => console.log("ADD EXPORT ERRORS", errors))}>
                    <DialogTitle>Add export profile</DialogTitle>
                    <DialogContent>
                        <ExportProfileForm form={form} />
                    </DialogContent>
                    <DialogActions>
                        <Button onClick={() => setOpen(false)}>Cancel</Button>
                        <Button type="submit" variant="contained" disabled={createExportProfile.isPending}>
                            Save
                        </Button>
                    </DialogActions>
                </form>
            </Dialog>
        </>
    );
}