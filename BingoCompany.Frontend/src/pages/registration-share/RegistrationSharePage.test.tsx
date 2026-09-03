import { render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { RegistrationSharePage } from "./RegistrationSharePage";

const { getPublicEvent } = vi.hoisted(() => ({ getPublicEvent: vi.fn() }));

vi.mock("../../features/bingo", () => ({
	bingoApi: { getPublicEvent }
}));
vi.mock("../../features/bingo/qr", () => ({ RegistrationQrCode: () => <img alt="QR Code de inscrição" /> }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => children }));

describe("RegistrationSharePage", () => {
	it("não exibe o QR Code quando as inscrições estão fechadas", async () => {
		getPublicEvent.mockResolvedValue({
			id: "event-1",
			name: "Festa",
			publicCode: "EVENTO",
			status: "Running",
			markingMode: "Automatic",
			participants: 1,
			cards: 1
		});
		render(
			<MemoryRouter initialEntries={["/inscricao/EVENTO"]}>
				<Routes>
					<Route path="/inscricao/:publicCode" element={<RegistrationSharePage />} />
				</Routes>
			</MemoryRouter>
		);

		expect(await screen.findByText("As inscrições deste bingo estão fechadas.")).toBeInTheDocument();
		expect(screen.queryByAltText("QR Code de inscrição")).not.toBeInTheDocument();
	});
});
