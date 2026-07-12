using BingoCompany.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddDbContext<BingoDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("Bingo") ?? "Data Source=bingo.db"));
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true)));

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BingoDbContext>();
    await db.Database.EnsureCreatedAsync();
    await UpgradeDevelopmentSchemaAsync(db);
}
app.UseSwagger(); app.UseSwaggerUI();
app.UseCors(); app.UseHttpsRedirection(); app.MapControllers(); app.MapHub<BingoCompany.Api.Hubs.BingoHub>("/hubs/bingo");
app.Run();
static async Task UpgradeDevelopmentSchemaAsync(BingoDbContext db)
{
    var connection = db.Database.GetDbConnection(); await connection.OpenAsync();
    await using (var check = connection.CreateCommand())
    {
        check.CommandText = "PRAGMA table_info('Events')"; await using var reader = await check.ExecuteReaderAsync(); var hasMarkingMode = false;
        while (await reader.ReadAsync()) if (reader.GetString(1) == "MarkingMode") hasMarkingMode = true;
        await reader.CloseAsync(); if (!hasMarkingMode) await db.Database.ExecuteSqlRawAsync("ALTER TABLE Events ADD COLUMN MarkingMode INTEGER NOT NULL DEFAULT 0");
    }
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS RoundEligibleCards (Id TEXT NOT NULL PRIMARY KEY, RoundId TEXT NOT NULL, CardId TEXT NOT NULL, ParticipantId TEXT NOT NULL, IncludedAt TEXT NOT NULL, FOREIGN KEY (RoundId) REFERENCES Rounds(Id) ON DELETE CASCADE)");
    await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_RoundEligibleCards_RoundId_CardId ON RoundEligibleCards(RoundId, CardId)");
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS CardMarks (Id TEXT NOT NULL PRIMARY KEY, RoundId TEXT NOT NULL, CardId TEXT NOT NULL, Number INTEGER NOT NULL, DrawSequence INTEGER NOT NULL, MarkedAt TEXT NOT NULL)");
    await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_CardMarks_RoundId_CardId_Number ON CardMarks(RoundId, CardId, Number)");
    await db.Database.ExecuteSqlRawAsync("CREATE TABLE IF NOT EXISTS RoundWinners (Id TEXT NOT NULL PRIMARY KEY, RoundId TEXT NOT NULL, StageId TEXT NOT NULL, CardId TEXT NOT NULL, ParticipantId TEXT NOT NULL, DrawSequence INTEGER NOT NULL, TieBreakerNumber INTEGER NULL, IsWinner INTEGER NOT NULL, DetectedAt TEXT NOT NULL, RevealedAt TEXT NULL)");
    await db.Database.ExecuteSqlRawAsync("CREATE UNIQUE INDEX IF NOT EXISTS IX_RoundWinners_StageId_CardId ON RoundWinners(StageId, CardId)");
}
public partial class Program;
