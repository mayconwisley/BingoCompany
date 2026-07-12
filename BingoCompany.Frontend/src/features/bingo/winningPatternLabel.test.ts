import { describe, expect, it } from "vitest";
import { eventStatusLabel, markingModeLabel, roundStatusLabel, winningPatternLabel } from "./winningPatternLabel";

describe("winningPatternLabel", () =>
{
    it.each([
        ["HorizontalLine", "Uma linha"],
        ["TwoHorizontalLines", "Duas linhas"],
    ["FourCorners", "Quatro cantos"],
	["FullCard", "Cartela cheia"],
	["MainDiagonal", "Diagonal B-I-N-G-O"],
	["SecondaryDiagonal", "Diagonal O-G-N-I-B"],
	["BColumn", "Vertical B"],
	["OColumn", "Vertical O"]
    ])("traduz %s para português brasileiro", (pattern, label) =>
    {
        expect(winningPatternLabel(pattern)).toBe(label);
    });

    it("traduz estados e modos internos para linguagem natural", () =>
    {
        expect(markingModeLabel("Automatic")).toBe("Automática");
        expect(roundStatusLabel("WinnerDetected")).toBe("Vencedor aguardando revelação");
        expect(eventStatusLabel("RegistrationOpen")).toBe("Inscrições abertas");
    });
});
