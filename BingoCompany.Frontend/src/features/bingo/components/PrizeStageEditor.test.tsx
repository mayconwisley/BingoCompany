import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PrizeStageEditor } from "./PrizeStageEditor";

describe("PrizeStageEditor", () => {
    it("adiciona uma etapa sem reordenar as existentes", () => {
        const onChange = vi.fn();
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={onChange} />);

        fireEvent.click(screen.getByRole("button", { name: "+ Adicionar prêmio" }));

        expect(onChange).toHaveBeenCalledWith([
            { sequence: 1, prizeName: "TV", pattern: "FullCard" },
            { sequence: 2, prizeName: "Novo prêmio", pattern: "BColumn" }
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

    it("oferece um campo para selecionar a foto do prêmio", () => {
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={vi.fn()} />);

        expect(screen.getByLabelText("Foto do prêmio 1")).toHaveAttribute("accept", "image/jpeg,image/png,image/webp");
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
