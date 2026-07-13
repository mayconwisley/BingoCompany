import { http } from "../../../shared/api/httpClient";
import type {
    CardState,
    CreateEventInput,
    EventDetails,
    EventSummary,
    JoinEventInput,
    PrizeDraft,
    PublicAudit,
    PublicEvent
} from "../model/types";

type PrintedCard = {
    publicCode: string;
    fingerprint: string;
    status: string;
    companyName: string;
    numbers: number[][];
    qrCodeValue: string;
};

export const bingoApi = {
    listEvents: () => http<EventSummary[]>("/api/events"),
    getEvent: (eventId: string) => http<EventDetails>(`/api/events/${eventId}`),
    createEvent: (request: CreateEventInput) => http<EventSummary>("/api/events", { method: "POST", body: JSON.stringify(request) }),
    openRegistration: (eventId: string) => http<void>(`/api/events/${eventId}/registration/open`, { method: "POST" }),
    joinEvent: (eventId: string, request: JoinEventInput) =>
        http<{ publicCode: string }>(`/api/events/${eventId}/participants`, { method: "POST", body: JSON.stringify(request) }),
    createRound: (eventId: string, name: string, stages: PrizeDraft[]) =>
        http<{ id: string }>(`/api/events/${eventId}/rounds`, { method: "POST", body: JSON.stringify({ name, stages }) }),
    updateRound: (eventId: string, roundId: string, name: string, stages: PrizeDraft[]) =>
        http<void>(`/api/events/${eventId}/rounds/${roundId}`, { method: "PUT", body: JSON.stringify({ name, stages }) }),
    startRound: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/start`, { method: "POST" }),
    draw: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/draw`, { method: "POST" }),
    reveal: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/reveal`, { method: "POST" }),
    closeWinnerPresentation: (eventId: string, roundId: string) =>
        http<void>(`/api/events/${eventId}/rounds/${roundId}/winner-presentation/close`, { method: "POST" }),
    finishEvent: (eventId: string) => http<void>(`/api/events/${eventId}/finish`, { method: "POST" }),
    generatePrintedCards: (eventId: string, quantity: number) =>
        http<PrintedCard[]>(`/api/events/${eventId}/cards/printed`, { method: "POST", body: JSON.stringify({ quantity }) }),
    getPrintedCards: (eventId: string) => http<PrintedCard[]>(`/api/events/${eventId}/cards/printed`),
    assignPrintedCard: (eventId: string, cardCode: string, participantId: string) =>
        http<void>(`/api/events/${eventId}/cards/${cardCode}/assign`, { method: "POST", body: JSON.stringify({ participantId }) }),
    activatePrintedCard: (eventId: string, cardCode: string) =>
        http<void>(`/api/events/${eventId}/cards/${cardCode}/activate`, { method: "POST" }),
    getCard: (eventId: string, cardCode: string) => http<CardState>(`/api/events/${eventId}/cards/${cardCode}/state`),
    generateNextCard: (eventId: string, cardCode: string) =>
        http<{ publicCode: string }>(`/api/events/${eventId}/cards/${cardCode}/next`, { method: "POST" }),
    mark: (eventId: string, cardCode: string, number: number) =>
        http<void>(`/api/events/${eventId}/cards/${cardCode}/marks`, { method: "POST", body: JSON.stringify({ number }) }),
    getPublicEvent: (code: string) => http<PublicEvent>(`/api/public/events/${code}`),
    join: (code: string, request: JoinEventInput) =>
        http<{ publicCode: string }>(`/api/public/events/${code}/join`, { method: "POST", body: JSON.stringify(request) }),
    getAudit: (code: string) => http<PublicAudit>(`/api/public/events/${code}/audit`)
};
