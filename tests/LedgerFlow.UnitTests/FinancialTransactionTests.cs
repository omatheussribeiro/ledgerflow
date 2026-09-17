using FluentAssertions;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.UnitTests;

public sealed class FinancialTransactionTests
{
    [Fact]
    public void Create_WithValidValues_NormalizesMoneyAndDescription()
    {
        var transaction = FinancialTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense,
            "  Cloud hosting  ", 49.999m, new DateOnly(2026, 9, 15));

        transaction.Description.Should().Be("Cloud hosting");
        transaction.Amount.Should().Be(50.00m);
        transaction.Status.Should().Be(TransactionStatus.Cleared);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Create_WithNonPositiveAmount_RejectsTransaction(decimal amount)
    {
        var action = () => FinancialTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense,
            "Invalid", amount, new DateOnly(2026, 9, 15));

        action.Should().Throw<DomainException>().WithMessage("*greater than zero*");
    }

    [Fact]
    public void Update_ChangesEditableFieldsAndNormalizesValues()
    {
        var transaction = FinancialTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense,
            "Hosting", 50m, new DateOnly(2026, 9, 15));
        var accountId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        transaction.Update(
            accountId,
            categoryId,
            TransactionType.Income,
            "  Consulting  ",
            99.999m,
            new DateOnly(2026, 9, 16),
            TransactionStatus.Pending,
            "  Awaiting payment  ");

        transaction.AccountId.Should().Be(accountId);
        transaction.CategoryId.Should().Be(categoryId);
        transaction.Description.Should().Be("Consulting");
        transaction.Amount.Should().Be(100m);
        transaction.Status.Should().Be(TransactionStatus.Pending);
        transaction.Notes.Should().Be("Awaiting payment");
    }

    [Fact]
    public void Delete_SetsDeletionTimestamp()
    {
        var transaction = FinancialTransaction.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TransactionType.Expense,
            "Hosting", 50m, new DateOnly(2026, 9, 15));
        var deletedAt = DateTimeOffset.UtcNow;

        transaction.Delete(deletedAt);

        transaction.DeletedAt.Should().Be(deletedAt);
    }
}
