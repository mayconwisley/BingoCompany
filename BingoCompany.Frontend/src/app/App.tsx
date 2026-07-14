import { lazy, Suspense } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { ProtectedRoute } from "../shared/ui/ProtectedRoute";
import { RouteLoadingState } from "../shared/ui/RouteLoadingState";

const AdminPage = lazy(() => import("../pages/admin/AdminPage").then(({ AdminPage: page }) => ({ default: page })));
const AuditPage = lazy(() => import("../pages/audit/AuditPage").then(({ AuditPage: page }) => ({ default: page })));
const AuthenticationPage = lazy(() =>
    import("../pages/authentication/AuthenticationPage").then(({ AuthenticationPage: page }) => ({ default: page }))
);
const CardPage = lazy(() => import("../pages/card/CardPage").then(({ CardPage: page }) => ({ default: page })));
const DisplayPage = lazy(() => import("../pages/display/DisplayPage").then(({ DisplayPage: page }) => ({ default: page })));
const EventSetupPage = lazy(() => import("../pages/event-setup/EventSetupPage").then(({ EventSetupPage: page }) => ({ default: page })));
const HelpPage = lazy(() => import("../pages/help/HelpPage").then(({ HelpPage: page }) => ({ default: page })));
const HomePage = lazy(() => import("../pages/home/HomePage").then(({ HomePage: page }) => ({ default: page })));
const JoinPage = lazy(() => import("../pages/join/JoinPage").then(({ JoinPage: page }) => ({ default: page })));
const OperatorPage = lazy(() => import("../pages/operator/OperatorPage").then(({ OperatorPage: page }) => ({ default: page })));
const PrintCardsPage = lazy(() => import("../pages/print-cards/PrintCardsPage").then(({ PrintCardsPage: page }) => ({ default: page })));
const RegistrationSharePage = lazy(() =>
    import("../pages/registration-share/RegistrationSharePage").then(({ RegistrationSharePage: page }) => ({ default: page }))
);

export function App() {
    return (
        <Suspense fallback={<RouteLoadingState />}>
            <Routes>
                <Route path="/" element={<HomePage />} />
                <Route path="/ajuda" element={<HelpPage />} />
                <Route path="/entrar" element={<AuthenticationPage mode="login" />} />
                <Route path="/cadastro" element={<AuthenticationPage mode="register" />} />
                <Route
                    path="/admin"
                    element={
                        <ProtectedRoute>
                            <AdminPage />
                        </ProtectedRoute>
                    }
                />
                <Route
                    path="/admin/eventos/:eventId"
                    element={
                        <ProtectedRoute>
                            <EventSetupPage />
                        </ProtectedRoute>
                    }
                />
                <Route
                    path="/admin/eventos/:eventId/impressao"
                    element={
                        <ProtectedRoute>
                            <PrintCardsPage />
                        </ProtectedRoute>
                    }
                />
                <Route path="/inscricao/:publicCode" element={<RegistrationSharePage />} />
                <Route path="/participar/:publicCode" element={<JoinPage />} />
                <Route path="/cartela/:eventId/:cardCode" element={<CardPage />} />
                <Route
                    path="/operacao/:eventId/:roundId"
                    element={
                        <ProtectedRoute>
                            <OperatorPage />
                        </ProtectedRoute>
                    }
                />
                <Route path="/display/:publicCode" element={<DisplayPage />} />
                <Route path="/auditoria/:publicCode" element={<AuditPage />} />
                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </Suspense>
    );
}
