import { fireEvent, render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { MyCardsPage } from "./MyCardsPage";

vi.mock("../../features/bingo", () => ({
	authApi: { participantLogin: vi.fn(), participantRegister: vi.fn() },
	bingoApi: { activateDigitalCard: vi.fn(), getMyCards: vi.fn() }
}));
vi.mock("../../shared/auth/SessionContext", () => ({ useSession: () => ({ session: undefined, isLoading: false }) }));
vi.mock("../../shared/auth/session", () => ({ saveSession: vi.fn() }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => children }));

describe("MyCardsPage", () => {
	it("permite mostrar e ocultar a senha da conta de participante", () => {
		render(
			<MemoryRouter>
				<MyCardsPage />
			</MemoryRouter>
		);

		const password = screen.getByLabelText("Senha");
		expect(password).toHaveAttribute("type", "password");

		fireEvent.click(screen.getByRole("button", { name: "Mostrar senha" }));

		expect(password).toHaveAttribute("type", "text");
		expect(screen.getByRole("button", { name: "Ocultar senha" })).toBeInTheDocument();
	});
});
