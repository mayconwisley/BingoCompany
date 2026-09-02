import { useCallback, useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { bingoApi, bingoBallLabel, DrawSuspense, TieBreakerStone, useLiveBingo, winningPatternLabel } from "../../features/bingo";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { ThemeToggle } from "../../shared/ui/ThemeToggle";
import "./displayPage.css";

const winnerSuspenseDelayInMilliseconds = 3000;

export function DisplayPage() {
	const { publicCode = "" } = useParams();
	const loader = useCallback(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
	const event = useAsyncResource(loader);
	useLiveBingo(event.data?.id, event.data?.round?.id, event.reload);

	const round = event.data?.round;
	const winner = round?.winner;
	const winnerDetectedCount = round?.winnerDetectedCount ?? 0;
	const lastDrawnNumber = round?.drawnNumbers.at(-1);
	const hasPendingWinner = !winner && winnerDetectedCount > 0;
	const [isWinnerSuspenseVisible, setIsWinnerSuspenseVisible] = useState(false);

	useEffect(() => {
		if (!hasPendingWinner) {
			setIsWinnerSuspenseVisible(false);
			return;
		}

		setIsWinnerSuspenseVisible(false);
		const timeout = window.setTimeout(() => setIsWinnerSuspenseVisible(true), winnerSuspenseDelayInMilliseconds);

		return () => window.clearTimeout(timeout);
	}, [hasPendingWinner, round?.id, lastDrawnNumber]);

	const isWinnerSuspense = hasPendingWinner && isWinnerSuspenseVisible;
	const prizeImage =
		winner?.prizeImageDataUrl ?? (isWinnerSuspense ? round?.presentationPrizeImageDataUrl : round?.currentPrizeImageDataUrl);
	const shouldHighlightPrizeName = !prizeImage && Boolean(round?.currentPrize);

	return (
		<main className="display">
			<ThemeToggle className="display-theme-toggle" />
			<p className="eyebrow">{event.data?.name}</p>
			{winner ? (
				<section className="winnerpresentation">
					<p>🎉 GANHADOR(A) 🎉</p>
					<h1>{winner.participantName}</h1>
					<p className="winnerprize">{winner.prizeName}</p>
					{prizeImage && <img className="display-prize-image" src={prizeImage} alt={`Prêmio: ${winner.prizeName}`} />}
					<p className="winnerpattern">Regra vencedora: {winningPatternLabel(winner.pattern)}</p>
					{winner.tieBreakers && winner.tieBreakers.length > 0 && (
						<section className="tie-breaker-results" aria-label="Resultado do desempate">
							<h2>Resultado do desempate</h2>
							<p>Pedras sorteadas para cada participante</p>
							<ul>
								{winner.tieBreakers.map((tieBreaker, index) => (
									<li className={tieBreaker.isWinner ? "winner" : ""} key={tieBreaker.cardCode}>
										<span>{tieBreaker.participantName}</span>
										<TieBreakerStone number={tieBreaker.number} delay={index * 1200} />
										<small>Cartela {tieBreaker.cardCode}</small>
										{tieBreaker.isWinner && <em>Vencedor(a)</em>}
									</li>
								))}
							</ul>
						</section>
					)}
					<p>Aguardando o operador continuar o sorteio.</p>
				</section>
			) : isWinnerSuspense ? (
				<section className="winnerpresentation" aria-live="polite">
					<p>{round?.tieBreakerRequired ? "⚖️ DESEMPATE" : "🎉 TEMOS UM VENCEDOR"}</p>
					<h1>{round?.tieBreakerRequired ? "Confira as cartelas" : "Confira a cartela!"}</h1>
					<p className="winning-stone-label">PEDRA VENCEDORA</p>
					<strong className="winning-stone">{lastDrawnNumber === undefined ? "—" : bingoBallLabel(lastDrawnNumber)}</strong>
					<p className="winnerprize">{round?.currentPrize}</p>
					{prizeImage && <img className="display-prize-image" src={prizeImage} alt={`Prêmio: ${round?.currentPrize}`} />}
					<p className="winnerpattern">
						{round?.tieBreakerRequired ? `${winnerDetectedCount} cartelas empataram` : "O nome será revelado pelo operador."}
					</p>
					<p>O sorteio está pausado para a apresentação do resultado.</p>
				</section>
			) : (
				<>
					<section className="display-draw-layout">
						<div className={shouldHighlightPrizeName ? "display-prize display-prize--without-image" : "display-prize"}>
							{shouldHighlightPrizeName ? (
								<>
									<p className="display-prize-round">{round?.name}</p>
									<p className="display-prize-label">Prêmio em disputa</p>
									<h1>{round?.currentPrize}</h1>
								</>
							) : (
								<p>
									{round?.name || "Aguardando início"} · {round?.currentPrize || "Aguardando próxima etapa"}
								</p>
							)}
							{prizeImage && (
								<img
									className="display-prize-image display-prize-image--active"
									src={prizeImage}
									alt={`Prêmio em disputa: ${round?.currentPrize}`}
								/>
							)}
						</div>
						<DrawSuspense number={round?.drawnNumbers.at(-1)} />
					</section>
					<h2>{round?.drawnNumbers.length ?? 0} pedras sorteadas</h2>
					<div className="stages">
						{round?.stages.map((stage) => (
							<span className={stage.isActive ? "active" : stage.isCompleted ? "completed" : ""} key={stage.prizeName}>
								{stage.prizeName} · {winningPatternLabel(stage.pattern)}
							</span>
						))}
					</div>
					{round?.statistics && (
						<section className="displaystats" aria-label="Estatísticas do sorteio">
							<span>{round.statistics.totalCards} cartelas na rodada</span>
							<span>{round.statistics.oneNumberAway} cartelas precisam de 1 pedra para ganhar</span>
							<span>{round.statistics.twoNumbersAway} cartelas precisam de 2 pedras para ganhar</span>
							<span>{round.statistics.threeNumbersAway} cartelas precisam de 3 pedras para ganhar</span>
						</section>
					)}
				</>
			)}
			<div className="displayhistory">
				{round?.drawnNumbers.map((number) => (
					<span key={number}>{bingoBallLabel(number)}</span>
				))}
			</div>
			<footer>
				{event.data?.cards ?? 0} cartelas · Auditoria {round?.sequenceHash?.slice(0, 16) ?? "aguardando"}
			</footer>
		</main>
	);
}
