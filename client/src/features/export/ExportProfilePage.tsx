import { Box, Grid, Paper, Typography } from "@mui/material";
import { useExportProfiles } from "../../hooks/useExportProfiles";
import { useSelection } from "../../hooks/useSelection";
import { useCurrentUser } from "../../hooks/useUsers";
import ExportProfileList from "./details/ExportProfileList";
import ExportProfileDetail from "./details/ExportProfileDetail";

export default function ExportProfilePage() {
    const { data: profiles } = useExportProfiles();
    const { data: currentUser } = useCurrentUser();
    const defaultId = profiles?.[0]?.id;
    const { selectedId, select } = useSelection(defaultId);

    return (
        <Box sx={{ p: 2 }}>
            <Typography variant="h5" mb={2}>
                Export profiles
            </Typography>

            <Grid container spacing={2}>
                <Grid>
                    <Paper sx={{ p: 2, height: "100%" }}>
                        <ExportProfileList
                            selectedId={selectedId}
                            onSelect={select}
                            user={currentUser!}
                        />
                    </Paper>
                </Grid>

                <Grid>
                    <Paper sx={{ p: 2, height: "100%" }}>
                        <ExportProfileDetail
                            selectedId={selectedId}
                            user={currentUser!}
                        />
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
}