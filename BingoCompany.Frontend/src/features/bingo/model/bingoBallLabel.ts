const bingoColumns = ["B", "I", "N", "G", "O"] as const;
const numbersPerColumn = 15;

export function bingoBallLabel(number: number): string {
	const columnIndex = Math.floor((number - 1) / numbersPerColumn);
	const column = bingoColumns[columnIndex];

	return column ? `${column}-${number}` : String(number);
}
