type Props = {
	numbers: number[][];
	drawnNumbers: number[];
	markedNumbers: number[];
	manual: boolean;
	markableNumbers?: number[];
	disabled?: boolean;
	onMark: (number: number) => void;
};
export function BingoCardGrid({ numbers, drawnNumbers, markedNumbers, manual, markableNumbers, disabled, onMark }: Props) {
	return (
		<section className="bingocard" aria-label="Cartela de bingo">
			<div className="letters">
				{"BINGO".split("").map((x) => (
					<b key={x}>{x}</b>
				))}
			</div>
			{numbers.map((row, r) => (
				<div className="numbers" key={r}>
					{row.map((number, c) => {
						const drawn = drawnNumbers.includes(number);
						const marked = markedNumbers.includes(number) || (!manual && drawn);
						const canMark = markableNumbers === undefined || markableNumbers.includes(number);
						return (
							<button
								type="button"
								key={c}
								aria-label={
									number === 0
										? "Espaço livre"
										: `Número ${number}${drawn ? ", sorteado" : ""}${marked ? ", marcado" : ""}`
								}
								className={`${number === 0 ? "free" : ""} ${drawn ? "drawn" : ""} ${marked ? "marked" : ""}`}
								disabled={disabled || number === 0 || !drawn || marked || !manual || !canMark}
								onClick={() => onMark(number)}
							>
								{number === 0 ? "★" : number}
							</button>
						);
					})}
				</div>
			))}
		</section>
	);
}
