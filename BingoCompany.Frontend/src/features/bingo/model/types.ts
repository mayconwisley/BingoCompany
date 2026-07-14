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
	markingMode: CardMarkingMode;
	createdAt: string;
};
export type EventPage = { items: EventSummary[]; page: number; pageSize: number; totalItems: number; totalPages: number };
export type EventDetails = {
	id: string;
	name: string;
	publicCode: string;
	status: string;
	participants: number;
	cards: number;
	participantList: { id: string; name: string; type: string }[];
	cardList: { publicCode: string; type: string; status: string; fingerprint: string; participantId?: string }[];
	rounds: {
		id: string;
		name: string;
		status: string;
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
		stages: PrizeStage[];
		drawnNumbers: number[];
		winnerDetectedCount: number;
		tieBreakerRequired: boolean;
		statistics?: RoundStatistics;
		winner?: {
			participantName: string;
			prizeName: string;
			pattern: string;
			prizeImageDataUrl?: string;
			tieBreakers?: { participantName: string; number: number; isWinner: boolean }[];
		};
	};
};
export type CardState = {
	id: string;
	publicCode: string;
	numbers: number[][];
	markingMode: string;
	roundId?: string;
	roundStatus?: string;
	currentPrize?: string;
	currentPattern?: string;
	drawnNumbers: number[];
	markedNumbers: number[];
	lastSequence: number;
	canGenerateNextCard: boolean;
};
export type PrizeDraft = { sequence: number; prizeName: string; pattern: WinningPattern; prizeImageDataUrl?: string };
export type CreateEventInput = { name: string; markingMode: CardMarkingMode; cardsPerParticipant: number };
export type JoinEventInput = { name: string; type: ParticipantType; employeeRegistration?: string; responsibleEmployeeName?: string };
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
			prizeName: string;
			isWinner: boolean;
			tieBreakerNumber?: number;
			detectedAt: string;
			revealedAt?: string;
		}[];
	}[];
	entries: { action: string; details: string; occurredAt: string }[];
};
