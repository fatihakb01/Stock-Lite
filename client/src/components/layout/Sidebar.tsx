import { useState } from "react";
import { NavLink } from "react-router";
import {
  Menu,
  Home,
  ShoppingBag,
  Users,
  CreditCard,
} from "lucide-react";

import {
  Box,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Divider,
  useTheme
} from "@mui/material";
import { useColorMode } from "./ThemeContext"; 
import { Brightness4, Brightness7 } from "@mui/icons-material";


export default function Sidebar() {
  const [open, setOpen] = useState(false);
  const theme = useTheme();
  const { mode, toggleMode } = useColorMode();

  const toggleDrawer = (newOpen: boolean) => () => setOpen(newOpen);

  const menuItems = [
    { text: "Home", icon: <Home />, to: "/" },
    { text: "Products", icon: <ShoppingBag />, to: "/products" },
    { text: "Customers", icon: <Users />, to: "/customers" },
    { text: "Transactions", icon: <CreditCard />, to: "/transactions" },
  ];

  const iconColor = theme.palette.mode === "light" ? "black" : "white";

  return (
    <Box>
      {/* Toggle button (top-left corner) */}
      <IconButton
        onClick={toggleDrawer(true)}
        sx={{ 
          color: iconColor, 
          position: "fixed", 
          top: 16, 
          left: 16, 
          zIndex: 1300,
          "&:hover": { backgroundColor: "transparent" },
        }}
      >
        {<Menu />}
      </IconButton>

      {/* Drawer itself */}
      <Drawer anchor="left" open={open} onClose={toggleDrawer(false)}>
        <Box
          sx={{ width: 250 }}
          role="presentation"
          onClick={toggleDrawer(false)}
        >
          <Box
            sx={{
              p: 2,
              fontWeight: "bold",
              fontSize: "1.2rem",
              color: "#1976d2",
              textAlign: "center",
            }}
          >
            Stock Lite
          </Box>
          <Divider />
          
          {/* Show each page in the side bar */}
          <List>
            {menuItems.map((item) => (
              <ListItem key={item.text} disablePadding>
                <NavLink
                  to={item.to}
                  style={{ width: "100%", textDecoration: "none", color: "inherit" }}
                >
                  {({ isActive }: { isActive: boolean }) => (
                    <ListItemButton
                      sx={{
                        bgcolor: isActive ? "action.selected" : "transparent",
                      }}
                    >
                      <ListItemIcon sx={{ color: isActive ? "primary.main" : "inherit" }}>
                        {item.icon}
                      </ListItemIcon>
                      <ListItemText primary={item.text} />
                    </ListItemButton>
                  )}
                </NavLink>
              </ListItem>
            ))}
          </List>
          <Divider />

          {/* Enabling/disabling dark mode */}
          <ListItem disablePadding>
            <ListItemButton onClick={toggleMode}>
              <ListItemIcon>
                {mode === "light" ? <Brightness4 /> : <Brightness7 />}
              </ListItemIcon>
              <ListItemText
                primary={mode === "light" ? "Dark Mode" : "Light Mode"}
              />
            </ListItemButton>
          </ListItem>
        </Box>
      </Drawer>
    </Box>
  );
}
