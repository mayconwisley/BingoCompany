import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { ApplicationFooter } from "./ApplicationFooter";

describe("ApplicationFooter", () => {
    it("exibe o link do projeto e o aviso de dados demonstrativos", () => {
        render(
            <MemoryRouter>
                <ApplicationFooter application={{ name: "Bingo Company", description: "Bingo corporativo", version: "1.0.2" }} />
            </MemoryRouter>
        );

        expect(screen.getByRole("link", { name: "Projeto open source" })).toHaveAttribute(
            "href",
            "https://github.com/mayconwisley/BingoCompany"
        );
        expect(screen.getByText("Projeto demonstrativo online: não informe dados pessoais, reais ou sensíveis.")).toBeInTheDocument();
        expect(screen.getByText("Desenvolvido por Maycon Wisley")).toBeInTheDocument();
    });
});
