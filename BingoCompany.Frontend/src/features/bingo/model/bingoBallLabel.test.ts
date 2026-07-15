import { describe, expect, it } from "vitest";
import { bingoBallLabel } from "./bingoBallLabel";

describe("bingoBallLabel", () => {
	it.each([
		[1, "B-1"],
		[15, "B-15"],
		[16, "I-16"],
		[30, "I-30"],
		[31, "N-31"],
		[45, "N-45"],
		[46, "G-46"],
		[60, "G-60"],
		[61, "O-61"],
		[75, "O-75"]
	])("identifica a coluna da pedra %s", (number, expectedLabel) => {
		expect(bingoBallLabel(number)).toBe(expectedLabel);
	});
});
