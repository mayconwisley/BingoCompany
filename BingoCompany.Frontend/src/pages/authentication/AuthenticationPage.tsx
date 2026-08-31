import { useState } from "react";
import type { FormEvent } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Box, Button, IconButton, InputAdornment, TextField } from "@mui/material";
import { authApi } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { saveSession } from "../../shared/auth/session";
import { AppShell } from "../../shared/ui/AppShell";
import { FeedbackMessage } from "../../shared/ui/FeedbackMessage";

type Props = { mode: "login" | "register" };

export function AuthenticationPage({ mode }: Props) {
	const navigate = useNavigate();
	const [companyName, setCompanyName] = useState("");
	const [name, setName] = useState("");
	const [email, setEmail] = useState("");
	const [password, setPassword] = useState("");
	const [isPasswordVisible, setIsPasswordVisible] = useState(false);
	const [error, setError] = useState("");
	const [isSubmitting, setIsSubmitting] = useState(false);
	const isRegistering = mode === "register";
	const isSubmitDisabled =
		isSubmitting ||
		!email.trim() ||
		password.length < 12 ||
		password.length > 128 ||
		(isRegistering && (!companyName.trim() || !name.trim()));

	const submit = async (event: FormEvent<HTMLFormElement>) => {
		event.preventDefault();
		if (isSubmitDisabled) return;

		try {
			setError("");
			setIsSubmitting(true);
			const session = isRegistering
				? await authApi.register(companyName, name, email, password)
				: await authApi.login(email, password);
			saveSession(session);
			navigate("/admin", { replace: true });
		} catch (exception) {
			setError(getErrorMessage(exception, "Não foi possível autenticar a conta."));
		} finally {
			setIsSubmitting(false);
		}
	};

	return (
		<AppShell showAdministration={false}>
			<main className="join auth-page">
				<header className="pagehead">
					<div>
						<p className="eyebrow">Área da empresa</p>
						<h1>{isRegistering ? "Crie sua conta" : "Acesse sua conta"}</h1>
						<p className="subtitle">
							{isRegistering
								? "Cadastre sua empresa para criar e administrar eventos de bingo."
								: "Entre para acessar os eventos da sua empresa."}
						</p>
					</div>
				</header>
				<Box component="form" className="panel authentication-form" onSubmit={submit}>
					{isRegistering && (
						<>
							<TextField
								autoComplete="organization"
								fullWidth
								label="Empresa"
								value={companyName}
								onChange={(event) => setCompanyName(event.target.value)}
							/>
							<TextField
								autoComplete="name"
								fullWidth
								label="Seu nome"
								value={name}
								onChange={(event) => setName(event.target.value)}
							/>
						</>
					)}
					<TextField
						autoComplete="email"
						fullWidth
						label="E-mail"
						type="email"
						value={email}
						onChange={(event) => setEmail(event.target.value)}
					/>
					<TextField
						autoComplete={isRegistering ? "new-password" : "current-password"}
						fullWidth
						label="Senha"
						type={isPasswordVisible ? "text" : "password"}
						helperText={isRegistering ? "A senha deve ter entre 12 e 128 caracteres." : undefined}
						slotProps={{
							input: {
								endAdornment: (
									<InputAdornment position="end">
										<IconButton
											aria-label={isPasswordVisible ? "Ocultar senha" : "Mostrar senha"}
											edge="end"
											onClick={() => setIsPasswordVisible((isVisible) => !isVisible)}
										>
											<svg aria-hidden="true" focusable="false" viewBox="0 0 24 24">
												<path d="M2.5 12s3.5-6.5 9.5-6.5 9.5 6.5 9.5 6.5-3.5 6.5-9.5 6.5S2.5 12 2.5 12Z" />
												<circle cx="12" cy="12" r="2.75" />
												{isPasswordVisible && <path d="m4 4 16 16" />}
											</svg>
										</IconButton>
									</InputAdornment>
								)
							},
							htmlInput: { minLength: 12, maxLength: 128 }
						}}
						value={password}
						onChange={(event) => setPassword(event.target.value)}
					/>
					<Button variant="contained" size="large" disabled={isSubmitDisabled} type="submit">
						{isSubmitting ? "Aguarde..." : isRegistering ? "Cadastrar empresa" : "Entrar"}
					</Button>
					<FeedbackMessage error={error} onClose={() => setError("")} />
					<Box component="p" className="authentication-alternative">
						{isRegistering ? "Já possui uma conta?" : "Ainda não possui uma empresa cadastrada?"}{" "}
						<Link to={isRegistering ? "/entrar" : "/cadastro"}>{isRegistering ? "Entrar" : "Cadastre-se"}</Link>
					</Box>
				</Box>
			</main>
		</AppShell>
	);
}
