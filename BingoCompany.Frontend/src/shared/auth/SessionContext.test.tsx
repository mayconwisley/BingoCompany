import { render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { authApi } from "../../features/bingo";
import { SessionProvider, useSession } from "./SessionContext";
import { clearSession, saveSession } from "./session";

vi.mock("../../features/bingo", () => ({
	authApi: { getSession: vi.fn() }
}));

vi.mock("./session", () => ({
	clearSession: vi.fn(),
	getSession: vi.fn(),
	saveSession: vi.fn(),
	sessionChangedEvent: "bingo-company-session-changed"
}));

function SessionStatus() {
	const { session, isLoading } = useSession();
	if (isLoading) return <p>Validando sessão</p>;
	return <p>{session ? `Sessão de ${session.name}` : "Sessão encerrada"}</p>;
}

describe("SessionProvider", () => {
	beforeEach(() => {
		vi.resetAllMocks();
	});

	it("remove a sessão persistida quando a API não reconhece mais o cookie", async () => {
		vi.mocked(authApi.getSession).mockRejectedValue(new Error("Unauthorized"));

		render(
			<SessionProvider>
				<SessionStatus />
			</SessionProvider>
		);

		expect(screen.getByText("Validando sessão")).toBeInTheDocument();
		await screen.findByText("Sessão encerrada");
		expect(clearSession).toHaveBeenCalledOnce();
	});

	it("persiste a sessão confirmada pela API", async () => {
		vi.mocked(authApi.getSession).mockResolvedValue({ name: "Ana", companyName: "Empresa", accountType: "company" });

		render(
			<SessionProvider>
				<SessionStatus />
			</SessionProvider>
		);

		await screen.findByText("Sessão de Ana");
		await waitFor(() => expect(saveSession).toHaveBeenCalledWith({ name: "Ana", companyName: "Empresa", accountType: "company" }));
	});
});
