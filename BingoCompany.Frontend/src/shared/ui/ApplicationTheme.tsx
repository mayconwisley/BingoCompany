import { CssBaseline, ThemeProvider, createTheme } from "@mui/material";
import { createContext, useContext, useEffect, useMemo, useState } from "react";
import type { PropsWithChildren } from "react";

type ApplicationThemeMode = "light" | "dark";

type ApplicationThemeContextValue = {
	mode: ApplicationThemeMode;
	toggleMode: () => void;
};

const storageKey = "bingo-company-theme";
const ApplicationThemeContext = createContext<ApplicationThemeContextValue | undefined>(undefined);

function getInitialMode(): ApplicationThemeMode {
	try {
		const savedMode = window.localStorage.getItem(storageKey);
		if (savedMode === "light" || savedMode === "dark") return savedMode;
	} catch {
		return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
	}

	return window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
}

export function ApplicationThemeProvider({ children }: PropsWithChildren) {
	const [mode, setMode] = useState<ApplicationThemeMode>(getInitialMode);
	const theme = useMemo(
		() =>
			createTheme({
				palette: {
					mode,
					primary: { main: mode === "dark" ? "#918bff" : "#635bff" },
					secondary: { main: mode === "dark" ? "#3dd7b4" : "#0dbb95" },
					background: { default: mode === "dark" ? "#101522" : "#f7f8fc", paper: mode === "dark" ? "#181f30" : "#ffffff" }
				},
				shape: { borderRadius: 12 },
				typography: { fontFamily: '"DM Sans", sans-serif', button: { fontWeight: 700, textTransform: "none" } },
				components: {
					MuiButton: { defaultProps: { disableElevation: true } },
					MuiPaper: { styleOverrides: { root: { backgroundImage: "none" } } }
				}
			}),
		[mode]
	);

	useEffect(() => {
		document.documentElement.dataset.theme = mode;
		try {
			window.localStorage.setItem(storageKey, mode);
		} catch {
			// The visual theme remains usable when storage is unavailable.
		}
	}, [mode]);

	const value = useMemo(
		() => ({ mode, toggleMode: () => setMode((currentMode) => (currentMode === "dark" ? "light" : "dark")) }),
		[mode]
	);

	return (
		<ApplicationThemeContext.Provider value={value}>
			<ThemeProvider theme={theme}>
				<CssBaseline />
				{children}
			</ThemeProvider>
		</ApplicationThemeContext.Provider>
	);
}

export function useApplicationTheme() {
	const context = useOptionalApplicationTheme();
	if (!context) throw new Error("useApplicationTheme deve ser utilizado dentro de ApplicationThemeProvider.");
	return context;
}

export function useOptionalApplicationTheme() {
	return useContext(ApplicationThemeContext);
}
