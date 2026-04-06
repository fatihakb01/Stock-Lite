import { Box, Typography, Divider, Button, Stack, Chip, MenuItem, TextField } from "@mui/material";
import { useDeleteExportProfile, useExportProfile, useRunExportProfile } from "../../../hooks/useExportProfiles";
import { useStreamExportProfile } from "../../../hooks/useExportProfiles";
import type { CurrentUser } from "../../../types/users/userTypes";
import { useState } from "react";
import ExportProfileEditForm from "../forms/ExportProfileEditForm";
import InfoRow from "../../../components/general/InfoRow";

type Props = {
    selectedId: string | null;
    user: CurrentUser;
};

export default function ExportProfileDetail({ selectedId, user }: Props) {
    const { data: profile, isLoading } = useExportProfile(selectedId);
    const runExport = useRunExportProfile(selectedId);
    const [fileType, setFileType] = useState("Csv");
    const streamExport = useStreamExportProfile(selectedId, fileType);
    const deleteExport = useDeleteExportProfile(selectedId);
    const [editing, setEditing] = useState(false);

    const isSuperAdminOnly = user && user.role === "SuperAdmin";
    const isAdminOrSuperAdmin = user && (user.role === "Admin" || user.role === "SuperAdmin");
    const isKeyUserOrAdminOrSuperAdmin = user && (user.role === "KeyUser" || user.role === "Admin" || user.role === "SuperAdmin");

    if (!selectedId)
        return <Typography color="text.secondary">Select an export profile</Typography>;

    if (isLoading)
        return <Typography>Loading...</Typography>;

    if (!profile)
        return <Typography>Profile not found.</Typography>;

    const hasTargetPath = !!profile.targetPath;

    return (
        <Box>
            <Typography
                variant="h6"
                sx={{ whiteSpace: "normal", wordBreak: "break-word", minWidth: 400, maxWidth: 500 }}
            >
                {profile.name}
            </Typography>

            <Divider sx={{ mb: 2 }} />

            <Stack spacing={1.5}>
                <InfoRow label="Description" value={profile.description ?? "No Description"} />

                <Box>
                    <Typography variant="caption" color="text.secondary">Type</Typography>
                    <Box mt={0.5}>
                        <Chip
                            size="small"
                            label={profile.exportType ?? profile.customType ?? "Custom"}
                            variant="outlined"
                        />
                    </Box>
                </Box>

                <Box sx={{ whiteSpace: "normal", wordBreak: "break-word", maxWidth: 500 }}>
                    <InfoRow label="Custom Source" value={profile.customSource} />
                    <InfoRow label="Target Path" value={profile.targetPath ?? "Dynamic (browser download)"} />
                </Box>

                {!hasTargetPath && (
                    <TextField
                        select
                        label="File Type"
                        value={fileType}
                        onChange={(e) => setFileType(e.target.value)}
                        size="small"
                        sx={{ maxWidth: 160 }}
                    >
                        <MenuItem value="Csv">CSV</MenuItem>
                        <MenuItem value="Excel">Excel</MenuItem>
                    </TextField>
                )}

                <Stack spacing={1.5} direction="row">
                    <Button
                        size="small"
                        variant="contained"
                        disabled={!isAdminOrSuperAdmin}
                        onClick={() => setEditing(true)}
                    >
                        Edit
                    </Button>

                    {hasTargetPath ? (
                        <Button
                            size="small"
                            variant="contained"
                            color="success"
                            disabled={runExport.isPending || !isKeyUserOrAdminOrSuperAdmin}
                            onClick={() => runExport.mutate()}
                        >
                            Run
                        </Button>
                    ) : (
                        <Button
                            size="small"
                            variant="contained"
                            color="success"
                            disabled={streamExport.isPending || !isKeyUserOrAdminOrSuperAdmin}
                            onClick={() => streamExport.mutate()}
                        >
                            Download
                        </Button>
                    )}

                    <Button
                        size="small"
                        variant="contained"
                        color="error"
                        disabled={deleteExport.isPending || !isSuperAdminOnly}
                        onClick={() => deleteExport.mutate()}
                    >
                        Delete
                    </Button>
                </Stack>
            </Stack>

            {editing && (
                <ExportProfileEditForm
                    profile={profile}
                    onCancel={() => setEditing(false)}
                    onSaved={() => setEditing(false)}
                />
            )}
        </Box>
    );
}