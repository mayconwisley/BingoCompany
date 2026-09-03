import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { bingoApi } from "../api/bingoApi";
import type { EventDetails } from "../model/types";
import { CardPurchaseManagementSection } from "./CardPurchaseManagementSection";

vi.mock("../api/bingoApi", () => ({
	bingoApi: {
		createCardPurchaseInvitation: vi.fn()
	}
}));

vi.mock("./RegistrationQrCode", () => ({
	RegistrationQrCode: ({ description }: { description?: string }) => <div>{description ?? "QR Code de inscrição"}</div>
}));

const event: EventDetails = {
	id: "event-1",
	name: "Confraternização",
	publicCode: "EVENTO1",
	status: "RegistrationOpen",
	isCardPurchaseOpen: true,
	cardPurchaseLimit: 50,
	cardPurchasePerParticipantLimit: 5,
	cardPurchaseLowStockThreshold: 10,
	cardPurchaseRemaining: 50,
	participants: 0,
	cards: 0,
	eligibleCards: 0,
	awardedCards: { items: [], page: 1, pageSize: 3, totalItems: 0, totalPages: 0 },
	rounds: []
};

describe("CardPurchaseManagementSection", () => {
	it("gera um convite promocional para a inscrição pública, sem expor o painel do evento", async () => {
		const copyPublicLink = vi.fn().mockResolvedValue(undefined);
		vi.mocked(bingoApi.createCardPurchaseInvitation).mockResolvedValue({ code: "PROMO1", bonusCards: 2 });

		render(
			<CardPurchaseManagementSection
				eventId="event-1"
				event={event}
				registrationPath="/participar/EVENTO1"
				isFinished={false}
				copyPublicLink={copyPublicLink}
				onReload={vi.fn().mockResolvedValue(undefined)}
			/>
		);

		expect(screen.getByText("Configurar reserva")).toBeInTheDocument();
		expect(screen.getByText("Convites promocionais")).toBeInTheDocument();
		expect(screen.getByText("Encerrar venda de cartelas")).toBeInTheDocument();
		expect(screen.getByLabelText("Cartelas extras do convite")).not.toBeVisible();
		expect(screen.queryByRole("button", { name: /Copiar link de reserva/i })).not.toBeInTheDocument();
		fireEvent.click(screen.getByText("Convites promocionais"));

		expect(screen.getByRole("heading", { name: "Convite promocional" })).toBeVisible();
		expect(screen.getByText(/não dá acesso às configurações do evento/i)).toBeVisible();

		fireEvent.change(screen.getByLabelText("Cartelas extras do convite"), { target: { value: "2" } });
		fireEvent.click(screen.getByRole("button", { name: "Gerar link promocional" }));

		await waitFor(() => expect(bingoApi.createCardPurchaseInvitation).toHaveBeenCalledWith("event-1", 2));
		expect(await screen.findByText(/Link promocional pronto/i)).toBeInTheDocument();
		expect(screen.getByText(/inscrição pública com o benefício do convite/i)).toBeInTheDocument();

		fireEvent.click(screen.getByRole("button", { name: "Copiar link promocional" }));
		expect(copyPublicLink).toHaveBeenCalledWith("/participar/EVENTO1?convite=PROMO1", "Link promocional");
	});
});
