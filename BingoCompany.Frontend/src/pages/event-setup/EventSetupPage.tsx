import { useCallback, useEffect, useRef, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi, eventStatusLabel, PrizeStageEditor, QrCardScanner, roundStatusLabel } from "../../features/bingo";
import type { ParticipantType, PrizeDraft } from "../../features/bingo";
import { useAsyncAction } from "../../shared/hooks/useAsyncAction";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

const roundCreationDateFormatter = new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" });
const roundOperationPriority: Record<string, number> = {
	Drawing: 0,
	WinnerDetected: 0,
	TieBreaker: 0,
	Ready: 1,
	Finished: 2,
	Cancelled: 2
};

const nextRoundName = (rounds: readonly unknown[]) => `Rodada ${rounds.length + 1}`;

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
	const hasInitializedRoundName = useRef(false);
	const [roundName, setRoundName] = useState("Rodada 1");
	const [stages, setStages] = useState<PrizeDraft[]>([
		{ sequence: 1, prizeName: "Vale-presente", pattern: "HorizontalLine" },
		{ sequence: 2, prizeName: "Prêmio principal", pattern: "FullCard" }
	]);
	const [editingRoundId, setEditingRoundId] = useState<string>();
	const [printedQuantity, setPrintedQuantity] = useState(10);
	const [cardPurchaseQuantity, setCardPurchaseQuantity] = useState(100);
	const [registrationCardsQuantity, setRegistrationCardsQuantity] = useState(1);
	const [cardPurchaseCancellationReason, setCardPurchaseCancellationReason] = useState("");
	const [selectedCard, setSelectedCard] = useState("");
	const [printedParticipantName, setPrintedParticipantName] = useState("");
	const [printedParticipantType, setPrintedParticipantType] = useState<ParticipantType>("Employee");
	const [printedParticipantRegistration, setPrintedParticipantRegistration] = useState("");
	const [printedParticipantResponsible, setPrintedParticipantResponsible] = useState("");
	const [copiedLink, setCopiedLink] = useState("");
	const openRound = (roundId: string) => navigate(`/operacao/${eventId}/${roundId}?code=${code}`);
	const onCardCodeRead = useCallback((cardCode: string) => setSelectedCard(cardCode), []);
	const copyPublicLink = async (path: string, label: string) => {
		try {
			await navigator.clipboard.writeText(new URL(path, window.location.origin).toString());
			setCopiedLink(`${label} copiado.`);
		} catch {
			setCopiedLink("Não foi possível copiar o link. Copie-o pela barra de endereço.");
		}
	};
	const printedCards = event.data?.cardList.filter((card) => card.type === "Printed") ?? [];
	const selectedCardState = printedCards.find((card) => card.publicCode === selectedCard)?.status;
	const isFinished = event.data?.status === "Finished";
	const registrationsOpen = event.data?.status === "RegistrationOpen";
	const isCardPurchaseOpen = event.data?.isCardPurchaseOpen ?? false;
	const isPublicRegistrationOpen =
		event.data?.status === "RegistrationOpen" && !isCardPurchaseOpen && !event.data?.cardPurchaseCancellationReason;
	const cardPurchaseRemaining = event.data?.cardPurchaseRemaining;
	const cardPurchaseLimit = event.data?.cardPurchaseLimit;
	useEffect(() => {
		if (cardPurchaseLimit) setCardPurchaseQuantity(cardPurchaseLimit);
	}, [cardPurchaseLimit]);
	const hasEligibleCard = event.data?.cardList.some((card) => card.status === "Active" && Boolean(card.participantId)) ?? false;
	const canFinishEvent =
		event.data?.status === "Running" &&
		event.data.rounds.length > 0 &&
		event.data.rounds.every((round) => round.status === "Finished" || round.status === "Cancelled");
	const hasFinishedRound = event.data?.rounds.some((round) => round.status === "Finished" || round.status === "Cancelled") ?? false;
	const orderedRounds = [...(event.data?.rounds ?? [])].sort((left, right) => {
		const operationPriority = (roundOperationPriority[left.status] ?? 1) - (roundOperationPriority[right.status] ?? 1);
		return operationPriority || new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime();
	});
	useEffect(() => {
		if (hasInitializedRoundName.current || !event.data || editingRoundId) return;
		setRoundName(nextRoundName(event.data.rounds));
		hasInitializedRoundName.current = true;
	}, [editingRoundId, event.data]);

	const resetRoundForm = () => {
		setEditingRoundId(undefined);
		setRoundName(nextRoundName(event.data?.rounds ?? []));
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
			<main className="event-setup">
				<header className="pagehead event-setup-header">
					<div className="event-setup-header-top">
						<Link className="event-back-link" to="/admin">
							<span aria-hidden="true">←</span> Todos os eventos
						</Link>
						{event.data && <span className="badge event-status-badge">{eventStatusLabel(event.data.status)}</span>}
					</div>
					<div className="event-setup-title">
						<div>
							<p className="eyebrow">Evento {code}</p>
							<h1>{event.data?.name || "Preparação do bingo"}</h1>
							<p className="subtitle">Prepare participantes, prêmios e cartelas antes de iniciar a operação.</p>
						</div>
						{event.data && (
							<div className="event-setup-summary" aria-label="Resumo do evento">
								<div>
									<strong>{event.data.participants}</strong>
									<span>participantes</span>
								</div>
								<div>
									<strong>{event.data.cards}</strong>
									<span>cartelas</span>
								</div>
								<div>
									<strong>{event.data.rounds.length}</strong>
									<span>rodadas</span>
								</div>
							</div>
						)}
					</div>
				</header>
				<PageState loading={event.loading} error={event.error} onRetry={event.reload} />
				{event.data && (
					<>
						<section className="event-access panel" aria-labelledby="event-access-title">
							<div className="event-section-intro">
								<p className="eyebrow">Acessos do evento</p>
								<h2 id="event-access-title">Canais públicos e operação</h2>
								<p>Use estes atalhos para abrir ou compartilhar cada experiência do bingo.</p>
							</div>
							<nav className="access-menu" aria-label="Acessos do evento">
								{!registrationsOpen ? (
									<span className="access-item is-disabled" aria-disabled="true">
										<span aria-hidden="true">↗</span>
										<strong>Inscrição</strong>
										<small>Inscrições encerradas</small>
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
						<div className="two event-workspace">
							<section className="panel participants-panel">
								<div className="participants-panel-header">
									<div>
										<p className="eyebrow">Participantes</p>
										<h2>Inscrições</h2>
									</div>
									<span className="setup-step" aria-hidden="true">
										01
									</span>
								</div>
								<p>
									{registrationsOpen
										? "Abra a página pública de compartilhamento para exibir o QR Code aos participantes."
										: "As inscrições estão fechadas. Elas são encerradas automaticamente quando a primeira rodada é iniciada."}
								</p>
								<div className="participant-metrics">
									<div>
										<span>Participantes</span>
										<strong>{event.data.participants}</strong>
									</div>
									<div className="card-count" aria-label={`${event.data.cards} cartelas geradas para o evento`}>
										<span>Cartelas geradas</span>
										<strong>{event.data.cards}</strong>
									</div>
								</div>
								<div className="participant-actions">
									<section className="participant-action-group" aria-labelledby="public-registration-title">
										<div className="participant-action-heading">
											<span aria-hidden="true">A</span>
											<div>
												<h3 id="public-registration-title">Inscrição pública</h3>
												<p>Compartilhe o acesso e defina quantas cartelas cada pessoa receberá.</p>
											</div>
										</div>
										<div className="participant-link-actions">
											{!registrationsOpen ? (
												<>
													<span className="button is-disabled" aria-disabled="true">
														Abrir QR Code de inscrição
													</span>
													<span className="button is-disabled" aria-disabled="true">
														Visualizar inscrição
													</span>
												</>
											) : (
												<>
													<Link
														className="button"
														to={registrationSharePath}
														target="_blank"
														rel="noopener noreferrer"
													>
														Abrir QR Code
													</Link>
													<button
														type="button"
														onClick={() => void copyPublicLink(registrationSharePath, "Link do QR Code")}
													>
														Copiar link do QR Code
													</button>
													<Link className="button" to={registrationPath}>
														Visualizar inscrição
													</Link>
													<button
														type="button"
														onClick={() => void copyPublicLink(registrationPath, "Link de inscrição")}
													>
														Copiar link de inscrição
													</button>
												</>
											)}
										</div>
										{event.data.status === "Draft" && (
											<div className="participant-action-config">
												<label>
													<span>Cartelas por participante</span>
													<input
														aria-label="Cartelas digitais por participante"
														type="number"
														min="1"
														max="100"
														value={registrationCardsQuantity}
														onChange={(input) =>
															setRegistrationCardsQuantity(
																Math.min(100, Math.max(1, Number(input.target.value) || 1))
															)
														}
													/>
												</label>
												<button
													className="primary"
													disabled={action.isPending}
													onClick={async () => {
														await action.execute(
															() => bingoApi.openRegistration(eventId, registrationCardsQuantity),
															"Inscrições públicas abertas."
														);
														await event.reload();
													}}
												>
													Abrir inscrições públicas
												</button>
											</div>
										)}
										{isPublicRegistrationOpen && (
											<p className="participant-action-status" role="status">
												Inscrições abertas · {event.data.cardsPerParticipant ?? registrationCardsQuantity}{" "}
												cartela(s) por pessoa
											</p>
										)}
									</section>

									<section className="participant-action-group" aria-labelledby="card-purchase-title">
										<div className="participant-action-heading">
											<span aria-hidden="true">B</span>
											<div>
												<h3 id="card-purchase-title">Venda de cartelas</h3>
												<p>Libere um lote digital com quantidade controlada para compra.</p>
											</div>
										</div>
										{event.data.status === "Draft" && (
											<div className="participant-action-config">
												<label>
													<span>Cartelas disponíveis</span>
													<input
														aria-label="Cartelas disponíveis para venda"
														type="number"
														min="1"
														max="10000"
														value={cardPurchaseQuantity}
														onChange={(input) =>
															setCardPurchaseQuantity(
																Math.min(10000, Math.max(1, Number(input.target.value) || 1))
															)
														}
													/>
												</label>
												<button
													className="reveal"
													disabled={isFinished || isCardPurchaseOpen || action.isPending}
													onClick={async () => {
														await action.execute(
															() => bingoApi.openCardPurchase(eventId, cardPurchaseQuantity),
															"Compra de cartelas aberta."
														);
														await event.reload();
													}}
												>
													Abrir compra de cartelas
												</button>
											</div>
										)}
										{isCardPurchaseOpen && (
											<>
												<p className="participant-action-status" role="status">
													{cardPurchaseRemaining ?? 0} cartela(s) disponível(is) para venda
												</p>
												<div className="participant-action-config">
													<label>
														<span>Novo limite</span>
														<input
															aria-label="Limite de cartelas para venda"
															type="number"
															min="1"
															max="10000"
															value={cardPurchaseQuantity}
															onChange={(input) =>
																setCardPurchaseQuantity(
																	Math.min(10000, Math.max(1, Number(input.target.value) || 1))
																)
															}
														/>
													</label>
													<button
														disabled={
															isFinished || action.isPending || cardPurchaseQuantity === cardPurchaseLimit
														}
														onClick={async () => {
															await action.execute(
																() => bingoApi.updateCardPurchase(eventId, cardPurchaseQuantity),
																"Limite de venda atualizado."
															);
															await event.reload();
														}}
													>
														Atualizar quantidade
													</button>
												</div>
												<div className="participant-action-config participant-action-cancel">
													<label>
														<span>Motivo do cancelamento</span>
														<input
															aria-label="Motivo do cancelamento da venda"
															value={cardPurchaseCancellationReason}
															onChange={(input) => setCardPurchaseCancellationReason(input.target.value)}
															placeholder="Ex.: alteração na programação"
														/>
													</label>
													<button
														className="reveal"
														disabled={
															isFinished ||
															action.isPending ||
															cardPurchaseCancellationReason.trim().length < 3
														}
														onClick={async () => {
															await action.execute(
																() => bingoApi.cancelCardPurchase(eventId, cardPurchaseCancellationReason),
																"Venda cancelada e cartelas invalidadas."
															);
															setCardPurchaseCancellationReason("");
															await event.reload();
														}}
													>
														Cancelar venda de cartelas
													</button>
												</div>
											</>
										)}
										{event.data.cardPurchaseCancellationReason && (
											<p className="participant-action-status is-cancelled" role="status">
												Venda cancelada: {event.data.cardPurchaseCancellationReason}
											</p>
										)}
									</section>
								</div>
								{isFinished && (
									<div className="awarded-cards" aria-labelledby="awarded-cards-title">
										<h3 id="awarded-cards-title">Cartelas premiadas</h3>
										{event.data.awardedCards.length > 0 ? (
											<ul>
												{event.data.awardedCards.map((card) => (
													<li key={`${card.publicCode}-${card.roundName}-${card.prizeName}`}>
														<span className="awarded-card-icon" aria-hidden="true">
															★
														</span>
														<div>
															<strong>{card.publicCode}</strong>
															<span>{card.participantName}</span>
															<small>
																{card.roundName} · {card.prizeName}
															</small>
														</div>
													</li>
												))}
											</ul>
										) : (
											<p>Nenhuma cartela foi premiada neste evento.</p>
										)}
									</div>
								)}
							</section>
							<section className="panel round-setup-panel">
								<div className="round-setup-header">
									<div>
										<p className="eyebrow">Configuração da rodada</p>
										<h2>
											{editingRoundId
												? "Editar rodada"
												: hasFinishedRound
													? "Preparar próximo sorteio"
													: "Prêmios e regras"}
										</h2>
									</div>
									<span className="setup-step" aria-hidden="true">
										02
									</span>
								</div>
								{hasFinishedRound && !editingRoundId && (
									<p>
										As cartelas digitais e impressas ativas serão reaproveitadas automaticamente. Antes de iniciar,
										aguarde participantes gerarem novas cartelas ou gere, associe e ative novas cartelas impressas se
										necessário.
									</p>
								)}
								<label className="round-name-field">
									<span>Nome da rodada</span>
									<input
										aria-label="Nome da rodada"
										disabled={isFinished}
										value={roundName}
										onChange={(input) => setRoundName(input.target.value)}
									/>
								</label>
								<PrizeStageEditor stages={stages} onChange={setStages} disabled={isFinished} />
								<div className="actions round-form-actions">
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
						<section className="panel printed-cards-panel">
							<div className="panel-section-header">
								<div>
									<p className="eyebrow">Cartelas físicas</p>
									<h2>Cartelas impressas</h2>
									<p>Gere lotes para impressão e associe cada cartela ao participante responsável.</p>
								</div>
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
							</div>
							<div className="printed-card-generator">
								<label>
									<span>Quantidade</span>
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
							</div>
							{printedCards.length > 0 && (
								<>
									<h3>Registrar e ativar cartela</h3>
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
										Nome do participante
										<input
											aria-label="Nome do participante da cartela impressa"
											disabled={isFinished}
											value={printedParticipantName}
											onChange={(input) => setPrintedParticipantName(input.target.value)}
										/>
									</label>
									<label>
										Tipo de participante
										<select
											aria-label="Tipo do participante da cartela impressa"
											disabled={isFinished}
											value={printedParticipantType}
											onChange={(input) => {
												const value = input.target.value;
												if (value === "Employee" || value === "FamilyMember" || value === "Guest")
													setPrintedParticipantType(value);
											}}
										>
											<option value="Employee">Colaborador</option>
											<option value="FamilyMember">Familiar</option>
											<option value="Guest">Convidado</option>
										</select>
									</label>
									{printedParticipantType === "Employee" ? (
										<label>
											Matrícula <small>(opcional)</small>
											<input
												aria-label="Matrícula do participante da cartela impressa"
												disabled={isFinished}
												value={printedParticipantRegistration}
												onChange={(input) => setPrintedParticipantRegistration(input.target.value)}
											/>
										</label>
									) : (
										<label>
											Colaborador responsável
											<input
												aria-label="Colaborador responsável da cartela impressa"
												disabled={isFinished}
												value={printedParticipantResponsible}
												onChange={(input) => setPrintedParticipantResponsible(input.target.value)}
											/>
										</label>
									)}
									<div className="actions">
										<button
											disabled={
												isFinished ||
												!selectedCard ||
												selectedCardState !== "Printed" ||
												!printedParticipantName.trim() ||
												(printedParticipantType !== "Employee" && !printedParticipantResponsible.trim()) ||
												action.isPending
											}
											onClick={async () => {
												await action.execute(
													() =>
														bingoApi.registerPrintedCard(eventId, selectedCard, {
															name: printedParticipantName,
															type: printedParticipantType,
															employeeRegistration: printedParticipantRegistration || undefined,
															responsibleEmployeeName: printedParticipantResponsible || undefined
														}),
													"Participante registrado e cartela ativada para a rodada."
												);
												setPrintedParticipantName("");
												setPrintedParticipantRegistration("");
												setPrintedParticipantResponsible("");
												await event.reload();
											}}
										>
											Registrar e ativar cartela
										</button>
									</div>
								</>
							)}
						</section>
						{event.data.rounds.length > 0 && (
							<section className="panel event-rounds-panel">
								<div className="panel-section-header">
									<div>
										<p className="eyebrow">Operação</p>
										<h2>Rodadas criadas</h2>
										<p>Acompanhe o estado de cada rodada e retome a operação quando necessário.</p>
									</div>
								</div>
								<div className="round-list">
									{orderedRounds.map((round) => (
										<article key={round.id}>
											<div>
												<strong>{round.name}</strong>
												<span>{roundStatusLabel(round.status)}</span>
												<small>Criada em {roundCreationDateFormatter.format(new Date(round.createdAt))}</small>
											</div>
											<div className="actions">
												{round.status === "Ready" && (
													<button disabled={isFinished} onClick={() => editRound(round)}>
														Editar rodada
													</button>
												)}
												<button
													disabled={
														isFinished ||
														!hasEligibleCard ||
														round.status === "Finished" ||
														round.status === "Cancelled"
													}
													title={
														!hasEligibleCard
															? "Gere e ative ao menos uma cartela antes de abrir a operação."
															: undefined
													}
													onClick={() => openRound(round.id)}
												>
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
						<FeedbackMessage
							error={action.error}
							success={action.success || copiedLink}
							onClose={() => {
								action.clear();
								setCopiedLink("");
							}}
						/>
					</>
				)}
			</main>
		</AppShell>
	);
}
