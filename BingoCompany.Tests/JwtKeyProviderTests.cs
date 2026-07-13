using BingoCompany.Api.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BingoCompany.Tests;

public sealed class JwtKeyProviderTests
{
    [Fact]
    public void GetKey_RejectsShortProductionKey()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["BingoJwtKey"] = "short" })
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyProvider(configuration, new TestHostEnvironment()));

        Assert.Contains("ao menos 32 bytes", exception.Message);
    }

    [Fact]
    public void GetKey_RequiresConfiguredKeyOutsideDevelopment()
    {
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() => new JwtKeyProvider(configuration, new TestHostEnvironment()));

        Assert.Contains("BingoJwtKey", exception.Message);
    }

    [Fact]
    public void GetKey_CreatesTemporaryKeyOnlyInDevelopment()
    {
        var provider = new JwtKeyProvider(new ConfigurationBuilder().Build(), new TestHostEnvironment { EnvironmentName = Environments.Development });

        Assert.True(System.Text.Encoding.UTF8.GetByteCount(provider.GetKey()) >= 32);
        Assert.Equal(provider.GetKey(), provider.GetKey());
    }
}
