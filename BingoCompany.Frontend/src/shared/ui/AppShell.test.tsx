import { fireEvent, render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it } from "vitest";
import { AppShell } from "./AppShell";

describe("AppShell", () => {
	beforeEach(() => {
		document.documentElement.dataset.theme = "";
	});

	it("oculta o acesso administrativo quando a tela é de participante", () => {
		render(
			<MemoryRouter>
				<AppShell showAdministration={false}>
					<p>Minha cartela</p>
				</AppShell>
			</MemoryRouter>
		);

		expect(screen.queryByRole("link", { name: "Administração" })).not.toBeInTheDocument();
		expect(screen.getByText("Minha cartela")).toBeInTheDocument();
	});

	it("alterna o tema escolhido", () => {
		render(
			<MemoryRouter>
				<AppShell>
					<p>Conteúdo</p>
				</AppShell>
			</MemoryRouter>
		);

		fireEvent.click(screen.getByRole("button", { name: /ativar tema/i }));

		expect(document.documentElement.dataset.theme).toBe("dark");
	});

	it("carrega a logo a partir da base pública configurada", () => {
		const { container } = render(
			<MemoryRouter>
				<AppShell>
					<p>Conteúdo</p>
				</AppShell>
			</MemoryRouter>
		);

		expect(container.querySelector(".brand-logo")).toHaveAttribute("src", `${import.meta.env.BASE_URL}assets/bingo-company-logo.png`);
	});

	it("organiza cabeçalho, conteúdo e rodapé em um contêiner de página", () => {
		const { container } = render(
			<MemoryRouter>
				<AppShell>
					<p>Conteúdo</p>
				</AppShell>
			</MemoryRouter>
		);

		expect(container.firstElementChild).toHaveClass("app-shell");
	});
});
