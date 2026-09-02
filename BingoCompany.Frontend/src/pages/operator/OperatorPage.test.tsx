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
		startRound: vi.fn(),
		validatePrintedWinner: vi.fn()
	}
}));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));
vi.mock("../../shared/ui/ThemeToggle", () => ({ ThemeToggle: () => null }));
vi.mock("../../features/bingo/components/QrCardScanner", () => ({
	QrCardScanner: ({ onCardCodeRead, disabled }: { onCardCodeRead: (cardCode: string) => void; disabled?: boolean }) => (
		<button disabled={disabled} onClick={() => onCardCodeRead("CARTELA-QR")}>
			Ler QR Code da cartela
		</button>
	)
}));

describe("OperatorPage", () => {
	beforeEach(() => {
		vi.clearAllMocks();
		vi.mocked(bingoApi.draw).mockResolvedValue({ roundId: "round-1", number: 10, sequence: 1, winnersDetected: 0 });
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

	it("impede um segundo sorteio enquanto o primeiro aguarda confirmação", async () => {
		vi.mocked(bingoApi.draw).mockImplementation(() => new Promise(() => {}));
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		const drawButton = await screen.findByRole("button", { name: "SORTEAR PRÓXIMA PEDRA" });
		fireEvent.click(drawButton);
		fireEvent.click(drawButton);

		expect(bingoApi.draw).toHaveBeenCalledTimes(1);
		expect(drawButton).toBeDisabled();
	});

	it("bloqueia o início e informa a pendência quando existem cartelas, mas nenhuma é elegível", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "RegistrationOpen",
			markingMode: "ManualRequired",
			participants: 1,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Ready",
				eligibleCards: 0,
				stages: [{ prizeName: "Vale-presente", pattern: "HorizontalLine", isActive: true, isCompleted: false }],
				drawnNumbers: [],
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

		expect(await screen.findByRole("heading", { name: "Há pendências para iniciar" })).toBeInTheDocument();
		expect(screen.getByText("0 cartelas elegíveis")).toBeInTheDocument();
		expect(screen.getByRole("button", { name: "INICIAR RODADA" })).toBeDisabled();
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

	it("valida uma cartela física pelo QR Code dentro do painel no modo manual", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "ManualRequired",
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
				tieBreakerRequired: false
			}
		});
		vi.mocked(bingoApi.validatePrintedWinner).mockResolvedValue({
			participantName: "Ana",
			cardCode: "CARTELA-QR",
			tieBreakerRequired: false
		});

		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "Ler QR Code da cartela" }));

		expect(bingoApi.validatePrintedWinner).toHaveBeenCalledWith("event-1", "round-1", "CARTELA-QR");
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

	it("abre o telão em outra aba", async () => {
		render(
			<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}>
				<Routes>
					<Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} />
				</Routes>
			</MemoryRouter>
		);

		const displayLink = await screen.findByRole("link", { name: "ABRIR TELÃO" });
		expect(displayLink).toHaveAttribute("href", "/display/ABC");
		expect(displayLink).toHaveAttribute("target", "_blank");
	});

	it("encerra a apresentação anterior antes de preparar o próximo sorteio", async () => {
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
				status: "Finished",
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
					<Route path="/admin/eventos/:eventId" element={<p>Configurações do evento</p>} />
				</Routes>
			</MemoryRouter>
		);

		fireEvent.click(await screen.findByRole("button", { name: "PREPARAR PRÓXIMO SORTEIO" }));

		expect(bingoApi.closeWinnerPresentation).toHaveBeenCalledWith("event-1", "round-1");
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
