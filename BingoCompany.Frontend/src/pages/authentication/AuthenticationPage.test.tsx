import { fireEvent, render, screen } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { AuthenticationPage } from "./AuthenticationPage";

vi.mock("../../features/bingo", () => ({ authApi: { login: vi.fn(), register: vi.fn() } }));
vi.mock("../../shared/auth/session", () => ({ saveSession: vi.fn() }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => children }));

describe("AuthenticationPage", () => {
    it("informa os requisitos de senha durante o cadastro", () => {
        render(
            <MemoryRouter>
                <AuthenticationPage mode="register" />
            </MemoryRouter>
        );

        expect(screen.getByText("A senha deve ter entre 12 e 128 caracteres.")).toBeInTheDocument();
        expect(screen.getByLabelText("Senha")).toHaveAttribute("minlength", "12");
        expect(screen.getByLabelText("Senha")).toHaveAttribute("maxlength", "128");
    });

    it("permite mostrar e ocultar a senha", () => {
        render(
            <MemoryRouter>
                <AuthenticationPage mode="login" />
            </MemoryRouter>
        );

        const passwordInput = screen.getByLabelText("Senha");
        expect(passwordInput).toHaveAttribute("type", "password");

        fireEvent.click(screen.getByRole("button", { name: "Mostrar senha" }));

        expect(passwordInput).toHaveAttribute("type", "text");
        expect(screen.getByRole("button", { name: "Ocultar senha" })).toBeInTheDocument();
    });
});
