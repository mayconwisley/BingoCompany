import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { bingoApi } from "../features/bingo/bingoApi";
import { useAsyncResource } from "../shared/hooks/useAsyncResource";
import { AppShell } from "../shared/ui/AppShell";
import { PageState } from "../shared/ui/PageState";

export function JoinPage()
{
    const { publicCode = "" } = useParams();
    const navigate = useNavigate();
    const [name, setName] = useState("");
    const [responsible, setResponsible] = useState("");
    const [error, setError] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const event = useAsyncResource(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
    const eventId = event.data?.id;

    const createCard = async () =>
    {
        try
        {
            setError("");
            setIsSubmitting(true);
            const card = await bingoApi.join(publicCode, name, responsible || undefined);
            localStorage.setItem(`bingo:${eventId}`, card.publicCode);
            navigate(`/cartela/${eventId}/${card.publicCode}`);
        }
        catch
        {
            setError("Não foi possível gerar sua cartela. Tente novamente em alguns instantes.");
        }
        finally
        {
            setIsSubmitting(false);
        }
    };

    return <AppShell><main className="join"><PageState loading={event.loading} error={event.error} />{event.data && <><p className="eyebrow">Você foi convidado</p><h1>{event.data.name}</h1><section className="panel"><label>Seu nome<input value={name} onChange={e => setName(e.target.value)} /></label><label>Colaborador responsável <small>(opcional)</small><input value={responsible} onChange={e => setResponsible(e.target.value)} /></label><button className="primary" disabled={!name.trim() || isSubmitting} onClick={createCard}>{isSubmitting ? "Gerando cartela..." : "Gerar minha cartela"}</button>{error && <p className="error" role="alert">{error}</p>}</section></>}</main></AppShell>;
}
