import { useCallback, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi, useLiveBingo } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function OperatorPage() {
	const { eventId = "", roundId = "" } = useParams();
	const [query] = useSearchParams();
	const navigate = useNavigate();
	const [error, setError] = useState("");
	const code = query.get("code") ?? "";
	const loader = useCallback(() => bingoApi.getPublicEvent(code), [code]);
	const event = useAsyncResource(loader);
	useLiveBingo(eventId, roundId, event.reload);

	if (!event.data?.round)
		return (
			<AppShell>
				<PageState loading={event.loading} error={event.error || "Crie uma rodada antes de abrir a operação."} />
			</AppShell>
		);

	const round = event.data.round;
	const isCurrentRound = round.id === roundId;
	const hasWinnerPresentation = Boolean(round.winner);
	const isPrizeDeliveryPending = round.winner?.isPrizeDeliveryPending ?? false;
	const hasCards = event.data.cards > 0;
	const canStart = isCurrentRound && round.status === "Ready" && hasCards;
	const canDraw = isCurrentRound && round.status === "Drawing" && !hasWinnerPresentation;
	const canCancel = isCurrentRound && round.status === "Drawing" && !hasWinnerPresentation;
	const actionLabel = canStart ? "INICIAR RODADA" : "SORTEAR PRÓXIMA PEDRA";
	const isFinished = isCurrentRound && (round.status === "Finished" || round.status === "Cancelled");
	const isCancelled = isCurrentRound && round.status === "Cancelled";

	const performAction = async () => {
		try {
			setError("");
			if (canStart) await bingoApi.startRound(eventId, roundId);
			else if (canDraw) await bingoApi.draw(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível concluir a ação. Atualize o painel para conferir o estado atual da rodada."));
		}
	};

	const revealWinner = async () => {
		try {
			setError("");
			await bingoApi.reveal(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível revelar o vencedor."));
		}
	};

	const continueDraw = async () => {
		try {
			setError("");
			await bingoApi.closeWinnerPresentation(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível voltar o telão para o sorteio."));
		}
	};

	const markPrizeDelivered = async () => {
		try {
			setError("");
			await bingoApi.markPrizeDelivered(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível registrar a entrega do prêmio."));
		}
	};

	const markPrizeDeclined = async () => {
		try {
			setError("");
			await bingoApi.markPrizeDeclined(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível registrar a ausência do vencedor."));
		}
	};

	const cancelRound = async () => {
		try {
			setError("");
			await bingoApi.cancelRound(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível cancelar a rodada."));
		}
	};

	return (
		<AppShell>
			<main>
				<header className="pagehead">
					<div>
						<p className="eyebrow">Painel do operador</p>
						<h1>{round.name}</h1>
					</div>
				</header>
				<div className="operator">
					<section className="drawball">
						<small>ÚLTIMA PEDRA</small>
						<strong>{round.drawnNumbers.at(-1) ?? "—"}</strong>
						<p>{isCancelled ? "Rodada cancelada" : round.currentPrize || "Rodada finalizada"}</p>
					</section>
					<section className="panel controls">
						<button className="primary big" disabled={!canStart && !canDraw} onClick={performAction}>
							{actionLabel}
						</button>
						<button
							className="reveal"
							disabled={round.status !== "WinnerDetected" && round.status !== "TieBreaker"}
							onClick={revealWinner}
						>
							{round.status === "TieBreaker" ? "REALIZAR DESEMPATE" : "REVELAR VENCEDOR"}
						</button>
						<button className="danger" disabled={!canCancel} onClick={cancelRound}>
							CANCELAR RODADA
						</button>
						{hasWinnerPresentation && (
							<>
								<FeedbackMessage
									success={`O telão está exibindo ${round.winner?.participantName}, vencedor(a) de ${round.winner?.prizeName}.`}
								/>
								{isPrizeDeliveryPending ? (
									<>
										<button className="primary" onClick={markPrizeDelivered}>
											PRÊMIO ENTREGUE
										</button>
										<button className="danger" onClick={markPrizeDeclined}>
											VENCEDOR NÃO RETIROU O PRÊMIO
										</button>
									</>
								) : (
									<button className="primary" onClick={continueDraw}>
										CONTINUAR SORTEIO NO TELÃO
									</button>
								)}
							</>
						)}
						{isFinished && (
							<>
								<FeedbackMessage success={isCancelled ? "A rodada foi cancelada sem vencedor." : "Cartela cheia concluída. A rodada foi encerrada."} />
								<p>
									Prepare a próxima rodada antes de iniciar: as cartelas ativas serão reutilizadas; participantes podem
									gerar novas cartelas e você pode registrar novas impressas.
								</p>
								<button className="primary big" onClick={() => navigate(`/admin/eventos/${eventId}?code=${code}`)}>
									PREPARAR PRÓXIMO SORTEIO
								</button>
							</>
						)}
						<p>{round.drawnNumbers.length} pedras sorteadas</p>
						{round.status === "Ready" && !hasCards && <FeedbackMessage warning="Gere e ative ao menos uma cartela antes de iniciar a rodada." />}
						{!isCurrentRound && (
							<FeedbackMessage warning="Esta não é a rodada atual do evento. Volte à configuração e abra a rodada correta." />
						)}
						<FeedbackMessage error={error} onClose={() => setError("")} />
					</section>
				</div>
			</main>
		</AppShell>
	);
}
