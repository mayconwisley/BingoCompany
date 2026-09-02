import { useCallback, useState } from "react";
import { useParams } from "react-router-dom";
import { bingoApi, winningPatternLabel } from "../../features/bingo";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { PageState } from "../../shared/ui/PageState";
import "./auditPage.css";

const auditEntriesPageSize = 25;
const dateTimeFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "medium", timeStyle: "short" });

export function AuditPage() {
	const { publicCode = "" } = useParams();
	const [page, setPage] = useState(1);
	const [selectedRoundSequence, setSelectedRoundSequence] = useState<number>();
	const loader = useCallback(
		() => bingoApi.getAudit(publicCode, page, auditEntriesPageSize, selectedRoundSequence),
		[page, publicCode, selectedRoundSequence]
	);
	const audit = useAsyncResource(loader);

	if (!audit.data)
		return (
			<AppShell>
				<PageState loading={audit.loading} error={audit.error} onRetry={audit.reload} />
			</AppShell>
		);

	const { eventInfo, participants, cards, rounds, entries } = audit.data;
	const drawnNumbers = rounds.reduce((total, round) => total + round.drawnNumbers.length, 0);
	const displayedRounds = selectedRoundSequence ? rounds.filter((round) => round.sequence === selectedRoundSequence) : rounds;
	const selectedRound = rounds.find((round) => round.sequence === selectedRoundSequence);
	const selectRound = (value: string) => {
		setSelectedRoundSequence(value ? Number(value) : undefined);
		setPage(1);
	};

	return (
		<AppShell>
			<main className="audit-page">
				<header className="audit-page-header">
					<div>
						<p className="eyebrow">Consulta pública verificável</p>
						<h1>Auditoria do evento</h1>
						<p>Confira a integridade dos sorteios, prêmios e ações registradas pela organização.</p>
					</div>
					<span className="audit-public-code">Evento {eventInfo.publicCode}</span>
				</header>

				<section className="audit-summary" aria-labelledby="audit-event-title">
					<div>
						<p className="eyebrow">Evento auditado</p>
						<h2 id="audit-event-title">{eventInfo.name}</h2>
						<p>Criado em {dateTimeFormatter.format(new Date(eventInfo.createdAt))}</p>
					</div>
					<dl>
						<div>
							<dt>Participantes</dt>
							<dd>{participants.length}</dd>
						</div>
						<div>
							<dt>Cartelas</dt>
							<dd>{cards.length}</dd>
						</div>
						<div>
							<dt>Pedras sorteadas</dt>
							<dd>{drawnNumbers}</dd>
						</div>
						<div>
							<dt>Rodadas</dt>
							<dd>{rounds.length}</dd>
						</div>
					</dl>
				</section>

				<section className="audit-rounds" aria-labelledby="audit-rounds-title">
					<div className="audit-section-heading">
						<div>
							<p className="eyebrow">Integridade do sorteio</p>
							<h2 id="audit-rounds-title">Rodadas e sequências</h2>
						</div>
						<p>O hash é publicado antes da primeira pedra; a sequência é revelada após o encerramento.</p>
					</div>
					<label className="audit-round-filter">
						<span>Exibir auditoria da rodada</span>
						<select value={selectedRoundSequence ?? ""} onChange={(event) => selectRound(event.target.value)}>
							<option value="">Todas as rodadas</option>
							{rounds.map((round) => (
								<option key={round.sequence} value={round.sequence}>
									Rodada {round.sequence} — {round.name}
								</option>
							))}
						</select>
					</label>
					<div className="audit-round-grid">
						{displayedRounds.map((round) => (
							<article className="audit-round-card" key={round.sequence}>
								<div className="audit-round-card-header">
									<div>
										<span>Rodada {round.sequence}</span>
										<h3>{round.name}</h3>
									</div>
									<strong>{round.drawnNumbers.length} pedras</strong>
								</div>
								<div className="audit-hash">
									<span>Hash SHA-256</span>
									<code>{round.sequenceHash || "Rodada não iniciada"}</code>
								</div>
								<ul className="audit-stages">
									{round.stages.map((stage) => (
										<li className={stage.isCompleted ? "is-completed" : ""} key={stage.prizeName}>
											<span>{stage.prizeName}</span>
											<small>{winningPatternLabel(stage.pattern)}</small>
										</li>
									))}
								</ul>
								{round.winners.length > 0 && (
									<div className="audit-winners">
										{round.winners.map((winner) => (
											<p key={winner.cardCode}>
												<strong>{winner.isWinner ? "Vencedor" : "Empatado"}</strong> {winner.participantName} ·{" "}
												<span className="audit-winner-card-code">Cartela {winner.cardCode}</span> ·{" "}
												{winner.prizeName}
												{winner.tieBreakerNumber ? ` · desempate ${winner.tieBreakerNumber}` : ""}
											</p>
										))}
									</div>
								)}
								{round.fullSequence && (
									<details className="audit-sequence">
										<summary>Ver sequência completa</summary>
										<code>{round.fullSequence.join(", ")}</code>
									</details>
								)}
							</article>
						))}
					</div>
				</section>

				<section className="audit-timeline" aria-labelledby="audit-timeline-title">
					<div className="audit-section-heading">
						<div>
							<p className="eyebrow">Registro de ações</p>
							<h2 id="audit-timeline-title">Linha do tempo</h2>
						</div>
						<p>
							{entries.totalItems} registros{selectedRound ? ` da Rodada ${selectedRound.sequence}` : ""} · exibindo{" "}
							{entries.items.length} nesta página
						</p>
					</div>
					{entries.items.length === 0 ? (
						<p className="audit-empty">
							Nenhuma ação foi registrada {selectedRound ? "para esta rodada" : "para este evento"}.
						</p>
					) : (
						<ol>
							{entries.items.map((entry) => (
								<li key={`${entry.occurredAt}-${entry.action}`}>
									<time dateTime={entry.occurredAt}>{dateTimeFormatter.format(new Date(entry.occurredAt))}</time>
									<div>
										<strong>{entry.action}</strong>
										<p>{entry.details}</p>
									</div>
								</li>
							))}
						</ol>
					)}
					{entries.totalPages > 1 && (
						<nav className="pagination audit-pagination" aria-label="Paginação da linha do tempo">
							<button disabled={entries.page === 1 || audit.loading} onClick={() => setPage((current) => current - 1)}>
								Mais recentes
							</button>
							<span aria-live="polite">
								Página {entries.page} de {entries.totalPages}
							</span>
							<button
								disabled={entries.page >= entries.totalPages || audit.loading}
								onClick={() => setPage((current) => current + 1)}
							>
								Mais antigos
							</button>
						</nav>
					)}
				</section>
			</main>
		</AppShell>
	);
}
