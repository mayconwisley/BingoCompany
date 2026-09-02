using BingoCompany.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BingoCompany.Api.Health;

public sealed class PostgresReadinessHealthCheck(IServiceScopeFactory serviceScopeFactory) : IHealthCheck
{
	public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
	{
		await using var scope = serviceScopeFactory.CreateAsyncScope();
		var db = scope.ServiceProvider.GetRequiredService<BingoDbContext>();
		var canConnect = await db.Database.CanConnectAsync(cancellationToken);
		return canConnect
			? HealthCheckResult.Healthy("PostgreSQL está disponível.")
			: HealthCheckResult.Unhealthy("Não foi possível conectar ao PostgreSQL.");
	}
}
