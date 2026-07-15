import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { OperatorPage } from "./OperatorPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({
	bingoApi: {
		cancelRound: vi.fn(),
		closeWinnerPresentation: vi.fn(),
		draw: vi.fn(),
		getPublicEvent: vi.fn(),
		markPrizeDeclined: vi.fn(),
		markPrizeDelivered: vi.fn(),
		reveal: vi.fn(),
		startRound: vi.fn()
	}
}));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("OperatorPage", () => {
	beforeEach(() => {
		vi.clearAllMocks();
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

	it("exibe a última pedra com a letra da coluna da cartela", async () => {
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
				drawnNumbers: [46],
				winnerDetectedCount: 0,
				tieBreakerRequired: false
			}
		});

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("G-46", { selector: "strong" })).toBeInTheDocument();
	});

	it("volta para as configurações do evento", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
					<Route path="/admin/eventos/:eventId" element={<p>Configurações do evento</p>} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "VOLTAR ÀS CONFIGURAÇÕES" }));

		expect(await screen.findByText("Configurações do evento")).toBeInTheDocument();
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

	it("pede confirmação antes de registrar que o vencedor não retirou o prêmio", async () => {
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
		expect(screen.getByRole("dialog", { name: "Confirmar ausência do vencedor?" })).toBeInTheDocument();
		expect(bingoApi.markPrizeDeclined).not.toHaveBeenCalled();
		fireEvent.click(screen.getByRole("button", { name: "Confirmar ausência" }));

		expect(bingoApi.markPrizeDeclined).toHaveBeenCalledWith("event-1", "round-1");
	});

	it("permite confirmar a entrega quando o vencedor já foi revelado, mas ainda não foi carregado", async () => {
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
				drawnNumbers: [51],
				winnerDetectedCount: 1,
				tieBreakerRequired: false,
				hasPrizeDeliveryPending: true
			}
		});

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "PRÊMIO ENTREGUE" }));

		expect(bingoApi.markPrizeDelivered).toHaveBeenCalledWith("event-1", "round-1");
		expect(screen.getByRole("button", { name: "REVELAR VENCEDOR" })).toBeDisabled();
	});

	it("impede uma segunda revelação enquanto a primeira está em andamento", async () => {
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
				status: "TieBreaker",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 2,
				tieBreakerRequired: true
			}
		});
		vi.mocked(bingoApi.reveal).mockImplementation(() => new Promise(() => {}));

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		const revealButton = await screen.findByRole("button", { name: "REALIZAR DESEMPATE" });
		fireEvent.click(revealButton);
		fireEvent.click(revealButton);

		expect(bingoApi.reveal).toHaveBeenCalledTimes(1);
		expect(revealButton).toBeDisabled();
	});

	it("pede confirmação antes de cancelar uma rodada em sorteio", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "CANCELAR RODADA" }));
		expect(screen.getByRole("dialog", { name: "Cancelar esta rodada?" })).toBeInTheDocument();
		expect(bingoApi.cancelRound).not.toHaveBeenCalled();
		fireEvent.click(screen.getByRole("button", { name: "Cancelar rodada" }));

		expect(bingoApi.cancelRound).toHaveBeenCalledWith("event-1", "round-1");
	});

	it("permite fechar a confirmação de cancelamento com Escape", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "CANCELAR RODADA" }));
		fireEvent.keyDown(document, { key: "Escape" });

		expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
		expect(bingoApi.cancelRound).not.toHaveBeenCalled();
	});
});
