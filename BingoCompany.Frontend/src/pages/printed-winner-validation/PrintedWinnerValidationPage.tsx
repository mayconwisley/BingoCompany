import { useState } from "react";
import { useParams } from "react-router-dom";
import { bingoApi, QrCardScanner } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";

export function PrintedWinnerValidationPage() {
	const { eventId = "", roundId = "" } = useParams();
	const [error, setError] = useState("");
	const [success, setSuccess] = useState("");

	const validateCard = async (cardCode: string) => {
		try {
			setError("");
			const result = await bingoApi.validatePrintedWinner(eventId, roundId, cardCode);
			setSuccess(result.tieBreakerRequired ? `${result.participantName} entrou no empate.` : `${result.participantName} foi confirmado(a) como candidato(a).`);
		} catch (validationError) {
			setError(getErrorMessage(validationError, "Não foi possível validar esta cartela impressa."));
		}
	};

	return (
		<AppShell>
			<main className="join">
				<header className="pagehead">
					<div>
						<p className="eyebrow">Conferência do operador</p>
						<h1>Validar cartela impressa</h1>
						<p className="subtitle">Leia o QR Code da cartela apresentada. O sistema confere a regra usando somente as pedras já sorteadas.</p>
					</div>
				</header>
				<section className="panel">
					<QrCardScanner onCardCodeRead={(cardCode) => void validateCard(cardCode)} />
					<p>Confira todas as cartelas físicas apresentadas antes de revelar o vencedor no painel do operador.</p>
					<FeedbackMessage error={error} success={success} onClose={() => { setError(""); setSuccess(""); }} />
				</section>
			</main>
		</AppShell>
	);
}
