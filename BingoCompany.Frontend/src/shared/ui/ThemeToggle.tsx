import { IconButton, Tooltip } from "@mui/material";
import { useState } from "react";
import { useOptionalApplicationTheme } from "./ApplicationTheme";

type Props = { className?: string };

export function ThemeToggle({ className = "" }: Props) {
	const applicationTheme = useOptionalApplicationTheme();
	const [fallbackMode, setFallbackMode] = useState<"light" | "dark">(() =>
		document.documentElement.dataset.theme === "dark" ? "dark" : "light"
	);
	const mode = applicationTheme?.mode ?? fallbackMode;
	const toggleMode = () => {
		if (applicationTheme) {
			applicationTheme.toggleMode();
			return;
		}
		setFallbackMode((currentMode) => {
			const nextMode = currentMode === "dark" ? "light" : "dark";
			document.documentElement.dataset.theme = nextMode;
			return nextMode;
		});
	};
	const nextTheme = mode === "dark" ? "light" : "dark";
	return (
		<Tooltip title={`Ativar tema ${nextTheme === "dark" ? "escuro" : "claro"}`}>
			<IconButton
				className={`theme-toggle ${className}`.trim()}
				onClick={toggleMode}
				aria-label={`Ativar tema ${nextTheme === "dark" ? "escuro" : "claro"}`}
			>
				<span aria-hidden="true">{mode === "dark" ? "☀" : "☾"}</span>
			</IconButton>
		</Tooltip>
	);
}
