import { fireEvent, render, screen, waitFor, within } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import type { EventDetails } from "../../features/bingo/model/types";
import { EventSetupPage } from "./EventSetupPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getEvent: vi.fn() } }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => children }));

const finishedEvent: EventDetails = {
	id: "event-1",
	name: "Confraternização",
	publicCode: "EVENTO1",
	status: "Finished",
	participants: 1,
	cards: 1,
	eligibleCards: 0,
	awardedCards: { items: [], page: 1, pageSize: 3, totalItems: 0, totalPages: 0 },
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
	it("sugere o próximo número ao preparar uma nova rodada", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({
			...finishedEvent,
			status: "Running",
			rounds: [
				finishedEvent.rounds[0],
				{ id: "round-2", name: "Rodada especial", status: "Finished", createdAt: "2026-07-14T11:30:00Z", stages: [] }
			]
		});

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		await waitFor(() => expect(screen.getByLabelText("Nome da rodada")).toHaveValue("Rodada 3"));
	});

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

		expect(screen.getByLabelText("Resumo do evento")).toBeInTheDocument();
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
			awardedCards: {
				items: [
					{
						winnerId: "winner-1",
						publicCode: "BINGO-12",
						participantName: "Ana",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					}
				],
				page: 1,
				pageSize: 3,
				totalItems: 1,
				totalPages: 1
			}
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

	it("carrega as cartelas premiadas em páginas de três itens", async () => {
		vi.mocked(bingoApi.getEvent)
			.mockResolvedValueOnce({
				...finishedEvent,
				awardedCards: {
					items: [
						{
							winnerId: "winner-1",
							publicCode: "CARTELA-1",
							participantName: "Ana",
							roundName: "Rodada 1",
							prizeName: "Prêmio 1"
						},
						{
							winnerId: "winner-2",
							publicCode: "CARTELA-2",
							participantName: "Bruno",
							roundName: "Rodada 1",
							prizeName: "Prêmio 2"
						},
						{
							winnerId: "winner-3",
							publicCode: "CARTELA-3",
							participantName: "Carla",
							roundName: "Rodada 2",
							prizeName: "Prêmio 3"
						}
					],
					page: 1,
					pageSize: 3,
					totalItems: 4,
					totalPages: 2
				}
			})
			.mockResolvedValueOnce({
				...finishedEvent,
				awardedCards: {
					items: [
						{
							winnerId: "winner-4",
							publicCode: "CARTELA-4",
							participantName: "Diego",
							roundName: "Rodada 2",
							prizeName: "Prêmio 4"
						}
					],
					page: 2,
					pageSize: 3,
					totalItems: 4,
					totalPages: 2
				}
			});

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("CARTELA-1")).toBeInTheDocument();
		expect(screen.getByText("CARTELA-3")).toBeInTheDocument();
		expect(screen.queryByText("CARTELA-4")).not.toBeInTheDocument();
		expect(screen.getByText("Página 1 de 2")).toBeInTheDocument();

		fireEvent.click(screen.getByRole("button", { name: "Próxima" }));

		await waitFor(() => expect(bingoApi.getEvent).toHaveBeenLastCalledWith("event-1", 2));
		expect(await screen.findByText("CARTELA-4")).toBeInTheDocument();
		expect(screen.queryByText("CARTELA-1")).not.toBeInTheDocument();
	});

	it("substitui a página anterior mesmo quando as cartelas têm os mesmos dados visíveis", async () => {
		const firstPage = {
			...finishedEvent,
			awardedCards: {
				items: [
					{
						winnerId: "winner-1",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					},
					{
						winnerId: "winner-2",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					},
					{
						winnerId: "winner-3",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					}
				],
				page: 1,
				pageSize: 3,
				totalItems: 6,
				totalPages: 2
			}
		};
		const secondPage = {
			...finishedEvent,
			awardedCards: {
				items: [
					{
						winnerId: "winner-4",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					},
					{
						winnerId: "winner-5",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					},
					{
						winnerId: "winner-6",
						publicCode: "CARTELA-REPETIDA",
						participantName: "Teste",
						roundName: "Rodada 1",
						prizeName: "Vale-presente"
					}
				],
				page: 2,
				pageSize: 3,
				totalItems: 6,
				totalPages: 2
			}
		};
		vi.mocked(bingoApi.getEvent).mockResolvedValueOnce(firstPage).mockResolvedValueOnce(secondPage).mockResolvedValueOnce(firstPage);

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		const awardedCardsList = await screen.findByRole("list", { name: "Lista de cartelas premiadas" });
		expect(within(awardedCardsList).getAllByRole("listitem")).toHaveLength(3);
		fireEvent.click(screen.getByRole("button", { name: "Próxima" }));
		await waitFor(() => expect(screen.getByText("Página 2 de 2")).toBeInTheDocument());
		expect(within(awardedCardsList).getAllByRole("listitem")).toHaveLength(3);
		fireEvent.click(screen.getByRole("button", { name: "Anterior" }));
		await waitFor(() => expect(screen.getByText("Página 1 de 2")).toBeInTheDocument());
		expect(within(awardedCardsList).getAllByRole("listitem")).toHaveLength(3);
	});

	it("ordena as rodadas pela criação e bloqueia a operação da rodada finalizada", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({
			...finishedEvent,
			status: "Running",
			eligibleCards: 1,
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
		vi.mocked(bingoApi.getEvent).mockResolvedValue({ ...finishedEvent, status: "RegistrationOpen", rounds: [] });

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

	it("bloqueia os links de inscrição depois que a primeira rodada começa", async () => {
		vi.mocked(bingoApi.getEvent).mockResolvedValue({ ...finishedEvent, status: "Running" });

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1?code=EVENTO1"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId" element={<EventSetupPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText(/As inscrições estão fechadas/)).toBeInTheDocument();
		expect(screen.getByText("Inscrição").closest("[aria-disabled]")).toHaveAttribute("aria-disabled", "true");
		expect(screen.queryByRole("button", { name: "Copiar link do QR Code" })).not.toBeInTheDocument();
		expect(screen.queryByRole("button", { name: "Copiar link de inscrição" })).not.toBeInTheDocument();
	});
});
