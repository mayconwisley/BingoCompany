import { useState } from "react";
import { useParams, useSearchParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { useLiveBingo } from "../features/bingo/useLiveBingo";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";
import { AppShell } from "../shared/ui/AppShell";
import { PageState } from "../shared/ui/PageState";

export function OperatorPage()
{
    const { eventId = "", roundId = "" } = useParams();
    const [query] = useSearchParams();
    const [error, setError] = useState("");
    const code = query.get("code") ?? "";
    const event = useAsyncResource(() => bingoApi.getPublicEvent(code), [code]);
    useLiveBingo(eventId, roundId, event.reload);

    if (!event.data?.round) return <AppShell><PageState loading={event.loading} error={event.error || "Crie uma rodada antes de abrir a operação."} /></AppShell>;

    const round = event.data.round;
    const isCurrentRound = round.id === roundId;
    const hasWinnerPresentation = Boolean(round.winner);
    const canStart = isCurrentRound && round.status === "Ready";
    const canDraw = isCurrentRound && round.status === "Drawing" && !hasWinnerPresentation;
    const actionLabel = canStart ? "INICIAR RODADA" : "SORTEAR PRÓXIMA PEDRA";

    const performAction = async () =>
    {
        try
        {
            setError("");
            if (canStart) await bingoApi.startRound(eventId, roundId);
            else if (canDraw) await bingoApi.draw(eventId, roundId);
            await event.reload();
        }
        catch { setError("Não foi possível concluir a ação. Atualize o painel para conferir o estado atual da rodada."); }
    };

    const revealWinner = async () =>
    {
        try { setError(""); await bingoApi.reveal(eventId, roundId); await event.reload(); }
        catch { setError("Não foi possível revelar o vencedor."); }
    };

    const continueDraw = async () =>
    {
        try { setError(""); await bingoApi.closeWinnerPresentation(eventId, roundId); await event.reload(); }
        catch { setError("Não foi possível voltar o telão para o sorteio."); }
    };

    return <AppShell><main><header className="pagehead"><div><p className="eyebrow">Painel do operador</p><h1>{round.name}</h1></div></header><div className="operator"><section className="drawball"><small>ÚLTIMA PEDRA</small><strong>{round.drawnNumbers.at(-1) ?? "—"}</strong><p>{round.currentPrize || "Rodada finalizada"}</p></section><section className="panel controls"><button className="primary big" disabled={!canStart && !canDraw} onClick={performAction}>{actionLabel}</button><button className="reveal" disabled={round.status !== "WinnerDetected"} onClick={revealWinner}>REVELAR VENCEDOR</button>{hasWinnerPresentation && <><p className="success">O telão está exibindo {round.winner?.participantName}, vencedor(a) de {round.winner?.prizeName}.</p><button className="primary" onClick={continueDraw}>CONTINUAR SORTEIO NO TELÃO</button></>}<p>{round.drawnNumbers.length} pedras sorteadas</p>{!isCurrentRound && <p className="error">Esta não é a rodada atual do evento. Volte à configuração e abra a rodada correta.</p>}{error && <p className="error" role="alert">{error}</p>}</section></div></main></AppShell>;
}
