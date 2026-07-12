using BingoCompany.Application;
using BingoCompany.Domain;

namespace BingoCompany.Tests;

public sealed class BingoRulesTests
{
    private static readonly int[,] Card = { { 1, 16, 31, 46, 61 }, { 2, 17, 32, 47, 62 }, { 3, 18, 0, 48, 63 }, { 4, 19, 34, 49, 64 }, { 5, 20, 35, 50, 65 } };
    [Fact] public void Full_card_requires_all_non_free_cells() => Assert.False(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int>(Enumerable.Range(1, 75).Where(x => x != 65)), WinningPattern.FullCard));
    [Fact] public void Horizontal_line_is_completed_when_all_values_are_drawn() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 16, 31, 46, 61 }, WinningPattern.HorizontalLine));
    [Fact] public void Four_corners_requires_every_corner() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 61, 5, 65 }, WinningPattern.FourCorners));
	[Fact] public void Main_diagonal_requires_the_bingo_diagonal() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 17, 49, 65 }, WinningPattern.MainDiagonal));
	[Fact] public void Secondary_diagonal_requires_the_reverse_bingo_diagonal() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 61, 47, 19, 5 }, WinningPattern.SecondaryDiagonal));
	[Fact] public void B_column_requires_all_numbers_in_the_b_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 2, 3, 4, 5 }, WinningPattern.BColumn));
	[Fact] public void O_column_requires_all_numbers_in_the_o_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 61, 62, 63, 64, 65 }, WinningPattern.OColumn));
    [Fact] public void Secure_sequence_contains_each_bingo_number_once() { var sequence = SecureDrawSequence.Generate(); Assert.Equal(75, sequence.Distinct().Count()); Assert.Equal(Enumerable.Range(1, 75), sequence.Order()); }
    [Fact] public void Generated_card_obeys_bingo_column_ranges() { var card = new Bingo75CardGenerator().Generate(); for (var c = 0; c < 5; c++) for (var r = 0; r < 5; r++) if (r != 2 || c != 2) Assert.InRange(card[r, c], c * 15 + 1, c * 15 + 15); }
    [Fact]
    public void Next_draw_requires_operator_to_close_winner_presentation()
    {
        var round = new BingoRound(Guid.CreateVersion7(), 1, "Rodada 1");
        round.AddStage(new PrizeStage(round.Id, 1, "Linha", WinningPattern.HorizontalLine));
        round.AddStage(new PrizeStage(round.Id, 2, "Bingo", WinningPattern.FullCard));
        var sequence = Enumerable.Range(1, 75).ToArray();
        round.Start(sequence, SecureDrawSequence.Hash(sequence));
        round.FinishStage();

        Assert.True(round.HasWinnerPresentationPending);
        Assert.Throws<InvalidOperationException>(() => round.DrawNext());

        round.CloseWinnerPresentation();

        Assert.False(round.HasWinnerPresentationPending);
        Assert.Equal(1, round.DrawNext().Number);
    }
    [Fact]
    public void Completed_card_can_only_be_replaced_once_by_a_card_for_the_same_participant()
    {
        var eventId = Guid.CreateVersion7();
        var participantId = Guid.CreateVersion7();
        var card = new BingoCard(eventId, participantId, CardType.Digital, Card);
        var replacement = new BingoCard(eventId, participantId, CardType.Digital, Card);

        Assert.True(card.IsComplete(Enumerable.Range(1, 75)));

        card.ReplaceWith(replacement);

        Assert.Equal(replacement.Id, card.ReplacementCardId);
		Assert.False(card.IsEligible);
        Assert.Throws<InvalidOperationException>(() => card.ReplaceWith(new BingoCard(eventId, participantId, CardType.Digital, Card)));
    }
}
