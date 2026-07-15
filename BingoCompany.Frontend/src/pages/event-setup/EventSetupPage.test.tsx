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
	awardedCards: [],
	rounds: [
		{
			id: "round-1",
			name: "Rodada 1",
			status: "Finished",
			createdAt: "2026-07-14T10:30:00Z",
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
		expect(screen.getByText(/Criada em 14\/07\/2026/)).toBeInTheDocument();
		expect(screen.getByRole("button", { name: "Encerrar evento e publicar auditoria" })).toBeDisabled();
		expect(screen.getByLabelText("Nome da rodada")).toBeDisabled();
		expect(screen.getByLabelText("Quantidade de cartelas impressas")).toBeDisabled();
	});

	it("mostra o total de cartelas e as cartelas premiadas ao encerrar o evento", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({
			...finishedEvent,
			cards: 12,
			awardedCards: [{ publicCode: "BINGO-12", participantName: "Ana", roundName: "Rodada 1", prizeName: "Vale-presente" }]
		});

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByLabelText("12 cartelas geradas para o evento")).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Cartelas premiadas" })).toBeInTheDocument();
		expect(screen.getByText("BINGO-12")).toBeInTheDocument();
		expect(screen.getByText("Rodada 1 · Vale-presente")).toBeInTheDocument();
	});

	it("ordena as rodadas pela criação e bloqueia a operação da rodada finalizada", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({
			...finishedEvent,
			status: "Running",
			cardList: [
				{ publicCode: "CARTELA1", type: "Digital", status: "Active", fingerprint: "fingerprint", participantId: "participant-1" }
			],
			rounds: [
				{ ...finishedEvent.rounds[0], name: "Rodada finalizada", createdAt: "2026-07-14T11:00:00Z" },
				{ id: "round-2", name: "Rodada em sorteio", status: "Drawing", createdAt: "2026-07-14T12:00:00Z", stages: [] }
			]
		});

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		const activeRound = await screen.findByText("Rodada em sorteio");
		const finishedRound = screen.getByText("Rodada finalizada");
		expect(activeRound.compareDocumentPosition(finishedRound) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
		const operationButtons = screen.getAllByRole("button", { name: "Abrir operação" });
		expect(operationButtons[0]).toBeEnabled();
		expect(operationButtons[1]).toBeDisabled();
	});

	it("bloqueia a abertura da operação sem cartela ativa", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({
			...finishedEvent,
			status: "RegistrationOpen",
			rounds: [{ ...finishedEvent.rounds[0], status: "Ready" }]
		});

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("button", { name: "Abrir operação" })).toBeDisabled();
	});

	it("oferece links copiáveis para o QR Code e para a inscrição", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({ ...finishedEvent, status: "Draft", rounds: [] });

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("button", { name: "Copiar link do QR Code" })).toBeInTheDocument();
		expect(screen.getByRole("button", { name: "Copiar link de inscrição" })).toBeInTheDocument();
	});
});
