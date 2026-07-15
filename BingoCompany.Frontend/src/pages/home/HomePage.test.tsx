import { cleanup, fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter, useLocation } from "react-router-dom";
import { afterEach, describe, expect, it, vi } from "vitest";
import { HomePage } from "./HomePage";

const { getSession } = vi.hoisted(() => ({ getSession: vi.fn() }));

vi.mock("../../shared/auth/session", () => ({ clearSession: vi.fn(), getSession, sessionChangedEvent: "bingo-company-session-changed" }));

function Location() {
	return <span data-testid="location">{useLocation().pathname}</span>;
}

afterEach(() => {
	cleanup();
	vi.clearAllMocks();
});

describe("HomePage", () => {
	it("leva a empresa ao cadastro", () => {
		render(
			<MemoryRouter>
				<HomePage />
				<Location />
			</MemoryRouter>
		);

		fireEvent.click(screen.getByRole("button", { name: "Cadastrar empresa" }));

		expect(screen.getByTestId("location")).toHaveTextContent("/cadastro");
	});

	it("mostra a administração em vez de cadastro e login para empresa autenticada", () => {
		getSession.mockReturnValue({ name: "Ana", companyName: "Empresa" });
		render(
			<MemoryRouter>
				<HomePage />
				<Location />
			</MemoryRouter>
		);

		fireEvent.click(screen.getByRole("button", { name: "Acessar administração" }));

		expect(screen.queryByRole("button", { name: "Cadastrar empresa" })).not.toBeInTheDocument();
		expect(screen.queryByRole("button", { name: "Entrar" })).not.toBeInTheDocument();
		expect(screen.getByTestId("location")).toHaveTextContent("/admin");
	});

	it("abre uma jornada específica para participar", () => {
		render(
			<MemoryRouter>
				<HomePage />
				<Location />
			</MemoryRouter>
		);

		fireEvent.click(screen.getByRole("button", { name: "Participar" }));
		expect(screen.getByRole("heading", { name: "Participar do bingo" })).toBeInTheDocument();
		expect(screen.queryByRole("button", { name: "Consultar auditoria" })).not.toBeInTheDocument();
		fireEvent.change(screen.getByLabelText("Código do evento"), { target: { value: "AB12CD" } });
		fireEvent.click(screen.getByRole("button", { name: "Acessar bingo" }));

		expect(screen.getByTestId("location")).toHaveTextContent("/participar/AB12CD");
	});

	it("permite consultar a auditoria pública sem login", () => {
		render(
			<MemoryRouter>
				<HomePage />
				<Location />
			</MemoryRouter>
		);

		fireEvent.click(screen.getByRole("button", { name: "Auditoria pública" }));
		expect(screen.getByRole("heading", { name: "Consultar auditoria pública" })).toBeInTheDocument();
		expect(screen.queryByRole("button", { name: "Acessar bingo" })).not.toBeInTheDocument();
		fireEvent.change(screen.getByLabelText("Código do evento"), { target: { value: "AB12CD" } });
		fireEvent.click(screen.getByRole("button", { name: "Consultar auditoria" }));

		expect(screen.getByTestId("location")).toHaveTextContent("/auditoria/AB12CD");
	});
});
