import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { PrintCardsPage } from "./PrintCardsPage";

vi.mock("qrcode", () => ({ default: { toDataURL: vi.fn().mockResolvedValue("data:image/png;base64,qr") } }));
vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { getPrintedCards: vi.fn() } }));

describe("PrintCardsPage", () => {
	it("exibe o QR Code ao lado da empresa na cartela", async () => {
		vi.mocked(bingoApi.getPrintedCards).mockResolvedValue([
			{
				publicCode: "ABC123",
				fingerprint: "fingerprint",
				status: "Printed",
				companyName: "Empresa Exemplo",
				numbers: [
					[1, 16, 31, 46, 61],
					[2, 17, 32, 47, 62],
					[3, 18, 0, 48, 63],
					[4, 19, 34, 49, 64],
					[5, 20, 35, 50, 65]
				],
				qrCodeValue: "BINGO:ABC123"
			}
		]);

		render(
			<MemoryRouter initialEntries={["/admin/eventos/event-1/impressao"]}>
				<Routes>
					<Route path="/admin/eventos/:eventId/impressao" element={<PrintCardsPage />} />
				</Routes>
			</MemoryRouter>
		);

		const qrCode = await screen.findByRole("img", { name: "QR Code da cartela ABC123" });
		const companyName = screen.getByText("Empresa Exemplo");

		expect(qrCode.parentElement).toContainElement(companyName);
	});
});
