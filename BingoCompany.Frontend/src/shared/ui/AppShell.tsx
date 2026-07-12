import type { PropsWithChildren } from "react";
import { Link } from "react-router-dom";
export function AppShell({children}:PropsWithChildren){return <><nav><Link className="brand" to="/">BINGO<span>COMPANY</span></Link><Link className="navlink" to="/admin">Administração</Link></nav>{children}</>}
