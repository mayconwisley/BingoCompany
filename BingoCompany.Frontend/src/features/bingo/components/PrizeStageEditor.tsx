import { orderPrizeStages } from "../model/prizeStageOrder";
import type { PrizeDraft, WinningPattern } from "../model/types";

const patterns = [
	["BColumn", "Coluna B"],
	["IColumn", "Coluna I"],
	["NColumn", "Coluna N"],
	["GColumn", "Coluna G"],
	["OColumn", "Coluna O"],
	["FourCorners", "Quatro cantos"],
	["BDiagonal", "Diagonal B"],
	["ODiagonal", "Diagonal O"],
	["HorizontalLine", "Uma linha"],
	["XPattern", "X"],
	["TPattern", "T"],
	["Cross", "Cruz"],
	["TwoHorizontalLines", "Duas linhas"],
	["Frame", "Moldura"],
	["FullCard", "Cartela cheia"]
] as const;

const maximumPrizeImageSize = 2 * 1024 * 1024;

type PrizeStageEditorProps = {
	stages: PrizeDraft[];
	onChange: (value: PrizeDraft[]) => void;
	disabled?: boolean;
};

export function PrizeStageEditor({ stages, onChange, disabled = false }: PrizeStageEditorProps) {
	const patternCount = (pattern: string) => stages.filter((stage) => stage.pattern === pattern).length;
	const hasReachedPatternLimit = (pattern: WinningPattern) => patternCount(pattern) > 0;
	const update = (index: number, patch: Partial<PrizeDraft>) =>
		onChange(orderPrizeStages(stages.map((stage, stageIndex) => (stageIndex === index ? { ...stage, ...patch } : stage))));
	const updatePattern = (index: number, selectedPattern: string) => {
		const pattern = patterns.find(([value]) => value === selectedPattern)?.[0];
		if (pattern) update(index, { pattern });
	};
	const nextPattern = patterns.find(([pattern]) => !hasReachedPatternLimit(pattern))?.[0];
	const updateImage = (index: number, file?: File) => {
		if (!file || !file.type.match(/^image\/(jpeg|png|webp)$/) || file.size > maximumPrizeImageSize) return;
		const reader = new FileReader();
		reader.addEventListener("load", () => typeof reader.result === "string" && update(index, { prizeImageDataUrl: reader.result }));
		reader.readAsDataURL(file);
	};

	return (
		<div className="prize-stage-editor">
			{stages.map((stage, index) => (
				<section className="prize" key={`${stage.sequence}-${stage.pattern}`} aria-labelledby={`prize-stage-${index + 1}`}>
					<header className="prize-header">
						<div>
							<span>Prêmio {index + 1}</span>
							<strong id={`prize-stage-${index + 1}`}>{stage.prizeName || "Prêmio sem nome"}</strong>
						</div>
						{stages.length > 1 && (
							<button
								className="prize-remove-button"
								type="button"
								disabled={disabled}
								onClick={() => onChange(orderPrizeStages(stages.filter((_, stageIndex) => stageIndex !== index)))}
							>
								Remover prêmio
							</button>
						)}
					</header>
					<div className="prize-fields">
						<label>
							<span>Nome do prêmio</span>
							<input
								aria-label={`Prêmio ${index + 1}`}
								disabled={disabled}
								value={stage.prizeName}
								onChange={(event) => update(index, { prizeName: event.target.value })}
							/>
						</label>
						<label>
							<span>Regra para vencer</span>
							<select
								aria-label={`Regra do prêmio ${index + 1}`}
								disabled={disabled}
								value={stage.pattern}
								onChange={(event) => updatePattern(index, event.target.value)}
							>
								{patterns.map(([value, label]) => (
									<option value={value} key={value} disabled={value !== stage.pattern && hasReachedPatternLimit(value)}>
										{label}
									</option>
								))}
							</select>
						</label>
					</div>
					<label className="prize-image-input">
						<span>Foto do prêmio</span>
						<input
							aria-label={`Foto do prêmio ${index + 1}`}
							disabled={disabled}
							type="file"
							accept="image/jpeg,image/png,image/webp"
							onChange={(event) => updateImage(index, event.target.files?.[0])}
						/>
						{stage.prizeImageDataUrl && (
							<>
								<img src={stage.prizeImageDataUrl} alt={`Prévia de ${stage.prizeName}`} />
								<button type="button" disabled={disabled} onClick={() => update(index, { prizeImageDataUrl: undefined })}>
									Remover foto
								</button>
							</>
						)}
					</label>
				</section>
			))}
			<button
				className="prize-add-button"
				type="button"
				disabled={disabled || !nextPattern}
				onClick={() =>
					nextPattern &&
					onChange(orderPrizeStages([...stages, { sequence: stages.length + 1, prizeName: "Novo prêmio", pattern: nextPattern }]))
				}
			>
				+ Adicionar prêmio
			</button>
		</div>
	);
}
