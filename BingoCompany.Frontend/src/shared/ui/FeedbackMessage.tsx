import { Alert, Snackbar } from "@mui/material";
import { useEffect, useState } from "react";

const feedbackDuration = 5_000;

type FeedbackMessageProps = {
	error?: string;
	warning?: string;
	success?: string;
	onClose?: () => void;
};

export function FeedbackMessage({ error, warning, success, onClose }: FeedbackMessageProps) {
	const [isOpen, setIsOpen] = useState(Boolean(error || warning || success));
	const message = error ?? warning ?? success;
	const severity = error ? "error" : warning ? "warning" : "success";

	useEffect(() => {
		setIsOpen(Boolean(message));
	}, [message]);

	const close = () => {
		setIsOpen(false);
		onClose?.();
	};

	return (
		<Snackbar
			anchorOrigin={{ vertical: "bottom", horizontal: "center" }}
			autoHideDuration={feedbackDuration}
			open={isOpen && Boolean(message)}
			onClose={close}
		>
			<Alert severity={severity} variant="filled" onClose={close}>
				{message}
			</Alert>
		</Snackbar>
	);
}
