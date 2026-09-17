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

    [Fact]
    public void Update_ChangesEditableFields()
    {
        var category = Category.Create(Guid.NewGuid(), "Hosting", TransactionType.Expense, "#123456");

        category.Update("  Consulting  ", TransactionType.Income, "#abcdef");

        category.Name.Should().Be("Consulting");
        category.Type.Should().Be(TransactionType.Income);
        category.Color.Should().Be("#abcdef");
    }

    [Fact]
    public void Delete_SetsDeletionTimestamp()
    {
        var category = Category.Create(Guid.NewGuid(), "Hosting", TransactionType.Expense, null);
        var deletedAt = DateTimeOffset.UtcNow;

        category.Delete(deletedAt);

        category.DeletedAt.Should().Be(deletedAt);
    }
}
