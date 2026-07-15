import { useCallback, useState } from "react";
import { Link } from "react-router-dom";
import { bingoApi, eventStatusLabel } from "../../features/bingo";
import type { CardMarkingMode } from "../../features/bingo";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

const createdAtFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium", timeStyle: "short" });

export function AdminPage() {
	const [name, setName] = useState("");
	const [mode, setMode] = useState<CardMarkingMode>("Automatic");
	const [cardsPerParticipant, setCardsPerParticipant] = useState(1);
	const [page, setPage] = useState(1);
	const loader = useCallback(() => bingoApi.listEvents(page), [page]);
	const events = useAsyncResource(loader);
	const createEvent = useAsyncAction();

	const submit = async () => {
		const event = await createEvent.execute(
			() => bingoApi.createEvent({ name, markingMode: mode, cardsPerParticipant }),
			"Evento criado. Agora configure inscrições, cartelas e prêmios.",
			"Não foi possível criar o evento. Revise os dados e tente novamente."
		);

		if (!event) return;
		setName("");
		setCardsPerParticipant(1);
		if (page === 1) await events.reload();
		else setPage(1);
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
				<section className="panel event-creation" aria-labelledby="event-creation-title">
					<div className="event-creation-heading">
						<p className="eyebrow">Novo evento</p>
						<h2 id="event-creation-title">Configure o próximo bingo</h2>
						<p>Defina as regras iniciais. Você poderá preparar rodadas, prêmios e inscrições em seguida.</p>
					</div>
					<div className="formrow">
						<label>
							Nome do evento
							<input
								aria-label="Nome do evento"
								placeholder="Festa de Confraternização 2026"
								value={name}
								onChange={(event) => setName(event.target.value)}
							/>
						</label>
						<label>
							Modo de marcação
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
						</label>
						<label>
							Cartelas digitais por participante
							<input
								aria-label="Cartelas digitais por participante"
								type="number"
								min="1"
								max="100"
								value={cardsPerParticipant}
								onChange={(event) => setCardsPerParticipant(Math.min(100, Math.max(1, Number(event.target.value) || 1)))}
							/>
						</label>
						<button className="primary" disabled={!name.trim() || createEvent.isPending} onClick={submit}>
							{createEvent.isPending ? "Criando..." : "Criar evento"}
						</button>
					</div>
					<FeedbackMessage error={createEvent.error} success={createEvent.success} onClose={createEvent.clear} />
				</section>
				<PageState loading={events.loading} error={events.error} onRetry={events.reload} />
				<section className="grid">
					{events.data?.items.map((event) => (
						<article className="event" key={event.id}>
							<span className="badge">{eventStatusLabel(event.status)}</span>
							<h2>{event.name}</h2>
							<div className="event-details">
								<p>
									<span>Código público</span>
									<strong>{event.publicCode}</strong>
								</p>
								<p>
									<span>Criado em</span>
									<time dateTime={event.createdAt}>{createdAtFormatter.format(new Date(event.createdAt))}</time>
								</p>
							</div>
							<div className="actions event-actions">
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
				{events.data && events.data.totalPages > 1 && (
					<nav className="pagination" aria-label="Paginação de eventos">
						<button type="button" onClick={() => setPage(page - 1)} disabled={page === 1}>
							Página anterior
						</button>
						<span aria-live="polite">
							Página {events.data.page} de {events.data.totalPages}
						</span>
						<button type="button" onClick={() => setPage(page + 1)} disabled={page >= events.data.totalPages}>
							Próxima página
						</button>
					</nav>
				)}
			</main>
		</AppShell>
	);
}
