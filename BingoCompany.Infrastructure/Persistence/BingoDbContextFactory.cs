using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BingoCompany.Infrastructure.Persistence;

public sealed class BingoDbContextFactory : IDesignTimeDbContextFactory<BingoDbContext>
{
	public BingoDbContext CreateDbContext(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(ResolveApiProjectDirectory())
			.AddJsonFile("appsettings.json", optional: false)
			.AddEnvironmentVariables()
			.Build();
		var options = new DbContextOptionsBuilder<BingoDbContext>();

		PostgresDatabaseBootstrapper.EnsureDatabaseExists(configuration);
		PostgresConfiguration.Configure(options, PostgresConfiguration.CreateConnectionString(configuration));

		return new BingoDbContext(options.Options);
	}

	private static string ResolveApiProjectDirectory()
	{
		var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
		while (directory is not null)
		{
			if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
			{
				return directory.FullName;
			}

			var apiDirectory = Path.Combine(directory.FullName, "BingoCompany.Api");
			if (File.Exists(Path.Combine(apiDirectory, "appsettings.json")))
			{
				return apiDirectory;
			}

			directory = directory.Parent;
		}

		throw new InvalidOperationException("Não foi possível localizar o arquivo appsettings.json da API.");
	}
}
