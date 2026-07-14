import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it } from "vitest";
import { App } from "./App";

describe("App", () => {
    it("carrega a tela inicial pela rota", async () => {
        render(
            <MemoryRouter initialEntries={["/"]}>
                <App />
            </MemoryRouter>
        );

        expect(await screen.findByRole("heading", { name: /a festa inteira/i }, { timeout: 3000 })).toBeInTheDocument();
    });
});
