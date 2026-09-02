import { useCallback } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { BingoCardGrid, bingoApi, bingoBallLabel, CardProgress, markingModeLabel, useLiveBingo, winningPatternLabel } from "../../features/bingo";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { ConnectionBadge } from "../../shared/ui/ConnectionBadge";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function ActiveCardsPage() {
	const { eventId = "" } = useParams();
	const [searchParams] = useSearchParams();
	const cardCodesParameter = searchParams.get("codes") ?? "";
	const loader = useCallback(async () => {
		const cardCodes = cardCodesParameter.split(",").filter(Boolean);
		if (cardCodes.length > 0) return Promise.all(cardCodes.map((cardCode) => bingoApi.getCard(eventId, cardCode)));
		const cards = await bingoApi.getMyCards();
		const activeCards = cards.activeCards.filter((card) => card.eventId === eventId);
		return Promise.all(activeCards.map((card) => bingoApi.getCard(eventId, card.publicCode)));
	}, [cardCodesParameter, eventId]);
	const cards = useAsyncResource(loader);
	const primaryCard = cards.data?.[0];
	const connection = useLiveBingo(eventId, primaryCard?.roundId, cards.reload);

	if (!cards.data || !primaryCard)
		return (
			<AppShell showAdministration={false}>
				<PageState loading={cards.loading} error={cards.error || "Nenhuma cartela ativa foi encontrada para este evento."} />
			</AppShell>
		);

	const manual = primaryCard.markingMode !== "Automatic";
	const isMandatoryManual = primaryCard.markingMode === "ManualRequired";
	const isEventFinished = primaryCard.eventStatus === "Finished";
	return (
		<AppShell showAdministration={false}>
			<main className="cardpage active-cardspage">
				<header className="pagehead">
					<div>
						<p className="eyebrow">{isEventFinished ? "Evento encerrado" : primaryCard.currentPrize || "Aguardando rodada"}</p>
						<h1>Minhas cartelas</h1>
						<p className="card-marking-mode">
							Tipo de marcação: <strong>{markingModeLabel(primaryCard.markingMode)}</strong>
						</p>
						{primaryCard.participantName && (
							<div className="card-owner">
								<span>Participante</span>
								<strong>{primaryCard.participantName}</strong>
								{primaryCard.responsibleEmployeeName && (
									<small>Colaborador responsável: {primaryCard.responsibleEmployeeName}</small>
								)}
							</div>
						)}
						{primaryCard.currentPattern && <p>Regra atual: {winningPatternLabel(primaryCard.currentPattern)}</p>}
					</div>
					<ConnectionBadge status={connection} />
				</header>
				{isEventFinished && (
					<FeedbackMessage warning="Este evento foi encerrado. Estas cartelas permanecem disponíveis apenas para consulta." />
				)}
				<section className="active-bingo-cards-grid" aria-label="Cartelas ativas no sorteio">
					{cards.data.map((card) => {
						const currentNumber = card.drawnNumbers.at(-1);
						return (
							<article key={card.publicCode}>
								<p className="active-card-code">Cartela {card.publicCode}</p>
								<CardProgress
									prizeName={card.currentPrize}
									pattern={card.currentPattern}
									remainingNumbersToWin={card.remainingNumbersToWin}
								/>
								<BingoCardGrid
									numbers={card.numbers}
									drawnNumbers={card.drawnNumbers}
									markedNumbers={card.markedNumbers}
									manual={manual}
									markableNumbers={isMandatoryManual && currentNumber ? [currentNumber] : undefined}
									isWinner={card.isWinner}
									onMark={async (number) => {
										await bingoApi.mark(eventId, card.publicCode, number);
										await cards.reload();
									}}
								/>
								{card.isWinner && (
									<p className="winner-card-message" role="status">
										🎉 Parabéns! Esta é a cartela vencedora.
									</p>
								)}
							</article>
						);
					})}
				</section>
				<p className="hint">
					{isMandatoryManual
						? "Marque a pedra atual antes que a próxima seja sorteada."
						: manual
							? "Toque nos números destacados para marcá-los."
							: "Os números sorteados são marcados automaticamente."}
				</p>
				<section className="history">
					<h2>Pedras sorteadas</h2>
					<div>
						{primaryCard.drawnNumbers.map((number) => (
							<span key={number}>{bingoBallLabel(number)}</span>
						))}
					</div>
				</section>
			</main>
		</AppShell>
	);
}
