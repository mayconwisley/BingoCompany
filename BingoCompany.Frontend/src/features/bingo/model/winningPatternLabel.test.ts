import { describe, expect, it } from "vitest";
import { eventStatusLabel, markingModeLabel, roundStatusLabel, winningPatternLabel } from "./winningPatternLabel";

describe("winningPatternLabel", () => {
	it.each([
		["HorizontalLine", "Uma linha"],
		["TwoHorizontalLines", "Duas linhas"],
		["FourCorners", "Quatro cantos"],
		["FullCard", "Cartela cheia"],
		["BDiagonal", "Diagonal B"],
		["ODiagonal", "Diagonal O"],
		["XPattern", "X"],
		["TPattern", "T"],
		["Frame", "Moldura"],
		["Cross", "Cruz"],
		["BColumn", "Coluna B"],
		["IColumn", "Coluna I"],
		["NColumn", "Coluna N"],
		["GColumn", "Coluna G"],
		["OColumn", "Coluna O"]
	])("traduz %s para português brasileiro", (pattern, label) => {
		expect(winningPatternLabel(pattern)).toBe(label);
	});

	it("traduz estados e modos internos para linguagem natural", () => {
		expect(markingModeLabel("Automatic")).toBe("Automática");
		expect(roundStatusLabel("WinnerDetected")).toBe("Vencedor aguardando revelação");
		expect(eventStatusLabel("RegistrationOpen")).toBe("Inscrições abertas");
	});
});
