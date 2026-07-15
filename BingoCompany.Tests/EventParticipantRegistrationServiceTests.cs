using BingoCompany.Api.Contracts;
using BingoCompany.Api.Services;
using BingoCompany.Domain;
using BingoCompany.Domain.Entities;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BingoCompany.Tests;

public sealed class EventParticipantRegistrationServiceTests
{
	[Fact]
	public async Task Register_generates_a_lot_of_assigned_cards_when_the_event_allows_multiple_digital_cards()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa", cardsPerParticipant: 3);
		bingoEvent.OpenRegistration();
		db.Events.Add(bingoEvent);
		await db.SaveChangesAsync();
		var service = new EventParticipantRegistrationService(db);

		var registration = await service.Register(bingoEvent.Id, new JoinEventRequest("Ana"), null, CancellationToken.None);

		Assert.NotNull(registration);
		Assert.Equal(3, registration.Cards.Count);
		Assert.All(registration.Cards, card => Assert.Equal(CardStatus.Assigned, card.Status));
		Assert.Equal(3, registration.Cards.Select(card => card.PublicCode).Distinct().Count());
	}

	[Fact]
	public async Task Register_keeps_the_existing_single_card_flow_active()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenRegistration();
		db.Events.Add(bingoEvent);
		await db.SaveChangesAsync();
		var service = new EventParticipantRegistrationService(db);

		var registration = await service.Register(bingoEvent.Id, new JoinEventRequest("Ana"), null, CancellationToken.None);

		var card = Assert.Single(registration!.Cards);
		Assert.Equal(CardStatus.Active, card.Status);
	}

	[Fact]
	public async Task Register_uses_the_quantity_selected_by_the_participant_when_card_purchase_is_open()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa", cardsPerParticipant: 1);
		bingoEvent.OpenCardPurchase(10);
		db.Events.Add(bingoEvent);
		await db.SaveChangesAsync();
		var service = new EventParticipantRegistrationService(db);

		var account = new ParticipantAccount("Ana", "ana@example.com", "hash");
		db.ParticipantAccounts.Add(account);
		await db.SaveChangesAsync();
		var registration = await service.Register(bingoEvent.Id, new JoinEventRequest("Ana", CardsQuantity: 4), account.Id, CancellationToken.None);

		Assert.Equal(4, registration!.Cards.Count);
		Assert.All(registration.Cards, card => Assert.Equal(CardStatus.Assigned, card.Status));
	}

	[Fact]
	public async Task Register_rejects_a_purchase_above_the_event_sale_limit()
	{
		var options = new DbContextOptionsBuilder<BingoDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
		await using var db = new BingoDbContext(options);
		var bingoEvent = new BingoEvent(Guid.CreateVersion7(), "Festa");
		bingoEvent.OpenCardPurchase(3);
		var account = new ParticipantAccount("Ana", "ana@example.com", "hash");
		db.Events.Add(bingoEvent);
		db.ParticipantAccounts.Add(account);
		await db.SaveChangesAsync();
		var service = new EventParticipantRegistrationService(db);

		var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.Register(bingoEvent.Id, new JoinEventRequest("Ana", CardsQuantity: 4), account.Id, CancellationToken.None));

		Assert.Equal("Não há cartelas suficientes disponíveis para esta compra.", exception.Message);
	}
}
