import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { AppShell } from "./AppShell";

describe("AppShell", () =>
{
    it("oculta o acesso administrativo quando a tela é de participante", () =>
    {
        render(<MemoryRouter><AppShell showAdministration={false}><p>Minha cartela</p></AppShell></MemoryRouter>);

        expect(screen.queryByRole("link", { name: "Administração" })).not.toBeInTheDocument();
        expect(screen.getByText("Minha cartela")).toBeInTheDocument();
    });
});
