import { Navigate, Route, Routes } from "react-router-dom";
import { AdminPage } from "../pages/AdminPage";
import { AuditPage } from "../pages/AuditPage";
import { CardPage } from "../pages/CardPage";
import { DisplayPage } from "../pages/DisplayPage";
import { EventSetupPage } from "../pages/EventSetupPage";
import { HomePage } from "../pages/HomePage";
import { JoinPage } from "../pages/JoinPage";
import { OperatorPage } from "../pages/OperatorPage";
import { PrintCardsPage } from "../pages/PrintCardsPage";
export function App(){return <Routes><Route path="/" element={<HomePage/>}/><Route path="/admin" element={<AdminPage/>}/><Route path="/admin/eventos/:eventId" element={<EventSetupPage/>}/><Route path="/admin/eventos/:eventId/impressao" element={<PrintCardsPage/>}/><Route path="/participar/:publicCode" element={<JoinPage/>}/><Route path="/cartela/:eventId/:cardCode" element={<CardPage/>}/><Route path="/operacao/:eventId/:roundId" element={<OperatorPage/>}/><Route path="/display/:publicCode" element={<DisplayPage/>}/><Route path="/auditoria/:publicCode" element={<AuditPage/>}/><Route path="*" element={<Navigate to="/" replace/>}/></Routes>}
