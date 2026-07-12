const labels: Record<string, string> = {
    HorizontalLine: "Uma linha",
    TwoHorizontalLines: "Duas linhas",
    FourCorners: "Quatro cantos",
	FullCard: "Cartela cheia",
	MainDiagonal: "Diagonal B-I-N-G-O",
	SecondaryDiagonal: "Diagonal O-G-N-I-B",
	BColumn: "Vertical B",
	OColumn: "Vertical O"
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
    Finished: "Finalizada"
};

const eventStatusLabels: Record<string, string> = {
    Draft: "Em preparação",
    RegistrationOpen: "Inscrições abertas",
    Running: "Em andamento",
    Finished: "Finalizado"
};

export function winningPatternLabel(pattern: string): string
{
    return labels[pattern] ?? "Regra de bingo";
}

export function markingModeLabel(markingMode: string): string
{
    return markingModeLabels[markingMode] ?? "Não informado";
}

export function roundStatusLabel(status: string): string
{
    return roundStatusLabels[status] ?? "Status não informado";
}

export function eventStatusLabel(status: string): string
{
    return eventStatusLabels[status] ?? "Status não informado";
}
