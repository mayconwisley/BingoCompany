import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ConnectionBadge } from "./ConnectionBadge";
describe("ConnectionBadge", () => {
	it("expõe o estado da conexão", () => {
		render(<ConnectionBadge status="Reconectando" />);
		expect(screen.getByLabelText("Conexão: Reconectando")).toBeInTheDocument();
	});
});
