import { useCallback, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { BingoCardGrid, bingoApi, bingoBallLabel, CardProgress, markingModeLabel, useLiveBingo, winningPatternLabel } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { ConnectionBadge } from "../../shared/ui/ConnectionBadge";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function CardPage() {
	const { eventId = "", cardCode = "" } = useParams();
	const navigate = useNavigate();
	const card = useAsyncResource(useCallback(() => bingoApi.getCard(eventId, cardCode), [eventId, cardCode]));
	const connection = useLiveBingo(eventId, card.data?.roundId, card.reload);
	const [isGeneratingNextCard, setIsGeneratingNextCard] = useState(false);
	const [nextCardError, setNextCardError] = useState("");

	if (!card.data)
		return (
			<AppShell showAdministration={false}>
				<PageState loading={card.loading} error={card.error} />
			</AppShell>
		);

	const manual = card.data.markingMode !== "Automatic";
	const isMandatoryManual = card.data.markingMode === "ManualRequired";
	const isEventFinished = card.data.eventStatus === "Finished";
	const currentNumber = card.data.drawnNumbers.at(-1);
	const currentBall = currentNumber === undefined ? undefined : bingoBallLabel(currentNumber);
	const generateNextCard = async () => {
		try {
			setNextCardError("");
			setIsGeneratingNextCard(true);
			const nextCard = await bingoApi.generateNextCard(eventId, cardCode);
			navigate(`/cartela/${eventId}/${nextCard.publicCode}`);
		} catch (error) {
			setNextCardError(getErrorMessage(error, "Não foi possível gerar a nova cartela. Atualize a página e tente novamente."));
		} finally {
			setIsGeneratingNextCard(false);
		}
	};

	return (
		<AppShell showAdministration={false}>
			<main className="cardpage">
				<header className="pagehead">
					<div>
						<p className="eyebrow">{isEventFinished ? "Evento encerrado" : card.data.currentPrize || "Aguardando rodada"}</p>
						<h1>Minha cartela</h1>
						<p className="card-marking-mode">
							Tipo de marcação: <strong>{markingModeLabel(card.data.markingMode)}</strong>
						</p>
						{card.data.participantName && (
							<div className="card-owner">
								<span>Participante</span>
								<strong>{card.data.participantName}</strong>
								{card.data.responsibleEmployeeName && (
									<small>Colaborador responsável: {card.data.responsibleEmployeeName}</small>
								)}
							</div>
						)}
						{card.data.currentPattern && <p>Regra atual: {winningPatternLabel(card.data.currentPattern)}</p>}
					</div>
					<ConnectionBadge status={connection} />
				</header>
				{isEventFinished && (
					<FeedbackMessage warning="Este evento foi encerrado. Esta cartela permanece disponível apenas para consulta." />
				)}
				{manual && (
					<section className="current-draw" role="status" aria-live="polite" aria-atomic="true">
						<span>{isMandatoryManual ? "MARQUE AGORA" : "ÚLTIMA PEDRA SORTEADA"}</span>
						<strong>{currentBall ?? "Aguardando a primeira pedra"}</strong>
						<p>
							{isMandatoryManual
								? "Toque nesta pedra na sua cartela antes do próximo sorteio."
								: "Os números sorteados ficam destacados e podem ser marcados na cartela."}
						</p>
					</section>
				)}
				<CardProgress
					prizeName={card.data.currentPrize}
					pattern={card.data.currentPattern}
					remainingNumbersToWin={card.data.remainingNumbersToWin}
				/>
				<BingoCardGrid
					numbers={card.data.numbers}
					drawnNumbers={card.data.drawnNumbers}
					markedNumbers={card.data.markedNumbers}
					manual={manual}
					markableNumbers={isMandatoryManual && currentNumber ? [currentNumber] : undefined}
					isWinner={card.data.isWinner}
					onMark={async (number) => {
						await bingoApi.mark(eventId, cardCode, number);
						await card.reload();
					}}
				/>
				{card.data.isWinner && (
					<p className="winner-card-message" role="status">
						🎉 Parabéns! Esta é a cartela vencedora.
					</p>
				)}
				<p className="hint">
					{isMandatoryManual
						? "Marque a pedra atual antes que a próxima seja sorteada."
						: manual
							? "Toque nos números destacados para marcá-los."
							: "Os números sorteados são marcados automaticamente."}
				</p>
				{card.data.canGenerateNextCard && (
					<section className="panel">
						<h2>Nova rodada</h2>
						<p>Esta cartela participou da rodada concluída. Gere novos números para a próxima rodada.</p>
						<button className="primary" onClick={generateNextCard} disabled={isGeneratingNextCard}>
							{isGeneratingNextCard ? "Gerando nova cartela..." : "Gerar nova cartela"}
						</button>
						<FeedbackMessage error={nextCardError} onClose={() => setNextCardError("")} />
					</section>
				)}
				<section className="history">
					<h2>Pedras sorteadas</h2>
					<div>
						{card.data.drawnNumbers.map((number) => (
							<span key={number}>{bingoBallLabel(number)}</span>
						))}
					</div>
				</section>
			</main>
		</AppShell>
	);
}
