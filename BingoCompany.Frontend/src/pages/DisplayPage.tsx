import { useParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { DrawSuspense } from "../features/bingo/components/DrawSuspense";
import { useLiveBingo } from "../features/bingo/useLiveBingo";
import { winningPatternLabel } from "../features/bingo/winningPatternLabel";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";
import { ThemeToggle } from "../shared/ui/ThemeToggle";
import "./displayPage.css";

export function DisplayPage()
{
    const { publicCode = "" } = useParams();
    const event = useAsyncResource(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
    useLiveBingo(event.data?.id, event.data?.round?.id, event.reload);

    const round = event.data?.round;
    const winner = round?.winner;
    const winnerDetectedCount = round?.winnerDetectedCount ?? 0;
    const isWinnerSuspense = !winner && winnerDetectedCount > 0;
    const prizeImage = winner?.prizeImageDataUrl ?? (isWinnerSuspense ? round?.presentationPrizeImageDataUrl : round?.currentPrizeImageDataUrl);

    return <main className="display"><ThemeToggle className="display-theme-toggle" /><p className="eyebrow">{event.data?.name}</p>{winner ? <section className="winnerpresentation"><p>🎉 GANHADOR(A) 🎉</p><h1>{winner.participantName}</h1><p className="winnerprize">{winner.prizeName}</p>{prizeImage && <img className="display-prize-image" src={prizeImage} alt={`Prêmio: ${winner.prizeName}`} />}<p className="winnerpattern">Regra vencedora: {winningPatternLabel(winner.pattern)}</p><p>Aguardando o operador continuar o sorteio.</p></section> : isWinnerSuspense ? <section className="winnerpresentation" aria-live="polite"><p>{round?.tieBreakerRequired ? "⚖️ DESEMPATE" : "🎉 TEMOS UM VENCEDOR"}</p><h1>{round?.tieBreakerRequired ? "Preparando o desempate" : "Confira a cartela!"}</h1><p className="winnerprize">{round?.currentPrize}</p>{prizeImage && <img className="display-prize-image" src={prizeImage} alt={`Prêmio: ${round?.currentPrize}`} />}<p className="winnerpattern">{round?.tieBreakerRequired ? `${winnerDetectedCount} cartelas empataram` : "O nome será revelado pelo operador."}</p><p>O sorteio está pausado para a apresentação do resultado.</p></section> : <><p>{round?.name || "Aguardando início"} · {round?.currentPrize || "Aguardando próxima etapa"}</p>{prizeImage && <img className="display-prize-image display-prize-image--active" src={prizeImage} alt={`Prêmio em disputa: ${round?.currentPrize}`} />}<DrawSuspense number={round?.drawnNumbers.at(-1)} /><h2>{round?.drawnNumbers.length ?? 0} pedras sorteadas</h2><div className="stages">{round?.stages.map(stage => <span className={stage.isActive ? "active" : stage.isCompleted ? "completed" : ""} key={stage.prizeName}>{stage.prizeName} · {winningPatternLabel(stage.pattern)}</span>)}</div>{round?.statistics && <section className="displaystats" aria-label="Estatísticas do sorteio"><span>{round.statistics.totalCards} cartelas</span><span>{round.statistics.oneNumberAway} a 1 pedra</span><span>{round.statistics.twoNumbersAway} a 2 pedras</span><span>{round.statistics.threeNumbersAway} a 3 pedras</span></section>}</>}<div className="displayhistory">{round?.drawnNumbers.map(number => <span key={number}>{number}</span>)}</div><footer>{event.data?.cards ?? 0} cartelas · Auditoria {round?.sequenceHash?.slice(0, 16) ?? "aguardando"}</footer></main>;
}
