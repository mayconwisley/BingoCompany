import { http } from "../../../shared/api/httpClient";
import type {
	CardState,
	CreateEventInput,
	EventDetails,
	EventPage,
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
	listEvents: (page = 1, pageSize = 12) => http<EventPage>(`/api/events?page=${page}&pageSize=${pageSize}`),
	getEvent: (eventId: string) => http<EventDetails>(`/api/events/${eventId}`),
	createEvent: (request: CreateEventInput) => http<EventSummary>("/api/events", { method: "POST", body: JSON.stringify(request) }),
	openRegistration: (eventId: string, cardsPerParticipant: number) =>
		http<void>(`/api/events/${eventId}/registration/open`, { method: "POST", body: JSON.stringify({ cardsPerParticipant }) }),
	openCardPurchase: (eventId: string, quantity: number) =>
		http<void>(`/api/events/${eventId}/card-purchase/open`, { method: "POST", body: JSON.stringify({ quantity }) }),
	updateCardPurchase: (eventId: string, quantity: number) =>
		http<void>(`/api/events/${eventId}/card-purchase`, { method: "PUT", body: JSON.stringify({ quantity }) }),
	cancelCardPurchase: (eventId: string, reason: string) =>
		http<void>(`/api/events/${eventId}/card-purchase/cancel`, { method: "POST", body: JSON.stringify({ reason }) }),
	joinEvent: (eventId: string, request: JoinEventInput) =>
		http<RegistrationResult>(`/api/events/${eventId}/participants`, { method: "POST", body: JSON.stringify(request) }),
	createRound: (eventId: string, name: string, stages: PrizeDraft[]) =>
		http<{ id: string }>(`/api/events/${eventId}/rounds`, { method: "POST", body: JSON.stringify({ name, stages }) }),
	updateRound: (eventId: string, roundId: string, name: string, stages: PrizeDraft[]) =>
		http<void>(`/api/events/${eventId}/rounds/${roundId}`, { method: "PUT", body: JSON.stringify({ name, stages }) }),
	startRound: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/start`, { method: "POST" }),
	cancelRound: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/cancel`, { method: "POST" }),
	draw: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/draw`, { method: "POST" }),
	reveal: (eventId: string, roundId: string) => http<void>(`/api/events/${eventId}/rounds/${roundId}/reveal`, { method: "POST" }),
	markPrizeDelivered: (eventId: string, roundId: string) =>
		http<void>(`/api/events/${eventId}/rounds/${roundId}/prize-delivered`, { method: "POST" }),
	markPrizeDeclined: (eventId: string, roundId: string) =>
		http<void>(`/api/events/${eventId}/rounds/${roundId}/prize-declined`, { method: "POST" }),
	closeWinnerPresentation: (eventId: string, roundId: string) =>
		http<void>(`/api/events/${eventId}/rounds/${roundId}/winner-presentation/close`, { method: "POST" }),
	finishEvent: (eventId: string) => http<void>(`/api/events/${eventId}/finish`, { method: "POST" }),
	generatePrintedCards: (eventId: string, quantity: number) =>
		http<PrintedCard[]>(`/api/events/${eventId}/cards/printed`, { method: "POST", body: JSON.stringify({ quantity }) }),
	getPrintedCards: (eventId: string) => http<PrintedCard[]>(`/api/events/${eventId}/cards/printed`),
	assignPrintedCard: (eventId: string, cardCode: string, participantId: string) =>
		http<void>(`/api/events/${eventId}/cards/${cardCode}/assign`, { method: "POST", body: JSON.stringify({ participantId }) }),
	registerPrintedCard: (
		eventId: string,
		cardCode: string,
		request: { name: string; type: string; employeeRegistration?: string; responsibleEmployeeName?: string }
	) =>
		http<{ cardCode: string; participantName: string }>(`/api/events/${eventId}/cards/${cardCode}/register`, {
			method: "POST",
			body: JSON.stringify(request)
		}),
	activatePrintedCard: (eventId: string, cardCode: string) =>
		http<void>(`/api/events/${eventId}/cards/${cardCode}/activate`, { method: "POST" }),
	getCard: (eventId: string, cardCode: string) => http<CardState>(`/api/events/${eventId}/cards/${cardCode}/state`),
	generateNextCard: (eventId: string, cardCode: string) =>
		http<{ publicCode: string }>(`/api/events/${eventId}/cards/${cardCode}/next`, { method: "POST" }),
	mark: (eventId: string, cardCode: string, number: number) =>
		http<void>(`/api/events/${eventId}/cards/${cardCode}/marks`, { method: "POST", body: JSON.stringify({ number }) }),
	validatePrintedWinner: (eventId: string, roundId: string, cardCode: string) =>
		http<{ participantName: string; cardCode: string; tieBreakerRequired: boolean }>(
			`/api/events/${eventId}/rounds/${roundId}/printed-cards/${cardCode}/validate-winner`,
			{ method: "POST" }
		),
	getPublicEvent: (code: string) => http<PublicEvent>(`/api/public/events/${code}`),
	join: (code: string, request: JoinEventInput) =>
		http<RegistrationResult>(`/api/public/events/${code}/join`, { method: "POST", body: JSON.stringify(request) }),
	activateDigitalCard: (eventCode: string, cardCode: string) =>
		http<void>(`/api/public/events/${eventCode}/cards/${cardCode}/activate`, { method: "POST" }),
	getAudit: (code: string, page = 1, pageSize = 25, roundSequence?: number) => {
		const filter = roundSequence ? `&roundSequence=${roundSequence}` : "";
		return http<PublicAudit>(`/api/public/events/${code}/audit?page=${page}&pageSize=${pageSize}${filter}`);
	},
	getMyCards: (page = 1, pageSize = 5) => http<ParticipantCardsPage>(`/api/participant/cards?page=${page}&pageSize=${pageSize}`)
};

export type RegistrationResult = {
	participantId: string;
	cards: { cardId: string; publicCode: string; numbers: number[][]; status: string }[];
};

export type ParticipantCard = {
	eventId: string;
	eventPublicCode: string;
	eventName: string;
	eventStatus: string;
	publicCode: string;
	status: string;
	invalidationReason?: string;
	eventCancellationReason?: string;
	createdAt: string;
};
export type ParticipantCardsPage = {
	activeCards: ParticipantCard[];
	history: { items: ParticipantCard[]; page: number; pageSize: number; totalItems: number; totalPages: number };
};
