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
}
