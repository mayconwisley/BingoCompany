import { Alert, Box, Button, CircularProgress } from "@mui/material";

export function PageState({ loading, error, onRetry }: { loading?: boolean; error?: string; onRetry?: () => void }) {
	if (loading)
		return (
			<Box
				className="page-state"
				aria-live="polite"
				sx={{ display: "flex", alignItems: "center", justifyContent: "center", gap: 1.5 }}
			>
				<CircularProgress size={24} aria-hidden="true" />
				Carregando informações...
			</Box>
		);
	if (error)
		return (
			<Alert
				className="page-state page-state-error"
				severity="error"
				action={
					onRetry && (
						<Button color="inherit" size="small" onClick={onRetry}>
							Tentar novamente
						</Button>
					)
				}
			>
				{error}
			</Alert>
		);
	return null;
}
