import { Box } from "@mui/material";
import { Outlet } from "react-router";
import Sidebar from "./Sidebar";

export default function AppLayout() {
  return (
    <Box
      display="flex"
      minHeight="100vh"
    >
      {/* Sidebar */}
      <Sidebar />

      {/* Main content area */}
      <Box
        component="main"
        flex={1}
        p={3}
        sx={{ overflowY: "auto" }}
      >
        <Outlet />
      </Box>
    </Box>
  );
}
