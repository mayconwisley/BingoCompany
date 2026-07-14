import { useCallback, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { bingoApi } from "../../features/bingo";
import type { ParticipantType } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { useAsyncResource } from "../../shared/hooks/useAsyncResource";
import { AppShell } from "../../shared/ui/AppShell";
import { PageState } from "../../shared/ui/PageState";

export function JoinPage() {
	const { publicCode = "" } = useParams();
	const navigate = useNavigate();
	const [name, setName] = useState("");
	const [type, setType] = useState<ParticipantType>("Employee");
	const [registration, setRegistration] = useState("");
	const [responsible, setResponsible] = useState("");
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const loader = useCallback(() => bingoApi.getPublicEvent(publicCode), [publicCode]);
	const event = useAsyncResource(loader);

	const createCard = async () => {
		try {
			setError("");
			setIsSubmitting(true);
			const card = await bingoApi.join(publicCode, {
				name,
				type,
				employeeRegistration: registration || undefined,
				responsibleEmployeeName: responsible || undefined
			});
			navigate(`/cartela/${event.data?.id}/${card.publicCode}`);
		} catch (error) {
			setError(getErrorMessage(error, "Não foi possível gerar sua cartela. Tente novamente em alguns instantes."));
		} finally {
			setIsSubmitting(false);
		}
	};

	const updateParticipantType = (value: string) => {
		if (value === "Employee" || value === "FamilyMember" || value === "Guest") setType(value);
	};

	const needsResponsible = type !== "Employee";
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
								<p className="subtitle">Informe seus dados para receber sua cartela digital.</p>
							</div>
						</header>
						<section className="panel">
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
							<button
								className="primary"
								disabled={!name.trim() || (needsResponsible && !responsible.trim()) || isSubmitting}
								onClick={createCard}
							>
								{isSubmitting ? "Gerando cartela..." : "Gerar minha cartela"}
							</button>
							{error && (
								<p className="error" role="alert">
									{error}
								</p>
							)}
						</section>
					</>
				)}
			</main>
		</AppShell>
	);
}
