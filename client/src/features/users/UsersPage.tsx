import { Box, Typography, Fab, CircularProgress } from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import UserCard from "./UserCard";
import UserDialog from "./UserDialog";
import NewUserDialog from "./NewUserDialog";
import { useState } from "react";
import { useUsers, useCurrentUser } from "../../hooks/useUsers";

export default function UsersPage() {
  const { data: currentUser } = useCurrentUser();
  const { data: users, isLoading } = useUsers();

  const [selectedUserId, setSelectedUserId] = useState<string | null>(null);
  const [createOpen, setCreateOpen] = useState(false);

  const openUserDialog = (id: string) => {
    setSelectedUserId(id);
  };

  return (
    <Box>
      <Typography variant="h5" mb={2}>User Management</Typography>

      {isLoading ? (
        <Box display="flex" justifyContent="center" mt={4}>
          <CircularProgress />
        </Box>
      ) : (
        <Box
          display="grid"
          gridTemplateColumns="repeat(4, 1fr)"
          gap={2}
        >
          {users?.map((user) => (
            <Box key={user.id}>
              <UserCard user={user} onClick={() => setSelectedUserId(user.id)} />
            </Box>
          ))}
        </Box>
      )}

      {currentUser && currentUser.role === "SuperAdmin" && (
        <Fab
          color="primary"
          sx={{ position: "fixed", bottom: 32, right: 32 }}
          onClick={() => setCreateOpen(true)}
        >
          <AddIcon />
        </Fab>
      )}

      <UserDialog
        open={!!selectedUserId}
        userId={selectedUserId}
        onClose={() => setSelectedUserId(null)}
      />

      <NewUserDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        onUserCreated={openUserDialog}
      />
    </Box>
  );
}
