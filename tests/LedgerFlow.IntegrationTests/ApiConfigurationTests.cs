using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LedgerFlow.IntegrationTests;

public sealed class ApiConfigurationTests
{
    [Fact]
    public async Task Host_UsesConfigurationProvidedByWebApplicationFactory()
    {
        using var factory = new ConfigurationApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/health", CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed class ConfigurationApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:LedgerFlow"] = "Server=unused;Database=unused;User Id=unused;Password=unused;TrustServerCertificate=True",
                    ["Jwt:Issuer"] = "LedgerFlow.Tests",
                    ["Jwt:Audience"] = "LedgerFlow.Tests",
                    ["Jwt:SigningKey"] = "integration-tests-only-signing-key-with-more-than-32-bytes",
                    ["Database:RunMigrations"] = "false",
                    ["Database:SeedDevelopmentData"] = "false"
                }));
        }
    }
}
