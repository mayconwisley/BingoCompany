import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { ActiveCardsPage } from "./ActiveCardsPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getCard: vi.fn(), getMyCards: vi.fn() } }));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("ActiveCardsPage", () => {
	it("informa que as cartelas de um evento encerrado são apenas para consulta", async () => {
		vi.mocked(bingoApi.getMyCards).mockResolvedValue({
			activeCards: [
				{
					eventId: "event-1",
					eventPublicCode: "ABC",
					eventName: "Festa",
					eventStatus: "Finished",
					publicCode: "CARTELA-1",
					status: "Active",
					hasParticipatedInRound: false,
					isEligibleForNextRound: false,
					createdAt: "2026-07-15T00:00:00Z"
				}
			],
			history: { items: [], page: 1, pageSize: 5, totalItems: 0, totalPages: 0 }
		});
		vi.mocked(bingoApi.getCard).mockResolvedValue({
			id: "card-1",
			publicCode: "CARTELA-1",
			eventStatus: "Finished",
			isWinner: false,
			numbers: [[1, 16, 31, 46, 61]],
			markingMode: "Automatic",
			drawnNumbers: [54],
			markedNumbers: [],
			lastSequence: 0,
			canGenerateNextCard: false
		});

		render(
			<MemoryRouter initialEntries={["/cartelas/event-1"]}>
				<Routes>
					<Route path="/cartelas/:eventId" element={<ActiveCardsPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("Evento encerrado")).toBeInTheDocument();
		expect(
			screen.getByText("Este evento foi encerrado. Estas cartelas permanecem disponíveis apenas para consulta.")
		).toBeInTheDocument();
		expect(screen.queryByText("Aguardando rodada")).not.toBeInTheDocument();
		expect(screen.getByText("G-54")).toBeInTheDocument();
	});
});
