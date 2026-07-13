import type { PrizeDraft } from "./types";

const patternPriority: Record<string, number> = {
    BColumn: 1,
    IColumn: 2,
    NColumn: 3,
    GColumn: 4,
    OColumn: 5,
    FourCorners: 6,
    HorizontalLine: 7,
    TwoHorizontalLines: 8,
    FullCard: 9
};

export function orderPrizeStages(stages: PrizeDraft[]): PrizeDraft[]
{
    return [...stages]
        .sort((left, right) => patternPriority[left.pattern] - patternPriority[right.pattern])
        .map((stage, index) => ({ ...stage, sequence: index + 1 }));
}
