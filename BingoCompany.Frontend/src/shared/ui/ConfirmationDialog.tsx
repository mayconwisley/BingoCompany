import { useEffect, useRef } from "react";
import { Button, Dialog, DialogActions, DialogContent, DialogContentText, DialogTitle } from "@mui/material";

type Props = {
	title: string;
	description: string;
	confirmLabel: string;
	isConfirming?: boolean;
	onCancel: () => void;
	onConfirm: () => void;
};

export function ConfirmationDialog({ title, description, confirmLabel, isConfirming, onCancel, onConfirm }: Props) {
	const confirmButton = useRef<HTMLButtonElement>(null);

	useEffect(() => {
		const previouslyFocusedElement = document.activeElement instanceof HTMLElement ? document.activeElement : undefined;
		confirmButton.current?.focus();
		const onKeyDown = (event: KeyboardEvent) => {
			if (event.key === "Escape") onCancel();
		};
		document.addEventListener("keydown", onKeyDown);
		return () => {
			document.removeEventListener("keydown", onKeyDown);
			previouslyFocusedElement?.focus();
		};
	}, [onCancel]);

	return (
		<Dialog open onClose={isConfirming ? undefined : onCancel} aria-labelledby="confirmation-dialog-title">
			<DialogTitle id="confirmation-dialog-title">{title}</DialogTitle>
			<DialogContent>
				<DialogContentText>{description}</DialogContentText>
			</DialogContent>
			<DialogActions sx={{ px: 3, pb: 2.5 }}>
				<Button onClick={onCancel} disabled={isConfirming}>Voltar</Button>
				<Button ref={confirmButton} variant="contained" color="error" onClick={onConfirm} disabled={isConfirming}>
					{isConfirming ? "Confirmando..." : confirmLabel}
				</Button>
			</DialogActions>
		</Dialog>
	);
}
