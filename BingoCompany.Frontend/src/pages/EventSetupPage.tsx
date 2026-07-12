import { useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { PrizeStageEditor } from "../features/bingo/components/PrizeStageEditor";
import type { PrizeDraft } from "../features/bingo/types";
import { roundStatusLabel } from "../features/bingo/winningPatternLabel";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";
import { AppShell } from "../shared/ui/AppShell";
import { PageState } from "../shared/ui/PageState";

export function EventSetupPage()
{
    const { eventId = "" } = useParams();
    const [query] = useSearchParams();
    const navigate = useNavigate();
    const code = query.get("code") ?? "";
    const event = useAsyncResource(() => bingoApi.getEvent(eventId), [eventId]);
    const [roundName, setRoundName] = useState("Rodada 1");
    const [stages, setStages] = useState<PrizeDraft[]>([{ sequence: 1, prizeName: "Vale-presente", pattern: "HorizontalLine" }, { sequence: 2, prizeName: "Prêmio principal", pattern: "FullCard" }]);
    const [message, setMessage] = useState("");
    const openRound = (roundId: string) => navigate(`/operacao/${eventId}/${roundId}?code=${code}`);

    return <AppShell><main><p className="eyebrow">Evento {code}</p><h1>Preparação do bingo</h1><PageState loading={event.loading} error={event.error} /><div className="two"><section className="panel"><h2>Inscrições</h2><p>Compartilhe <code>/participar/{code}</code> com os convidados.</p><button className="primary" disabled={event.data?.status !== "Draft"} onClick={async () => { await bingoApi.openRegistration(eventId); setMessage("Inscrições abertas"); await event.reload(); }}>Abrir inscrições</button></section><section className="panel"><h2>Nova rodada e prêmios</h2><input aria-label="Nome da rodada" value={roundName} onChange={event => setRoundName(event.target.value)} /><PrizeStageEditor stages={stages} onChange={setStages} /><button className="primary" onClick={async () => { const round = await bingoApi.createRound(eventId, roundName, stages); setMessage("Rodada criada"); await event.reload(); openRound(round.id); }}>Salvar e ir para operação</button></section></div>{event.data?.rounds.length ? <section className="panel"><h2>Rodadas criadas</h2>{event.data.rounds.map(round => <p key={round.id}>{round.name} · {roundStatusLabel(round.status)} <button onClick={() => openRound(round.id)}>Abrir operação</button></p>)}</section> : null}<p className="success" role="status">{message}</p></main></AppShell>;
}
