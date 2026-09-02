import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { CardProgress } from "./CardProgress";

describe("CardProgress", () => {
	it("explica que a casa livre reduz a quantidade da linha central", () => {
		render(<CardProgress pattern="HorizontalLine" remainingNumbersToWin={4} />);

		expect(screen.getByText("Faltam 4 números")).toBeInTheDocument();
		expect(screen.getByText("A casa livre do centro já conta como marcada.")).toBeInTheDocument();
	});
});
