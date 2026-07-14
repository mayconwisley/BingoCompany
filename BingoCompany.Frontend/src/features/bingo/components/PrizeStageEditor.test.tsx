import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PrizeStageEditor } from "./PrizeStageEditor";

describe("PrizeStageEditor", () => {
    it("adiciona uma etapa na ordem progressiva das regras", () => {
        const onChange = vi.fn();
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={onChange} />);

        fireEvent.click(screen.getByRole("button", { name: "+ Adicionar prêmio" }));

        expect(onChange).toHaveBeenCalledWith([
            { sequence: 1, prizeName: "Novo prêmio", pattern: "BColumn" },
            { sequence: 2, prizeName: "TV", pattern: "FullCard" }
        ]);
    });

    it("impede repetir qualquer regra, inclusive coluna", () => {
        render(
            <PrizeStageEditor
                stages={[
                    { sequence: 1, prizeName: "Linha", pattern: "HorizontalLine" },
                    { sequence: 2, prizeName: "B 1", pattern: "BColumn" }
                ]}
                onChange={vi.fn()}
            />
        );

        expect(screen.getAllByRole("option", { name: "Uma linha" }).filter((option) => option.hasAttribute("disabled"))).toHaveLength(1);
        expect(screen.getAllByRole("option", { name: "Coluna B" }).filter((option) => option.hasAttribute("disabled"))).toHaveLength(1);
    });

    it("oferece as regras de diagonal B e diagonal O", () => {
		 render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={vi.fn()} />);

		 expect(screen.getByRole("option", { name: "Diagonal B" })).toBeInTheDocument();
		 expect(screen.getByRole("option", { name: "Diagonal O" })).toBeInTheDocument();
	 });

    it("oferece um campo para selecionar a foto do prêmio", () => {
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={vi.fn()} />);

        expect(screen.getByLabelText("Foto do prêmio 1")).toHaveAttribute("accept", "image/jpeg,image/png,image/webp");
    });

	 it("oferece as regras X, T, moldura e cruz", () => {
		 render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={vi.fn()} />);

		 expect(screen.getByRole("option", { name: "X" })).toBeInTheDocument();
		 expect(screen.getByRole("option", { name: "T" })).toBeInTheDocument();
		 expect(screen.getByRole("option", { name: "Moldura" })).toBeInTheDocument();
		 expect(screen.getByRole("option", { name: "Cruz" })).toBeInTheDocument();
	 });

    it("permite remover uma etapa mantendo ao menos uma", () => {
        const onChange = vi.fn();
        render(
            <PrizeStageEditor
                stages={[
                    { sequence: 1, prizeName: "Linha", pattern: "HorizontalLine" },
                    { sequence: 2, prizeName: "Bingo", pattern: "FullCard" }
                ]}
                onChange={onChange}
            />
        );

        fireEvent.click(screen.getAllByRole("button", { name: "Remover prêmio" })[1]);

        expect(onChange).toHaveBeenCalledWith([{ sequence: 1, prizeName: "Linha", pattern: "HorizontalLine" }]);
    });
});
