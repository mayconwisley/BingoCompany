import { useCallback, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi, bingoBallLabel, markingModeLabel, useLiveBingo, winningPatternLabel } from "../../features/bingo";
import { QrCardScanner } from "../../features/bingo/qr";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";
import { ConfirmationDialog } from "../../shared/ui/ConfirmationDialog";
import { ConnectionBadge } from "../../shared/ui/ConnectionBadge";

type PendingConfirmation = "cancelRound" | "declinePrize";

export function OperatorPage() {
	const { eventId = "", roundId = "" } = useParams();
	const [query] = useSearchParams();
	const navigate = useNavigate();
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [isValidatingPrintedCard, setIsValidatingPrintedCard] = useState(false);
	const [pendingConfirmation, setPendingConfirmation] = useState<PendingConfirmation>();
	const [lastConfirmedAction, setLastConfirmedAction] = useState("");
	const [printedValidationError, setPrintedValidationError] = useState("");
	const [printedValidationSuccess, setPrintedValidationSuccess] = useState("");
	const code = query.get("code") ?? "";
	const displayPath = `/display/${code}`;
	const loader = useCallback(() => bingoApi.getPublicEvent(code), [code]);
	const event = useAsyncResource(loader);
	const connection = useLiveBingo(eventId, roundId, event.reload);

	if (!event.data?.round)
		return (
			<AppShell>
				<PageState loading={event.loading} error={event.error || "Crie uma rodada antes de abrir a operação."} />
			</AppShell>
		);

	const round = event.data.round;
	const lastDrawnNumber = round.drawnNumbers.at(-1);
	const isCurrentRound = round.id === roundId;
	const hasWinnerPresentation = Boolean(round.winner);
	const isPrizeDeliveryPending = isCurrentRound && (round.winner?.isPrizeDeliveryPending ?? round.hasPrizeDeliveryPending ?? false);
	const eligibleCards = round.eligibleCards ?? 0;
	const openingStage = round.status === "Ready" ? round.stages.at(0) : undefined;
	const hasEligibleCards = eligibleCards > 0;
	const isRealtimeUnavailable = connection !== undefined && connection !== "Conectado";
	const canStart = isCurrentRound && round.status === "Ready" && hasEligibleCards && !isRealtimeUnavailable;
	const canDraw = isCurrentRound && round.status === "Drawing" && !hasWinnerPresentation && !isRealtimeUnavailable;
	const needsPrintedValidation = event.data.markingMode !== "Automatic";
	const canValidatePrintedCards =
		needsPrintedValidation && isCurrentRound && round.status !== "Ready" && round.status !== "Finished" && round.status !== "Cancelled";
	const canCancel = isCurrentRound && round.status === "Drawing" && !hasWinnerPresentation;
	const actionLabel = round.status === "Ready" ? "INICIAR RODADA" : "SORTEAR PRÓXIMA PEDRA";
	const isFinished = isCurrentRound && (round.status === "Finished" || round.status === "Cancelled");
	const isCancelled = isCurrentRound && round.status === "Cancelled";
	const actionDisabledReason = !isCurrentRound
		? "Abra a rodada atual para realizar ações."
		: isRealtimeUnavailable
			? "Aguarde a reconexão com o telão antes de iniciar ou sortear uma pedra."
			: round.status === "Ready" && !hasEligibleCards
				? "Ative ao menos uma cartela antes de iniciar a rodada."
				: hasWinnerPresentation
					? "O telão está apresentando o vencedor. Conclua essa apresentação antes de continuar o sorteio."
					: undefined;

	const performAction = async () => {
		if (isSubmitting) return;
		setIsSubmitting(true);
		try {
			setError("");
			if (canStart) {
				await bingoApi.startRound(eventId, roundId);
				setLastConfirmedAction("Rodada iniciada e confirmada pelo servidor.");
			} else if (canDraw) {
				const result = await bingoApi.draw(eventId, roundId);
				setLastConfirmedAction(
					result
						? `Pedra ${bingoBallLabel(result.number)} sorteada e confirmada pelo servidor.`
						: "Pedra sorteada e confirmada pelo servidor."
				);
			}
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível concluir a ação. Atualize o painel para conferir o estado atual da rodada."));
		} finally {
			setIsSubmitting(false);
		}
	};

	const revealWinner = async () => {
		if (isSubmitting) return;
		setIsSubmitting(true);
		try {
			setError("");
			await bingoApi.reveal(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível revelar o vencedor."));
		} finally {
			setIsSubmitting(false);
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
		if (isSubmitting) return;
		setIsSubmitting(true);
		try {
			setError("");
			await bingoApi.markPrizeDelivered(eventId, roundId);
			await event.reload();
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível registrar a entrega do prêmio."));
		} finally {
			setIsSubmitting(false);
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
	const validatePrintedCard = async (cardCode: string) => {
		if (isValidatingPrintedCard) return;
		setIsValidatingPrintedCard(true);
		try {
			setPrintedValidationError("");
			setPrintedValidationSuccess("");
			const result = await bingoApi.validatePrintedWinner(eventId, roundId, cardCode);
			setPrintedValidationSuccess(
				result.tieBreakerRequired
					? `${result.participantName} entrou no desempate com a cartela ${result.cardCode}.`
					: `${result.participantName} foi confirmado(a) com a cartela ${result.cardCode}.`
			);
			await event.reload();
		} catch (validationError) {
			setPrintedValidationError(getErrorMessage(validationError, "Não foi possível validar esta cartela impressa."));
		} finally {
			setIsValidatingPrintedCard(false);
		}
	};
	const prepareNextRound = async () => {
		if (isSubmitting) return;
		setIsSubmitting(true);
		try {
			setError("");
			if (hasWinnerPresentation) await bingoApi.closeWinnerPresentation(eventId, roundId);
			await event.reload();
			navigate(`/admin/eventos/${eventId}?code=${code}`);
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível encerrar a apresentação do resultado anterior."));
		} finally {
			setIsSubmitting(false);
		}
	};
	const confirmPendingAction = async () => {
		const action = pendingConfirmation;
		if (!action) return;
		setIsSubmitting(true);
		try {
			if (action === "cancelRound") await cancelRound();
			else await markPrizeDeclined();
			setPendingConfirmation(undefined);
		} finally {
			setIsSubmitting(false);
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
					<div className="actions operator-header-actions">
						<ConnectionBadge status={connection} />
						<a className="button" href={displayPath} target="_blank" rel="noopener noreferrer">
							ABRIR TELÃO
						</a>
						<button onClick={() => navigate(`/admin/eventos/${eventId}?code=${code}`)}>VOLTAR ÀS CONFIGURAÇÕES</button>
					</div>
				</header>
				<div className="operator">
					<section className="drawball">
						<small>ÚLTIMA PEDRA</small>
						<strong>{lastDrawnNumber === undefined ? "—" : bingoBallLabel(lastDrawnNumber)}</strong>
						<p>{isCancelled ? "Rodada cancelada" : round.currentPrize || "Rodada finalizada"}</p>
					</section>
					<section className="panel controls">
						{round.status === "Ready" && isCurrentRound && (
							<section className="round-readiness" aria-labelledby="round-readiness-title">
								<div>
									<p className="eyebrow">Checklist de abertura</p>
									<h2 id="round-readiness-title">
										{hasEligibleCards ? "Tudo pronto para iniciar" : "Há pendências para iniciar"}
									</h2>
								</div>
								<ul>
									<li className={hasEligibleCards ? "is-ready" : "is-pending"}>
										{eligibleCards} {eligibleCards === 1 ? "cartela elegível" : "cartelas elegíveis"}
									</li>
									<li className={round.stages.length > 0 ? "is-ready" : "is-pending"}>
										{round.stages.length} {round.stages.length === 1 ? "etapa configurada" : "etapas configuradas"}
									</li>
									<li className={openingStage ? "is-ready" : "is-pending"}>
										{openingStage
											? `${openingStage.prizeName} · ${winningPatternLabel(openingStage.pattern)}`
											: "Defina o prêmio inicial"}
									</li>
									<li className="is-ready">Marcação {markingModeLabel(event.data.markingMode)}</li>
								</ul>
							</section>
						)}
						{canValidatePrintedCards && (
							<section className="printed-card-validation" aria-labelledby="printed-card-validation-title">
								<div>
									<p className="eyebrow">Conferência de cartela física</p>
									<h2 id="printed-card-validation-title">Ler QR Code da cartela</h2>
									<p>O servidor confirma a regra usando apenas as pedras sorteadas nesta rodada.</p>
								</div>
								<QrCardScanner
									onCardCodeRead={(cardCode) => void validatePrintedCard(cardCode)}
									disabled={isValidatingPrintedCard}
								/>
								<FeedbackMessage
									error={printedValidationError}
									success={printedValidationSuccess}
									onClose={() => {
										setPrintedValidationError("");
										setPrintedValidationSuccess("");
									}}
								/>
							</section>
						)}
						<button
							className="primary big"
							disabled={isSubmitting || (!canStart && !canDraw)}
							aria-describedby={actionDisabledReason ? "operator-action-hint" : undefined}
							onClick={performAction}
						>
							{isSubmitting ? "PROCESSANDO..." : actionLabel}
						</button>
						{actionDisabledReason && (
							<p id="operator-action-hint" className="action-hint" role="status">
								{actionDisabledReason}
							</p>
						)}
						<button
							className="reveal"
							disabled={
								isSubmitting ||
								isPrizeDeliveryPending ||
								(round.status !== "WinnerDetected" && round.status !== "TieBreaker")
							}
							onClick={revealWinner}
						>
							{round.status === "TieBreaker" ? "REALIZAR DESEMPATE" : "REVELAR VENCEDOR"}
						</button>
						<button className="danger" disabled={!canCancel} onClick={() => setPendingConfirmation("cancelRound")}>
							CANCELAR RODADA
						</button>
						{hasWinnerPresentation && (
							<>
								<FeedbackMessage
									success={`O telão está exibindo ${round.winner?.participantName}, vencedor(a) de ${round.winner?.prizeName}.`}
								/>
								{!isPrizeDeliveryPending && (
									<button className="primary" onClick={continueDraw}>
										CONTINUAR SORTEIO NO TELÃO
									</button>
								)}
							</>
						)}
						{isPrizeDeliveryPending && (
							<>
								{!hasWinnerPresentation && (
									<FeedbackMessage warning="O vencedor já foi registrado. Confirme a entrega do prêmio para concluir esta etapa." />
								)}
								<button className="primary" disabled={isSubmitting} onClick={markPrizeDelivered}>
									PRÊMIO ENTREGUE
								</button>
								<button className="danger" disabled={isSubmitting} onClick={() => setPendingConfirmation("declinePrize")}>
									VENCEDOR NÃO RETIROU O PRÊMIO
								</button>
							</>
						)}
						{isFinished && (
							<>
								<FeedbackMessage
									success={
										isCancelled
											? "A rodada foi cancelada sem vencedor."
											: "Cartela cheia concluída. A rodada foi encerrada."
									}
								/>
								<p>
									Prepare a próxima rodada antes de iniciar: as cartelas ativas serão reutilizadas; participantes podem
									gerar novas cartelas e você pode registrar novas impressas.
								</p>
								<button className="primary big" disabled={isSubmitting} onClick={prepareNextRound}>
									PREPARAR PRÓXIMO SORTEIO
								</button>
							</>
						)}
						<p>{round.drawnNumbers.length} pedras sorteadas</p>
						{lastConfirmedAction && (
							<p className="action-hint" role="status" aria-live="polite">
								{lastConfirmedAction}
							</p>
						)}
						{round.status === "Ready" && !hasEligibleCards && (
							<FeedbackMessage warning="Gere e ative ao menos uma cartela antes de iniciar a rodada." />
						)}
						{!isCurrentRound && (
							<FeedbackMessage warning="Esta não é a rodada atual do evento. Volte à configuração e abra a rodada correta." />
						)}
						<FeedbackMessage error={error} onClose={() => setError("")} />
					</section>
				</div>
				{pendingConfirmation === "cancelRound" && (
					<ConfirmationDialog
						title="Cancelar esta rodada?"
						description="A rodada será encerrada sem vencedor e não poderá voltar ao sorteio."
						confirmLabel="Cancelar rodada"
						isConfirming={isSubmitting}
						onCancel={() => setPendingConfirmation(undefined)}
						onConfirm={confirmPendingAction}
					/>
				)}
				{pendingConfirmation === "declinePrize" && (
					<ConfirmationDialog
						title="Confirmar ausência do vencedor?"
						description="O prêmio não será entregue a este vencedor e a etapa continuará procurando outra cartela válida."
						confirmLabel="Confirmar ausência"
						isConfirming={isSubmitting}
						onCancel={() => setPendingConfirmation(undefined)}
						onConfirm={confirmPendingAction}
					/>
				)}
			</main>
		</AppShell>
	);
}
