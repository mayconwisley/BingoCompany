import { act, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DrawSuspense } from "./DrawSuspense";

describe("DrawSuspense", () =>
{
    it("apresenta suspense antes de revelar uma nova pedra recebida", () =>
    {
        vi.useFakeTimers();
        const view = render(<DrawSuspense number={10} />);

        view.rerender(<DrawSuspense number={11} />);

        expect(screen.getByText("A próxima pedra é...")).toBeInTheDocument();
        act(() => vi.advanceTimersByTime(1600));
        expect(screen.getByText("ÚLTIMA PEDRA")).toBeInTheDocument();
        expect(screen.getByText("11")).toBeInTheDocument();
        vi.useRealTimers();
    });
});
