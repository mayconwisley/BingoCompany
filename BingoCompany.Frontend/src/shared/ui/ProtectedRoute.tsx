import type { PropsWithChildren } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { getSession } from "../auth/session";

export function ProtectedRoute({ children }: PropsWithChildren) {
	const location = useLocation();
	const session = getSession();
	if (!session) return <Navigate to="/entrar" replace state={{ from: location.pathname }} />;
	return session.accountType === "participant" ? <Navigate to="/minhas-cartelas" replace /> : children;
}
