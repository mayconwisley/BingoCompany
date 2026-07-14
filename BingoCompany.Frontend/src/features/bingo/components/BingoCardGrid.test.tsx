import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { BingoCardGrid } from "./BingoCardGrid";
const numbers = [
	[1, 16, 31, 46, 61],
	[2, 17, 32, 47, 62],
	[3, 18, 0, 48, 63],
	[4, 19, 34, 49, 64],
	[5, 20, 35, 50, 65]
];
describe("BingoCardGrid", () => {
	it("permite marcar somente um número sorteado no modo manual", () => {
		const onMark = vi.fn();
		render(<BingoCardGrid numbers={numbers} drawnNumbers={[16]} markedNumbers={[]} manual onMark={onMark} />);
		expect(screen.getByRole("button", { name: "Número 1" })).toBeDisabled();
		fireEvent.click(screen.getByRole("button", { name: "Número 16, sorteado" }));
		expect(onMark).toHaveBeenCalledWith(16);
	});
	it("renderiza a casa central como livre", () => {
		render(<BingoCardGrid numbers={numbers} drawnNumbers={[]} markedNumbers={[]} manual onMark={() => {}} />);
		expect(screen.getByRole("button", { name: "Espaço livre" })).toBeDisabled();
	});
});
