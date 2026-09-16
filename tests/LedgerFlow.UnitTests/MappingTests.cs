using FluentAssertions;
using LedgerFlow.Application.Mappings;
using LedgerFlow.Domain.Enums;
using LedgerFlow.Domain.Financial;
using LedgerFlow.Domain.Identity;

namespace LedgerFlow.UnitTests;

public sealed class MappingTests
{
    [Fact]
    public void AccountMapping_CopiesDomainDataAndCalculatedBalance()
    {
        var account = Account.Create(Guid.NewGuid(), "Main account", AccountType.Checking, 500m);

        var dto = account.ToDto(725m);

        dto.Id.Should().Be(account.Id);
        dto.Name.Should().Be("Main account");
        dto.Type.Should().Be(AccountType.Checking);
        dto.InitialBalance.Should().Be(500m);
        dto.Balance.Should().Be(725m);
    }

    [Fact]
    public void UserMapping_DoesNotExposePasswordHash()
    {
        var user = User.Create("Test User", "test@example.com", "secret-hash");

        var dto = user.ToDto();

        dto.Should().BeEquivalentTo(new
        {
            user.Id,
            user.Name,
            user.Email,
            user.Role
        });
    }
}
