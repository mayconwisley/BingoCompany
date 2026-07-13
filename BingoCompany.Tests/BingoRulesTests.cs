using BingoCompany.Application;
using BingoCompany.Domain;

namespace BingoCompany.Tests;

public sealed class BingoRulesTests
{
    private static readonly int[,] Card = { { 1, 16, 31, 46, 61 }, { 2, 17, 32, 47, 62 }, { 3, 18, 0, 48, 63 }, { 4, 19, 34, 49, 64 }, { 5, 20, 35, 50, 65 } };
    [Fact] public void Full_card_requires_all_non_free_cells() => Assert.False(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int>(Enumerable.Range(1, 75).Where(x => x != 65)), WinningPattern.FullCard));
    [Fact] public void Horizontal_line_is_completed_when_all_values_are_drawn() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 16, 31, 46, 61 }, WinningPattern.HorizontalLine));
    [Fact] public void Four_corners_requires_every_corner() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 61, 5, 65 }, WinningPattern.FourCorners));
	[Fact] public void B_column_requires_all_numbers_in_the_b_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 1, 2, 3, 4, 5 }, WinningPattern.BColumn));
    [Fact] public void O_column_requires_all_numbers_in_the_o_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 61, 62, 63, 64, 65 }, WinningPattern.OColumn));
	[Fact] public void I_column_requires_all_numbers_in_the_i_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 16, 17, 18, 19, 20 }, WinningPattern.IColumn));
	[Fact] public void N_column_counts_the_free_center() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 31, 32, 34, 35 }, WinningPattern.NColumn));
	[Fact] public void G_column_requires_all_numbers_in_the_g_column() => Assert.True(WinningPatternEvaluator.IsCompleted(Card, new HashSet<int> { 46, 47, 48, 49, 50 }, WinningPattern.GColumn));
	[Fact] public void Statistics_count_cards_by_numbers_remaining_for_the_current_rule()
	{
		var eventId = Guid.CreateVersion7();
		var cards = new[]
		{
			new BingoCard(eventId, Guid.CreateVersion7(), CardType.Digital, Card),
			new BingoCard(eventId, Guid.CreateVersion7(), CardType.Digital, Card)
		};
		var marks = new Dictionary<Guid, IReadOnlySet<int>>
		{
			[cards[0].Id] = new HashSet<int> { 1, 16, 31, 46 },
			[cards[1].Id] = new HashSet<int> { 1, 16, 31 }
		};

		var statistics = BingoRoundStatisticsCalculator.Calculate(cards, card => marks[card.Id], WinningPattern.HorizontalLine);

		Assert.Equal(2, statistics.TotalCards);
		Assert.Equal(1, statistics.OneNumberAway);
		Assert.Equal(1, statistics.TwoNumbersAway);
	}
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
	public void Column_stage_draws_only_numbers_from_its_column_range()
	{
		var round = new BingoRound(Guid.CreateVersion7(), 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Coluna I", WinningPattern.IColumn));
		var sequence = Enumerable.Range(1, 75).ToArray();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));

		var drawn = round.DrawNext();

		Assert.Equal(16, drawn.Number);
	}
	[Fact]
	public void Round_rejects_a_third_stage_with_the_same_pattern()
	{
		var round = new BingoRound(Guid.CreateVersion7(), 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Coluna B 1", WinningPattern.BColumn));
		round.AddStage(new PrizeStage(round.Id, 2, "Coluna B 2", WinningPattern.BColumn));

		var exception = Assert.Throws<InvalidOperationException>(() => round.AddStage(new PrizeStage(round.Id, 3, "Coluna B 3", WinningPattern.BColumn)));

		Assert.Equal("Uma rodada permite no máximo duas etapas com a mesma regra de premiação.", exception.Message);
	}
	[Fact]
	public void Prize_stages_follow_the_recommended_pattern_order()
	{
		var orderedPatterns = PrizeStageOrdering.Order(
			new[] { WinningPattern.FullCard, WinningPattern.OColumn, WinningPattern.HorizontalLine, WinningPattern.BColumn },
			pattern => pattern);

		Assert.Equal(new[] { WinningPattern.BColumn, WinningPattern.OColumn, WinningPattern.HorizontalLine, WinningPattern.FullCard }, orderedPatterns);
	}
	[Fact]
	public void Tie_breaker_blocks_drawing_until_the_winner_is_revealed()
	{
		var round = new BingoRound(Guid.CreateVersion7(), 1, "Rodada 1");
		round.AddStage(new PrizeStage(round.Id, 1, "Linha", WinningPattern.HorizontalLine));
		var sequence = Enumerable.Range(1, 75).ToArray();
		round.Start(sequence, SecureDrawSequence.Hash(sequence));
		round.DetectWinner();
		round.StartTieBreaker();

		Assert.Equal(RoundStatus.TieBreaker, round.Status);
		Assert.Throws<InvalidOperationException>(() => round.DrawNext());
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
	[Fact]
	public void Printed_card_only_becomes_eligible_after_association_and_activation()
	{
		var card = new BingoCard(Guid.CreateVersion7(), null, CardType.Printed, Card);

		Assert.Equal(CardStatus.Printed, card.Status);
		Assert.False(card.IsEligible);
		Assert.NotEmpty(card.Fingerprint);

		card.Assign(Guid.CreateVersion7());
		Assert.Equal(CardStatus.Assigned, card.Status);
		Assert.False(card.IsEligible);

		card.Activate();
		Assert.True(card.IsEligible);
	}
}
