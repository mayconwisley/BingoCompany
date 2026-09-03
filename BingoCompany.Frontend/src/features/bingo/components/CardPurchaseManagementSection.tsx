import { useEffect, useState } from "react";
import { bingoApi } from "../api/bingoApi";
import { RegistrationQrCode } from "./RegistrationQrCode";
import type { EventDetails } from "../model/types";
import { useAsyncAction } from "../../../shared/hooks/useAsyncAction";
import { FeedbackMessage } from "../../../shared/ui/FeedbackMessage";

type Props = {
	eventId: string;
	event: EventDetails;
	registrationPath: string;
	isFinished: boolean;
	copyPublicLink: (path: string, label: string) => Promise<void>;
	onReload: () => Promise<unknown>;
};

const toInputDateTime = (value?: string | null) => (value ? value.slice(0, 16) : "");

export function CardPurchaseManagementSection({ eventId, event, registrationPath, isFinished, copyPublicLink, onReload }: Props) {
	const action = useAsyncAction();
	const [quantity, setQuantity] = useState(event.cardPurchaseLimit ?? 100);
	const [perParticipantLimit, setPerParticipantLimit] = useState(event.cardPurchasePerParticipantLimit ?? 5);
	const [closesAt, setClosesAt] = useState(toInputDateTime(event.cardPurchaseClosesAt));
	const [lowStockThreshold, setLowStockThreshold] = useState(event.cardPurchaseLowStockThreshold ?? 10);
	const [bonusCards, setBonusCards] = useState(1);
	const [cancellationReason, setCancellationReason] = useState("");
	const [latestInvitation, setLatestInvitation] = useState<{ code: string; bonusCards: number; expiresAt?: string }>();
	const isOpen = event.isCardPurchaseOpen ?? false;
	const settings = () => ({
		quantity,
		perParticipantLimit,
		closesAt: closesAt ? new Date(closesAt).toISOString() : undefined,
		lowStockThreshold
	});

	useEffect(() => {
		setQuantity(event.cardPurchaseLimit ?? 100);
		setPerParticipantLimit(event.cardPurchasePerParticipantLimit ?? 5);
		setClosesAt(toInputDateTime(event.cardPurchaseClosesAt));
		setLowStockThreshold(event.cardPurchaseLowStockThreshold ?? 10);
	}, [event.cardPurchaseClosesAt, event.cardPurchaseLimit, event.cardPurchaseLowStockThreshold, event.cardPurchasePerParticipantLimit]);

	const openPurchase = async () => {
		const opened = await action.execute(
			async () => {
				await bingoApi.openCardPurchase(eventId, settings());
				return true;
			},
			"Reserva de cartelas aberta.",
			"Não foi possível abrir a reserva."
		);
		if (!opened) return;
		await onReload();
	};
	const updatePurchase = async () => {
		const updated = await action.execute(
			async () => {
				await bingoApi.updateCardPurchase(eventId, settings());
				return true;
			},
			"Configurações atualizadas.",
			"Não foi possível atualizar a reserva."
		);
		if (!updated) return;
		await onReload();
	};
	const cancelPurchase = async () => {
		const reason = cancellationReason.trim();
		if (!reason) return;
		const cancelled = await action.execute(
			async () => {
				await bingoApi.cancelCardPurchase(eventId, reason);
				return true;
			},
			"Reserva cancelada e cartelas invalidadas.",
			"Não foi possível cancelar a reserva."
		);
		if (!cancelled) return;
		setCancellationReason("");
		await onReload();
	};
	const createInvitation = async () => {
		const invitation = await action.execute(
			() => bingoApi.createCardPurchaseInvitation(eventId, bonusCards),
			"Convite promocional criado.",
			"Não foi possível criar o convite promocional."
		);
		if (invitation) setLatestInvitation(invitation);
	};
	return (
		<section className="participant-action-group" aria-labelledby="card-purchase-title">
			<div className="participant-action-heading">
				<span aria-hidden="true">B</span>
				<div>
					<h3 id="card-purchase-title">Venda de cartelas</h3>
					<p>Libere um lote digital com quantidade controlada para reserva.</p>
				</div>
			</div>
			{event.status === "Draft" && (
				<div className="participant-action-config">
					<label>
						<span>Cartelas disponíveis</span>
						<input
							aria-label="Cartelas disponíveis para venda"
							type="number"
							min="1"
							max="10000"
							value={quantity}
							onChange={(input) => setQuantity(Math.min(10000, Math.max(1, Number(input.target.value) || 1)))}
						/>
					</label>
					<label>
						<span>Máximo por participante</span>
						<input
							aria-label="Máximo de cartelas por participante"
							type="number"
							min="1"
							max="100"
							value={perParticipantLimit}
							onChange={(input) => setPerParticipantLimit(Math.min(100, Math.max(1, Number(input.target.value) || 1)))}
						/>
					</label>
					<label>
						<span>Encerramento programado</span>
						<input
							aria-label="Encerramento programado da venda"
							type="datetime-local"
							value={closesAt}
							onChange={(input) => setClosesAt(input.target.value)}
						/>
					</label>
					<label>
						<span>Alerta de estoque baixo</span>
						<input
							aria-label="Alerta de estoque baixo"
							type="number"
							min="0"
							max="10000"
							value={lowStockThreshold}
							onChange={(input) => setLowStockThreshold(Math.min(10000, Math.max(0, Number(input.target.value) || 0)))}
						/>
					</label>
					<button className="reveal" disabled={isFinished || isOpen || action.isPending} onClick={() => void openPurchase()}>
						Abrir reserva de cartelas
					</button>
				</div>
			)}
			{event.cardPurchaseLimit != null && (
				<>
					<p className="participant-action-status" role="status">
						{isOpen
							? `${event.cardPurchaseRemaining ?? 0} de ${event.cardPurchaseLimit} cartela(s) disponível(is) para reserva`
							: "Reserva de cartelas encerrada"}
					</p>
					{event.cardPurchaseDashboard && (
						<>
							<section className="purchase-dashboard" aria-label="Resumo da reserva de cartelas">
								<div>
									<strong>{event.cardPurchaseDashboard.total}</strong>
									<span>disponibilizadas</span>
								</div>
								<div>
									<strong>{event.cardPurchaseDashboard.reserved}</strong>
									<span>reservadas</span>
								</div>
								<div>
									<strong>{event.cardPurchaseDashboard.activated}</strong>
									<span>ativadas</span>
								</div>
							</section>
							<details className="purchase-secondary-metrics">
								<summary>Ver indicadores adicionais</summary>
								<section className="purchase-dashboard" aria-label="Indicadores adicionais da reserva de cartelas">
									<div>
										<strong>{event.cardPurchaseDashboard.eligibleForNextRound}</strong>
										<span>aptas para rodada</span>
									</div>
									<div>
										<strong>{event.cardPurchaseDashboard.awaitingActivation}</strong>
										<span>aguardando ativação</span>
									</div>
									<div>
										<strong>{event.cardPurchaseDashboard.waitlistEntries}</strong>
										<span>lista de espera</span>
									</div>
								</section>
							</details>
						</>
					)}
					{event.cardPurchaseClosesAt && (
						<p className="action-hint">
							Encerramento programado:{" "}
							{new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" }).format(
								new Date(event.cardPurchaseClosesAt)
							)}
							.
						</p>
					)}
					<details className="purchase-management-details">
						<summary>Configurar reserva</summary>
						<div className="participant-action-config">
							<label>
								<span>Novo limite</span>
								<input
									aria-label="Limite de cartelas para venda"
									type="number"
									min="1"
									max="10000"
									value={quantity}
									onChange={(input) => setQuantity(Math.min(10000, Math.max(1, Number(input.target.value) || 1)))}
								/>
							</label>
							<label>
								<span>Máximo por participante</span>
								<input
									aria-label="Novo máximo de cartelas por participante"
									type="number"
									min="1"
									max="100"
									value={perParticipantLimit}
									onChange={(input) =>
										setPerParticipantLimit(Math.min(100, Math.max(1, Number(input.target.value) || 1)))
									}
								/>
							</label>
							<label>
								<span>Encerramento programado</span>
								<input
									aria-label="Novo encerramento programado da venda"
									type="datetime-local"
									value={closesAt}
									onChange={(input) => setClosesAt(input.target.value)}
								/>
							</label>
							<label>
								<span>Alerta de estoque baixo</span>
								<input
									aria-label="Novo alerta de estoque baixo"
									type="number"
									min="0"
									max="10000"
									value={lowStockThreshold}
									onChange={(input) =>
										setLowStockThreshold(Math.min(10000, Math.max(0, Number(input.target.value) || 0)))
									}
								/>
							</label>
							<button disabled={isFinished || action.isPending || !isOpen} onClick={() => void updatePurchase()}>
								Atualizar configurações
							</button>
						</div>
					</details>
					<details className="purchase-invitations">
						<summary>Convites promocionais</summary>
						<div className="purchase-details-content">
							<div className="purchase-invitations-heading">
								<h4>Convite promocional</h4>
								<p>Crie um link de uso único com cartelas extras para uma pessoa específica.</p>
							</div>
							<p className="purchase-invitation-guidance" role="note">
								Quem receber o convite faz a inscrição em uma página pública. O link não dá acesso às configurações do
								evento.
							</p>
							<div className="purchase-invitation-create">
								<label>
									<span>Cartelas extras</span>
									<input
										aria-label="Cartelas extras do convite"
										type="number"
										min="1"
										max="100"
										value={bonusCards}
										onChange={(input) => setBonusCards(Math.min(100, Math.max(1, Number(input.target.value) || 1)))}
									/>
								</label>
								<button type="button" disabled={action.isPending || !isOpen} onClick={() => void createInvitation()}>
									Gerar link promocional
								</button>
							</div>
							{latestInvitation && (
								<div className="purchase-invitation-result">
									<p>
										<strong>Link promocional pronto:</strong> <code>{latestInvitation.code}</code> ·{" "}
										{latestInvitation.bonusCards} cartela(s) extra(s).
									</p>
									<RegistrationQrCode
										registrationPath={`${registrationPath}?convite=${encodeURIComponent(latestInvitation.code)}`}
										description="Aponte a câmera para abrir a inscrição pública com o benefício do convite."
									/>
									<button
										type="button"
										onClick={() =>
											void copyPublicLink(
												`${registrationPath}?convite=${encodeURIComponent(latestInvitation.code)}`,
												"Link promocional"
											)
										}
									>
										Copiar link promocional
									</button>
								</div>
							)}
						</div>
					</details>
					<details className="purchase-cancellation">
						<summary>Encerrar venda de cartelas</summary>
						<div className="purchase-details-content">
							<div>
								<p>Use somente se a reserva precisar ser interrompida. As cartelas reservadas serão invalidadas.</p>
							</div>
							<div className="participant-action-config participant-action-cancel">
								<label>
									<span>Motivo do cancelamento</span>
									<input
										aria-label="Motivo do cancelamento da venda"
										value={cancellationReason}
										onChange={(input) => setCancellationReason(input.target.value)}
										placeholder="Ex.: alteração na programação"
									/>
								</label>
								<button
									className="reveal"
									disabled={
										isFinished ||
										!isOpen ||
										Boolean(event.cardPurchaseCancellationReason) ||
										action.isPending ||
										cancellationReason.trim().length < 3
									}
									onClick={() => void cancelPurchase()}
								>
									Cancelar venda de cartelas
								</button>
							</div>
						</div>
					</details>
				</>
			)}
			{event.cardPurchaseCancellationReason && (
				<p className="participant-action-status is-cancelled" role="status">
					Venda cancelada: {event.cardPurchaseCancellationReason}
				</p>
			)}
			<FeedbackMessage error={action.error} success={action.success} />
		</section>
	);
}
