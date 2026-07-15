import { useEffect, useRef } from "react";

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
		<div className="confirmation-backdrop" role="presentation">
			<section className="confirmation-dialog" role="dialog" aria-modal="true" aria-labelledby="confirmation-dialog-title">
				<h2 id="confirmation-dialog-title">{title}</h2>
				<p>{description}</p>
				<div className="actions">
					<button type="button" onClick={onCancel} disabled={isConfirming}>
						Voltar
					</button>
					<button ref={confirmButton} type="button" className="danger" onClick={onConfirm} disabled={isConfirming}>
						{isConfirming ? "Confirmando..." : confirmLabel}
					</button>
				</div>
			</section>
		</div>
	);
}
