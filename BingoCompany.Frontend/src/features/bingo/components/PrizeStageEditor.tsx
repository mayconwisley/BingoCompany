import { orderPrizeStages } from "../model/prizeStageOrder";
import type { PrizeDraft, WinningPattern } from "../model/types";

const patterns = [
    ["BColumn", "Coluna B"],
    ["IColumn", "Coluna I"],
    ["NColumn", "Coluna N"],
    ["GColumn", "Coluna G"],
    ["OColumn", "Coluna O"],
    ["FourCorners", "Quatro cantos"],
    ["HorizontalLine", "Uma linha"],
    ["TwoHorizontalLines", "Duas linhas"],
    ["FullCard", "Cartela cheia"]
] as const;

const maximumPrizeImageSize = 2 * 1024 * 1024;

export function PrizeStageEditor({ stages, onChange }: { stages: PrizeDraft[]; onChange: (value: PrizeDraft[]) => void })
{
    const patternCount = (pattern: string) => stages.filter(stage => stage.pattern === pattern).length;
    const hasReachedPatternLimit = (pattern: WinningPattern) => patternCount(pattern) > 0;
    const update = (index: number, patch: Partial<PrizeDraft>) => onChange(orderPrizeStages(stages.map((stage, stageIndex) => stageIndex === index ? { ...stage, ...patch } : stage)));
    const updatePattern = (index: number, selectedPattern: string) =>
    {
        const pattern = patterns.find(([value]) => value === selectedPattern)?.[0];
        if (pattern) update(index, { pattern });
    };
    const nextPattern = patterns.find(([pattern]) => !hasReachedPatternLimit(pattern))?.[0];
    const updateImage = (index: number, file?: File) =>
    {
        if (!file || !file.type.match(/^image\/(jpeg|png|webp)$/) || file.size > maximumPrizeImageSize) return;
        const reader = new FileReader();
        reader.addEventListener("load", () => typeof reader.result === "string" && update(index, { prizeImageDataUrl: reader.result }));
        reader.readAsDataURL(file);
    };

    return <div>
        {stages.map((stage, index) => <div className="prize" key={`${stage.sequence}-${stage.pattern}`}>
            <input aria-label={`Prêmio ${index + 1}`} value={stage.prizeName} onChange={event => update(index, { prizeName: event.target.value })} />
            <select aria-label={`Regra do prêmio ${index + 1}`} value={stage.pattern} onChange={event => updatePattern(index, event.target.value)}>
                {patterns.map(([value, label]) => <option value={value} key={value} disabled={value !== stage.pattern && hasReachedPatternLimit(value)}>{label}</option>)}
            </select>
            <label className="prize-image-input">Foto do prêmio<input aria-label={`Foto do prêmio ${index + 1}`} type="file" accept="image/jpeg,image/png,image/webp" onChange={event => updateImage(index, event.target.files?.[0])} />{stage.prizeImageDataUrl && <><img src={stage.prizeImageDataUrl} alt={`Prévia de ${stage.prizeName}`} /><button type="button" onClick={() => update(index, { prizeImageDataUrl: undefined })}>Remover foto</button></>}</label>
            {stages.length > 1 && <button type="button" onClick={() => onChange(orderPrizeStages(stages.filter((_, stageIndex) => stageIndex !== index)))}>Remover prêmio</button>}
        </div>)}
        <button type="button" disabled={!nextPattern} onClick={() => nextPattern && onChange(orderPrizeStages([...stages, { sequence: stages.length + 1, prizeName: "Novo prêmio", pattern: nextPattern }]))}>+ Adicionar prêmio</button>
    </div>;
}
