import type { PrizeDraft } from "./types";

export function orderPrizeStages(stages: PrizeDraft[]): PrizeDraft[]
{
    return stages
        .map((stage, index) => ({ ...stage, sequence: index + 1 }));
}
