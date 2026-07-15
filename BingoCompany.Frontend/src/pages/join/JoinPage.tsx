import { useCallback, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
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
	const navigate = useNavigate();
	const [name, setName] = useState("");
	const [type, setType] = useState<ParticipantType>("Employee");
	const [registration, setRegistration] = useState("");
	const [responsible, setResponsible] = useState("");
	const [cardsQuantity, setCardsQuantity] = useState(1);
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [registrationResult, setRegistrationResult] = useState<RegistrationResult>();
	const [activatingCode, setActivatingCode] = useState("");
	const loader = useCallback(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
	const event = useAsyncResource(loader);
	const participantSession = getSession();
	const isCardPurchase = Boolean(event.data?.isCardPurchaseOpen);
	const registrationsOpen = event.data?.status === "RegistrationOpen";

	const createCard = async () => {
		try {
			setError("");
			setIsSubmitting(true);
			const registrationResult = await bingoApi.join(
				publicCode,
				isCardPurchase
					? { name: participantSession?.name, cardsQuantity }
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
	const cardPurchaseCancellationReason = event.data?.cardPurchaseCancellationReason;
	const submitDisabledReason = requiresParticipantAccount
		? "Entre ou crie uma conta de participante para comprar cartelas neste evento."
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
										? "Escolha quantas cartelas deseja comprar. Usaremos os dados da sua conta de participante."
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
								<p>Guarde estes códigos. Ative somente as cartelas que deseja usar neste evento.</p>
								<ul className="card-purchase-list">
									{registrationResult.cards.map((card, index) => (
										<li key={card.publicCode}>
											<div className="card-purchase-code">
												<span>Cartela {index + 1}</span>
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
									<label>
										Quantas cartelas deseja adquirir?{" "}
										{availableCards !== undefined && <small>({availableCards} disponível(is))</small>}
										<input
											aria-label="Quantidade de cartelas digitais"
											type="number"
											min="1"
											max={availableCards ?? 100}
											value={cardsQuantity}
											onChange={(input) =>
												setCardsQuantity(Math.min(100, Math.max(1, Number(input.target.value) || 1)))
											}
										/>
									</label>
								)}
								<button
									className="primary"
									disabled={
										requiresParticipantAccount ||
										availableCards === 0 ||
										(!isCardPurchase && !name.trim()) ||
										(needsResponsible && !responsible.trim()) ||
										isSubmitting
									}
									aria-describedby={submitDisabledReason ? "join-submit-hint" : undefined}
									onClick={createCard}
								>
									{isSubmitting ? "Processando..." : isCardPurchase ? "Comprar cartelas" : "Gerar minha cartela"}
								</button>
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
