import { BrowserQRCodeReader, type IScannerControls } from "@zxing/browser";
import { useEffect, useRef, useState } from "react";

type QrCardScannerProps = {
	onCardCodeRead: (cardCode: string) => void;
};

export function QrCardScanner({ onCardCodeRead }: QrCardScannerProps) {
	const video = useRef<HTMLVideoElement>(null);
	const [isOpen, setIsOpen] = useState(false);
	const [error, setError] = useState("");

	useEffect(() => {
		if (!isOpen || !video.current) return;
		const reader = new BrowserQRCodeReader();
		let controls: IScannerControls | undefined;
		void reader
			.decodeFromVideoDevice(undefined, video.current, (result) => {
				if (!result) return;
				const parts = result.getText().split(":");
				const cardCode = parts.length === 4 && parts[0] === "BINGO" ? parts[2] : "";
				if (!cardCode) {
					setError("Este QR Code não pertence a uma cartela do Bingo Company.");
					return;
				}
				controls?.stop();
				onCardCodeRead(cardCode);
				setIsOpen(false);
			})
			.then((value) => (controls = value))
			.catch(() => setError("Não foi possível acessar a câmera. Verifique a permissão do navegador."));
		return () => controls?.stop();
	}, [isOpen, onCardCodeRead]);

	return (
		<section className="qrscanner">
			<button
				type="button"
				onClick={() => {
					setError("");
					setIsOpen((value) => !value);
				}}
			>
				{isOpen ? "Fechar câmera" : "Ler QR Code da cartela"}
			</button>
			{isOpen && <video ref={video} muted playsInline aria-label="Câmera para leitura de QR Code" />}
			{error && (
				<p className="error" role="alert">
					{error}
				</p>
			)}
		</section>
	);
}
