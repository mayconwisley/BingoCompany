import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { CardPage } from "./CardPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { generateNextCard: vi.fn(), getCard: vi.fn(), mark: vi.fn() } }));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("CardPage", () => {
	it("permite marcar somente a pedra atual na marcação manual obrigatória", async () => {
		vi.mocked(bingoApi.getCard).mockResolvedValue({
			id: "card-1",
			publicCode: "MANUAL",
			numbers: [[1, 16, 31, 46, 61]],
			markingMode: "ManualRequired",
			drawnNumbers: [1, 16],
			markedNumbers: [],
			lastSequence: 2,
			canGenerateNextCard: false
		});

		render(
			<MemoryRouter initialEntries={["/cartela/event-1/MANUAL"]}>
				<Routes>
					<Route path="/cartela/:eventId/:cardCode" element={<CardPage />} />
				</Routes>
			</MemoryRouter>
		);

		const previousNumber = await screen.findByRole("button", { name: "Número 1, sorteado" });
		const currentNumber = screen.getByRole("button", { name: "Número 16, sorteado" });
		expect(previousNumber).toBeDisabled();
		expect(currentNumber).toBeEnabled();
		fireEvent.click(currentNumber);
		expect(bingoApi.mark).toHaveBeenCalledWith("event-1", "MANUAL", 16);
	});

	it("identifica o familiar e o colaborador responsável na cartela digital", async () => {
		vi.mocked(bingoApi.getCard).mockResolvedValue({
			id: "card-1",
			publicCode: "FAMILIAR",
			participantName: "Marina Silva",
			responsibleEmployeeName: "Carlos Silva",
			numbers: [[1, 16, 31, 46, 61]],
			markingMode: "Automatic",
			drawnNumbers: [],
			markedNumbers: [],
			lastSequence: 0,
			canGenerateNextCard: false
		});

		render(
			<MemoryRouter initialEntries={["/cartela/event-1/FAMILIAR"]}>
				<Routes>
					<Route path="/cartela/:eventId/:cardCode" element={<CardPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("Marina Silva")).toBeInTheDocument();
		expect(screen.getByText("Colaborador responsável: Carlos Silva")).toBeInTheDocument();
	});

	it("permite gerar uma nova cartela quando a anterior está completa", async () => {
		vi.mocked(bingoApi.getCard).mockResolvedValue({
			id: "card-1",
			publicCode: "ANTERIOR",
			numbers: [
				[1, 16, 31, 46, 61],
				[2, 17, 32, 47, 62],
				[3, 18, 0, 48, 63],
				[4, 19, 34, 49, 64],
				[5, 20, 35, 50, 65]
			],
			markingMode: "Automatic",
			drawnNumbers: [],
			markedNumbers: [],
			lastSequence: 0,
			canGenerateNextCard: true
		});
		vi.mocked(bingoApi.generateNextCard).mockResolvedValue({ publicCode: "NOVA" });

		render(
			<MemoryRouter initialEntries={["/cartela/event-1/ANTERIOR"]}>
				<Routes>
					<Route path="/cartela/:eventId/:cardCode" element={<CardPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "Gerar nova cartela" }));

		expect(bingoApi.generateNextCard).toHaveBeenCalledWith("event-1", "ANTERIOR");
	});
});
