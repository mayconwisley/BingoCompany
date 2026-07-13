import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../features/bingo/bingoApi";
import { OperatorPage } from "./OperatorPage";

vi.mock("../features/bingo/bingoApi", () => ({ bingoApi: { closeWinnerPresentation: vi.fn(), draw: vi.fn(), getPublicEvent: vi.fn(), reveal: vi.fn(), startRound: vi.fn() } }));
vi.mock("../features/bingo/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("OperatorPage", () =>
{
    beforeEach(() =>
    {
        vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({ id: "event-1", name: "Festa", publicCode: "ABC", status: "Running", markingMode: "Automatic", participants: 1, cards: 1, round: { id: "round-1", name: "Rodada 1", sequence: 1, status: "Drawing", stages: [], drawnNumbers: [], winnerDetectedCount: 0, tieBreakerRequired: false } });
    });

    it("sorteia a primeira pedra de uma rodada já iniciada sem tentar iniciá-la novamente", async () =>
    {
        render(<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}><Routes><Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} /></Routes></MemoryRouter>);

        fireEvent.click(await screen.findByRole("button", { name: "SORTEAR PRÓXIMA PEDRA" }));

        expect(bingoApi.draw).toHaveBeenCalledWith("event-1", "round-1");
        expect(bingoApi.startRound).not.toHaveBeenCalled();
    });

    it("mantém o sorteio bloqueado enquanto o telão apresenta o vencedor", async () =>
    {
        vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({ id: "event-1", name: "Festa", publicCode: "ABC", status: "Running", markingMode: "Automatic", participants: 1, cards: 1, round: { id: "round-1", name: "Rodada 1", sequence: 1, status: "Drawing", stages: [], drawnNumbers: [10], winnerDetectedCount: 0, tieBreakerRequired: false, winner: { participantName: "Ana", prizeName: "Linha", pattern: "HorizontalLine" } } });

        render(<MemoryRouter initialEntries={["/operacao/event-1/round-1?code=ABC"]}><Routes><Route path="/operacao/:eventId/:roundId" element={<OperatorPage />} /></Routes></MemoryRouter>);

        const drawButton = await screen.findByRole("button", { name: "SORTEAR PRÓXIMA PEDRA" });
        expect(drawButton).toBeDisabled();
        fireEvent.click(screen.getByRole("button", { name: "CONTINUAR SORTEIO NO TELÃO" }));

        expect(bingoApi.closeWinnerPresentation).toHaveBeenCalledWith("event-1", "round-1");
    });
});
