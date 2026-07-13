using BingoCompany.Domain;
using BingoCompany.Infrastructure;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class PersistenceIntegrationTests
{
    [Fact]
    public async Task Persists_event_with_printed_card_and_participant()
    {
        var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        await using (var write = new BingoDbContext(options)) { var e = new BingoEvent(Guid.CreateVersion7(), "Festa"); var participant = new Participant(e.Id, "Ana"); e.AddParticipant(participant); e.AddCard(new BingoCard(e.Id, participant.Id, CardType.Digital, new int[5, 5])); write.Events.Add(e); await write.SaveChangesAsync(); }
        await using var read = new BingoDbContext(options); var saved = await read.Events.Include(x => x.Participants).Include(x => x.Cards).SingleAsync(); Assert.Equal("Festa", saved.Name); Assert.Single(saved.Participants); Assert.True(saved.Cards.Single().IsEligible);
    }
}
