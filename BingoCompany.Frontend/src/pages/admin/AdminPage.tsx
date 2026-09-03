import { useCallback, useState } from "react";
import type { FormEvent } from "react";
import { Link } from "react-router-dom";
import { bingoApi, eventStatusLabel, markingModeLabel } from "../../features/bingo";
import type { CardMarkingMode } from "../../features/bingo";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

const createdAtFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium", timeStyle: "short" });
const markingModeDescriptions: Record<CardMarkingMode, string> = {
	Automatic: "Os números sorteados são marcados automaticamente nas cartelas.",
	ManualRequired: "O participante deve marcar a pedra atual a cada sorteio.",
	AssistedManual: "O participante pode marcar manualmente qualquer número já sorteado."
};

export function AdminPage() {
	const [name, setName] = useState("");
	const [mode, setMode] = useState<CardMarkingMode>("Automatic");
	const [page, setPage] = useState(1);
	const loader = useCallback(() => bingoApi.listEvents(page), [page]);
	const events = useAsyncResource(loader);
	const createEvent = useAsyncAction();

	const submit = async (formEvent: FormEvent<HTMLFormElement>) => {
		formEvent.preventDefault();
		if (!name.trim() || createEvent.isPending) return;

		const createdEvent = await createEvent.execute(
			() => bingoApi.createEvent({ name, markingMode: mode }),
			"Evento criado. Agora configure inscrições, cartelas e prêmios.",
			"Não foi possível criar o evento. Revise os dados e tente novamente."
		);

		if (!createdEvent) return;
		setName("");
		if (page === 1) await events.reload();
		else setPage(1);
	};

	return (
		<AppShell>
			<main className="admin-page">
				<header className="pagehead admin-pagehead">
					<div>
						<p className="eyebrow">Central de eventos</p>
						<h1>Administração</h1>
						<p className="subtitle">Organize seus eventos, acompanhe a preparação e acesse rapidamente cada operação.</p>
					</div>
					{events.data && (
						<div className="admin-event-count" aria-label={`${events.data.totalItems} eventos cadastrados`}>
							<strong>{events.data.totalItems}</strong>
							<span>{events.data.totalItems === 1 ? "evento cadastrado" : "eventos cadastrados"}</span>
						</div>
					)}
				</header>
				<section className="panel event-creation" aria-labelledby="event-creation-title">
					<div className="event-creation-heading">
						<span className="event-creation-kicker">Novo evento</span>
						<h2 id="event-creation-title">Configure o próximo bingo</h2>
						<p>
							Comece pelo nome e pelo comportamento das cartelas. Rodadas, prêmios e inscrições são configurados na próxima
							etapa.
						</p>
					</div>
					<form className="event-creation-form" onSubmit={submit}>
						<div className="event-creation-fields">
							<label>
								<span>Nome do evento</span>
								<input
									aria-label="Nome do evento"
									autoComplete="off"
									placeholder="Ex.: Festa de Confraternização 2026"
									value={name}
									onChange={(event) => setName(event.target.value)}
								/>
								<small>Escolha um nome fácil de identificar na operação e no telão.</small>
							</label>
							<label>
								<span>Modo de marcação</span>
								<select
									aria-label="Modo de marcação"
									value={mode}
									onChange={(event) => {
										const value = event.target.value;
										if (value === "Automatic" || value === "ManualRequired" || value === "AssistedManual")
											setMode(value);
									}}
								>
									<option value="Automatic">Marcação automática</option>
									<option value="ManualRequired">Manual obrigatória</option>
									<option value="AssistedManual">Manual assistida</option>
								</select>
								<small aria-live="polite">{markingModeDescriptions[mode]}</small>
							</label>
						</div>
						<div className="event-creation-footer">
							<p>Você poderá revisar as demais configurações antes de iniciar o evento.</p>
							<button className="primary" disabled={!name.trim() || createEvent.isPending} type="submit">
								{createEvent.isPending ? "Criando..." : "Criar evento"}
							</button>
						</div>
					</form>
					<FeedbackMessage error={createEvent.error} success={createEvent.success} onClose={createEvent.clear} />
				</section>
				<PageState loading={events.loading} error={events.error} onRetry={events.reload} />
				{events.data && (
					<section className="admin-events" aria-labelledby="admin-events-title">
						<div className="admin-section-heading">
							<div>
								<p className="eyebrow">Visão geral</p>
								<h2 id="admin-events-title">Seus eventos</h2>
							</div>
							<span>{events.data.totalItems} no total</span>
						</div>
						{events.data.items.length === 0 ? (
							<div className="admin-empty-state">
								<strong>Nenhum evento cadastrado</strong>
								<p>Crie o primeiro bingo usando o formulário acima.</p>
							</div>
						) : (
							<div className="event-grid">
								{events.data.items.map((event) => (
									<article className="event" key={event.id}>
										<div className="event-card-header">
											<span className="badge">{eventStatusLabel(event.status)}</span>
											<span className="event-marking-mode">{markingModeLabel(event.markingMode)}</span>
										</div>
										<h2>{event.name}</h2>
										<dl className="event-details">
											<div>
												<dt>Código público</dt>
												<dd className="event-public-code">{event.publicCode}</dd>
											</div>
											<div>
												<dt>Criado em</dt>
												<dd>
													<time dateTime={event.createdAt}>
														{createdAtFormatter.format(new Date(event.createdAt))}
													</time>
												</dd>
											</div>
										</dl>
										<div className="event-actions">
											<Link
												className="button primary event-primary-action"
												to={`/admin/eventos/${event.id}?code=${event.publicCode}`}
											>
												Configurar
											</Link>
											{event.status === "Finished" ? (
												<button
													className="button"
													disabled
													title="O telão não está disponível para eventos finalizados."
													type="button"
												>
													Abrir telão
												</button>
											) : (
												<Link
													className="button"
													to={`/display/${event.publicCode}`}
													target="_blank"
													rel="noopener noreferrer"
												>
													Abrir telão
												</Link>
											)}
										</div>
									</article>
								))}
							</div>
						)}
					</section>
				)}
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
