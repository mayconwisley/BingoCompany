import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { OperatorPage } from "./OperatorPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({
	bingoApi: { cancelRound: vi.fn(), closeWinnerPresentation: vi.fn(), draw: vi.fn(), getPublicEvent: vi.fn(), markPrizeDeclined: vi.fn(), markPrizeDelivered: vi.fn(), reveal: vi.fn(), startRound: vi.fn() }
}));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("OperatorPage", () => {
	beforeEach(() => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 1,
			cards: 1,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				stages: [],
				drawnNumbers: [],
				winnerDetectedCount: 0,
				tieBreakerRequired: false
			}
		});
	});

	it("sorteia a primeira pedra de uma rodada já iniciada sem tentar iniciá-la novamente", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "SORTEAR PRÓXIMA PEDRA" }));

		expect(bingoApi.draw).toHaveBeenCalledWith("event-1", "round-1");
		expect(bingoApi.startRound).not.toHaveBeenCalled();
	});

	it("mantém o sorteio bloqueado enquanto o telão apresenta o vencedor", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 1,
			cards: 1,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false,
				winner: { participantName: "Ana", prizeName: "Linha", pattern: "HorizontalLine", isPrizeDeliveryPending: false }
			}
		});

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		const drawButton = await screen.findByRole("button", { name: "SORTEAR PRÓXIMA PEDRA" });
		expect(drawButton).toBeDisabled();
		fireEvent.click(screen.getByRole("button", { name: "CONTINUAR SORTEIO NO TELÃO" }));

		expect(bingoApi.closeWinnerPresentation).toHaveBeenCalledWith("event-1", "round-1");
	});

	it("registra que o vencedor não retirou o prêmio para manter a mesma regra em sorteio", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 1,
			cards: 1,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "WinnerDetected",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 1,
				tieBreakerRequired: false,
				winner: { participantName: "Ana", prizeName: "Linha", pattern: "HorizontalLine", isPrizeDeliveryPending: true }
			}
		});

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "VENCEDOR NÃO RETIROU O PRÊMIO" }));

		expect(bingoApi.markPrizeDeclined).toHaveBeenCalledWith("event-1", "round-1");
	});

	it("permite cancelar uma rodada em sorteio", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "CANCELAR RODADA" }));

		expect(bingoApi.cancelRound).toHaveBeenCalledWith("event-1", "round-1");
	});
});
