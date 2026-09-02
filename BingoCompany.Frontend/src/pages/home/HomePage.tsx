import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { Button, Paper, Stack, TextField } from "@mui/material";
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
						<Button
							variant="contained"
							size="large"
							onClick={() => navigate(session.accountType === "participant" ? "/minhas-cartelas" : "/admin")}
						>
							{session.accountType === "participant" ? "Minhas cartelas" : "Acessar administração"}
						</Button>
					) : (
						<>
							<Button variant="contained" size="large" onClick={() => navigate("/cadastro")}>
								Cadastrar empresa
							</Button>
							<Button variant="outlined" size="large" onClick={() => navigate("/entrar")}>
								Entrar
							</Button>
						</>
					)}
					<Button variant="text" onClick={() => setPublicAccessIntent("join")}>
						Participar
					</Button>
					<Button variant="text" onClick={() => setPublicAccessIntent("audit")}>
						Auditoria pública
					</Button>
				</div>
				{publicAccessIntent && (
					<Paper component="form" className="panel" elevation={0} onSubmit={submitEventCode}>
						<h2>{publicAccessIntent === "join" ? "Participar do bingo" : "Consultar auditoria pública"}</h2>
						<p>
							{publicAccessIntent === "join"
								? "Informe o código compartilhado pela organização para abrir sua inscrição."
								: "Informe o código do evento para conferir os resultados e registros públicos."}
						</p>
						<TextField
							fullWidth
							label="Código do evento"
							autoFocus
							value={eventCode}
							onChange={(event) => setEventCode(event.target.value)}
							placeholder="Ex.: AB12CD"
						/>
						<Stack className="actions" direction="row" spacing={1} sx={{ flexWrap: "wrap" }}>
							<Button
								variant="contained"
								type={publicAccessIntent === "join" ? "submit" : "button"}
								disabled={!eventCode.trim()}
								onClick={publicAccessIntent === "audit" ? openAudit : undefined}
							>
								{publicAccessIntent === "join" ? "Acessar bingo" : "Consultar auditoria"}
							</Button>
							<Button type="button" variant="outlined" onClick={() => setPublicAccessIntent(undefined)}>
								Cancelar
							</Button>
						</Stack>
					</Paper>
				)}
			</main>
		</AppShell>
	);
}
