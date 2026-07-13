import { useState } from "react";
import { Link } from "react-router-dom";
import { bingoApi, eventStatusLabel } from "../../features/bingo";
import type { CardMarkingMode } from "../../features/bingo";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function AdminPage() {
    const [name, setName] = useState("");
    const [mode, setMode] = useState<CardMarkingMode>("Automatic");
    const events = useAsyncResource(() => bingoApi.listEvents(), []);
    const createEvent = useAsyncAction();

    const submit = async () => {
        const event = await createEvent.execute(
            () => bingoApi.createEvent({ name, markingMode: mode, cardsPerParticipant: 1 }),
            "Evento criado. Agora configure inscrições, cartelas e prêmios.",
            "Não foi possível criar o evento. Revise os dados e tente novamente."
        );

        if (!event) return;
        setName("");
        await events.reload();
    };

    return (
        <AppShell>
            <main>
                <header className="pagehead">
                    <div>
                        <p className="eyebrow">Central de eventos</p>
                        <h1>Administração</h1>
                        <p className="subtitle">Crie e acompanhe os eventos de bingo da empresa.</p>
                    </div>
                </header>
                <section className="panel formrow" aria-label="Criar evento">
                    <input
                        aria-label="Nome do evento"
                        placeholder="Festa de Confraternização 2026"
                        value={name}
                        onChange={(event) => setName(event.target.value)}
                    />
                    <select
                        aria-label="Modo de marcação"
                        value={mode}
                        onChange={(event) => {
                            const value = event.target.value;
                            if (value === "Automatic" || value === "ManualRequired" || value === "AssistedManual") setMode(value);
                        }}
                    >
                        <option value="Automatic">Marcação automática</option>
                        <option value="ManualRequired">Manual obrigatória</option>
                        <option value="AssistedManual">Manual assistida</option>
                    </select>
                    <button className="primary" disabled={!name.trim() || createEvent.isPending} onClick={submit}>
                        {createEvent.isPending ? "Criando..." : "Criar evento"}
                    </button>
                    <FeedbackMessage error={createEvent.error} success={createEvent.success} />
                </section>
                <PageState loading={events.loading} error={events.error} onRetry={events.reload} />
                <section className="grid">
                    {events.data?.map((event) => (
                        <article className="event" key={event.id}>
                            <span className="badge">{eventStatusLabel(event.status)}</span>
                            <h2>{event.name}</h2>
                            <p>
                                Código público <strong>{event.publicCode}</strong>
                            </p>
                            <div className="actions">
                                <Link className="button" to={`/admin/eventos/${event.id}?code=${event.publicCode}`}>
                                    Configurar
                                </Link>
                                <Link className="button" to={`/display/${event.publicCode}`} target="_blank" rel="noopener noreferrer">
                                    Abrir telão
                                </Link>
                            </div>
                        </article>
                    ))}
                </section>
            </main>
        </AppShell>
    );
}
