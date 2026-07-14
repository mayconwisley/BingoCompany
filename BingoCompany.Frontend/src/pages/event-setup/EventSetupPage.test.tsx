import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import type { EventDetails } from "../../features/bingo/model/types";
import { EventSetupPage } from "./EventSetupPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getEvent: vi.fn() } }));

const finishedEvent: EventDetails = {
	id: "event-1",
	name: "Confraternização",
	publicCode: "EVENTO1",
	status: "Finished",
	participants: 1,
	cards: 1,
	participantList: [],
	cardList: [],
	rounds: [
		{
			id: "round-1",
			name: "Rodada 1",
			status: "Finished",
			stages: []
		}
	]
};

describe("EventSetupPage", () => {
	it("mantém somente a auditoria acessível quando o evento está finalizado", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue(finishedEvent);

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		const auditLink = await screen.findByRole("link", { name: /auditoria/i });

		expect(auditLink).toHaveAttribute("href", "/auditoria/EVENTO1");
		expect(screen.getByText("Inscrição").closest("[aria-disabled]")).toHaveAttribute("aria-disabled", "true");
		expect(screen.getByText("Telão").closest("[aria-disabled]")).toHaveAttribute("aria-disabled", "true");
		expect(screen.getByText("Cartelas").closest("[aria-disabled]")).toHaveAttribute("aria-disabled", "true");
		expect(screen.getByRole("button", { name: "Abrir operação" })).toBeDisabled();
		expect(screen.getByRole("button", { name: "Encerrar evento e publicar auditoria" })).toBeDisabled();
		expect(screen.getByLabelText("Nome da rodada")).toBeDisabled();
		expect(screen.getByLabelText("Quantidade de cartelas impressas")).toBeDisabled();
	});
});
