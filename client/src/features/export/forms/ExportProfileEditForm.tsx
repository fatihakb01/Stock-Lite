import { Dialog, DialogTitle, DialogContent, DialogActions, Button } from "@mui/material";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { exportProfileSchema, type ExportProfileFormValues } from "../schemas/exportProfileSchema";
import { useEditExportProfile } from "../../../hooks/useExportProfiles";
import type { ExportProfileDetail } from "../../../types/exportProfiles/exportProfileTypes";
import ExportProfileForm from "../forms/ExportProfileForm";

type Props = {
    profile: ExportProfileDetail;
    onCancel: () => void;
    onSaved: () => void;
};

export default function ExportProfileEditForm({ profile, onCancel, onSaved }: Props) {
    const editExportProfile = useEditExportProfile(profile.id);

    const form = useForm<ExportProfileFormValues>({
        resolver: zodResolver(exportProfileSchema),
        defaultValues: {
            name: profile.name,
            description: profile.description || undefined,
            exportType: profile.exportType || undefined,
            customType: profile.customType || undefined,
            customSource: profile.customSource || "",
            targetPath: profile.targetPath || ""
        }
    });

    const onSubmit = async (values: ExportProfileFormValues) => {
        await editExportProfile.mutateAsync({
            name: values.name,
            description: values.description || undefined,
            exportType: values.exportType || undefined,
            customType: values.customType || undefined,
            customSource: values.customSource || undefined,
            targetPath: values.targetPath || undefined
        });
        onSaved();
    };

    return (
        <Dialog open fullWidth onClose={onCancel}>
            <form onSubmit={form.handleSubmit(onSubmit, errors => console.log("EDIT EXPORT ERRORS", errors))}>
                <DialogTitle>Edit export profile</DialogTitle>
                <DialogContent>
                    <ExportProfileForm form={form} />
                </DialogContent>
                <DialogActions>
                    <Button onClick={onCancel}>Cancel</Button>
                    <Button type="submit" variant="contained" disabled={editExportProfile.isPending}>
                        Save
                    </Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}