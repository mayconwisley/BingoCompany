import type { PropsWithChildren } from "react";
import { Link } from "react-router-dom";
import { ThemeToggle } from "./ThemeToggle";
type Props = PropsWithChildren<{ showAdministration?: boolean }>;

export function AppShell({ children, showAdministration = true }: Props)
{
    return <>
        <nav className="topbar">
            <Link className="brand" to="/" aria-label="Bingo Company, página inicial"><span className="brand-mark" aria-hidden="true">B</span><span>BINGO <b>COMPANY</b></span></Link>
            <div className="topbar-actions">
                {showAdministration && <Link className="navlink" to="/admin">Administração</Link>}
                <ThemeToggle />
            </div>
        </nav>
        {children}
    </>;
}
