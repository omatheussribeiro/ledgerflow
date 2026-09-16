using FluentAssertions;
using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.UnitTests;

public sealed class CategoryTests
{
    [Fact]
    public void Create_WithoutColor_UsesNeutralDefault()
    {
        Category.Create(Guid.NewGuid(), "Professional services", TransactionType.Income, null)
            .Color.Should().Be("#64748b");
    }

    [Theory]
    [InlineData("green")]
    [InlineData("#123")]
    [InlineData("#GGGGGG")]
    public void Create_WithInvalidColor_RejectsCategory(string color)
    {
        var action = () => Category.Create(Guid.NewGuid(), "Invalid", TransactionType.Expense, color);
        action.Should().Throw<DomainException>();
    }
}
