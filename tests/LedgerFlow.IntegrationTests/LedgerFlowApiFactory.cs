using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MsSql;

namespace LedgerFlow.IntegrationTests;

public sealed class LedgerFlowApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU20-ubuntu-22.04")
        .WithPassword("LedgerFlow_tests_2026!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:LedgerFlow"] = _database.GetConnectionString(),
            ["Jwt:Issuer"] = "LedgerFlow.Tests",
            ["Jwt:Audience"] = "LedgerFlow.Tests",
            ["Jwt:SigningKey"] = "integration-tests-only-signing-key-with-more-than-32-bytes",
            ["Database:RunMigrations"] = "true",
            ["Database:SeedDevelopmentData"] = "false"
        }));
    }

    public Task InitializeAsync() => _database.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }
}
