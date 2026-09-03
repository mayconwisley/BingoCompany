import { useCallback, useState } from "react";
import { Link, useNavigate, useParams, useSearchParams } from "react-router-dom";
import { bingoApi } from "../../features/bingo";
import type { ParticipantType, RegistrationResult } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { getSession } from "../../shared/auth/session";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

export function JoinPage() {
	const { publicCode = "" } = useParams();
	const [searchParams] = useSearchParams();
	const navigate = useNavigate();
	const [name, setName] = useState("");
	const [type, setType] = useState<ParticipantType>("Employee");
	const [registration, setRegistration] = useState("");
	const [responsible, setResponsible] = useState("");
	const [cardsQuantity, setCardsQuantity] = useState(1);
	const [invitationCode, setInvitationCode] = useState(() => searchParams.get("convite") ?? "");
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [registrationResult, setRegistrationResult] = useState<RegistrationResult>();
	const [activatingCode, setActivatingCode] = useState("");
	const [waitlistMessage, setWaitlistMessage] = useState("");
	const loader = useCallback(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
	const event = useAsyncResource(loader);
	const participantSession = getSession();
	const isCardPurchase = Boolean(event.data?.isCardPurchaseOpen || event.data?.cardPurchaseLimit);
	const isCardPurchaseOpen = Boolean(event.data?.isCardPurchaseOpen);
	const registrationsOpen = event.data?.status === "RegistrationOpen";

	const createCard = async () => {
		try {
			setError("");
			setIsSubmitting(true);
			const registrationResult = await bingoApi.join(
				publicCode,
				isCardPurchase
					? {
							name: participantSession?.name,
							cardsQuantity,
							...(invitationCode.trim() ? { invitationCode: invitationCode.trim() } : {})
						}
					: {
							name,
							type,
							employeeRegistration: registration || undefined,
							responsibleEmployeeName: responsible || undefined
						}
			);
			if (!isCardPurchase) {
				if (registrationResult.cards.length === 1) navigate(`/cartela/${event.data?.id}/${registrationResult.cards[0].publicCode}`);
				else navigate(`/cartelas/${event.data?.id}?codes=${registrationResult.cards.map((card) => card.publicCode).join(",")}`);
				return;
			}
			setRegistrationResult(registrationResult);
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível gerar sua cartela. Tente novamente em alguns instantes."));
		} finally {
			setIsSubmitting(false);
		}
	};
	const joinWaitlist = async () => {
		try {
			setError("");
			setIsSubmitting(true);
			const result = await bingoApi.joinCardPurchaseWaitlist(publicCode, cardsQuantity);
			setWaitlistMessage(
				result.alreadyRegistered
					? `Você já está na lista de espera, na posição ${result.position}, para ${result.requestedQuantity} cartela(s).`
					: `Você entrou na lista de espera na posição ${result.position}, para ${result.requestedQuantity} cartela(s).`
			);
		} catch (waitlistError) {
			setError(getErrorMessage(waitlistError, "Não foi possível entrar na lista de espera."));
		} finally {
			setIsSubmitting(false);
		}
	};
	const activateAndUseCard = async (cardCode: string) => {
		try {
			setError("");
			setActivatingCode(cardCode);
			await bingoApi.activateDigitalCard(publicCode, cardCode);
			setRegistrationResult((current) =>
				current
					? {
							...current,
							cards: current.cards.map((card) => (card.publicCode === cardCode ? { ...card, status: "Active" } : card))
						}
					: current
			);
		} catch (activationError) {
			setError(getErrorMessage(activationError, "Não foi possível ativar esta cartela. Tente novamente."));
		} finally {
			setActivatingCode("");
		}
	};

	const updateParticipantType = (value: string) => {
		if (value === "Employee" || value === "FamilyMember" || value === "Guest") setType(value);
	};

	const needsResponsible = !isCardPurchase && type !== "Employee";
	const requiresParticipantAccount = Boolean(isCardPurchase && participantSession?.accountType !== "participant");
	const availableCards = event.data?.cardPurchaseRemaining;
	const purchaseLimit = event.data?.cardPurchaseLimit;
	const purchasePerParticipantLimit = event.data?.cardPurchasePerParticipantLimit ?? 5;
	const lowStockThreshold = event.data?.cardPurchaseLowStockThreshold ?? 0;
	const isLowStock = availableCards != null && availableCards > 0 && availableCards <= lowStockThreshold;
	const cardPurchaseCancellationReason = event.data?.cardPurchaseCancellationReason;
	const submitDisabledReason = requiresParticipantAccount
		? "Entre ou crie uma conta de participante para comprar cartelas neste evento."
		: isCardPurchase && !isCardPurchaseOpen
			? "A reserva de cartelas está encerrada."
			: availableCards === 0
				? "Não há mais cartelas digitais disponíveis para compra."
				: !isCardPurchase && !name.trim()
					? "Informe seu nome para gerar uma cartela."
					: needsResponsible && !responsible.trim()
						? "Informe o colaborador responsável para continuar."
						: undefined;
	return (
		<AppShell>
			<main className="join">
				<PageState loading={event.loading} error={event.error} onRetry={event.reload} />
				{event.data && (
					<>
						<header className="pagehead">
							<div>
								<p className="eyebrow">Inscrição para o bingo</p>
								<h1>{event.data.name}</h1>
								<p className="subtitle">
									{isCardPurchase
										? "Reserve suas cartelas digitais. A ativação é individual antes da próxima rodada."
										: "Informe seus dados para receber sua cartela digital."}
								</p>
							</div>
						</header>
						{!registrationsOpen ? (
							<section className="panel">
								<p className="eyebrow">Inscrições encerradas</p>
								<FeedbackMessage warning="As inscrições deste bingo estão fechadas." />
								<p>
									A primeira rodada já foi iniciada ou o evento foi encerrado. Não é mais possível gerar ou comprar
									cartelas.
								</p>
							</section>
						) : registrationResult ? (
							<section className="panel">
								<h2>Suas cartelas digitais</h2>
								<p>Reserva confirmada. Guarde os códigos e ative cada cartela que deseja usar antes do início da rodada.</p>
								<ul className="card-purchase-list">
									{registrationResult.cards.map((card, index) => (
										<li key={card.publicCode}>
											<div className="card-purchase-code">
												<span>Cartela {index + 1} · adquirida</span>
												<code>{card.publicCode}</code>
											</div>
											{card.status === "Active" ? (
												<button
													className="primary"
													onClick={() => navigate(`/cartela/${event.data?.id}/${card.publicCode}`)}
												>
													Abrir cartela
												</button>
											) : (
												<button
													className="primary"
													disabled={Boolean(activatingCode)}
													onClick={() => activateAndUseCard(card.publicCode)}
												>
													{activatingCode === card.publicCode ? "Ativando..." : "Ativar"}
												</button>
											)}
										</li>
									))}
								</ul>
								<FeedbackMessage error={error} onClose={() => setError("")} />
							</section>
						) : (
							<section className="panel">
								{cardPurchaseCancellationReason && (
									<p role="status">
										A venda de cartelas foi cancelada. Motivo: {cardPurchaseCancellationReason}. As inscrições comuns
										continuam disponíveis.
									</p>
								)}
								{requiresParticipantAccount && (
									<p role="status">
										Para comprar cartelas,{" "}
										<Link to={`/minhas-cartelas?returnTo=${encodeURIComponent(`/participar/${publicCode}`)}`}>
											entre ou crie sua conta de participante
										</Link>
										. Assim seus códigos ficarão disponíveis depois.
									</p>
								)}
								{isCardPurchase && participantSession?.accountType === "participant" && (
									<p className="purchase-account-summary" role="status">
										Comprando como <strong>{participantSession.name}</strong>.
									</p>
								)}
								{!isCardPurchase && (
									<>
										<label>
											Seu nome
											<input aria-label="Seu nome" value={name} onChange={(event) => setName(event.target.value)} />
										</label>
										<label>
											Tipo de participante
											<select
												aria-label="Tipo de participante"
												value={type}
												onChange={(event) => updateParticipantType(event.target.value)}
											>
												<option value="Employee">Colaborador</option>
												<option value="FamilyMember">Familiar</option>
												<option value="Guest">Convidado</option>
											</select>
										</label>
										{type === "Employee" && (
											<label>
												Matrícula <small>(opcional)</small>
												<input
													aria-label="Matrícula"
													value={registration}
													onChange={(event) => setRegistration(event.target.value)}
												/>
											</label>
										)}
										{needsResponsible && (
											<label>
												Colaborador responsável
												<input
													aria-label="Colaborador responsável"
													value={responsible}
													onChange={(event) => setResponsible(event.target.value)}
												/>
											</label>
										)}
									</>
								)}
								{isCardPurchase && (
									<>
										<div className="purchase-stock" role="status">
											<strong>
												{availableCards ?? 0} de {purchaseLimit ?? 0} cartelas disponíveis
											</strong>
											<span>Máximo de {purchasePerParticipantLimit} cartela(s) por participante.</span>
										</div>
										{isLowStock && <FeedbackMessage warning={`Últimas ${availableCards} cartela(s) disponíveis.`} />}
										{event.data?.cardPurchaseClosesAt && (
											<p className="action-hint" role="status">
												Reservas encerram em{" "}
												{new Intl.DateTimeFormat("pt-BR", { dateStyle: "short", timeStyle: "short" }).format(
													new Date(event.data.cardPurchaseClosesAt)
												)}
												.
											</p>
										)}
										<div className="quantity-presets" aria-label="Pacotes de cartelas">
											{[1, 2, 3, 5].map((quantity) => (
												<button
													key={quantity}
													type="button"
													className={cardsQuantity === quantity ? "is-selected" : ""}
													disabled={
														!isCardPurchaseOpen ||
														(availableCards != null && quantity > availableCards) ||
														quantity > purchasePerParticipantLimit
													}
													onClick={() => setCardsQuantity(quantity)}
												>
													{quantity === 3
														? "Pacote com 3 cartelas"
														: `${quantity} cartela${quantity > 1 ? "s" : ""}`}
												</button>
											))}
										</div>
										<label>
											Escolher quantidade
											<input
												aria-label="Quantidade de cartelas digitais"
												type="number"
												min="1"
												max={Math.min(availableCards ?? 100, purchasePerParticipantLimit)}
												value={cardsQuantity}
												onChange={(input) =>
													setCardsQuantity(
														Math.min(
															Math.min(availableCards ?? 100, purchasePerParticipantLimit),
															Math.max(1, Number(input.target.value) || 1)
														)
													)
												}
											/>
										</label>
										<label>
											Código de convite <small>(opcional: libera cartelas bônus)</small>
											<input
												aria-label="Código de convite"
												value={invitationCode}
												onChange={(input) => setInvitationCode(input.target.value.toUpperCase())}
												maxLength={12}
											/>
										</label>
										{availableCards === 0 && (
											<p role="status">
												Lote esgotado. {event.data?.cardPurchaseWaitlistEntries ?? 0} pessoa(s) aguardam novas
												cartelas.
											</p>
										)}
									</>
								)}
								<button
									className="primary"
									disabled={
										requiresParticipantAccount ||
										(isCardPurchase && (!isCardPurchaseOpen || availableCards === 0)) ||
										(!isCardPurchase && !name.trim()) ||
										(needsResponsible && !responsible.trim()) ||
										isSubmitting
									}
									aria-describedby={submitDisabledReason ? "join-submit-hint" : undefined}
									onClick={createCard}
								>
									{isSubmitting ? "Processando..." : isCardPurchase ? "Reservar cartelas" : "Gerar minha cartela"}
								</button>
								{isCardPurchase &&
									isCardPurchaseOpen &&
									availableCards === 0 &&
									participantSession?.accountType === "participant" && (
										<button type="button" onClick={joinWaitlist} disabled={isSubmitting}>
											Entrar na lista de espera
										</button>
									)}
								{waitlistMessage && <FeedbackMessage success={waitlistMessage} onClose={() => setWaitlistMessage("")} />}
								{submitDisabledReason && (
									<p id="join-submit-hint" className="action-hint" role="status">
										{submitDisabledReason}
									</p>
								)}
								<FeedbackMessage error={error} onClose={() => setError("")} />
							</section>
						)}
					</>
				)}
			</main>
		</AppShell>
	);
}
