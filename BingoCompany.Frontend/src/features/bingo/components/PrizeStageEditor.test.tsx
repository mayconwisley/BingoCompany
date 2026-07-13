import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PrizeStageEditor } from "./PrizeStageEditor";

describe("PrizeStageEditor", () =>
{
    it("adiciona uma etapa respeitando a ordem recomendada", () =>
    {
        const onChange = vi.fn();
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={onChange} />);

        fireEvent.click(screen.getByRole("button", { name: "+ Adicionar prêmio" }));

        expect(onChange).toHaveBeenCalledWith([
            { sequence: 1, prizeName: "Novo prêmio", pattern: "BColumn" },
            { sequence: 2, prizeName: "TV", pattern: "FullCard" }
        ]);
    });

    it("impede selecionar uma terceira etapa com a mesma regra", () =>
    {
        render(<PrizeStageEditor stages={[
            { sequence: 1, prizeName: "B 1", pattern: "BColumn" },
            { sequence: 2, prizeName: "B 2", pattern: "BColumn" },
            { sequence: 3, prizeName: "Linha", pattern: "HorizontalLine" }
        ]} onChange={vi.fn()} />);

        expect(screen.getAllByRole("option", { name: "Coluna B" }).filter(option => option.hasAttribute("disabled"))).toHaveLength(1);
    });

    it("oferece um campo para selecionar a foto do prêmio", () =>
    {
        render(<PrizeStageEditor stages={[{ sequence: 1, prizeName: "TV", pattern: "FullCard" }]} onChange={vi.fn()} />);

        expect(screen.getByLabelText("Foto do prêmio 1")).toHaveAttribute("accept", "image/jpeg,image/png,image/webp");
    });
});
