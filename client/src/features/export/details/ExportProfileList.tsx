import { Table, TableHead, TableRow, TableCell, TableBody, Box } from "@mui/material";
import { useExportProfiles } from "../../../hooks/useExportProfiles";
import type { CurrentUser } from "../../../types/users/userTypes";
import AddExportProfileButton from "../forms/AddExportProfileButton";

type Props = {
    selectedId: string | null;
    onSelect: (id: string) => void;
    user: CurrentUser;
};

export default function ExportProfileList({ selectedId, onSelect, user }: Props) {
    const { data } = useExportProfiles();

    return (
        <Box>
            <Box mb={2}>
                <AddExportProfileButton user={ user } />
            </Box>

            <Table size="medium">
                <TableHead>
                    <TableRow>
                        <TableCell>Name</TableCell>
                        <TableCell>Type</TableCell>
                        <TableCell>Target Path</TableCell>
                    </TableRow>
                </TableHead>
                <TableBody>
                    {data && data.map(profile => (
                        <TableRow
                            key={profile.id}
                            hover
                            selected={selectedId === profile.id}
                            onClick={() => onSelect(profile.id)}
                            sx={{ cursor: "pointer" }}
                        >
                            <TableCell
                                sx={{
                                    whiteSpace: "normal",
                                    wordBreak: "break-word",
                                    maxWidth: 300
                                }}
                            >
                                {profile.name}
                            </TableCell>
                            <TableCell>
                                {profile.exportType ?? profile.customType ?? "—"}
                            </TableCell>
                            <TableCell
                                sx={{
                                    whiteSpace: "normal",
                                    wordBreak: "break-word",
                                    maxWidth: 300
                                }}
                            >
                                {profile.targetPath ?? "Dynamic path"}
                            </TableCell>
                        </TableRow>
                    ))}
                </TableBody>
            </Table>
        </Box>
    );
}