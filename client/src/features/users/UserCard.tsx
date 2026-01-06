import { Card, CardActionArea, CardContent, Typography, Avatar, Box } from "@mui/material";
import type { User } from "../../types/users/userTypes";
import { roleMap } from "../../types/users/roleMap";

export default function UserCard({ user, onClick }: { user: User; onClick: () => void }) {
  return (
    <Card>
      <CardActionArea onClick={onClick}>
        <CardContent>
          <Box display="flex" flexDirection="column" alignItems="center">
            <Avatar sx={{ width: 64, height: 64, mb: 1 }} />

            <Typography variant="h6">{user.userName}</Typography>
            {/* <Typography color="text.secondary">{user.email}</Typography> */}
            <Typography mt={1} fontSize={14}>
              {roleMap[Number(user.role)]}
            </Typography>

            <Typography fontSize={12} color="text.secondary" mt={1}>
              Created: {new Date(user.createdAt).toLocaleDateString()}
            </Typography>
          </Box>
        </CardContent>
      </CardActionArea>
    </Card>
  );
}
