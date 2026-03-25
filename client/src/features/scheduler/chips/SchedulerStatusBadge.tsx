import { Chip } from "@mui/material";
import CheckCircleOutlineIcon from "@mui/icons-material/CheckCircleOutline";
import ErrorOutlineIcon from "@mui/icons-material/ErrorOutline";
import HourglassEmptyIcon from "@mui/icons-material/HourglassEmpty";
import SyncIcon from "@mui/icons-material/Sync";

type Props = {
    status: string | undefined;
    label: string
};

const statusConfig = {
    Failed: { label: "Failed", color: "error", icon: <ErrorOutlineIcon fontSize="small" /> },
    Running: { label: "Running", color: "info", icon: <SyncIcon fontSize="small" /> },
    Succeeded: { label: "Succeeded", color: "success", icon: <CheckCircleOutlineIcon fontSize="small" /> },
    Pending: { label: "Pending", color: "warning", icon: <HourglassEmptyIcon fontSize="small" /> },
} as const;

export default function SchedulerStatusBadge({ status, label }: Props) {
    const config = status
        ? statusConfig[status as keyof typeof statusConfig]
        : undefined;

    if (!config) return null;

    return (
        <Chip
            label={label + " " + config.label}
            color={config.color}
            icon={config.icon}
            variant="outlined"
            size="small"
        />
    );
}