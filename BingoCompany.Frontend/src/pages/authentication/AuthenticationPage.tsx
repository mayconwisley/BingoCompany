import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { authApi } from "../../features/bingo";
import { getErrorMessage } from "../../shared/api/getErrorMessage";
import { saveSession } from "../../shared/auth/session";
import { AppShell } from "../../shared/ui/AppShell";

type Props = { mode: "login" | "register" };

export function AuthenticationPage({ mode }: Props) {
    const navigate = useNavigate();
    const [companyName, setCompanyName] = useState("");
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState("");
    const [isSubmitting, setIsSubmitting] = useState(false);
    const isRegistering = mode === "register";

    const submit = async () => {
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
                <section className="panel">
                    {isRegistering && (
                        <>
                            <label>
                                Empresa
                                <input aria-label="Empresa" value={companyName} onChange={(event) => setCompanyName(event.target.value)} />
                            </label>
                            <label>
                                Seu nome
                                <input aria-label="Seu nome" value={name} onChange={(event) => setName(event.target.value)} />
                            </label>
                        </>
                    )}
                    <label>
                        E-mail
                        <input aria-label="E-mail" type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
                    </label>
                    <label>
                        Senha
                        <input
                            aria-label="Senha"
                            type="password"
                            minLength={8}
                            value={password}
                            onChange={(event) => setPassword(event.target.value)}
                        />
                    </label>
                    <button
                        className="primary"
                        disabled={
                            isSubmitting || !email.trim() || password.length < 8 || (isRegistering && (!companyName.trim() || !name.trim()))
                        }
                        onClick={submit}
                    >
                        {isSubmitting ? "Aguarde..." : isRegistering ? "Cadastrar empresa" : "Entrar"}
                    </button>
                    {error && (
                        <p className="error" role="alert">
                            {error}
                        </p>
                    )}
                    <p>
                        {isRegistering ? "Já possui uma conta?" : "Ainda não possui uma empresa cadastrada?"}{" "}
                        <Link to={isRegistering ? "/entrar" : "/cadastro"}>{isRegistering ? "Entrar" : "Cadastre-se"}</Link>
                    </p>
                </section>
            </main>
        </AppShell>
    );
}
