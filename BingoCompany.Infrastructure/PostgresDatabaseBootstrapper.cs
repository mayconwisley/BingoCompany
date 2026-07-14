using Microsoft.Extensions.Configuration;
using Npgsql;

namespace BingoCompany.Infrastructure;

internal static class PostgresDatabaseBootstrapper
{
	public static void EnsureDatabaseExists(IConfiguration configuration)
	{
		var applicationConnectionString = PostgresConfiguration.CreateConnectionString(configuration);
		var applicationConnection = new NpgsqlConnectionStringBuilder(applicationConnectionString);
		var databaseName = applicationConnection.Database;
		if (string.IsNullOrWhiteSpace(databaseName))
		{
			throw new InvalidOperationException("O nome do database PostgreSQL não foi configurado.");
		}

		var administrationConnection = new NpgsqlConnectionStringBuilder(applicationConnection.ConnectionString)
		{
			Database = configuration["Database:AdministrationDatabase"] ?? "postgres",
			Port = GetAdministrationPort(configuration, applicationConnection.Port)
		};

		using var connection = new NpgsqlConnection(administrationConnection.ConnectionString);
		connection.Open();

		if (DatabaseExists(connection, databaseName))
		{
			return;
		}

		using var createDatabase = connection.CreateCommand();
		createDatabase.CommandText = $"CREATE DATABASE {QuoteIdentifier(databaseName)}";
		createDatabase.ExecuteNonQuery();
	}

	private static bool DatabaseExists(NpgsqlConnection connection, string databaseName)
	{
		using var command = connection.CreateCommand();
		command.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
		command.Parameters.AddWithValue("databaseName", databaseName);

		return command.ExecuteScalar() is not null;
	}

	private static int GetAdministrationPort(IConfiguration configuration, int applicationPort)
	{
		return int.TryParse(configuration["Database:AdministrationPort"], out var administrationPort)
			? administrationPort
			: applicationPort;
	}

	private static string QuoteIdentifier(string identifier) => $"\"{identifier.Replace("\"", "\"\"")}\"";
}
