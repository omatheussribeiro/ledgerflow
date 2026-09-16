using LedgerFlow.Domain.Common;
using LedgerFlow.Domain.Enums;

namespace LedgerFlow.Domain.Financial;

public sealed class Account
{
    private Account() { }

    public Guid Id { get; private init; }
    public Guid UserId { get; private init; }
    public string Name { get; private set; } = string.Empty;
    public AccountType Type { get; private set; }
    public decimal InitialBalance { get; private init; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    public static Account Create(Guid userId, string name, AccountType type, decimal initialBalance)
    {
        if (userId == Guid.Empty) throw new DomainException("A valid owner is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Account name is required.");
        if (name.Trim().Length > 100) throw new DomainException("Account name cannot exceed 100 characters.");

        return new Account
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            InitialBalance = initialBalance,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}
