import { HubConnectionBuilder } from "@microsoft/signalr";
import { useEffect, useRef, useState } from "react";
import { API_URL } from "../../../shared/api/httpClient";

type NumberDrawnEvent = {
	roundId: string;
	number: number;
	sequence: number;
	winnersDetected: number;
	statistics: {
		totalCards: number;
		oneNumberAway: number;
		twoNumbersAway: number;
		threeNumbersAway: number;
		awardedCards: number;
	};
};

type LiveBingoOptions = {
	onNumberDrawn?: (event: NumberDrawnEvent) => void;
};

export function useLiveBingo(eventId?: string, _roundId?: string, onChange?: () => void, options?: LiveBingoOptions) {
	const [state, setState] = useState("Conectando");
	const optionsReference = useRef(options);

	useEffect(() => {
		optionsReference.current = options;
	}, [options]);

	useEffect(() => {
		if (!eventId) return;

		const connection = new HubConnectionBuilder().withUrl(`${API_URL}/hubs/bingo`).withAutomaticReconnect().build();
		connection.on("NumberDrawn", (event: NumberDrawnEvent) => {
			if (optionsReference.current?.onNumberDrawn) {
				optionsReference.current.onNumberDrawn(event);
				return;
			}

			onChange?.();
		});
		[
			"WinnerDetected",
			"WinningCardDetected",
			"TieBreakerStarted",
			"WinnerRevealed",
			"WinnerPresentationClosed",
			"PrizeStageChanged",
			"RoundFinished",
			"RoundStarted"
		].forEach((event) => connection.on(event, () => onChange?.()));
		connection.onreconnecting(() => setState("Reconectando"));
		connection.onreconnected(() => {
			setState("Conectado");
			onChange?.();
		});
		connection.onclose(() => setState("Desconectado"));
		void connection
			.start()
			.then(async () => {
				await connection.invoke("JoinEvent", eventId);
				setState("Conectado");
			})
			.catch(() => setState("Sem conexão"));

		return () => {
			void connection.stop();
		};
	}, [eventId, onChange]);

	return state;
}
