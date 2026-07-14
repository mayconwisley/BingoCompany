import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RouteLoadingState } from "./RouteLoadingState";

describe("RouteLoadingState", () => {
	it("apresenta uma transição acessível enquanto a rota é carregada", () => {
		render(<RouteLoadingState />);

		expect(screen.getByRole("status")).toHaveTextContent("Preparando sua experiência de bingo...");
		expect(screen.getByRole("main")).toHaveAttribute("aria-busy", "true");
	});
});
