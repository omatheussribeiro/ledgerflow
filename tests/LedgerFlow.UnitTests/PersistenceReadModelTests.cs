using FluentAssertions;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Infrastructure.Persistence.Repositories.Models;

namespace LedgerFlow.UnitTests;

public sealed class PersistenceReadModelTests
{
    [Fact]
    public void TransactionReadModel_ConvertsDatabaseTypesToApplicationDto()
    {
        var occurredOn = new DateTime(2026, 9, 15);
        var row = new TransactionReadModel
        {
            Id = Guid.NewGuid(),
            AccountId = Guid.NewGuid(),
            AccountName = "Main account",
            CategoryId = Guid.NewGuid(),
            CategoryName = "Hosting",
            CategoryColor = "#123456",
            Type = (byte)TransactionType.Expense,
            Description = "Cloud hosting",
            Amount = 49.90m,
            OccurredOn = occurredOn,
            Status = (byte)TransactionStatus.Cleared,
            Notes = "Monthly plan"
        };

        var dto = row.ToDto();

        dto.Type.Should().Be(TransactionType.Expense);
        dto.Status.Should().Be(TransactionStatus.Cleared);
        dto.OccurredOn.Should().Be(DateOnly.FromDateTime(occurredOn));
        dto.Amount.Should().Be(49.90m);
    }

    [Fact]
    public void AccountAndCategoryReadModels_ConvertTinyIntValuesToEnums()
    {
        var account = new AccountReadModel
        {
            Id = Guid.NewGuid(),
            Name = "Main account",
            Type = (byte)AccountType.Checking,
            InitialBalance = 100m,
            Balance = 150m,
            IsActive = true
        };
        var category = new CategoryReadModel
        {
            Id = Guid.NewGuid(),
            Name = "Salary",
            Type = (byte)TransactionType.Income,
            Color = "#22c55e"
        };

        account.ToDto().Type.Should().Be(AccountType.Checking);
        category.ToDto().Type.Should().Be(TransactionType.Income);
    }
}
