export type CardMarkingMode = "Automatic" | "ManualRequired" | "AssistedManual";
export type ParticipantType = "Employee" | "FamilyMember" | "Guest";
export type WinningPattern =
	| "HorizontalLine"
	| "TwoHorizontalLines"
	| "FourCorners"
	| "FullCard"
	| "BDiagonal"
	| "ODiagonal"
	| "BColumn"
	| "IColumn"
	| "NColumn"
	| "GColumn"
	| "OColumn"
	| "XPattern"
	| "TPattern"
	| "Frame"
	| "Cross";
export type EventSummary = {
	id: string;
	name: string;
	publicCode: string;
	status: string;
	cardsPerParticipant?: number;
	markingMode: CardMarkingMode;
	isCardPurchaseOpen?: boolean;
	cardPurchaseLimit?: number | null;
	cardPurchasePerParticipantLimit?: number | null;
	cardPurchaseClosesAt?: string | null;
	cardPurchaseLowStockThreshold?: number | null;
	cardPurchaseRemaining?: number | null;
	cardPurchaseCancellationReason?: string | null;
	createdAt: string;
};
export type EventPage = { items: EventSummary[]; page: number; pageSize: number; totalItems: number; totalPages: number };
export type EventDetails = {
	id: string;
	name: string;
	publicCode: string;
	status: string;
	cardsPerParticipant?: number;
	isCardPurchaseOpen?: boolean;
	cardPurchaseLimit?: number | null;
	cardPurchasePerParticipantLimit?: number | null;
	cardPurchaseClosesAt?: string | null;
	cardPurchaseLowStockThreshold?: number | null;
	cardPurchaseRemaining?: number | null;
	cardPurchaseCancellationReason?: string | null;
	cardPurchaseDashboard?: {
		total: number;
		reserved: number;
		activated: number;
		eligibleForNextRound: number;
		awaitingActivation: number;
		waitlistEntries: number;
	};
	participants: number;
	cards: number;
	eligibleCards: number;
	awardedCards: {
		items: { winnerId: string; publicCode: string; participantName: string; roundName: string; prizeName: string }[];
		page: number;
		pageSize: number;
		totalItems: number;
		totalPages: number;
	};
	rounds: {
		id: string;
		name: string;
		status: string;
		createdAt: string;
		stages: {
			sequence: number;
			prizeName: string;
			pattern: WinningPattern;
			prizeImageDataUrl?: string;
			isActive: boolean;
			isCompleted: boolean;
		}[];
	}[];
};
export type PrizeStage = { prizeName: string; pattern: string; prizeImageDataUrl?: string; isActive: boolean; isCompleted: boolean };
export type RoundStatistics = {
	totalCards: number;
	oneNumberAway: number;
	twoNumbersAway: number;
	threeNumbersAway: number;
	awardedCards: number;
};
export type PublicEvent = {
	id: string;
	name: string;
	publicCode: string;
	status: string;
	markingMode: string;
	isCardPurchaseOpen?: boolean;
	cardPurchaseLimit?: number | null;
	cardPurchasePerParticipantLimit?: number | null;
	cardPurchaseClosesAt?: string | null;
	cardPurchaseLowStockThreshold?: number | null;
	cardPurchaseRemaining?: number | null;
	cardPurchaseCancellationReason?: string | null;
	cardPurchaseWaitlistEntries?: number;
	participants: number;
	cards: number;
	round?: {
		id: string;
		name: string;
		sequence: number;
		status: string;
		sequenceHash?: string;
		currentPrize?: string;
		currentPrizeImageDataUrl?: string;
		presentationPrizeImageDataUrl?: string;
		eligibleCards?: number;
		stages: PrizeStage[];
		drawnNumbers: number[];
		winnerDetectedCount: number;
		tieBreakerRequired: boolean;
		hasPrizeDeliveryPending?: boolean;
		statistics?: RoundStatistics;
		winner?: {
			participantName: string;
			prizeName: string;
			pattern: string;
			prizeImageDataUrl?: string;
			isPrizeDeliveryPending: boolean;
			tieBreakers?: { participantName: string; cardCode: string; number: number; isWinner: boolean }[];
		};
	};
};
export type CardState = {
	id: string;
	publicCode: string;
	eventStatus?: string;
	participantName?: string;
	responsibleEmployeeName?: string;
	isWinner: boolean;
	numbers: number[][];
	markingMode: string;
	roundId?: string;
	roundStatus?: string;
	currentPrize?: string;
	currentPattern?: string;
	remainingNumbersToWin?: number;
	drawnNumbers: number[];
	markedNumbers: number[];
	lastSequence: number;
	canGenerateNextCard: boolean;
};
export type PrizeDraft = { sequence: number; prizeName: string; pattern: WinningPattern; prizeImageDataUrl?: string };
export type CreateEventInput = { name: string; markingMode: CardMarkingMode };
export type JoinEventInput = {
	name?: string;
	type?: ParticipantType;
	employeeRegistration?: string;
	responsibleEmployeeName?: string;
	cardsQuantity?: number;
	invitationCode?: string;
};
export type PublicAudit = {
	eventInfo: { name: string; publicCode: string; status: string; createdAt: string };
	participants: { id: string; name: string; joinedAt: string }[];
	cards: { id: string; publicCode: string; type: string; status: string; participantId?: string; createdAt: string }[];
	rounds: {
		name: string;
		sequence: number;
		status: string;
		sequenceHash?: string;
		fullSequence?: number[];
		drawnNumbers: { number: number; sequence: number; drawnAt: string }[];
		stages: { prizeName: string; pattern: string; isCompleted: boolean }[];
		winners: {
			participantName: string;
			cardCode: string;
			prizeName: string;
			isWinner: boolean;
			tieBreakerNumber?: number;
			detectedAt: string;
			revealedAt?: string;
		}[];
	}[];
	entries: {
		items: { action: string; details: string; occurredAt: string }[];
		page: number;
		pageSize: number;
		totalItems: number;
		totalPages: number;
	};
};
