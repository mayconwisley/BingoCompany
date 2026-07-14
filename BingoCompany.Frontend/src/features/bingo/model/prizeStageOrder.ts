import type { PrizeDraft } from "./types";

const patternPriority: Record<PrizeDraft["pattern"], number> = {
    BColumn: 1,
    IColumn: 2,
    NColumn: 3,
    GColumn: 4,
    OColumn: 5,
    FourCorners: 6,
    BDiagonal: 7,
    ODiagonal: 8,
    HorizontalLine: 9,
    XPattern: 10,
    TPattern: 11,
    Cross: 12,
    TwoHorizontalLines: 13,
    Frame: 14,
    FullCard: 15
};

export function orderPrizeStages(stages: PrizeDraft[]): PrizeDraft[] {
    return [...stages]
        .sort((first, second) => patternPriority[first.pattern] - patternPriority[second.pattern])
        .map((stage, index) => ({ ...stage, sequence: index + 1 }));
}
