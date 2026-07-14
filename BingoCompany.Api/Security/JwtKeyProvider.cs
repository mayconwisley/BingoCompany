using System.Security.Cryptography;

namespace BingoCompany.Api.Security;

public sealed class JwtKeyProvider
{
	private readonly string _key;

	public JwtKeyProvider(IConfiguration configuration, IHostEnvironment environment)
	{
		_key = configuration["BingoJwtKey"]
			?? (environment.IsDevelopment()
				? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
				: throw new InvalidOperationException("Configure a variável de ambiente BingoJwtKey para assinar os tokens de acesso."));

		if (System.Text.Encoding.UTF8.GetByteCount(_key) < 32)
		{
			throw new InvalidOperationException("BingoJwtKey deve possuir ao menos 32 bytes UTF-8.");
		}
	}

	public string GetKey() => _key;
}
