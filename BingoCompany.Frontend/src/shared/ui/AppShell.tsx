import type { PropsWithChildren } from "react";
import { Link } from "react-router-dom";
type Props = PropsWithChildren<{ showAdministration?: boolean }>;

export function AppShell({ children, showAdministration = true }: Props)
{
    return <><nav><Link className="brand" to="/">BINGO<span>COMPANY</span></Link>{showAdministration && <Link className="navlink" to="/admin">Administração</Link>}</nav>{children}</>;
}
