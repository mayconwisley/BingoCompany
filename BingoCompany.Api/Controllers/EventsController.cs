using BingoCompany.Api.Hubs;
using BingoCompany.Api.Interfaces;
using BingoCompany.Api.Security;
using BingoCompany.Api.Contracts;
using BingoCompany.Api.Services;
using BingoCompany.Application;
using BingoCompany.Application.Services;
using BingoCompany.Application.Interfaces;
using BingoCompany.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BingoCompany.Domain;

namespace BingoCompany.Api.Controllers;

[ApiController, Route("api/events"), Authorize, ServiceFilter<CompanyEventOwnerFilter>]
public sealed partial class EventsController(BingoDbContext db, IHubContext<BingoHub> hub, IBingoRoundGameplayService gameplayService, IEventParticipantRegistrationService participantRegistrationService) : ControllerBase
{
	private const int MaximumPrizeImageLength = 2_800_000;
	private const int DefaultEventsPageSize = 12;
	private const int MaximumEventsPageSize = 100;
	private static readonly string[] SupportedPrizeImagePrefixes = ["data:image/jpeg;base64,", "data:image/png;base64,", "data:image/webp;base64,"];
	[HttpGet]
	public async Task<ActionResult<object>> List([FromQuery] int page = 1, [FromQuery] int pageSize = DefaultEventsPageSize)
	{
		var normalizedPage = Math.Max(page, 1);
		var normalizedPageSize = Math.Clamp(pageSize, 1, MaximumEventsPageSize);
		var events = db.Events
			.AsNoTracking()
			.Where(item => item.CompanyId == GetCompanyId())
			.OrderByDescending(item => item.CreatedAt)
			.ThenByDescending(item => item.Id);
		var totalItems = await events.CountAsync(HttpContext.RequestAborted);
		var items = await events
			.Skip((normalizedPage - 1) * normalizedPageSize)
			.Take(normalizedPageSize)
			.Select(item => new { item.Id, item.Name, item.PublicCode, item.Status, item.MarkingMode, item.CreatedAt })
			.ToListAsync(HttpContext.RequestAborted);
		return Ok(new { items, page = normalizedPage, pageSize = normalizedPageSize, totalItems, totalPages = (int)Math.Ceiling(totalItems / (double)normalizedPageSize) });
	}
	[HttpPost]
	public async Task<ActionResult<object>> Create(CreateEventRequest request)
	{
		if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest("Informe o nome do evento.");
		var bingoEvent = new BingoEvent(GetCompanyId(), request.Name, request.CardsPerParticipant <= 0 ? 1 : request.CardsPerParticipant, request.MarkingMode);
		db.Events.Add(bingoEvent); db.AuditEntries.Add(new AuditEntry(bingoEvent.Id, "Evento criado", $"Evento {bingoEvent.Name} criado.")); await db.SaveChangesAsync();
		return CreatedAtAction(nameof(Get), new { eventId = bingoEvent.Id }, new { bingoEvent.Id, bingoEvent.Name, bingoEvent.PublicCode, bingoEvent.Status, bingoEvent.MarkingMode, bingoEvent.CreatedAt });
	}
	[HttpGet("{eventId:guid}")]
	public async Task<ActionResult<object>> Get(Guid eventId)
	{
		var e = await db.Events.Include(x => x.Rounds).ThenInclude(x => x.Stages).Include(x => x.Cards).Include(x => x.Participants).SingleOrDefaultAsync(x => x.Id == eventId);
		if (e is null) return NotFound();

		AwardedCardSummary[] awardedCards = e.Status != EventStatus.Finished
			? []
			: await (
				from winner in db.RoundWinners.AsNoTracking()
				join card in db.Cards.AsNoTracking() on winner.CardId equals card.Id
				join participant in db.Participants.AsNoTracking() on winner.ParticipantId equals participant.Id
				join round in db.Rounds.AsNoTracking() on winner.RoundId equals round.Id
				join stage in db.PrizeStages.AsNoTracking() on winner.StageId equals stage.Id
				where winner.IsWinner && winner.RevealedAt.HasValue && round.EventId == eventId
				orderby round.Sequence, stage.Sequence
				select new AwardedCardSummary(card.PublicCode, participant.Name, round.Name, stage.PrizeName)
			).ToArrayAsync();

		return Ok(new { e.Id, e.Name, e.PublicCode, e.Status, participants = e.Participants.Count, cards = e.Cards.Count, participantList = e.Participants.OrderBy(item => item.Name).Select(item => new { item.Id, item.Name, item.Type }), cardList = e.Cards.OrderByDescending(item => item.CreatedAt).Select(item => new { item.PublicCode, item.Type, item.Status, item.Fingerprint, item.ParticipantId }), awardedCards, rounds = e.Rounds.OrderBy(x => x.CreatedAt).ThenBy(x => x.Sequence).Select(x => new { x.Id, x.Name, x.Status, x.CreatedAt, stages = x.Stages.OrderBy(stage => stage.Sequence).Select(stage => new { stage.Sequence, stage.PrizeName, stage.Pattern, stage.PrizeImageDataUrl, stage.IsActive, stage.IsCompleted }) }) });
	}
	[HttpPost("{eventId:guid}/registration/open")]
	public async Task<IActionResult> OpenRegistration(Guid eventId) { var e = await db.Events.FindAsync(eventId); if (e is null) return NotFound(); e.OpenRegistration(); db.AuditEntries.Add(new AuditEntry(eventId, "Inscrições abertas", "As inscrições do evento foram abertas.")); await db.SaveChangesAsync(); return NoContent(); }
	[HttpPost("{eventId:guid}/participants")]
	public async Task<ActionResult<object>> Join(Guid eventId, JoinEventRequest request)
	{
		try
		{
			var registration = await participantRegistrationService.Register(eventId, request, HttpContext.RequestAborted);
			return registration is null
				? NotFound()
				: Ok(new { participantId = registration.ParticipantId, cardId = registration.CardId, registration.PublicCode, registration.Numbers });
		}
		catch (InvalidOperationException exception)
		{
			return Conflict(exception.Message);
		}
	}
	private static int[][] ToRows(int[,] card) => Enumerable.Range(0, 5).Select(r => Enumerable.Range(0, 5).Select(c => card[r, c]).ToArray()).ToArray();
	private sealed record AwardedCardSummary(string PublicCode, string ParticipantName, string RoundName, string PrizeName);
	private async Task<RoundWinner?> FindPendingWinner(Guid roundId, Guid stageId) => (await db.RoundWinners
		.Where(item => item.RoundId == roundId && item.StageId == stageId && item.IsWinner && item.RevealedAt.HasValue && !item.PrizeDeliveredAt.HasValue && !item.PrizeDeclinedAt.HasValue)
		.ToListAsync())
		.OrderByDescending(item => item.TieBreakerNumber)
		.ThenBy(item => item.RevealedAt)
		.ThenBy(item => item.Id)
		.FirstOrDefault();
	private static bool IsValidPrizeImage(string? imageDataUrl) => string.IsNullOrWhiteSpace(imageDataUrl) || imageDataUrl.Length <= MaximumPrizeImageLength && SupportedPrizeImagePrefixes.Any(prefix => imageDataUrl.StartsWith(prefix, StringComparison.Ordinal));
	private static bool HasDuplicatePatterns(IEnumerable<CreatePrizeStageRequest> stages) => stages.GroupBy(stage => stage.Pattern).Any(group => group.Count() > 1);
	private Guid GetCompanyId() => Guid.Parse(User.FindFirstValue("company_id")!);
}
