using LedgerFlow.Application.Dtos.Financial;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Infrastructure.Persistence.Repositories.Models;

internal sealed class AccountReadModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Type { get; set; }
    public decimal InitialBalance { get; set; }
    public decimal Balance { get; set; }
    public bool IsActive { get; set; }

    public AccountResponseDto ToDto() => new(
        Id,
        Name,
        (AccountType)Type,
        InitialBalance,
        Balance,
        IsActive);
}
