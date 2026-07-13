import type { PropsWithChildren } from "react";
import { Link, useNavigate } from "react-router-dom";
import { clearSession, getSession } from "../auth/session";
import { useApplicationInfo } from "../../app/ApplicationInfoContext";
import { ApplicationFooter } from "./ApplicationFooter";
import { ThemeToggle } from "./ThemeToggle";
type Props = PropsWithChildren<{ showAdministration?: boolean }>;

export function AppShell({ children, showAdministration = true }: Props) {
    const navigate = useNavigate();
    const session = getSession();
    const application = useApplicationInfo();
    return (
        <>
            <nav className="topbar">
                <Link className="brand" to="/" aria-label="Bingo Company, página inicial">
                    <img className="brand-logo" src="/assets/bingo-company-logo.png" alt="" />
                    <span>
                        BINGO <b>COMPANY</b>
                    </span>
                </Link>
                <div className="topbar-actions">
                    <Link className="navlink" to="/ajuda">
                        Ajuda
                    </Link>
                    {showAdministration &&
                        (session ? (
                            <>
                                <Link className="navlink" to="/admin">
                                    {session.companyName}
                                </Link>
                                <button
                                    className="navlink"
                                    onClick={() => {
                                        clearSession();
                                        navigate("/");
                                    }}
                                >
                                    Sair
                                </button>
                            </>
                        ) : (
                            <Link className="navlink" to="/entrar">
                                Entrar
                            </Link>
                        ))}
                    <ThemeToggle />
                </div>
            </nav>
            {children}
            <ApplicationFooter application={application} />
        </>
    );
}
