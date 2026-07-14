import { useCallback, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi, PrizeStageEditor, QrCardScanner, roundStatusLabel } from "../../features/bingo";
import type { PrizeDraft } from "../../features/bingo";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function EventSetupPage() {
	const { eventId = "" } = useParams();
	const [query] = useSearchParams();
	const navigate = useNavigate();
	const code = query.get("code") ?? "";
	const registrationPath = `/participar/${code}`;
	const registrationSharePath = `/inscricao/${code}`;
	const displayPath = `/display/${code}`;
	const auditPath = `/auditoria/${code}`;
	const printCardsPath = `/admin/eventos/${eventId}/impressao`;
	const loader = useCallback(() => bingoApi.getEvent(eventId), [eventId]);
	const event = useAsyncResource(loader);
	const action = useAsyncAction();
	const [roundName, setRoundName] = useState("Rodada 1");
	const [stages, setStages] = useState<PrizeDraft[]>([
		{ sequence: 1, prizeName: "Vale-presente", pattern: "HorizontalLine" },
		{ sequence: 2, prizeName: "Prêmio principal", pattern: "FullCard" }
	]);
	const [editingRoundId, setEditingRoundId] = useState<string>();
	const [printedQuantity, setPrintedQuantity] = useState(10);
	const [selectedCard, setSelectedCard] = useState("");
	const [selectedParticipant, setSelectedParticipant] = useState("");
	const openRound = (roundId: string) => navigate(`/operacao/${eventId}/${roundId}?code=${code}`);
	const onCardCodeRead = useCallback((cardCode: string) => setSelectedCard(cardCode), []);
	const printedCards = event.data?.cardList.filter((card) => card.type === "Printed") ?? [];
	const selectedCardState = printedCards.find((card) => card.publicCode === selectedCard)?.status;
	const isFinished = event.data?.status === "Finished";
	const canFinishEvent =
		event.data?.status === "Running" && event.data.rounds.length > 0 && event.data.rounds.every((round) => round.status === "Finished");
	const hasFinishedRound = event.data?.rounds.some((round) => round.status === "Finished") ?? false;

	const resetRoundForm = () => {
		setEditingRoundId(undefined);
		setRoundName("Rodada 1");
		setStages([
			{ sequence: 1, prizeName: "Vale-presente", pattern: "HorizontalLine" },
			{ sequence: 2, prizeName: "Prêmio principal", pattern: "FullCard" }
		]);
	};
	const saveRound = async () => {
		if (editingRoundId) {
			const updated = await action.execute(
				async () => {
					await bingoApi.updateRound(eventId, editingRoundId, roundName, stages);
					return true;
				},
				"Rodada atualizada com sucesso.",
				"Não foi possível atualizar a rodada. Revise os prêmios configurados e tente novamente."
			);
			if (!updated) return;
			resetRoundForm();
			await event.reload();
			return;
		}

		const round = await action.execute(
			() => bingoApi.createRound(eventId, roundName, stages),
			"Rodada criada com sucesso.",
			"Não foi possível criar a rodada. Revise os prêmios configurados e tente novamente."
		);
		if (!round) return;
		await event.reload();
		openRound(round.id);
	};
	const editRound = (round: NonNullable<typeof event.data>["rounds"][number]) => {
		setEditingRoundId(round.id);
		setRoundName(round.name);
		setStages(
			round.stages.map((stage) => ({
				sequence: stage.sequence,
				prizeName: stage.prizeName,
				pattern: stage.pattern,
				prizeImageDataUrl: stage.prizeImageDataUrl
			}))
		);
	};

	return (
		<AppShell>
			<main>
				<header className="pagehead">
					<div>
						<p className="eyebrow">Evento {code}</p>
						<h1>{event.data?.name || "Preparação do bingo"}</h1>
						<p className="subtitle">Gerencie a operação e compartilhe os acessos públicos do evento.</p>
					</div>
				</header>
				<PageState loading={event.loading} error={event.error} onRetry={event.reload} />
				{event.data && (
					<>
						<section className="event-access panel" aria-labelledby="event-access-title">
							<div>
								<p className="eyebrow">Acessos do evento</p>
								<h2 id="event-access-title">Canais públicos e operação</h2>
								<p>Use estes atalhos para abrir ou compartilhar cada experiência do bingo.</p>
							</div>
							<nav className="access-menu" aria-label="Acessos do evento">
								{isFinished ? (
									<span className="access-item is-disabled" aria-disabled="true">
										<span aria-hidden="true">↗</span>
										<strong>Inscrição</strong>
										<small>Participantes entram no bingo</small>
									</span>
								) : (
									<Link className="access-item" to={registrationPath}>
										<span aria-hidden="true">↗</span>
										<strong>Inscrição</strong>
										<small>Participantes entram no bingo</small>
									</Link>
								)}
								{isFinished ? (
									<span className="access-item is-disabled" aria-disabled="true">
										<span aria-hidden="true">▣</span>
										<strong>Telão</strong>
										<small>Exibição pública do sorteio</small>
									</span>
								) : (
									<Link className="access-item" to={displayPath} target="_blank" rel="noopener noreferrer">
										<span aria-hidden="true">▣</span>
										<strong>Telão</strong>
										<small>Exibição pública do sorteio</small>
									</Link>
								)}
								<Link className="access-item" to={auditPath}>
									<span aria-hidden="true">✓</span>
									<strong>Auditoria</strong>
									<small>Dados e histórico verificáveis</small>
								</Link>
								{isFinished ? (
									<span className="access-item is-disabled" aria-disabled="true">
										<span aria-hidden="true">▤</span>
										<strong>Cartelas</strong>
										<small>Gerar e imprimir cartelas físicas</small>
									</span>
								) : (
									<Link className="access-item" to={printCardsPath}>
										<span aria-hidden="true">▤</span>
										<strong>Cartelas</strong>
										<small>Gerar e imprimir cartelas físicas</small>
									</Link>
								)}
							</nav>
						</section>
						<div className="two">
							<section className="panel">
								<p className="eyebrow">Participantes</p>
								<h2>Inscrições</h2>
								<p>Abra a página pública de compartilhamento para exibir o QR Code aos participantes.</p>
								<div className="actions">
									{isFinished ? (
										<span className="button is-disabled" aria-disabled="true">
											Abrir QR Code de inscrição
										</span>
									) : (
										<Link className="button" to={registrationSharePath} target="_blank" rel="noopener noreferrer">
											Abrir QR Code de inscrição
										</Link>
									)}
									{isFinished ? (
										<span className="button is-disabled" aria-disabled="true">
											Visualizar inscrição
										</span>
									) : (
										<Link className="button" to={registrationPath}>
											Visualizar inscrição
										</Link>
									)}
									<button
										className="primary"
										disabled={isFinished || event.data.status !== "Draft" || action.isPending}
										onClick={async () => {
											await action.execute(() => bingoApi.openRegistration(eventId), "Inscrições abertas.");
											await event.reload();
										}}
									>
										Abrir inscrições
									</button>
								</div>
							</section>
							<section className="panel">
								<p className="eyebrow">Configuração da rodada</p>
								<h2>
									{editingRoundId ? "Editar rodada" : hasFinishedRound ? "Preparar próximo sorteio" : "Prêmios e regras"}
								</h2>
								{hasFinishedRound && !editingRoundId && (
									<p>
										As cartelas digitais e impressas ativas serão reaproveitadas automaticamente. Antes de iniciar,
										aguarde participantes gerarem novas cartelas ou gere, associe e ative novas cartelas impressas se
										necessário.
									</p>
								)}
								<label>
									Nome da rodada
									<input
										aria-label="Nome da rodada"
										disabled={isFinished}
										value={roundName}
										onChange={(input) => setRoundName(input.target.value)}
									/>
								</label>
								<PrizeStageEditor stages={stages} onChange={setStages} disabled={isFinished} />
								<div className="actions">
									<button className="primary" disabled={isFinished || action.isPending} onClick={saveRound}>
										{action.isPending
											? "Salvando..."
											: editingRoundId
												? "Salvar alterações"
												: "Salvar e ir para operação"}
									</button>
									{editingRoundId && (
										<button type="button" disabled={isFinished || action.isPending} onClick={resetRoundForm}>
											Cancelar edição
										</button>
									)}
								</div>
							</section>
						</div>
						<section className="panel">
							<p className="eyebrow">Cartelas físicas</p>
							<h2>Cartelas impressas</h2>
							<div className="actions">
								{isFinished ? (
									<span className="button is-disabled" aria-disabled="true">
										Abrir para impressão
									</span>
								) : (
									<Link className="button" to={printCardsPath}>
										Abrir para impressão
									</Link>
								)}
							</div>
							<label>
								Quantidade
								<input
									aria-label="Quantidade de cartelas impressas"
									disabled={isFinished}
									type="number"
									min="1"
									max="1000"
									value={printedQuantity}
									onChange={(input) => setPrintedQuantity(Number(input.target.value))}
								/>
							</label>
							<button
								className="primary"
								disabled={isFinished || action.isPending}
								onClick={async () => {
									const cards = await action.execute(
										() => bingoApi.generatePrintedCards(eventId, printedQuantity),
										"Cartelas impressas geradas."
									);
									if (cards) await event.reload();
								}}
							>
								Gerar lote para impressão
							</button>
							{printedCards.length > 0 && (
								<>
									<h3>Associar e ativar cartela</h3>
									<QrCardScanner onCardCodeRead={onCardCodeRead} disabled={isFinished} />
									<label>
										Cartela
										<select
											aria-label="Cartela impressa"
											disabled={isFinished}
											value={selectedCard}
											onChange={(input) => setSelectedCard(input.target.value)}
										>
											<option value="">Selecione</option>
											{printedCards.map((card) => (
												<option key={card.publicCode} value={card.publicCode}>
													{card.publicCode} · {card.status}
												</option>
											))}
										</select>
									</label>
									<label>
										Participante
										<select
											aria-label="Participante"
											disabled={isFinished}
											value={selectedParticipant}
											onChange={(input) => setSelectedParticipant(input.target.value)}
										>
											<option value="">Selecione</option>
											{event.data.participantList.map((participant) => (
												<option key={participant.id} value={participant.id}>
													{participant.name} · {participant.type}
												</option>
											))}
										</select>
									</label>
									<div className="actions">
										<button
											disabled={
												isFinished ||
												!selectedCard ||
												!selectedParticipant ||
												selectedCardState !== "Printed" ||
												action.isPending
											}
											onClick={async () => {
												await action.execute(
													() => bingoApi.assignPrintedCard(eventId, selectedCard, selectedParticipant),
													"Cartela associada. Ative-a para participar."
												);
												await event.reload();
											}}
										>
											Associar cartela
										</button>
										<button
											className="reveal"
											disabled={isFinished || !selectedCard || selectedCardState !== "Assigned" || action.isPending}
											onClick={async () => {
												await action.execute(
													() => bingoApi.activatePrintedCard(eventId, selectedCard),
													"Cartela ativada para a próxima rodada."
												);
												await event.reload();
											}}
										>
											Ativar cartela
										</button>
									</div>
								</>
							)}
						</section>
						{event.data.rounds.length > 0 && (
							<section className="panel">
								<p className="eyebrow">Operação</p>
								<h2>Rodadas criadas</h2>
								<div className="round-list">
									{event.data.rounds.map((round) => (
										<article key={round.id}>
											<div>
												<strong>{round.name}</strong>
												<span>{roundStatusLabel(round.status)}</span>
											</div>
											<div className="actions">
												{round.status === "Ready" && (
													<button disabled={isFinished} onClick={() => editRound(round)}>
														Editar rodada
													</button>
												)}
												<button disabled={isFinished} onClick={() => openRound(round.id)}>
													Abrir operação
												</button>
											</div>
										</article>
									))}
								</div>
								<button
									className="reveal"
									disabled={!canFinishEvent || action.isPending}
									onClick={async () => {
										await action.execute(
											() => bingoApi.finishEvent(eventId),
											"Evento encerrado e auditoria publicada."
										);
										await event.reload();
									}}
								>
									Encerrar evento e publicar auditoria
								</button>
							</section>
						)}
						<FeedbackMessage error={action.error} success={action.success} onClose={action.clear} />
					</>
				)}
			</main>
		</AppShell>
	);
}
