import { Box, Typography } from "@mui/material";

export default function InfoRow({ label, value }: { label: string; value: React.ReactNode }) {
    if (!value) return null;
    return (
        <Box sx={{mb:2}}>
            <Typography variant="caption" color="text.secondary">{label}</Typography>
            <Typography
                variant="body2"
                sx={{
                    whiteSpace: "normal",
                    wordBreak: "break-word",
                    minWidth: 400,
                    maxWidth: 500
                }}
            >
                {value}
            </Typography>
        </Box>
    );
}