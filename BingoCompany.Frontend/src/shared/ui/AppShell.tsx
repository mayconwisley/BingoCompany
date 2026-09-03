import { useState } from "react";
import type { PropsWithChildren } from "react";
import { Link, useNavigate } from "react-router-dom";
import { authApi } from "../../features/bingo";
import { useApplicationInfo } from "../../app/ApplicationInfoContext";
import { clearSession } from "../auth/session";
import { useSession } from "../auth/SessionContext";
import { ApplicationFooter } from "./ApplicationFooter";
import { ThemeToggle } from "./ThemeToggle";

type Props = PropsWithChildren<{ showAdministration?: boolean }>;

export function AppShell({ children, showAdministration = true }: Props) {
	const navigate = useNavigate();
	const { session } = useSession();
	const application = useApplicationInfo();
	const [isNavigationOpen, setIsNavigationOpen] = useState(false);

	const logout = async () => {
		try {
			await authApi.logout();
		} finally {
			clearSession();
			navigate("/");
		}
	};

	const closeNavigation = () => setIsNavigationOpen(false);
	const navigationLinks =
		session?.accountType === "participant" ? (
			<>
				<Link className="navigation-link" to="/minhas-cartelas" onClick={closeNavigation}>
					Minhas cartelas
				</Link>
				<button className="navigation-link" onClick={logout}>
					Sair
				</button>
			</>
		) : session ? (
			showAdministration && (
				<>
					<Link className="navigation-link" to="/admin" onClick={closeNavigation}>
						{session.companyName}
					</Link>
					<button className="navigation-link" onClick={logout}>
						Sair
					</button>
				</>
			)
		) : (
			<>
				<Link className="navigation-link" to="/minhas-cartelas" onClick={closeNavigation}>
					Minhas cartelas
				</Link>
				<Link className="navigation-link" to="/entrar" onClick={closeNavigation}>
					Entrar
				</Link>
			</>
		);

	return (
		<div className="app-shell">
			<nav className="topbar">
				<Link className="brand" to="/" aria-label="Bingo Company, página inicial">
					<img className="brand-logo" src={`${import.meta.env.BASE_URL}assets/bingo-company-logo-512.png`} alt="" />
					<span>
						BINGO <b>COMPANY</b>
					</span>
				</Link>
				<div className="topbar-actions">
					<Link className="navlink" to="/ajuda">
						Ajuda
					</Link>
					<div className="desktop-navigation">{navigationLinks}</div>
					<ThemeToggle />
					<button
						type="button"
						className="mobile-menu-toggle"
						aria-expanded={isNavigationOpen}
						aria-controls="mobile-navigation"
						aria-label={isNavigationOpen ? "Fechar menu de navegação" : "Abrir menu de navegação"}
						onClick={() => setIsNavigationOpen((isOpen) => !isOpen)}
					>
						☰
					</button>
				</div>
			</nav>
			{isNavigationOpen && (
				<nav id="mobile-navigation" className="mobile-navigation" aria-label="Navegação principal">
					<Link className="mobile-navlink" to="/ajuda" onClick={closeNavigation}>
						Ajuda
					</Link>
					<div className="mobile-navigation-links">{navigationLinks}</div>
				</nav>
			)}
			{children}
			<ApplicationFooter application={application} />
		</div>
	);
}
