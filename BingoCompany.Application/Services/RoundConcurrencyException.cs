namespace BingoCompany.Application.Services;

public sealed class RoundConcurrencyException : Exception
{
	public RoundConcurrencyException() : base("A rodada foi alterada por outra operação. Recarregue a página antes de tentar novamente.") { }
}
