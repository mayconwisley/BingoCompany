import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo";
import { getSession } from "../../shared/auth/session";
import { JoinPage } from "./JoinPage";

vi.mock("../../features/bingo", () => ({
	bingoApi: {
		getPublicEvent: vi.fn(),
		join: vi.fn(),
		activateDigitalCard: vi.fn()
	}
}));
vi.mock("../../shared/auth/session", () => ({
	getSession: vi.fn(),
	saveSession: vi.fn(),
	clearSession: vi.fn(),
	sessionChangedEvent: "bingo-company-session-changed"
}));

describe("JoinPage", () => {
	beforeEach(() => {
		vi.mocked(getSession).mockReturnValue(undefined);
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "EVENTO",
			status: "RegistrationOpen",
			markingMode: "Automatic",
			participants: 0,
			cards: 0
		});
	});

	it("abre todas as cartelas ativas após uma inscrição pública com múltiplas cartelas", async () => {
		vi.mocked(bingoApi.join).mockResolvedValue({
			participantId: "participant-1",
			cards: [
				{ cardId: "card-1", publicCode: "CARD001", numbers: [], status: "Assigned" },
				{ cardId: "card-2", publicCode: "CARD002", numbers: [], status: "Assigned" }
			]
		});
		render(
			<MemoryRouter initialEntries={["/participar/EVENTO"]}>
				<Routes>
					<Route path="/participar/:publicCode" element={<JoinPage />} />
					<Route path="/cartela/:eventId/:cardCode" element={<p>Cartela aberta</p>} />
					<Route path="/cartelas/:eventId" element={<p>Cartelas abertas</p>} />
				</Routes>
			</MemoryRouter>
		);

		await screen.findByText("Festa");
		fireEvent.change(screen.getByLabelText("Seu nome"), { target: { value: "Ana" } });
		fireEvent.click(screen.getByRole("button", { name: "Gerar minha cartela" }));

		expect(await screen.findByText("Cartelas abertas")).toBeInTheDocument();
		expect(bingoApi.activateDigitalCard).not.toHaveBeenCalled();
	});

	it("envia a quantidade escolhida quando a compra de cartelas está aberta", async () => {
		vi.mocked(getSession).mockReturnValue({ name: "Ana", companyName: "", accountType: "participant" });
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "EVENTO",
			status: "RegistrationOpen",
			markingMode: "Automatic",
			isCardPurchaseOpen: true,
			participants: 0,
			cards: 0
		});
		vi.mocked(bingoApi.join).mockResolvedValue({
			participantId: "participant-1",
			cards: [{ cardId: "card-1", publicCode: "CARD001", numbers: [], status: "Assigned" }]
		});
		render(
			<MemoryRouter initialEntries={["/participar/EVENTO"]}>
				<Routes>
					<Route path="/participar/:publicCode" element={<JoinPage />} />
					<Route path="/cartela/:eventId/:cardCode" element={<p>Cartela aberta</p>} />
				</Routes>
			</MemoryRouter>
		);

		await screen.findByText("Festa");
		fireEvent.change(screen.getByLabelText("Seu nome"), { target: { value: "Ana" } });
		fireEvent.change(screen.getByLabelText("Quantidade de cartelas digitais"), { target: { value: "4" } });
		fireEvent.click(screen.getByRole("button", { name: "Gerar minha cartela" }));

		expect(await screen.findByText(/CARD001/)).toBeInTheDocument();
		expect(bingoApi.join).toHaveBeenCalledWith("EVENTO", expect.objectContaining({ cardsQuantity: 4 }));
	});
});
