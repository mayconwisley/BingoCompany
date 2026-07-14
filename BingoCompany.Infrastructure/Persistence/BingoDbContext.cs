using BingoCompany.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Infrastructure.Persistence;

public sealed class BingoDbContext(DbContextOptions<BingoDbContext> options) : DbContext(options)
{
	public DbSet<BingoEvent> Events => Set<BingoEvent>();
	public DbSet<Company> Companies => Set<Company>();
	public DbSet<CompanyUser> CompanyUsers => Set<CompanyUser>();
	public DbSet<Participant> Participants => Set<Participant>();
	public DbSet<BingoCard> Cards => Set<BingoCard>();
	public DbSet<BingoRound> Rounds => Set<BingoRound>();
	public DbSet<PrizeStage> PrizeStages => Set<PrizeStage>();
	public DbSet<DrawnNumber> DrawnNumbers => Set<DrawnNumber>();
	public DbSet<CardMark> CardMarks => Set<CardMark>();
	public DbSet<RoundEligibleCard> RoundEligibleCards => Set<RoundEligibleCard>();
	public DbSet<RoundWinner> RoundWinners => Set<RoundWinner>();
	public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.HasDefaultSchema("bingo");
		modelBuilder.HasAnnotation("BingoCompany:WinningPatternsRevision", "2");
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(BingoDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
