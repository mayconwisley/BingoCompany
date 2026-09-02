import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { HelpPage } from "./HelpPage";

describe("HelpPage", () => {
	it("explica o fluxo para organizadores e participantes", () => {
		render(
			<MemoryRouter>
				<HelpPage />
			</MemoryRouter>
		);

		expect(screen.getByRole("heading", { name: "Como usar o Bingo Company" })).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Organize um evento do começo ao fim" })).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Entre no bingo e use sua cartela" })).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Telão e auditoria" })).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Conduza o sorteio ao vivo" })).toBeInTheDocument();
		expect(screen.getByText(/Só uma cartela associada e ativa concorre/i)).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Compra de cartelas" })).toBeInTheDocument();
		expect(screen.getByText(/O hash SHA-256 é publicado no começo da rodada/i)).toBeInTheDocument();
		expect(screen.getByRole("heading", { name: "Conexão, confirmação e recuperação" })).toBeInTheDocument();
		expect(screen.getByText(/não repita imediatamente o sorteio/i)).toBeInTheDocument();
	});
});
