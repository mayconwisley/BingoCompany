import { useState } from "react";
import type { FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { getSession } from "../../shared/auth/session";
import { AppShell } from "../../shared/ui/AppShell";

export function HomePage()
{
    const navigate = useNavigate();
    const session = getSession();
    const [eventCode, setEventCode] = useState("");
    const [isJoinFormVisible, setIsJoinFormVisible] = useState(false);
    const submitEventCode = (event: FormEvent<HTMLFormElement>) =>
    {
        event.preventDefault();
        const code = eventCode.trim();
        if (code) navigate(`/participar/${code}`);
    };
    const openAudit = () =>
    {
        const code = eventCode.trim();
        if (code) navigate(`/auditoria/${code}`);
    };

    return <AppShell><main className="hero"><p className="eyebrow">Bingo corporativo em tempo real</p><h1>A festa inteira<br/><em>joga junto.</em></h1><p>Cartelas digitais, sorteio auditável e uma experiência de telão feita para criar suspense.</p><div className="actions">{session ? <button className="primary" onClick={() => navigate("/admin")}>Acessar administração</button> : <><button className="primary" onClick={() => navigate("/cadastro")}>Cadastrar empresa</button><button onClick={() => navigate("/entrar")}>Entrar</button></>}<button onClick={() => setIsJoinFormVisible(true)}>Participar</button><button onClick={() => setIsJoinFormVisible(true)}>Auditoria pública</button></div>{isJoinFormVisible && <form className="panel" onSubmit={submitEventCode}><label>Código do evento<input aria-label="Código do evento" autoFocus value={eventCode} onChange={event => setEventCode(event.target.value)} placeholder="Ex.: AB12CD" /></label><div className="actions"><button className="primary" type="submit" disabled={!eventCode.trim()}>Acessar bingo</button><button type="button" disabled={!eventCode.trim()} onClick={openAudit}>Consultar auditoria</button><button type="button" onClick={() => setIsJoinFormVisible(false)}>Cancelar</button></div></form>}</main></AppShell>;
}
