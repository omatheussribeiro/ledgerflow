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
    public decimal InitialBalance { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset? DeletedAt { get; private set; }

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

    public void Update(string name, AccountType type, decimal initialBalance)
    {
        if (DeletedAt is not null) throw new DomainException("Deleted accounts cannot be updated.");
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Account name is required.");
        if (name.Trim().Length > 100) throw new DomainException("Account name cannot exceed 100 characters.");
        if (!Enum.IsDefined(type)) throw new DomainException("Account type is invalid.");
        if (initialBalance is < -999999999.99m or > 999999999.99m)
            throw new DomainException("Initial balance is outside the supported range.");

        Name = name.Trim();
        Type = type;
        InitialBalance = initialBalance;
    }

    public void Delete(DateTimeOffset deletedAt)
    {
        if (DeletedAt is not null) return;
        DeletedAt = deletedAt;
        IsActive = false;
    }
}
