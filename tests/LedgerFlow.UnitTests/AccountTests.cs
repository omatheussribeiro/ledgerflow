using FluentAssertions;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;

namespace LedgerFlow.UnitTests;

public sealed class AccountTests
{
    [Fact]
    public void Update_ChangesEditableFields()
    {
        var account = Account.Create(Guid.NewGuid(), "Checking", AccountType.Checking, 100m);

        account.Update("  Emergency fund  ", AccountType.Savings, 250.129m);

        account.Name.Should().Be("Emergency fund");
        account.Type.Should().Be(AccountType.Savings);
        account.InitialBalance.Should().Be(250.129m);
    }

    [Fact]
    public void Delete_MarksAccountAsDeletedAndInactive()
    {
        var account = Account.Create(Guid.NewGuid(), "Checking", AccountType.Checking, 100m);
        var deletedAt = DateTimeOffset.UtcNow;

        account.Delete(deletedAt);

        account.DeletedAt.Should().Be(deletedAt);
        account.IsActive.Should().BeFalse();
    }
}
