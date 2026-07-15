import type { PropsWithChildren } from "react";
import { Link, useNavigate } from "react-router-dom";
import { clearSession } from "../auth/session";
import { useSession } from "../auth/SessionContext";
import { authApi } from "../../features/bingo";
import { useApplicationInfo } from "../../app/ApplicationInfoContext";
import { ApplicationFooter } from "./ApplicationFooter";
import { ThemeToggle } from "./ThemeToggle";
type Props = PropsWithChildren<{ showAdministration?: boolean }>;

export function AppShell({ children, showAdministration = true }: Props) {
	const navigate = useNavigate();
	const { session } = useSession();
	const application = useApplicationInfo();

	const logout = async () => {
		try {
			await authApi.logout();
		} finally {
			clearSession();
			navigate("/");
		}
	};

	return (
		<div className="app-shell">
			<nav className="topbar">
				<Link className="brand" to="/" aria-label="Bingo Company, página inicial">
					<img className="brand-logo" src={`${import.meta.env.BASE_URL}assets/bingo-company-logo.png`} alt="" />
					<span>
						BINGO <b>COMPANY</b>
					</span>
				</Link>
				<div className="topbar-actions">
					<Link className="navlink" to="/ajuda">
						Ajuda
					</Link>
					{session?.accountType === "participant" ? (
						<>
							<Link className="navlink" to="/minhas-cartelas">
								Minhas cartelas
							</Link>
							<button className="navlink" onClick={logout}>
								Sair
							</button>
						</>
					) : (
						showAdministration &&
						(session ? (
							<>
								<Link className="navlink" to="/admin">
									{session.companyName}
								</Link>
								<button className="navlink" onClick={logout}>
									Sair
								</button>
							</>
						) : (
							<>
								<Link className="navlink" to="/minhas-cartelas">
									Minhas cartelas
								</Link>
								<Link className="navlink" to="/entrar">
									Entrar
								</Link>
							</>
						))
					)}
					<ThemeToggle />
				</div>
			</nav>
			{children}
			<ApplicationFooter application={application} />
		</div>
	);
}
