import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { useSession } from "../../shared/auth/SessionContext";
import { AppShell } from "../../shared/ui/AppShell";

type PublicAccessIntent = "join" | "audit";

export function HomePage() {
	const navigate = useNavigate();
	const { session } = useSession();
	const [eventCode, setEventCode] = useState("");
	const [publicAccessIntent, setPublicAccessIntent] = useState<PublicAccessIntent>();
	const submitEventCode = (event: FormEvent<HTMLFormElement>) => {
		event.preventDefault();
		const code = eventCode.trim();
		if (code) navigate(`/participar/${code}`);
	};
	const openAudit = () => {
		const code = eventCode.trim();
		if (code) navigate(`/auditoria/${code}`);
	};

	return (
		<AppShell>
			<main className="hero home-page">
				<p className="eyebrow">Bingo corporativo em tempo real</p>
				<h1>
					A festa inteira
					<br />
					<em>joga junto.</em>
				</h1>
				<p>Cartelas digitais, sorteio auditável e uma experiência de telão feita para criar suspense.</p>
				<div className="actions">
					{session ? (
						<button
							className="primary"
							onClick={() => navigate(session.accountType === "participant" ? "/minhas-cartelas" : "/admin")}
						>
							{session.accountType === "participant" ? "Minhas cartelas" : "Acessar administração"}
						</button>
					) : (
						<>
							<button className="primary" onClick={() => navigate("/cadastro")}>
								Cadastrar empresa
							</button>
							<button onClick={() => navigate("/entrar")}>Entrar</button>
						</>
					)}
					<button onClick={() => setPublicAccessIntent("join")}>Participar</button>
					<button onClick={() => setPublicAccessIntent("audit")}>Auditoria pública</button>
				</div>
				{publicAccessIntent && (
					<form className="panel" onSubmit={submitEventCode}>
						<h2>{publicAccessIntent === "join" ? "Participar do bingo" : "Consultar auditoria pública"}</h2>
						<p>
							{publicAccessIntent === "join"
								? "Informe o código compartilhado pela organização para abrir sua inscrição."
								: "Informe o código do evento para conferir os resultados e registros públicos."}
						</p>
						<label>
							Código do evento
							<input
								aria-label="Código do evento"
								autoFocus
								value={eventCode}
								onChange={(event) => setEventCode(event.target.value)}
								placeholder="Ex.: AB12CD"
							/>
						</label>
						<div className="actions">
							<button
								className="primary"
								type={publicAccessIntent === "join" ? "submit" : "button"}
								disabled={!eventCode.trim()}
								onClick={publicAccessIntent === "audit" ? openAudit : undefined}
							>
								{publicAccessIntent === "join" ? "Acessar bingo" : "Consultar auditoria"}
							</button>
							<button type="button" onClick={() => setPublicAccessIntent(undefined)}>
								Cancelar
							</button>
						</div>
					</form>
				)}
			</main>
		</AppShell>
	);
}
