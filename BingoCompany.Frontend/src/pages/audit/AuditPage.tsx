import { useCallback } from "react";
import { useParams } from "react-router-dom";
import { bingoApi, winningPatternLabel } from "../../features/bingo";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { PageState } from "../../shared/ui/PageState";

export function AuditPage() {
	const { publicCode = "" } = useParams();
	const loader = useCallback(() => bingoApi.getAudit(publicCode), [publicCode]);
	const audit = useAsyncResource(loader);

	return (
		<AppShell>
			<main>
				<PageState loading={audit.loading} error={audit.error} />
				{audit.data && (
					<>
						<p className="eyebrow">Consulta pública verificável</p>
						<h1>Auditoria do evento</h1>
						<section className="panel">
							<h2>{audit.data.eventInfo.name}</h2>
							<p>
								{audit.data.participants.length} participantes · {audit.data.cards.length} cartelas
							</p>
						</section>
						<section className="panel">
							<h2>Rodadas e sequências</h2>
							{audit.data.rounds.map((round) => (
								<article key={round.sequence}>
									<h3>{round.name}</h3>
									<p>Hash SHA-256</p>
									<code className="hash">{round.sequenceHash || "Rodada não iniciada"}</code>
									<p>{round.drawnNumbers.length} pedras sorteadas</p>
									{round.fullSequence && <p>Sequência completa: {round.fullSequence.join(", ")}</p>}
									<ul>
										{round.stages.map((stage) => (
											<li key={stage.prizeName}>
												{stage.prizeName} — {winningPatternLabel(stage.pattern)} {stage.isCompleted ? "✓" : ""}
											</li>
										))}
									</ul>
									{round.winners.map((winner) => (
										<p key={`${winner.participantName}-${winner.prizeName}`}>
											{winner.isWinner ? "Vencedor" : "Empatado"}: {winner.participantName} · {winner.prizeName}
											{winner.tieBreakerNumber ? ` · desempate ${winner.tieBreakerNumber}` : ""}
										</p>
									))}
								</article>
							))}
						</section>
						<section className="panel">
							<h2>Linha do tempo</h2>
							<ol>
								{audit.data.entries.map((entry) => (
									<li key={`${entry.occurredAt}-${entry.action}`}>
										<strong>{entry.action}</strong> — {entry.details}{" "}
										<small>{new Date(entry.occurredAt).toLocaleString("pt-BR")}</small>
									</li>
								))}
							</ol>
						</section>
					</>
				)}
			</main>
		</AppShell>
	);
}
