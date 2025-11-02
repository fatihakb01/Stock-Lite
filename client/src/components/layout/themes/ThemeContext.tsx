import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { /*createTheme,*/ ThemeProvider, type PaletteMode } from "@mui/material";
import { buildTheme } from "./theme";

interface ThemeContextType {
  mode: PaletteMode;
  toggleMode: () => void;
}

const ColorModeContext = createContext<ThemeContextType>({
  mode: "light",
  toggleMode: () => {},
});

export function useColorMode() {
  return useContext(ColorModeContext);
}

export default function ThemeContextProvider({ children }: { children: ReactNode }) {
  const [mode, setMode] = useState<PaletteMode>("dark");

  const toggleMode = () =>
    setMode((prev) => (prev === "light" ? "dark" : "light"));

  const theme = useMemo(() => buildTheme(mode), [mode]);

  return (
    <ColorModeContext.Provider value={{ mode, toggleMode }}>
      <ThemeProvider theme={theme}>{children}</ThemeProvider>
    </ColorModeContext.Provider>
  );
}
