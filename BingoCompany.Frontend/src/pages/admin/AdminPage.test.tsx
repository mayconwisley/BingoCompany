import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import type { ReactNode } from "react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { bingoApi } from "../../features/bingo/api/bingoApi";
import { AdminPage } from "./AdminPage";
vi.mock("../../features/bingo/api/bingoApi", () => ({ bingoApi: { listEvents: vi.fn(), createEvent: vi.fn() } }));
vi.mock("../../shared/ui/AppShell", () => ({ AppShell: ({ children }: { children: ReactNode }) => children }));
describe("AdminPage", () => {
	beforeEach(() =>
		vi.mocked(bingoApi.listEvents).mockResolvedValue({
			items: [
				{
					id: "1",
					name: "Festa 2026",
					publicCode: "ABC123",
					status: "Draft",
					markingMode: "Automatic",
					createdAt: "2026-07-13T14:30:00Z"
				}
			],
			page: 1,
			pageSize: 12,
			totalItems: 1,
			totalPages: 1
		})
	);
	it("lista eventos retornados pelo serviço", async () => {
		render(
			<MemoryRouter>
				<AdminPage />
			</MemoryRouter>
		);
		expect(await screen.findByText("Festa 2026")).toBeInTheDocument();
		expect(screen.getByText("ABC123")).toBeInTheDocument();
		expect(screen.getByText("Automática")).toBeInTheDocument();
		expect(screen.getByText(/Criado em/)).toBeInTheDocument();
	});
	it("solicita a próxima página de eventos", async () => {
		vi.mocked(bingoApi.listEvents).mockResolvedValue({
			items: [
				{
					id: "1",
					name: "Festa 2026",
					publicCode: "ABC123",
					status: "Draft",
					markingMode: "Automatic",
					createdAt: "2026-07-13T14:30:00Z"
				}
			],
			page: 1,
			pageSize: 12,
			totalItems: 13,
			totalPages: 2
		});
		render(
			<MemoryRouter>
				<AdminPage />
			</MemoryRouter>
		);
		const nextPage = await screen.findByRole("button", { name: "Próxima página" });
		fireEvent.click(nextPage);
		await waitFor(() => expect(bingoApi.listEvents).toHaveBeenLastCalledWith(2));
	});
	it("abre o telão em outra aba", async () => {
		render(
			<MemoryRouter>
				<AdminPage />
			</MemoryRouter>
		);
		const displayLink = await screen.findByRole("link", { name: "Abrir telão" });
		expect(displayLink).toHaveAttribute("target", "_blank");
		expect(displayLink).toHaveAttribute("rel", "noopener noreferrer");
	});
	it("desativa o acesso ao telão para evento finalizado", async () => {
		vi.mocked(bingoApi.listEvents).mockResolvedValue({
			items: [
				{
					id: "1",
					name: "Festa encerrada",
					publicCode: "FINALIZADO",
					status: "Finished",
					markingMode: "Automatic",
					createdAt: "2026-07-13T14:30:00Z"
				}
			],
			page: 1,
			pageSize: 12,
			totalItems: 1,
			totalPages: 1
		});

		render(
			<MemoryRouter>
				<AdminPage />
			</MemoryRouter>
		);

		expect(await screen.findByRole("button", { name: "Abrir telão" })).toBeDisabled();
		expect(screen.queryByRole("link", { name: "Abrir telão" })).not.toBeInTheDocument();
	});
	it("envia o modo de marcação selecionado ao criar o evento", async () => {
		vi.mocked(bingoApi.createEvent).mockResolvedValue({
			id: "2",
			name: "Festa 2026",
			publicCode: "DEF456",
			status: "Draft",
			markingMode: "Automatic",
			createdAt: "2026-07-13T14:30:00Z"
		});
		render(
			<MemoryRouter>
				<AdminPage />
			</MemoryRouter>
		);

		fireEvent.change(screen.getByLabelText("Nome do evento"), { target: { value: "Festa" } });
		fireEvent.change(screen.getByLabelText("Modo de marcação"), { target: { value: "AssistedManual" } });
		fireEvent.click(screen.getByRole("button", { name: "Criar evento" }));

		await waitFor(() => expect(bingoApi.createEvent).toHaveBeenCalledWith({ name: "Festa", markingMode: "AssistedManual" }));
	});
});
