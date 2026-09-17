using FluentAssertions;
using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;
using LedgerFlow.Infrastructure.Persistence.Context;
using LedgerFlow.Infrastructure.Persistence.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace LedgerFlow.UnitTests;

public sealed class PersistenceContextTests
{
    [Fact]
    public void Model_MapsEveryDbSetToItsExpectedTable()
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseSqlServer("Server=localhost;Database=LedgerFlow;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new LedgerFlowDbContext(options);

        context.Model.FindEntityType(typeof(User))!.GetTableName().Should().Be("Users");
        context.Model.FindEntityType(typeof(Account))!.GetTableName().Should().Be("Accounts");
        context.Model.FindEntityType(typeof(Category))!.GetTableName().Should().Be("Categories");
        context.Model.FindEntityType(typeof(FinancialTransaction))!.GetTableName().Should().Be("Transactions");
        context.Model.FindEntityType(typeof(RefreshToken))!.GetTableName().Should().Be("RefreshTokens");
    }

    [Fact]
    public void Context_ExposesAllConfiguredDbSets()
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseSqlServer("Server=localhost;Database=LedgerFlow;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new LedgerFlowDbContext(options);

        context.Users.Should().NotBeNull();
        context.Accounts.Should().NotBeNull();
        context.Categories.Should().NotBeNull();
        context.Transactions.Should().NotBeNull();
        context.RefreshTokens.Should().NotBeNull();
    }

    [Theory]
    [InlineData(typeof(Account))]
    [InlineData(typeof(Category))]
    [InlineData(typeof(FinancialTransaction))]
    public void SoftDeletableEntities_HaveGlobalQueryFilters(Type entityType)
    {
        var options = new DbContextOptionsBuilder<LedgerFlowDbContext>()
            .UseSqlServer("Server=localhost;Database=LedgerFlow;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        using var context = new LedgerFlowDbContext(options);

        context.Model.FindEntityType(entityType)!.GetDeclaredQueryFilters().Should().NotBeEmpty();
    }
}
