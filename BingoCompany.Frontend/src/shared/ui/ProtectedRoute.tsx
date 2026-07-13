import type { PropsWithChildren } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { getSession } from "../auth/session";

export function ProtectedRoute({ children }: PropsWithChildren)
{
    const location = useLocation();
    return getSession() ? children : <Navigate to="/entrar" replace state={{ from: location.pathname }} />;
}
