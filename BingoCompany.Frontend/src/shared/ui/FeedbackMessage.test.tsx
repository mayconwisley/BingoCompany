import { act, render, screen } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";
import { FeedbackMessage } from "./FeedbackMessage";

describe("FeedbackMessage", () => {
	afterEach(() => vi.useRealTimers());

	it("exibe o alerta de sucesso e o fecha após cinco segundos", () => {
		vi.useFakeTimers();
		const onClose = vi.fn();

		render(<FeedbackMessage success="Rodada atualizada com sucesso." onClose={onClose} />);

		expect(screen.getByText("Rodada atualizada com sucesso.")).toBeInTheDocument();
		expect(screen.getByRole("alert")).toHaveClass("MuiAlert-colorSuccess");

		act(() => vi.advanceTimersByTime(5_000));

		expect(onClose).toHaveBeenCalledOnce();
	});

	it("prioriza mensagens de erro", () => {
		render(<FeedbackMessage error="Não foi possível salvar." warning="Revise os dados." success="Salvo." />);

		expect(screen.getByRole("alert")).toHaveTextContent("Não foi possível salvar.");
		expect(screen.getByRole("alert")).toHaveClass("MuiAlert-colorError");
	});
});
