import { useParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { DrawSuspense } from "../features/bingo/components/DrawSuspense";
import { useLiveBingo } from "../features/bingo/useLiveBingo";
import { winningPatternLabel } from "../features/bingo/winningPatternLabel";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";

export function DisplayPage()
{
    const { publicCode = "" } = useParams();
    const event = useAsyncResource(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
    useLiveBingo(event.data?.id, event.data?.round?.id, event.reload);

    const round = event.data?.round;
    const winner = round?.winner;

    return <main className="display"><p className="eyebrow">{event.data?.name}</p>{winner ? <section className="winnerpresentation"><p>🎉 GANHADOR(A) 🎉</p><h1>{winner.participantName}</h1><p className="winnerprize">{winner.prizeName}</p><p className="winnerpattern">Regra vencedora: {winningPatternLabel(winner.pattern)}</p><p>Aguardando o operador continuar o sorteio.</p></section> : <><p>{round?.name || "Aguardando início"} · {round?.currentPrize || "Aguardando próxima etapa"}</p><DrawSuspense number={round?.drawnNumbers.at(-1)} /><h2>{round?.drawnNumbers.length ?? 0} pedras sorteadas</h2><div className="stages">{round?.stages.map(stage => <span className={stage.isActive ? "active" : stage.isCompleted ? "completed" : ""} key={stage.prizeName}>{stage.prizeName} · {winningPatternLabel(stage.pattern)}</span>)}</div></>}<div className="displayhistory">{round?.drawnNumbers.map(number => <span key={number}>{number}</span>)}</div><footer>{event.data?.cards ?? 0} cartelas · Auditoria {round?.sequenceHash?.slice(0, 16) ?? "aguardando"}</footer></main>;
}
