import type { PropsWithChildren } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useSession } from "../auth/SessionContext";
import { RouteLoadingState } from "./RouteLoadingState";

export function ProtectedRoute({ children }: PropsWithChildren) {
	const location = useLocation();
	const { session, isLoading } = useSession();
	if (isLoading) return <RouteLoadingState />;
	if (!session) return <Navigate to="/entrar" replace state={{ from: location.pathname }} />;
	return session.accountType === "participant" ? <Navigate to="/minhas-cartelas" replace /> : children;
}
