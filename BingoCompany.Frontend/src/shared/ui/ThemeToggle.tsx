import { IconButton, Tooltip } from "@mui/material";
import { useApplicationTheme } from "./ApplicationTheme";

type Props = { className?: string };

export function ThemeToggle({ className = "" }: Props) {
	const { mode, toggleMode } = useApplicationTheme();
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
