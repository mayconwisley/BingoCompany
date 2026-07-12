using BingoCompany.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BingoCompany.Infrastructure;

public sealed class BingoDbContext(DbContextOptions<BingoDbContext> options) : DbContext(options)
{
	public DbSet<BingoEvent> Events => Set<BingoEvent>();
	public DbSet<Participant> Participants => Set<Participant>();
	public DbSet<BingoCard> Cards => Set<BingoCard>();
	public DbSet<BingoRound> Rounds => Set<BingoRound>();
	public DbSet<PrizeStage> PrizeStages => Set<PrizeStage>();
	public DbSet<DrawnNumber> DrawnNumbers => Set<DrawnNumber>();
	public DbSet<CardMark> CardMarks => Set<CardMark>();
	public DbSet<RoundEligibleCard> RoundEligibleCards => Set<RoundEligibleCard>();
	public DbSet<RoundWinner> RoundWinners => Set<RoundWinner>();
	protected override void OnModelCreating(ModelBuilder b)
	{
		b.Entity<BingoEvent>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.PublicCode).IsUnique(); e.HasMany(x => x.Rounds).WithOne().HasForeignKey(x => x.EventId); e.HasMany(x => x.Cards).WithOne().HasForeignKey(x => x.EventId); e.HasMany(x => x.Participants).WithOne().HasForeignKey(x => x.EventId); });
		b.Entity<BingoCard>(e => { e.HasKey(x => x.Id); e.HasIndex(x => x.PublicCode).IsUnique(); e.Property(x => x.Numbers).HasConversion(v => SerializeCard(v), v => DeserializeCard(v)); });
		b.Entity<BingoRound>(e => { e.HasKey(x => x.Id); e.Property(x => x.DrawSequence).HasConversion(v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default), v => v == null ? null : JsonSerializer.Deserialize<int[]>(v, JsonSerializerOptions.Default)); e.HasMany(x => x.Stages).WithOne().HasForeignKey(x => x.RoundId); e.HasMany(x => x.DrawnNumbers).WithOne().HasForeignKey(x => x.RoundId); e.HasMany(x => x.EligibleCards).WithOne().HasForeignKey(x => x.RoundId); });
		b.Entity<Participant>().HasKey(x => x.Id); b.Entity<PrizeStage>().HasKey(x => x.Id); b.Entity<DrawnNumber>().HasKey(x => x.Id); b.Entity<RoundEligibleCard>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.RoundId, x.CardId }).IsUnique(); }); b.Entity<CardMark>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.RoundId, x.CardId, x.Number }).IsUnique(); }); b.Entity<RoundWinner>(e => { e.HasKey(x => x.Id); e.HasIndex(x => new { x.StageId, x.CardId }).IsUnique(); });
	}
	private static string SerializeCard(int[,] card) => JsonSerializer.Serialize(Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => card[r, c]).ToArray()).ToArray());
	private static int[,] DeserializeCard(string json)
	{
		var rows = JsonSerializer.Deserialize<int[][]>(json)!; var result = new int[5, 5];
		for (var r = 0; r < 5; r++) for (var c = 0; c < 5; c++) result[r, c] = rows[r][c];
		return result;
	}
}
