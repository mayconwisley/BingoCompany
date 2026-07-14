const labels: Record<string, string> = {
	HorizontalLine: "Uma linha",
	TwoHorizontalLines: "Duas linhas",
	FourCorners: "Quatro cantos",
	FullCard: "Cartela cheia",
	BDiagonal: "Diagonal B",
	ODiagonal: "Diagonal O",
	XPattern: "X",
	TPattern: "T",
	Frame: "Moldura",
	Cross: "Cruz",
	BColumn: "Coluna B",
	IColumn: "Coluna I",
	NColumn: "Coluna N",
	GColumn: "Coluna G",
	OColumn: "Coluna O"
};

const markingModeLabels: Record<string, string> = {
	Automatic: "Automática",
	ManualRequired: "Manual obrigatória",
	AssistedManual: "Manual assistida"
};

const roundStatusLabels: Record<string, string> = {
	Ready: "Pronta para iniciar",
	Drawing: "Em sorteio",
	WinnerDetected: "Vencedor aguardando revelação",
	TieBreaker: "Desempate em andamento",
	Finished: "Finalizada"
};

const eventStatusLabels: Record<string, string> = {
	Draft: "Em preparação",
	RegistrationOpen: "Inscrições abertas",
	Running: "Em andamento",
	Finished: "Finalizado"
};

export function winningPatternLabel(pattern: string): string {
	return labels[pattern] ?? "Regra de bingo";
}

export function markingModeLabel(markingMode: string): string {
	return markingModeLabels[markingMode] ?? "Não informado";
}

export function roundStatusLabel(status: string): string {
	return roundStatusLabels[status] ?? "Status não informado";
}

export function eventStatusLabel(status: string): string {
	return eventStatusLabels[status] ?? "Status não informado";
}
