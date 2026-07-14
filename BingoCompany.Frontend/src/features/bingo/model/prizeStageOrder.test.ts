import { describe, expect, it } from "vitest";
import { orderPrizeStages } from "./prizeStageOrder";

describe("orderPrizeStages", () => {
	it("mantém regras componentes antes das regras compostas", () => {
		const stages = orderPrizeStages([
			{ sequence: 1, prizeName: "Bingo", pattern: "FullCard" },
			{ sequence: 2, prizeName: "Moldura", pattern: "Frame" },
			{ sequence: 3, prizeName: "X", pattern: "XPattern" },
			{ sequence: 4, prizeName: "Diagonal O", pattern: "ODiagonal" },
			{ sequence: 5, prizeName: "Linha", pattern: "HorizontalLine" },
			{ sequence: 6, prizeName: "Diagonal B", pattern: "BDiagonal" },
			{ sequence: 7, prizeName: "Cantos", pattern: "FourCorners" }
		]);

		expect(stages.map((stage) => stage.pattern)).toEqual([
			"FourCorners",
			"BDiagonal",
			"ODiagonal",
			"HorizontalLine",
			"XPattern",
			"Frame",
			"FullCard"
		]);
		expect(stages.map((stage) => stage.sequence)).toEqual([1, 2, 3, 4, 5, 6, 7]);
	});
});
