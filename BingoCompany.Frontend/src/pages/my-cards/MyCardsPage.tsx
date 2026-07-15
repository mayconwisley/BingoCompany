import { useCallback, useState } from "react";
import { Link, useNavigate, useSearchParams } from "react-router-dom";
import { authApi, bingoApi } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { getSession, saveSession } from "../../shared/auth/session";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";
import { PageState } from "../../shared/ui/PageState";

const CardsPerPage = 5;

export function MyCardsPage() {
	const session = getSession();
	const navigate = useNavigate();
	const [searchParams] = useSearchParams();
	const returnTo = searchParams.get("returnTo");
	const [isRegistering, setIsRegistering] = useState(false);
	const [name, setName] = useState("");
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const [activatingCode, setActivatingCode] = useState("");
	const [page, setPage] = useState(1);
	const cards = useAsyncResource(useCallback(() => (session?.accountType === "participant" ? bingoApi.getMyCards(page, CardsPerPage) : Promise.resolve({ activeCards: [], history: { items: [], page: 1, pageSize: CardsPerPage, totalItems: 0, totalPages: 0 } })), [page, session?.accountType]));
	const activeCards = cards.data?.activeCards ?? [];
	const historyCards = cards.data?.history.items ?? [];
	const totalPages = Math.max(1, cards.data?.history.totalPages ?? 1);

	const authenticate = async () => {
		try {
			setError("");
			setIsSubmitting(true);
			const account = isRegistering ? await authApi.participantRegister(name, email, password) : await authApi.participantLogin(email, password);
			saveSession({ name: account.name, companyName: "", accountType: "participant" });
			navigate(returnTo?.startsWith("/") ? returnTo : "/minhas-cartelas", { replace: true });
		} catch (exception) {
			setError(getErrorMessage(exception, "Não foi possível acessar sua conta de participante."));
		} finally {
			setIsSubmitting(false);
		}
	};
	const activateCard = async (eventPublicCode: string, cardCode: string) => {
		try {
			setError("");
			setActivatingCode(cardCode);
			await bingoApi.activateDigitalCard(eventPublicCode, cardCode);
			await cards.reload();
		} catch (exception) {
			setError(getErrorMessage(exception, "Não foi possível ativar esta cartela."));
		} finally {
			setActivatingCode("");
		}
	};

	if (session?.accountType !== "participant") {
		return (
			<AppShell showAdministration={false}>
				<main className="join auth-page">
					<header className="pagehead"><div><p className="eyebrow">Participante</p><h1>Minhas cartelas</h1><p className="subtitle">Entre para recuperar as cartelas adquiridas em cada evento.</p></div></header>
					<section className="panel">
						{isRegistering && <label>Seu nome<input aria-label="Seu nome" value={name} onChange={(event) => setName(event.target.value)} /></label>}
						<label>E-mail<input aria-label="E-mail" type="email" value={email} onChange={(event) => setEmail(event.target.value)} /></label>
						<label>Senha<input aria-label="Senha" type="password" minLength={12} maxLength={128} value={password} onChange={(event) => setPassword(event.target.value)} /></label>
						<button className="primary" disabled={isSubmitting || !email.trim() || password.length < 12 || (isRegistering && !name.trim())} onClick={authenticate}>{isSubmitting ? "Aguarde..." : isRegistering ? "Criar conta" : "Entrar"}</button>
						<button type="button" className="navlink" onClick={() => setIsRegistering((value) => !value)}>{isRegistering ? "Já tenho uma conta" : "Criar conta de participante"}</button>
						<FeedbackMessage error={error} onClose={() => setError("")} />
					</section>
				</main>
			</AppShell>
		);
	}

	return (
		<AppShell showAdministration={false}>
			<main className="join my-cards-page">
				<header className="pagehead"><div><p className="eyebrow">Participante</p><h1>Minhas cartelas</h1><p className="subtitle">Consulte códigos, situação e histórico das suas cartelas.</p></div></header>
				<PageState loading={cards.loading} error={cards.error} onRetry={cards.reload} />
				{cards.data && (
					<>
						<section className="panel active-cards-panel" aria-labelledby="active-cards-title">
							<p className="eyebrow">Prontas para jogar</p>
							<h2 id="active-cards-title">Cartelas ativas</h2>
							{activeCards.length === 0 ? <p>Nenhuma cartela ativa no momento.</p> : <div className="active-cards-grid">{activeCards.map((card) => <article key={`${card.eventId}-${card.publicCode}`}><div className="card-purchase-code"><span>{card.eventName}</span><code>{card.publicCode}</code></div><Link className="button" to={`/cartela/${card.eventId}/${card.publicCode}`}>Abrir cartela</Link></article>)}</div>}
						</section>
						<section className="panel" aria-labelledby="card-history-title">
							<p className="eyebrow">Acompanhar</p>
							<h2 id="card-history-title">Outras cartelas</h2>
							<div className="card-purchase-list">{historyCards.length === 0 ? <p>Nenhuma outra cartela para exibir.</p> : historyCards.map((card) => <article key={`${card.eventId}-${card.publicCode}`}><div className="card-purchase-code"><span>{card.eventName} · {card.eventStatus === "Finished" && card.status === "Cancelled" ? "Não utilizada — invalidada" : card.status}</span><code>{card.publicCode}</code>{card.status === "Cancelled" && <small>Motivo: {card.invalidationReason ?? card.eventCancellationReason ?? "Cartela indisponível para este evento."}</small>}</div>{card.status === "Assigned" ? <button className="primary" disabled={Boolean(activatingCode)} onClick={() => activateCard(card.eventPublicCode, card.publicCode)}>{activatingCode === card.publicCode ? "Ativando..." : "Ativar cartela"}</button> : <span className="badge">Invalidada</span>}</article>)}</div>
							{totalPages > 1 && <nav className="pagination" aria-label="Paginação das cartelas"><button type="button" onClick={() => setPage(page - 1)} disabled={page === 1}>Página anterior</button><span aria-live="polite">Página {cards.data.history.page} de {totalPages}</span><button type="button" onClick={() => setPage(page + 1)} disabled={page === totalPages}>Próxima página</button></nav>}
						</section>
						<FeedbackMessage error={error} onClose={() => setError("")} />
					</>
				)}
			</main>
		</AppShell>
	);
}
