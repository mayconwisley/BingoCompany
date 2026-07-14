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
		expect(screen.getByRole("heading", { name: "Veja a auditoria sem fazer login" })).toBeInTheDocument();
		expect(screen.getByText(/Se o vencedor não retirar/i)).toBeInTheDocument();
	});
});
