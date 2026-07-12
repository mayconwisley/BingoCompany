import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../features/bingo/bingoApi";
import { DisplayPage } from "./DisplayPage";

vi.mock("../features/bingo/bingoApi", () => ({ bingoApi: { getPublicEvent: vi.fn() } }));
vi.mock("../features/bingo/useLiveBingo", () => ({ useLiveBingo: vi.fn() }));

describe("DisplayPage", () =>
{
    it("mostra o prêmio e a regra que deram a vitória", async () =>
    {
        vi.mocked(bingoApi.getPublicEvent).mockResolvedValue({ id: "event-1", name: "Festa", publicCode: "ABC", status: "Running", markingMode: "Automatic", participants: 2, cards: 2, round: { id: "round-1", name: "Rodada 1", sequence: 1, status: "Drawing", stages: [], drawnNumbers: [10], winner: { participantName: "Ana", prizeName: "Vale-presente", pattern: "HorizontalLine" } } });

        render(<MemoryRouter initialEntries={["/display/ABC"]}><Routes><Route path="/display/:publicCode" element={<DisplayPage />} /></Routes></MemoryRouter>);

        expect(await screen.findByRole("heading", { name: "Ana" })).toBeInTheDocument();
        expect(screen.getByText("Vale-presente")).toBeInTheDocument();
        expect(screen.getByText("Regra vencedora: Uma linha")).toBeInTheDocument();
    });
});
