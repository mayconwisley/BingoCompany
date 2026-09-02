import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import type { ReactNode } from "react";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import type { PublicAudit } from "../../features/bingo/model/types";
import { AuditPage } from "./AuditPage";

vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getAudit: vi.fn() } }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => <>{children}</> }));

function createAudit(page: number, action: string): PublicAudit {
	return {
		eventInfo: { name: "Festa", publicCode: "ABC", status: "Finished", createdAt: "2026-09-01T12:00:00Z" },
		participants: [{ id: "participant-1", name: "Ana", joinedAt: "2026-09-01T12:00:00Z" }],
		cards: [{ id: "card-1", publicCode: "CARTELA-1", type: "Digital", status: "Active", createdAt: "2026-09-01T12:00:00Z" }],
		rounds: [
			{
				name: "Rodada 1",
				sequence: 1,
				status: "Finished",
				sequenceHash: "hash-seguro",
				fullSequence: [1, 2, 3],
				drawnNumbers: [{ number: 1, sequence: 1, drawnAt: "2026-09-01T12:01:00Z" }],
				stages: [{ prizeName: "Vale", pattern: "HorizontalLine", isCompleted: true }],
				winners: [
					{
						participantName: "Ana",
						cardCode: "CARTELA-1",
						prizeName: "Vale",
						isWinner: true,
						tieBreakerNumber: undefined,
						detectedAt: "2026-09-01T12:01:00Z",
						revealedAt: "2026-09-01T12:01:00Z"
					}
				]
			}
		],
		entries: {
			items: [{ action, details: `Detalhes de ${action}`, occurredAt: "2026-09-01T12:01:00Z" }],
			page,
			pageSize: 25,
			totalItems: 26,
			totalPages: 2
		}
	};
}

describe("AuditPage", () => {
	it("busca uma nova página de registros no backend", async () => {
		vi.mocked(bingoApi.getAudit)
			.mockResolvedValueOnce(createAudit(1, "Pedra sorteada"))
			.mockResolvedValueOnce(createAudit(2, "Inscrição aberta"));

		render(
			<MemoryRouter initialEntries={["/auditoria/ABC"]}>
				<Routes>
					<Route path="/auditoria/:publicCode" element={<AuditPage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("Pedra sorteada")).toBeInTheDocument();
		expect(screen.getByText("Cartela CARTELA-1")).toBeInTheDocument();
		expect(bingoApi.getAudit).toHaveBeenCalledWith("ABC", 1, 25);

		fireEvent.click(screen.getByRole("button", { name: "Mais antigos" }));

		expect(await screen.findByText("Inscrição aberta")).toBeInTheDocument();
		expect(bingoApi.getAudit).toHaveBeenLastCalledWith("ABC", 2, 25);
		expect(screen.getByText("Página 2 de 2")).toBeInTheDocument();
	});
});
