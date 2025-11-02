import { createTheme, type ThemeOptions } from "@mui/material/styles";

const palette = {
  primary: { main: "#1565C0", contrastText: "#fff" },   // blue
  secondary: { main: "#00BFA6", contrastText: "#fff" }, // teal
  success: { main: "#2E7D32", contrastText: "#fff" },   // green
  warning: { main: "#FF8F00", contrastText: "#fff" },   // amber
  error: { main: "#C62828", contrastText: "#fff" },     // red
  info: { main: "#0288D1", contrastText: "#fff" },      // info blue
  background: {
    default: "#F7F9FC",
    paper: "#FFFFFF"
  },
  text: {
    primary: "#1F2937",
    secondary: "#6B7280"
  }
} as Partial<ThemeOptions["palette"]>;

export function buildTheme(mode: "light" | "dark" = "light") {
  return createTheme({
    palette: {
      mode,
      ...palette,
      ...(mode === "light"
        ? {
            background: {
              default: "#A8CBE1", 
              paper: "#CADFED",   
            },
            text: {
              primary: "#1A273A",    
              secondary: "#4B5563",  
            },
          }
        : {
            background: {
              default: "#0B1220",
              paper: "#071022",
            },
            text: {
              primary: "#E6EEF7",
              secondary: "#A8B4C2",
            },
          }),
    },
    typography: {
      fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
      h1: { fontWeight: 600 },
      h2: { fontWeight: 600 },
      body1: { fontSize: 14 },
    },
    components: {
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: { borderRadius: 10, textTransform: "none", padding: "8px 14px" }
        }
      },
      MuiChip: {
        styleOverrides: {
          root: { fontWeight: 600 }
        }
      },
      MuiPaper: {
        styleOverrides: {
          root: {
            borderRadius: 12,
            boxShadow:
              mode === "light"
                ? "0px 2px 8px rgba(0,0,0,0.05)"
                : "0px 2px 8px rgba(0,0,0,0.3)",
          }
        }
      }
    }
  });
}
