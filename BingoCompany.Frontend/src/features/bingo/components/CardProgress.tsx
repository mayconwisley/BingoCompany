import { winningPatternLabel } from "../model/winningPatternLabel";

type Props = {
	prizeName?: string;
	pattern?: string;
	remainingNumbersToWin?: number;
};

export function CardProgress({ prizeName, pattern, remainingNumbersToWin }: Props) {
	if (!pattern || remainingNumbersToWin === undefined) return null;
	const includesFreeCenter = [
		"HorizontalLine",
		"TwoHorizontalLines",
		"FullCard",
		"BDiagonal",
		"ODiagonal",
		"XPattern",
		"TPattern",
		"Cross",
		"NColumn"
	].includes(pattern);

	const progressMessage =
		remainingNumbersToWin === 0
			? "Regra completa"
			: `Faltam ${remainingNumbersToWin} ${remainingNumbersToWin === 1 ? "número" : "números"}`;

	return (
		<section className="card-progress" aria-label="Seu progresso na rodada">
			<span>SEU PROGRESSO</span>
			<strong>{progressMessage}</strong>
			<p>
				{prizeName || "Prêmio atual"} · {winningPatternLabel(pattern)}
			</p>
			{includesFreeCenter && <small>A casa livre do centro já conta como marcada.</small>}
		</section>
	);
}
