import { act, render, screen, within } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { DisplayPage } from "./DisplayPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getPublicEvent: vi.fn() } }));
vi.mock("../../features/bingo/hooks/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("DisplayPage", () => {
	afterEach(() => vi.useRealTimers());

	it("permite alternar o tema sem sair do telão", async () => {
		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("button", { name: /ativar tema/i })).toBeInTheDocument();
	});

	it("mantém a pedra vencedora visível por três segundos antes do suspense", async () => {
		vi.useFakeTimers();
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "WinnerDetected",
				currentPrize: "Vale-presente",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 1,
				tieBreakerRequired: false
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		await act(async () => {
			await Promise.resolve();
		});
		expect(screen.getByText("ÚLTIMA PEDRA")).toBeInTheDocument();
		expect(screen.getByText("B-10", { selector: "strong" })).toBeInTheDocument();
		expect(screen.queryByText(/TEMOS UM VENCEDOR/)).not.toBeInTheDocument();

		act(() => vi.advanceTimersByTime(3000));

		expect(screen.getByText(/TEMOS UM VENCEDOR/)).toBeInTheDocument();
		expect(screen.getByText("Confira a cartela!")).toBeInTheDocument();
		expect(screen.getByText("PEDRA VENCEDORA")).toBeInTheDocument();
		expect(screen.getByText("B-10", { selector: "strong" })).toBeInTheDocument();
		expect(screen.queryByText(/Cartela ABC123/)).not.toBeInTheDocument();
	});

	it("mostra todos os participantes, as pedras e o vencedor do desempate", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false,
				winner: {
					participantName: "Ana",
					prizeName: "Vale-presente",
					pattern: "HorizontalLine",
					isPrizeDeliveryPending: false,
					tieBreakers: [
						{ participantName: "Ana", number: 71, isWinner: true },
						{ participantName: "Bruno", number: 24, isWinner: false }
					]
				}
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		const result = await screen.findByRole("region", { name: "Resultado do desempate" });
		expect(within(result).getByRole("heading", { name: "Resultado do desempate" })).toBeInTheDocument();
		expect(within(result).getByText("Ana")).toBeInTheDocument();
		expect(within(result).getByText("Bruno")).toBeInTheDocument();
		expect(within(result).getAllByText(/^[BINGO]-\d+$/)).toHaveLength(2);
		expect(within(result).getByText("Vencedor(a)")).toBeInTheDocument();
	});

	it("mostra o prêmio e a regra que deram a vitória", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false,
				winner: { participantName: "Ana", prizeName: "Vale-presente", pattern: "HorizontalLine", isPrizeDeliveryPending: false }
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("heading", { name: "Ana" })).toBeInTheDocument();
		expect(screen.getByText("Vale-presente")).toBeInTheDocument();
		expect(screen.getByText("Regra vencedora: Uma linha")).toBeInTheDocument();
	});

	it("exibe a foto do prêmio ativo", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				currentPrize: "Vale-presente",
				currentPrizeImageDataUrl: "data:image/png;base64,abc",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("img", { name: "Prêmio em disputa: Vale-presente" })).toHaveAttribute(
			"src",
			"data:image/png;base64,abc"
		);
	});

	it("destaca o nome do prêmio ativo quando não há foto", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 2,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				currentPrize: "Vale-presente",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByRole("heading", { name: "Vale-presente" })).toBeInTheDocument();
		expect(screen.getByText("Prêmio em disputa")).toBeInTheDocument();
	});

	it("explica quantas cartelas estão próximas do prêmio", async () => {
		vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "ABC",
			status: "Running",
			markingMode: "Automatic",
			participants: 2,
			cards: 6,
			round: {
				id: "round-1",
				name: "Rodada 1",
				sequence: 1,
				status: "Drawing",
				currentPrize: "Vale-presente",
				stages: [],
				drawnNumbers: [10],
				winnerDetectedCount: 0,
				tieBreakerRequired: false,
				statistics: { totalCards: 6, oneNumberAway: 0, twoNumbersAway: 2, threeNumbersAway: 4, awardedCards: 0 }
			}
		});

		render(
			<MemoryRouter initialEntries={["/display/ABC"]}>
				<Routes>
					<Route path="/display/:publicCode" element={<DisplayPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("6 cartelas na rodada")).toBeInTheDocument();
		expect(screen.getByText("0 cartelas precisam de 1 pedra para ganhar")).toBeInTheDocument();
		expect(screen.getByText("2 cartelas precisam de 2 pedras para ganhar")).toBeInTheDocument();
		expect(screen.getByText("4 cartelas precisam de 3 pedras para ganhar")).toBeInTheDocument();
	});
});
